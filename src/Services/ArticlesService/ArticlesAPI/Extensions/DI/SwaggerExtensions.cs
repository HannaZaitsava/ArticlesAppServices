using System.Reflection;
using Microsoft.OpenApi;

namespace ArticlesService.ArticlesAPI.Extensions.DI
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
               
                if (File.Exists(xmlCommentsFullPath))
                {
                    options.IncludeXmlComments(xmlCommentsFullPath);
                }

                // TODO Вернуть для отображения примеров данных для запросов и ответов.
                // (В публичных API — ОБЯЗАТЕЛЬНО. Во внутренних проектах (Enterprise) — ОЧЕНЬ ЖЕЛАТЕЛЬНО)
                // option.ExampleFilters(); 
                 
                // схема авторизации
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType. ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                    Description = "Please enter a valid token (e.g.: Bearer {token})",                     
                });

                // Делает авторизацию глобальной для всех эндпоинтов
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });


                //options.OperationFilter<ODataQueryOptionsFilter>();
            });
            

            // TODO Вернуть для отображения примеров данных для запросов и ответов.
            // (В публичных API — ОБЯЗАТЕЛЬНО. Во внутренних проектах (Enterprise) — ОЧЕНЬ ЖЕЛАТЕЛЬНО)
            //services.AddSwaggerExamplesFromAssemblyOf<UserResponseModelExample>();

            return services;
        }
    }
}
