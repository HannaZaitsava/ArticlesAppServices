using Domain.Entities.M2M;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesApp.Infrastructure.DataAccess.EntityConfigurations.MtoM
{
    public sealed class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTag>
    {
        public void Configure(EntityTypeBuilder<ArticleTag> builder)
        {
            builder.ToTable(nameof(ArticleTag));
            builder.HasKey(x => new { x.ArticleId, x.TagId });       
        }
    }
}
