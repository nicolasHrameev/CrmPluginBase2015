using System.Linq;

using Microsoft.Xrm.Sdk;

namespace CrmPluginBase.Extensions
{
    public static class EntityExtensions
    {
        public static TEntity Merge<TEntity>(this TEntity primary, TEntity secondary) where TEntity : Entity, new()
        {
            var merged = primary.Clone();
            secondary.Attributes
                     .Where(a => !merged.Contains(a.Key))
                     .ToList()
                     .ForEach(a => { merged[a.Key] = a.Value; });
            merged.Id = primary.Id;

            return merged;
        }

        public static TEntity Clone<TEntity>(this TEntity entity) where TEntity : Entity, new()
        {
            var clone = new TEntity();
            var primaryKeyName = entity.LogicalName.ToLowerInvariant() + "id";
            clone.Attributes.AddRange(entity.Attributes.Where(a => a.Key != primaryKeyName));
            clone.LogicalName = entity.LogicalName;

            return clone;
        }

        public static TEntity Without<TEntity>(this TEntity entity, params string[] excludedAttributes) where TEntity : Entity, new()
        {
            foreach (var attr in excludedAttributes)
            {
                entity.Attributes.Remove(attr);
            }

            return entity;
        }

        public static TEntity ToEntity<TEntity>(this object entity) where TEntity : Entity
        {
            switch (entity)
            {
                case null:
                    return null;
                case TEntity typedEntity:
                    return typedEntity;
                default:
                {
                    return (entity as Entity)?.ToEntity<TEntity>();
                }
            }
        }
    }
}
