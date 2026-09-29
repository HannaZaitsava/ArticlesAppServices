using System.Text.Json.Serialization;
using ArticlesService.Application.Abstractions;
using ArticlesService.Application.DI;
using ArticlesService.ArticlesAPI.Extensions.DI;
using ArticlesService.ArticlesAPI.Identity;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.Cache;
using ArticlesService.Infrastructure.DataAccess.DI;
using ArticlesService.Infrastructure.DataAccess.DI.Extensions;
using BuildingBlocks.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ArticlesService...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddScoped<IUserContext, UserContext>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();
    //builder.Services.AddApiErrorHandling();
    builder.Services.AddSwagger();
    builder.AddHealthChecks();
    builder.Services.AddMapper();

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    //.AddOData(options => options 
    //.EnableQueryFeatures(100) // Или .Select().Filter().OrderBy().Expand().Count().Search().SetMaxTop(100) 
    ////.AddRouteComponents(
    ////    routePrefix: "api",//"api/odata", // Префикс для OData-маршрутов
    ////    model: ODataModelConfiguration.GetEdmModel())
    //); 

    builder.Services.AddApiErrorHandling();
    builder.Services.AddSharedLogging();

    // INFRASTRUCTURE
    builder.Services.AddDataAccessServices(builder.Configuration, builder.Environment);
    builder.Services.AddCache(builder.Configuration);

    // Этот код должен следовать после AddDataAccessServices, чтобы дополнить настройки Identity
    /* 
         AddAuthentication, AddScheme, AddIdentityCore вызываются внутри - отдельно прописывать не нужно,
         НО  AddIdentityApiEndpoints настраивает аутентификацию на основе Cookie по умолчанию (для браузеров).
         Метод AddIdentityApiEndpoints регистрирует несколько схем.
         Если нужны JWT, то настраивать отдельно
    */
    builder.Services.AddIdentityApiEndpoints<User>();

    // APPLICATION
    builder.Services.AddApplicationServices();


    var app = builder.Build();


    app.UseApiErrorHandling();

    if (app.Environment.IsStaging() || app.Environment.IsProduction())
    {
        // The default HSTS value is 30 days.
        // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts(); 
    }

    if (app.Environment.IsDevelopment())
    {
        //app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.DocExpansion(DocExpansion.None);
        });
    }

    // автоматическое логирование HTTP-запросов
    app.UseSerilogRequestLogging();

    await app.MigrateAndSeedDatabase();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-dashboard"; 
    });

    app.UseHttpsRedirection();

    // TODO: добавить политики, когда будет разработан frontend  
    //app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapIdentityApi<User>().WithTags("AspNetIdentity"); 
    app.MapControllers();

    Log.Information("ArticlesService host configuration completed successfully.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ArticlesService terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
