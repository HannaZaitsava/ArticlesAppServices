using System;
using System.Collections.Generic;
using System.Text;
using ArticlesService.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations.Extensions
{
    public static class EntityConfigurationExtensions
    {
        public static void ApplyAuditProperties<T>(this EntityTypeBuilder<T> builder) where T : class, IAuditableEntity
        {
            builder.Property(e => e.CreatedBy).IsRequired();  
            
            builder.HasIndex(e => e.CreatedOn);
        }

        public static void ApplySoftDeleteProperties<T>(this EntityTypeBuilder<T> builder) where T : class, ISoftDelete
        {           
            builder.HasIndex(e => e.DeletedOn);
        }
    }
}
