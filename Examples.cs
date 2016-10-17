using System;
using System.Runtime.CompilerServices;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using CrmPluginBase;
using CrmPluginBase.Exceptions;
using ProxyClasses;

namespace PluginTests
{
    // ReSharper disable once RedundantExtendsListEntry
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    /// A simple example of CrmPluginBase usage with proxy class entity rare_search.
    /// Of course you can use any your own proxy class or even just Entity as generic type parameter (<see cref="TraceAllUpdateOperationsPlugin"/>)
    /// </summary>
    public class DenyActiveRecordDeletionPlugin : CrmPlugin<rare_search>, IPlugin
    {
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
    public class TraceAllUpdateOperationsPlugin : CrmPlugin<Entity>, IPlugin
    {
        public override void OnUpdate(IPluginExecutionContext context, Entity entity, Guid primaryEntityId)
        {
            var message = string.Format("Entity '{0}', Id = '{1}' updated", entity.LogicalName, primaryEntityId);
            var traceEntity = new Entity("rare_trace");
            traceEntity["rare_tracemessage"] = message;

            SystemOrgService.Create(traceEntity);
            TracingService.Trace(message);
        }
    }
    
    public class RestrictAccountsExportToExcelPlugin : CrmPlugin<Account>, IPlugin
    {
        public override void OnExportToExcel(IPluginExecutionContext context, QueryBase query, EntityCollection entityCollection)
        {
            if (!UserAvailableForExportAccountsToExcel(context.UserId))
            {
                throw new CrmException("You can't export accounts to Excel", expected: true);
            }

            var queryExpression = ToQueryExpression(query);
            if (queryExpression == null)
            {
                return;
            }

            // note: Add your own conditions here
            queryExpression.Criteria.AddCondition("donotpostalmail", ConditionOperator.Equal, false);

            // ReSharper disable once RedundantAssignment
            query = queryExpression;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private QueryExpression ToQueryExpression(QueryBase query)
        {
            var fetchExpression = query as FetchExpression;
            if (fetchExpression == null)
            {
                return query as QueryExpression;
            }

            var request =
                new FetchXmlToQueryExpressionRequest
                    {
                        FetchXml = fetchExpression.Query
                    };
            return ((FetchXmlToQueryExpressionResponse)SystemOrgService.Execute(request)).Query;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool UserAvailableForExportAccountsToExcel(Guid userId)
        {
            // note: Paste your custom check logic here
            return false;
        }
    }
}
