# CrmPluginBase

Basic functionality for MS Dynamics CRM 2015 plugins - see [example](https://github.com/abelevtsov/CrmPluginBase2015/edit/master/Examples.cs).

Provides ability for easy CRM plugins creation with focus on what you need to do and 
out of pain InputParameters/OutputParameters parsing.

Install package from [nuget](https://www.nuget.org/packages/CrmPluginBase2015/) and enjoy!

### IMPORTANT NOTE: 
**you must name your PreEntityImage as *`preimage`* and PostEntityImage as *`postimage`* exactly**

see [ParametersWrapper .ctor](https://github.com/abelevtsov/CrmPluginBase2015/blob/master/src/CrmPluginBase/ParametersWrapper.cs).

```csharp
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
    /// A simple example of CrmPluginBase usage with proxy class entity new_search.
    /// Certainly you can use any of your own proxy classes or even just Entity as generic type parameter (<see cref="TraceAllUpdateOperationsPlugin"/>)
    /// </summary>
    public class DenyActiveRecordDeletionPlugin : CrmPlugin<new_search>, IPlugin
    {
        /// <summary>
        /// Deny deletion of active new_search entities
        /// </summary>
        /// <exception cref="CrmException">Deletion of active new_search records is forbidden!</exception>
        public override void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId, new_search preEntityImage)
        {
            if (preEntityImage.statuscode == new_searchStatus.Active_Active)
            {
                throw new CrmException($"Deletion of active {new_search.EntityLogicalName} records is forbidden!", expected: true);
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
            var message = $"Entity '{entity.LogicalName}', Id = '{primaryEntityId}' updated";
            var traceEntity = new Entity("new_trace");
            traceEntity["new_tracemessage"] = message;

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

```
