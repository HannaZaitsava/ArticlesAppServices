using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Entities.MtoM;
using ArticlesService.Infrastructure.DataAccess.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations
{
    public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(a => new { a.CreatedOn, a.Id })
                .HasDatabaseName("IX_Articles_CreatedOn_Id");

            builder.HasIndex(a => new { a.Title, a.Id })
                .HasDatabaseName("IX_Articles_Title_Id");


            builder.Property(x => x.PublicationDate)
               .IsRequired(false);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(ArticleConstraints.TitleMaxLength);

            builder.Property(x => x.Content)
                .HasMaxLength(ArticleConstraints.ContentMaxLength);            

            builder
              .HasMany(e => e.Categories)
              .WithMany(e => e.Articles)
              .UsingEntity<ArticleArticleCategory>(
                  r => r.HasOne(at => at.Category).WithMany().HasForeignKey(e => e.CategoryId),
                  l => l.HasOne(at => at.Article).WithMany().HasForeignKey(e => e.ArticleId));

            builder
              .HasMany(e => e.Tags)
              .WithMany(e => e.Articles)
              .UsingEntity<ArticleTag>(
                  r => r.HasOne(at => at.Tag).WithMany().HasForeignKey(e => e.TagId),
                  l => l.HasOne(at => at.Article).WithMany().HasForeignKey(e => e.ArticleId),//.IsRequired(false),
                  j =>
                  {
                      j.ToTable("ArticleTag");
                      j.HasKey(at => new { at.ArticleId, at.TagId });
                  });

            // Auditable
            builder.ApplyAuditProperties();
            builder.ApplySoftDeleteProperties();            
            builder
               .HasOne<User>()
               .WithMany(u => u.Articles)
               .HasForeignKey(e => e.CreatedBy)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();           
        }
    }
}
