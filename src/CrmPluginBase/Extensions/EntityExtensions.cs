using System.Linq;

using Microsoft.Xrm.Sdk;

namespace CrmPluginBase.Extensions
{
    public static class EntityExtensions
    {
        public static T Merge<T>(this T primary, T secondary) where T : Entity, new()
        {
            var merged = primary.Clone();
            secondary.Attributes
                     .Where(a => !merged.Contains(a.Key))
                     .ToList()
                     .ForEach(a => { merged[a.Key] = a.Value; });
            merged.Id = primary.Id;

            return merged;
        }

        public static T Clone<T>(this T entity) where T : Entity, new()
        {
            var clone = new T();
            var primaryKeyName = entity.LogicalName.ToLowerInvariant() + "id";
            clone.Attributes.AddRange(entity.Attributes.Where(a => a.Key != primaryKeyName));
            clone.LogicalName = entity.LogicalName;

            return clone;
        }

        public static T Without<T>(this T entity, params string[] excludedAttributes) where T : Entity, new()
        {
            foreach (var attr in excludedAttributes)
            {
                entity.Attributes.Remove(attr);
            }

            return entity;
        }
    }
}
