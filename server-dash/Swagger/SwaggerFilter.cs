using Common.Utility;
using Dash.Model;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace server_dash.Swagger
{
    public class DocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            //swaggerDoc.Components.Schemas.Remove("AuthReq");
        }
    }

    public class AutoRestSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (type.IsEnum)
            {
                schema.Extensions.Add(
                    "x-ms-enum",
                    new OpenApiObject
                    {
                        ["name"] = new OpenApiString(type.Name),
                        ["modelAsString"] = new OpenApiBoolean(true)
                    }
                );
            };
        }
    }

    public class SchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || context.Type == null)
                return;

            var excludedProperties = context.Type.GetProperties()
                                         .Where(t =>
                                                t.GetCustomAttribute<SwaggerExcludeAttribute>()
                                                != null);

            foreach (var excludedProperty in excludedProperties)
            {
                var key = excludedProperty.Name.ToCamelCase();
                if (schema.Properties.ContainsKey(key))
                    schema.Properties.Remove(key);
            }
        }
    }

}