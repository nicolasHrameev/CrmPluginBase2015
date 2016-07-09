using System;

using CrmPluginBase;
using CrmPluginBase.Exceptions;
using Microsoft.Xrm.Sdk;
using ProxyClasses;

namespace PluginTests
{
    // ReSharper disable once RedundantExtendsListEntry
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// A simple example of CrmPluginBase usage with proxy class entity rare_search.
    /// Of course you can use any your own proxy class or even just Entity as generic type parameter (<see cref="TraceAllUpdateOperationsPlugin"/>)
    /// </summary>
    public class DenyActiveRecordDeletionPlugin : CrmPluginBase<rare_search>, IPlugin
    {
        public DenyActiveRecordDeletionPlugin(string unsecure, string secure = null)
            : base(unsecure, secure)
        {
        }

        /// <summary>
        /// Deny deletion of active rare_search entities
        /// </summary>
        /// <exception cref="CrmException">Deletion of active rare_search records is forbidden!</exception>
        public override void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId, rare_search preEntityImage)
        {
            if (preEntityImage.statuscode == rare_searchStatus.Активный_Активный)
            {
                throw new CrmException("Deletion of active rare_search records is forbidden!", expected: true);
            }
        }
    }

    // ReSharper disable once RedundantExtendsListEntry
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// A simple example of CrmPluginBase usage with Entity as generic type parameter
    /// </summary>
    public class TraceAllUpdateOperationsPlugin : CrmPluginBase<Entity>, IPlugin
    {
        public TraceAllUpdateOperationsPlugin(string unsecure, string secure = null)
            : base(unsecure, secure)
        {
        }

        public override void OnUpdate(IPluginExecutionContext context, Entity entity, Guid primaryEntityId)
        {
            var message = string.Format("Entity '{0}', Id = '{1}' updated", entity.LogicalName, primaryEntityId);
            var traceEntity = new Entity("rare_trace");
            traceEntity["rare_tracemessage"] = message;

            SystemOrgService.Create(traceEntity);
            TracingService.Trace(message);
        }
    }
}
