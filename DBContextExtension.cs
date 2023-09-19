using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;

namespace DBSerializer
{
    public static class DbContextExtensions
    {
        public static string GetTableName<T>(this DbContext context) where T : struct
        {
            var type = typeof(T);
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;

            // Get the entity type from the ObjectContext
            var entityType = objectContext.MetadataWorkspace
                .GetItems<EntityType>(DataSpace.CSpace)
                .FirstOrDefault(et => et.Name == type.Name);

            if (entityType != null)
            {
                // Get the entity set name for the entity type
                var entitySet = objectContext.MetadataWorkspace
                    .GetItems<EntityContainer>(DataSpace.CSpace)
                    .Single()
                    .EntitySets
                    .FirstOrDefault(es => es.ElementType.Name == type.Name);

                if (entitySet != null)
                {
                    // Get the table name from the entity set's name
                    return entitySet.Table ?? type.Name;
                }
            }

            return null;
        }
    }
}
