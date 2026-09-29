using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities.M2M;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesApp.Infrastructure.DataAccess.EntityConfigurations.MtoM
{
    public sealed class ArticleArticleCategoryConfiguration : IEntityTypeConfiguration<ArticleArticleCategory>
    {
        public void Configure(EntityTypeBuilder<ArticleArticleCategory> builder)
        {
            builder.ToTable(nameof(ArticleArticleCategory));
            builder.HasKey(x => new { x.ArticleId, x.CategoryId });
        }    
    }
}
