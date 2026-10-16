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

            // Составной индекс для cursor-пагинации по дате создания и Id
            builder.HasIndex(a => new { a.CreatedOn, a.Id })
                .HasDatabaseName("IX_Articles_CreatedOn_Id");

            builder.HasIndex(a => new { a.Title, a.Id })
                .HasDatabaseName("IX_Articles_Title_Id");

            builder.HasIndex(a => new { a.AuthorId, a.CreatedOn })
                .HasDatabaseName("IX_Articles_AuthorId_CreatedOn");

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.PublicationDate)
               .IsRequired(false);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(ArticleConstraints.TitleMaxLength);

            builder.Property(x => x.Content)
                .HasMaxLength(ArticleConstraints.ContentMaxLength);

            builder.Property(a => a.PreviewText)
           // Задаем точную максимальную длину с запасом под троеточие
           .HasMaxLength(ArticleConstraints.PreviewTextMaxLength + 5)
           // Отключаем Unicode, если пишем только на английском (сохранит в VARCHAR вместо NVARCHAR).
           // Но если статья может быть на русском, IsUnicode(true) оставляем (по дефолту true).
           .IsUnicode(true);

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

            builder
              .HasOne(a => a.Author)
              .WithMany(u => u.Articles)
              .HasForeignKey(e => e.AuthorId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            // Auditable
            builder.ApplyAuditProperties();
            builder.ApplySoftDeleteProperties();
        }
    }
}
