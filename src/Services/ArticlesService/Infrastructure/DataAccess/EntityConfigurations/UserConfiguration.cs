using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);            

            // Для админ-панели и пагинации списка пользователей
            builder.HasIndex(u => new { u.CreatedOn, u.Id })
                .HasDatabaseName("IX_AspNetUsers_CreatedOn_Id");

            // Для поиска людей по имени/фамилии
            builder.HasIndex(u => new { u.LastName, u.FirstName })
                .HasDatabaseName("IX_AspNetUsers_LastName_FirstName");


            builder.HasQueryFilter(user => !user.IsDeleted);

            builder.Property(x => x.FirstName).HasMaxLength(UserConstraints.FirstNameMaxLength);

            builder.Property(x => x.LastName).HasMaxLength(UserConstraints.LastNameMaxLength);

            // Просто плоская колонка Guid, 
            // запрещаем EF Core создавать Foreign Key на саму таблицу AspNetUsers.
            // Иначе при регистрации самого первого администратора приложение упадет
            builder.Property(u => u.CreatorId);

            // Auditable
            builder.ApplyAuditProperties();
            builder.ApplySoftDeleteProperties();
        }
    }
}
