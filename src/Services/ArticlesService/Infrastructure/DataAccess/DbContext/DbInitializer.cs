using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.Abstractions;
using ArticlesService.Infrastructure.DataAccess.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Infrastructure.DataAccess.DbContext
{
    public sealed class DbInitializer: IDbInitializer
    {
        private readonly AppDbContext _dbContext;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<DbInitializer> _logger;        
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;

        public DbInitializer(
            IHostEnvironment environment,
            ILogger<DbInitializer> logger, 
            AppDbContext dbContext, 
            RoleManager<IdentityRole<Guid>> roleManager, 
            UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        public async Task MigrateAsync(CancellationToken cancellationToken = default) 
        {
            Console.WriteLine($"CONN STRING: {Environment.GetEnvironmentVariable("ConnectionStrings__BaseDbConnection")}");
            try
            {
                if (_environment.IsDevelopment())
                {
                    await _dbContext.Database.MigrateAsync(cancellationToken);
                    await SeedDevelopmentDataAsync(cancellationToken);                  
                }
                //if(environment.IsTest()) // TODO можно создать расширение для окружения
                //{                
                //    await db.Database.EnsureCreatedAsync();
                //}
                if (_environment.IsProduction() || _environment.IsStaging())
                {
                    //- forbidden to use db.Database.Migrate() 
                    //- avaliable options:
                    //    - idempotemt SQL scripts. CI/ CD pipeline (GitHub Actions, GitLab CI) will use it before the app deployment.
                    //    - Migration Bundles (iterator is app standard). Собрать самодостаточный исполняемый файл миграций и запускать его в отдельном контейнере(init-container в K8s) перед запуском основного приложения.
                    //    - Data Seeding в Prod: В Production наполняются только справочники(роли, статусы) через HasData. 

                    await ApplyProductionMigrationsAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fatal database migration error");

                // If the error is critical - abort the application
                // In Docker/Kubernetes this will allow the system to restart the container or stop a bug
                throw;
            }
        }       

        private async Task ApplyProductionMigrationsAsync(CancellationToken cancellationToken = default)
        {
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                _logger.LogWarning("Pending migrations detected: {Migrations}. " +
                    "Migrations should be applied via CI/CD pipeline or migration bundles.",
                    string.Join(", ", pendingMigrations));

                // или 
                // throw new InvalidOperationException("Database is not up to date. Apply migrations before deployment.");
            }

            // Отдельный метод для Production
            // Seed только справочников, если нужно
            await SeedProductionDataAsync(cancellationToken); 
        }

        private async Task SeedProductionDataAsync(CancellationToken cancellationToken = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await SeedRoles();

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogCritical(ex, "Fatal database seeding error");
                throw;
            }
        }

        private async Task SeedDevelopmentDataAsync(CancellationToken cancellationToken = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await SeedRoles();
                await SeedUser();

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogCritical(ex, "Fatal database seeding error");
                throw;
            }
        }
        private async Task SeedRoles()
        {            
            var roleNames = new[] { "Admin", "Member" };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));

                    if (result.Succeeded)
                    {                        
                        _logger.LogInformation("Role '{RoleName}' created", roleName);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role '{RoleName}': {Errors}", roleName, result.ToErrorString());
                        throw new Exception($"Failed to create role '{roleName}': {result.ToErrorString()}");
                    }
                }
            }
        }
        private async Task SeedUser()
        {
            await CreateUser(email: "admin@gmail.com", password: "Admin111#", role: "Admin", firstName: "Alex", lastName: "Smith", phoneNumber: "+3776788989", true);
            await CreateUser(email: "member@gmail.com", password: "Member111#", role: "Member", firstName: "Bob", lastName: "Fox", phoneNumber: "+3776787778", false);        
        }

        private async Task CreateUser(
            string email, string password, string role, 
            string firstName, string lastName, 
            string? phoneNumber, bool isAdmin = false)
        {
            var userExist = await _userManager.FindByEmailAsync(email);
            if (userExist is null)
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    IsAdmin = isAdmin,
                    CreatedOn = DateTimeOffset.UtcNow,
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {                           
                    await _userManager.AddToRoleAsync(user, role);

                    //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //await _userManager.ConfirmEmailAsync(user, token);

                    _logger.LogInformation("User '{UserName}' created", user.UserName);
                }
                else
                {
                    _logger.LogError("Failed to create user '{UserName}': {Errors}", user.UserName, result.ToErrorString());
                    throw new Exception($"Failed to create the user {user.UserName} : {result.ToErrorString()}");
                }
            }
        }
    }
}
