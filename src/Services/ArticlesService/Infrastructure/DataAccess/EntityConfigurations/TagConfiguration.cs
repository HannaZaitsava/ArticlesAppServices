using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations
{   
    public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
    {       
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Label).IsUnique();

            builder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(TagConstraints.MaxLabelLength);

            builder.Property(x => x.Color)
                .IsRequired(false) 
                .HasMaxLength(TagConstraints.MaxColorLength);
            
            // В PostgreSQL Оператор ~* проверяет регулярное выражение без учета регистра (case-insensitive)
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Tag_Color_HexFormat",
                $"\"Color\" IS NULL OR \"Color\" ~* '{TagConstraints.HexColorRegex}'"));

            builder.HasData(DataForSeeding.SeedTags());
        }
    }
}
