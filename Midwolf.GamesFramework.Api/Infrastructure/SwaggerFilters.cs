using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Midwolf.GamesFramework.Api.Infrastructure
{
    public class PolymorphismSchemaFilter<T> : ISchemaFilter
    {
        private readonly Lazy<HashSet<Type>> derivedTypes = new Lazy<HashSet<Type>>(Init);

        private static HashSet<Type> Init()
        {
            var abstractType = typeof(T);
            var dTypes = abstractType.Assembly
                                     .GetTypes()
                                     .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

            var result = new HashSet<Type>();

            foreach (var item in dTypes)
                result.Add(item);

            return result;
        }

        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (!derivedTypes.Value.Contains(context.SystemType))
            {
                return;
            }

            // Prepare a dictionary of inherited properties
            var inheritedProperties = context.SystemType.GetProperties()
                .Where(x => x.DeclaringType != context.SystemType)
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            var clonedSchema = new Schema
            {
                // Exclude inherited properties. If not excluded, 
                // they would have appeared twice in nswag-generated typescript definition
                Properties =
                    model.Properties.Where(x => !inheritedProperties.ContainsKey(x.Key))
                        .ToDictionary(x => x.Key, x => x.Value),
                Type = model.Type,
                Required = model.Required
            };

            // Use the BaseType name for parentSchema instead of typeof(T), 
            // because we could have more abstract classes in the hierarchy
            var parentSchema = new Schema
            {
                Ref = "#/definitions/" + (context.SystemType.BaseType?.Name ?? typeof(T).Name)
            };
            model.AllOf = new List<Schema> { parentSchema, clonedSchema };

            // reset properties for they are included in allOf, should be null but code does not handle it
            model.Properties = new Dictionary<string, Schema>();
        }
    }

    public class PolymorphismDocumentFilter<T> : IDocumentFilter
    {
        private static void RegisterSubClasses(ISchemaRegistry schemaRegistry, Type abstractType)
        {
            const string discriminatorName = "discriminator";

            var parentSchema = schemaRegistry.Definitions[abstractType.Name];

            //set up a discriminator property (it must be required)
            parentSchema.Discriminator = discriminatorName;
            parentSchema.Required = new List<string> { discriminatorName };

            if (!parentSchema.Properties.ContainsKey(discriminatorName))
                parentSchema.Properties.Add(discriminatorName, new Schema { Type = "string" });

            //register all subclasses
            var derivedTypes = abstractType.Assembly
                                           .GetTypes()
                                           .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

            foreach (var item in derivedTypes)
                schemaRegistry.GetOrRegister(item);
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            RegisterSubClasses(context.SchemaRegistry, typeof(T));
        }
    }
}
