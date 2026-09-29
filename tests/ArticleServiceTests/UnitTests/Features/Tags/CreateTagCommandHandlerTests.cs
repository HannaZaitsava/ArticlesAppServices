using System.Linq.Expressions;
using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.CQRS.Commands.TagCommands.CreateTag;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Tests.UnitTests.Attributes;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;

namespace ArticlesService.Tests.UnitTests.Features.Tags
{
    public class CreateTagCommandHandlerTests
    {
        // Подход Solitary Unit Test с полной изоляцией handler: мокирование всех зависимостей, включая IMapper
        /*
         [Theory, AutoMoqData]
         internal async Task Handle_WhenTagDoesNotExist_ShouldAddTag_SaveChanges_PublishCacheAndReturnDto(
             [Frozen] Mock<IBaseRepository<Tag>> repositoryMock,
             [Frozen] Mock<ICacheInvalidationContext> cacheContextMock,
             [Frozen] Mock<IMapper> mapperMock,
             CreateTagCommand command,
             Tag tagEntity,
             TagResponseDTO responseDto,
             CreateTagCommandHandler handler)
         {
             // Arrange
             tagEntity.Articles = new List<Article>(); // keep articles empty to control expected cache keys

             repositoryMock
                 .Setup(r => r.IsExistingAsync(It.IsAny<Expression<Func<Tag, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

             mapperMock
                 .Setup(m => m.Map<Tag>(command))
                 .Returns(tagEntity);

             mapperMock
                 .Setup(m => m.Map<TagResponseDTO>(tagEntity))
                 .Returns(responseDto);

             // Act
             var result = await handler.Handle(command, CancellationToken.None);

             // Assert
             result.IsSuccess.Should().BeTrue();
             result.Value.Should().BeEquivalentTo(responseDto);

             repositoryMock.Verify(r => r.AddAsync(tagEntity, It.IsAny<CancellationToken>()), Times.Once);
             repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

             cacheContextMock.Verify(c => c.AddTag(CacheTags.Tags), Times.Once);

             mapperMock.Verify(m => m.Map<TagResponseDTO>(tagEntity), Times.Once);
         }
        */

        // Подход с Sociable Unit Test (общительный юнит-тест):
        /*
         Не мокируем внутренние зависимости, которые:
            - детерминированны (всегда вернет один и тот же результат, если переданы одни и те же входные данные)
            - не имеют побочных эффектов (не изменяют состояние вне себя)
         Изолируем только то, что выходит за пределы процесса (БД, сеть, файловая система)

         IMapper - внутрисистемная зависимость; это стаб, а не мок -стабы, Verify для стабов не нужен;
         Маппинг — это деталь реализации. Если тестировать детали реализации, то тест будет неустойчивым к рефакторингу, т.е. хрупким
        */
        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagDoesNotExist_ShouldCreateTagAndReturnSuccess(
           [Frozen] Mock<IBaseRepository<Tag>> repositoryMock,
           [Frozen] ICacheInvalidationContext cacheContext,
           CreateTagCommand command,
           CreateTagCommandHandler handler)
        {
            // Arrange           
            repositoryMock
                .Setup(r => r.IsExistingAsync(It.IsAny<Expression<Func<Tag, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Имитируем поведение БД: присваиваем Guid.NewGuid() объекту Tag
            repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
                .Callback<Tag, CancellationToken>((tag, _) => tag.Id = Guid.NewGuid()) 
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            // ID должен быть сгенерирован БД
            result.Value!.Id.Should().NotBeEmpty(); 
            // Так мы проверяем бизнес-правило: «То, что пришло в команде, должно оказаться в ответе».
            // Т.е. так мы проверяем именно работу хендлера (передал ли он данные), а не просто «вернул ли он тот же инстанс DTO, что и раньше»
            // Если полей 2-3, можно проверть их явно 
            result.Value!.Label.Should().Be(command.Label);
            result.Value.Color.Should().Be(command.Color);
            /*
             Если полей много(10 +): можно использовать BeEquivalentTo с объектом command,
             сравнивая только те поля, что есть и там, и там.

             Если нужно проверить, что никаких лишних полей не попало,
             можно использовать BeEquivalentTo с заранее созданным эталонным DTO

            //result.Value.Should().BeEquivalentTo(command, options => options.ExcludingMissingMembers());  
            */

            repositoryMock.Verify(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Once); 
            repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            cacheContext.Tags.Should().BeEquivalentTo(CacheTags.Tags);
        }

        [Theory, AutoMoqData]
        internal async Task Handle_WhenTagAlreadyExists_ShouldReturnFailureWithProperError(
            [Frozen] Mock<IBaseRepository<Tag>> repositoryMock,
            [Frozen] ICacheInvalidationContext cacheContext,
            CreateTagCommand command,
            CreateTagCommandHandler handler)
        {
            // Arrange
            repositoryMock
                .Setup(r => r.IsExistingAsync(It.IsAny<Expression<Func<Tag, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle()
                .Which.Should().Be(TagErrors.TagAlreadyExists(command.Label));

            repositoryMock.Verify(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
            repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            cacheContext.Tags.Should().BeEmpty();
        }
    }
}