using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations
{
    public sealed class ArticleCategoryConfiguration : IEntityTypeConfiguration<ArticleCategory>
    {
        public void Configure(EntityTypeBuilder<ArticleCategory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Name).IsUnique();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(ArticleCategoryConstraints.NameMaxLength);

            builder.HasData(DataForSeeding.SeedArticleCategories());
        }
    }
}
