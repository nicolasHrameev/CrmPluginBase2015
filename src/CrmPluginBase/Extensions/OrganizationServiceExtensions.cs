using System.Collections.Generic;
using System.Linq;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmPluginBase.Extensions
{
    public static class OrganizationServiceExtensions
    {
        public static IEnumerable<Entity> Fetch(this IOrganizationService orgService, string fetchXml) => orgService.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;

        public static IEnumerable<T> Fetch<T>(this IOrganizationService orgService, string fetchXml) where T : Entity => orgService.Fetch(fetchXml).Cast<T>();
    }
}
