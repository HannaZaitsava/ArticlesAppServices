using ArticlesService.Application.CQRS.Commands.TagCommands.UpdateTag;
using ArticlesService.Domain.Entities;
using Mapster;

namespace ArticlesService.Application.MappingProfiles
{  
    public class TagMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateTagCommand, Tag>()
               .IgnoreNullValues(true)
               .Ignore(dest => dest.Id);
        }
    }
}