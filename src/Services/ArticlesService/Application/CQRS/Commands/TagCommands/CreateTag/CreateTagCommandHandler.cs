using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.TagCommands.CreateTag
{
    internal class CreateTagCommandHandler (
        IBaseRepository<Tag> repository,
        ICacheInvalidationContext cacheContext,
        IMapper mapper) 
        : IRequestHandler<CreateTagCommand, Result<TagResponseDTO>>
    {        
        public async Task<Result<TagResponseDTO>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var tagLabel = request.Label;
            var exists = await repository.IsExistingAsync(t => t.Label == tagLabel, cancellationToken);

            if (exists)
            {                
                return Result<TagResponseDTO>.Failure([TagErrors.TagAlreadyExists(tagLabel)]);
            }

            var tag = mapper.Map<Tag>(request);
            //tag.Id = Guid.NewGuid(); // Генерация Guid на стороне приложения (Best Practice для CQRS)

            await repository.AddAsync(tag, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            cacheContext.AddTag(CacheTags.Tags);
            
            var tagResponseDTO = mapper.Map<TagResponseDTO>(tag);

            /*
            По стогим правилам CQRS команда не должна возвращать сущность(только Unit или bool).
             Id заполнится при сохранении в БД либо нужна генерация Guid на стороне приложения.

             По нестрогим правилам команда может вернуть:
              - Id, чтобы в контроллере можно было сформировать ссылку на созданный ресурс(return TypedResults.Created($"/tags/{result.Value.Id}", result.Value);).
                    Контроллер возвращает 201 Created с пустым телом, но заполняет заголовок Location
              - короткий ResponseDTO - стандарт индустрии.Это не превращает команду в запрос(Query). 
              - Result Pattern: Команда возвращает объект Result, который сигнализирует об успехе / ошибке.Это считается мета-информацией о выполнении операции,
                    а не «данными для чтения»
            */
            return Result<TagResponseDTO>.Success(tagResponseDTO);
        }
    }
}
