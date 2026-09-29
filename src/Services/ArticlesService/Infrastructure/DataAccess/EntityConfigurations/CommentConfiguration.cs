using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations
{
    public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(c => c.ArticleId);
            builder.HasIndex(c => c.ParentId);
            builder.HasIndex(c => c.RootCommentId);

            // Составной индекс для cursor-пагинации по дате создания и Id
            builder
                .HasIndex(a => new { a.CreatedOn, a.Id })
                .HasDatabaseName("IX_Comments_CreatedOn_Id");


            builder.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(CommentConstraints.MaxTextLength);
                      
            builder.HasOne(c => c.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.RootComment)
                .WithMany()
                .HasForeignKey(c => c.RootCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hierarchy (Self-reference)
            /*  Не удаляем дочерние комментарии (Replies)!
                При Soft Delete в иерархических структурах физические связи в БД (ParentId и RootCommentId) не разрываются. 
                Это позволяет:
                    - Сохранить структуру дерева (ответы не «отвалятся» и не пропадут).
                    - Видеть контекст беседы (пользователи поймут, на что были даны ответы ниже => на фронтенде просто не отображать текст комментария).
                    - Восстановить комментарий, просто сбросив флаг IsDeleted.
             */
            builder.HasOne(c => c.Parent)
                   .WithMany(p => p.Replies)
                   .HasForeignKey(c => c.ParentId)
                   // For the case when Replies should be deleted together with the Parent comment: Restrict or ClientCascade in order to not cause circle cascade delete. 
                   // For the case of SoftDelete of Cascade Hiding: NoAction in order to save the whole banch of conversation if the parent comment is deleted
                   .OnDelete(DeleteBehavior.NoAction);

            // Auditable
            builder.ApplyAuditProperties();
            builder.ApplySoftDeleteProperties();
            builder
               .HasOne<User>()
               .WithMany(u => u.Comments)
               .HasForeignKey(e => e.CreatedBy)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();    
        }
    }
}
