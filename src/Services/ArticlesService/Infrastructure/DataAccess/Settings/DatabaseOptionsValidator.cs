using FluentValidation;
using Npgsql;

namespace ArticlesService.Infrastructure.DataAccess.Settings
{
    public class DatabaseOptionsValidator : AbstractValidator<DatabaseOptions>
    {        
        public DatabaseOptionsValidator()
        {
            RuleFor(x => x.BaseDbConnection)
             .NotEmpty().WithMessage("The 'BaseDbConnection' string is required.")
             .Must(BeAValidPostgreSqlConnectionString)
             .WithMessage("The connection string has an invalid PostgreSQL format or is missing required fields (Host/Database).");

            RuleFor(x => x.MaxPoolSize)
             .GreaterThanOrEqualTo(x => x.MinPoolLimit)
             .WithMessage(x => $"MaxPoolSize cannot be less than MinPoolLimit ({x.MinPoolLimit}).")

             .LessThanOrEqualTo(x => x.MaxPoolLimit)
             .WithMessage(x => $"MaxPoolSize cannot be greater than MaxPoolLimit ({x.MaxPoolLimit}).");       
        }

        private bool BeAValidPostgreSqlConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return false;

            try
            {
                // Проверка, может ли Npgsql распарсить эту строку
                var builder = new NpgsqlConnectionStringBuilder(connectionString);

                // Проверка наличия критичных полей
                return !string.IsNullOrEmpty(builder.Host) && !string.IsNullOrEmpty(builder.Database);
            }
            catch
            {
                return false;
            }
        }
    }
}
