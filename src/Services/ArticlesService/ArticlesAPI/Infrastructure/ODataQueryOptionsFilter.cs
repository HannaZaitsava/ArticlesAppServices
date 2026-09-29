using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ArticlesService.ArticlesAPI.Infrastructure
{
    public class ODataQueryOptionsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Ищем параметры типа ODataQueryOptions
            var isOData = context.ApiDescription.ParameterDescriptions
                .Any(p => p.ParameterDescriptor?.ParameterType != null && // TODO: возможно, удалить
                          p.ParameterDescriptor.ParameterType.IsGenericType &&
                          p.ParameterDescriptor.ParameterType.GetGenericTypeDefinition() == typeof(Microsoft.AspNetCore.OData.Query.ODataQueryOptions<>));

            if (isOData && operation.Parameters is not null)
            {
                // Удаляем системный объект "options"
                operation.Parameters.Clear();

                // Добавляем чистые поля OData
                AddQueryParam(operation, "$filter", "Filter the response with OData filter queries. (e.g. 'Name eq 'Sony'')");
                AddQueryParam(operation, "$orderby", "Define the order by one or more fields (e.g. 'LastModified desc')");
                AddQueryParam(operation, "$top", "Number of objects to return. (e.g. 25)", "integer");
                AddQueryParam(operation, "$skip", "Number of objects to skip in the current order (e.g. 50)", "integer");
                AddQueryParam(operation, "$search", "Full-text search (e.g. 'apple AND iphone')");
                //AddQueryParam(operation, "$select", "Returns only the selected properties. (ex. FirstName, LastName, City)");
                //AddQueryParam(operation, "$expand", "Include only the selected objects. (ex.Childrens, Locations)");

            }
        }

        private void AddQueryParam(OpenApiOperation op, string name, string desc, string type = "string")
        {
            var schemaType = type.ToLower() switch
            {
                "integer" => JsonSchemaType.Integer,
                "number" => JsonSchemaType.Number,
                "boolean" => JsonSchemaType.Boolean,
                _ => JsonSchemaType.String
            };

            op.Parameters!.Add(new OpenApiParameter
            {
                Name = name,
                In = ParameterLocation.Query,
                Description = desc,
                Required = false,
                Schema = new OpenApiSchema { Type = schemaType }
            });
        }
    }
}
