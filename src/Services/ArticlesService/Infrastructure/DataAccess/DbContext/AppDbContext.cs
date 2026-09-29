using System.Reflection;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.EntityConfigurations.Extensions;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.IntegrationEventLogEF;

namespace ArticlesService.Infrastructure.DataAccess.DbContext
{
    public sealed class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IDataProtectionKeyContext
    {      
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }
        
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleCategory> ArticleCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.ApplyExcludeSoftDeletedEntitiesFilter();

            // добавляем Outbox-таблицу для хранения интеграционных событий
            builder.UseIntegrationEventLogs();
        }
    }
}
