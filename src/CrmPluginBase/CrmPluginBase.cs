using System;
using System.Collections.Generic;
using System.Reflection;

using CrmPluginBase.Exceptions;
using CrmPluginBase.Extensions;
using CrmPluginBase.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmPluginBase
{
    public abstract class CrmPluginBase<T> : IPluginMessageOperationExecutor<T>, IPlugin where T : Entity
    {
        private readonly IDictionary<string, Action<IPluginExecutionContext, ParametersWrapper<T>>> eventHandlers = new Dictionary<string, Action<IPluginExecutionContext, ParametersWrapper<T>>>();

        protected CrmPluginBase(string unsecure, string secure = null)
        {
            Unsecure = unsecure;
            Secure = secure;

            FillEventHandlers();
        }

        protected string Unsecure { get; }

        protected string Secure { get; }

        protected IServiceProvider ServiceProvider { get; set; }

        protected IOrganizationService SystemOrgService
        {
            get
            {
                var context = ServiceProvider.GetService<IPluginExecutionContext>();
                GetProxyTypesAssemblyProperty(context.GetType()).SetValue(context, GetProxyAssembly(), new object[0]);
                var serviceFactory = ServiceProvider.GetService<IOrganizationServiceFactory>();
                return serviceFactory.CreateOrganizationService(null);
            }
        }

        protected IOrganizationService UserOrgService
        {
            get
            {
                var serviceFactory = ServiceProvider.GetService<IOrganizationServiceFactory>();
                var context = ServiceProvider.GetService<IPluginExecutionContext>();
                GetProxyTypesAssemblyProperty(context.GetType()).SetValue(context, GetProxyAssembly(), new object[0]);
                return serviceFactory.CreateOrganizationService(context.InitiatingUserId);
            }
        }

        protected ITracingService TracingService => ServiceProvider.GetService<ITracingService>();

        public void Execute(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            ReconfigureServices(ServiceProvider);

            var executionContext = ServiceProvider.GetService<IPluginExecutionContext>();

            // ToDo: use Polly
            var exceptionMessage = string.Empty;
            Exception originException = null;

            try
            {
                OnExecute(executionContext);
            }
            catch (CrmException ex)
            {
                exceptionMessage = $"Error occured:\n{ex.Message}";
                if (!ex.Expected)
                {
                    exceptionMessage += "\nStackTrace:\n" + ex.StackTrace;
                }

                originException = ex;
            }
            catch (Exception ex)
            {
                exceptionMessage = $"Error occured:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}";
                originException = ex;
            }
            finally
            {
                if (originException != null)
                {
                    throw new InvalidPluginExecutionException(exceptionMessage, originException);
                }
            }
        }

        /// <summary>
        /// Override for Update plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the primary entity</param>
        /// <param name="preEntityImage">Entity image for pre operation</param>
        /// <param name="postEntityImage">Entity image for post operation</param>
        public virtual void OnUpdate(IPluginExecutionContext context, T entity, Guid primaryEntityId, T preEntityImage, T postEntityImage)
        {
            OnUpdate(context, entity, primaryEntityId);
        }

        /// <summary>
        /// Override for Create plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="postEntityImage">Entity image for post operation</param>
        public virtual void OnCreate(IPluginExecutionContext context, T entity, Guid primaryEntityId, T postEntityImage)
        {
            OnCreate(context, entity, primaryEntityId);
        }

        /// <summary>
        /// Override for Delete plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="preEntityImage">Entity image for pre operation</param>
        public virtual void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId, T preEntityImage)
        {
            OnDelete(context, entityName, primaryEntityId);
        }

        /// <summary>
        /// Override for Update plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the primary entity</param>
        public virtual void OnUpdate(IPluginExecutionContext context, T entity, Guid primaryEntityId)
        {
        }

        /// <summary>
        /// Override for Create plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        public virtual void OnCreate(IPluginExecutionContext context, T entity, Guid primaryEntityId)
        {
        }

        /// <summary>
        /// Override for Delete plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        public virtual void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId)
        {
        }

        /// <summary>
        /// Override for SetState plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="newState">New State of the entity</param>
        /// <param name="newStatus">New Status of the entity</param>
        public virtual void OnSetState(IPluginExecutionContext context, string entityName, Guid primaryEntityId, OptionSetValue newState, OptionSetValue newStatus)
        {
        }

        /// <summary>
        /// Override for Assign plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityLogicalName">Entity logical name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="assigneeEntityLogicalName">Assignee entity logical name</param>
        /// <param name="assigneeId">Assignee id</param>
        public virtual void OnAssign(IPluginExecutionContext context, string entityLogicalName, Guid primaryEntityId, string assigneeEntityLogicalName, Guid assigneeId)
        {
        }

        /// <summary>
        /// Override for Merge plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="targetId">Entity reference</param>
        /// <param name="subordinateId">Entity to merge</param>
        /// <param name="updateContent">Attributes to update from base entity</param>
        /// <param name="performParentingChecks">Perfor parenting checks</param>
        public virtual void OnMerge(IPluginExecutionContext context, EntityReference targetId, Guid subordinateId, Entity updateContent, bool performParentingChecks)
        {
        }

        /// <summary>
        /// Override for Close plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="closeEntity">Incident resolution entity</param>
        /// <param name="status">Status code</param>
        public virtual void OnClose(IPluginExecutionContext context, Entity closeEntity, OptionSetValue status)
        {
        }

        /// <summary>
        /// Override for Cancel plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="orderClose">Order close entity</param>
        /// <param name="status">Status code</param>
        public virtual void OnCancel(IPluginExecutionContext context, Entity orderClose, OptionSetValue status)
        {
        }

        /// <summary>
        /// Override for AddMember plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="listId">Id of the list entity</param>
        /// <param name="primaryEntityId">Id of the adding to list entity</param>
        /// <param name="listMemberId">Id of the created listmember entity</param>
        public virtual void OnAddMember(IPluginExecutionContext context, Guid listId, Guid primaryEntityId, Guid listMemberId)
        {
        }

        /// <summary>
        /// Override for RemoveMember plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="listId">Id of the list entity</param>
        /// <param name="primaryEntityId">Id of the removing from list entity</param>
        public virtual void OnRemoveMember(IPluginExecutionContext context, Guid listId, Guid primaryEntityId)
        {
        }

        /// <summary>
        /// Override for RetrieveMultiple plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="query">Query of RetrieveMultiple</param>
        /// <param name="entityCollection">Collection of entity</param>
        public virtual void OnRetrieveMultiple(IPluginExecutionContext context, QueryBase query, EntityCollection entityCollection)
        {
        }

        /// <summary>
        /// Override for ExportToExcel (RetrieveMultiple) plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="query">Query of RetrieveMultiple</param>
        /// <param name="entityCollection">Collection of entity</param>
        public virtual void OnExportToExcel(IPluginExecutionContext context, QueryBase query, EntityCollection entityCollection)
        {
        }

        /// <summary>
        /// Override for GrantAccess plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="principalEntityName">Principal entity name</param>
        /// <param name="principalId">Principal Id</param>
        /// <param name="mask">Principal access mask</param>
        public virtual void OnGrantAccess(IPluginExecutionContext context, string entityName, Guid primaryEntityId, string principalEntityName, Guid principalId, AccessRights mask)
        {
        }

        /// <summary>
        /// Override for GrantAccess plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="principalEntityName">Principal entity name</param>
        /// <param name="principalId">Principal Id</param>
        /// <param name="mask">Principal access mask</param>
        public virtual void OnModifyAccess(IPluginExecutionContext context, string entityName, Guid primaryEntityId, string principalEntityName, Guid principalId, AccessRights mask)
        {
        }

        /// <summary>
        /// Override for GrantAccess plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="revokeeEntityName">Revokee entity name</param>
        /// <param name="revokeeId">Revokee Id</param>
        public virtual void OnRevokeAccess(IPluginExecutionContext context, string entityName, Guid primaryEntityId, string revokeeEntityName, Guid revokeeId)
        {
        }

        /// <summary>
        /// Override for RetrieveFilteredForms message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Current entity name</param>
        /// <param name="userId">System user Id</param>
        /// <param name="formType">Form type</param>
        /// <param name="systemForms">System forms for current entity</param>
        public virtual void OnRetrieveFilteredForms(IPluginExecutionContext context, string entityName, Guid userId, OptionSetValue formType, EntityReferenceCollection systemForms)
        {
        }

        /// <summary>
        /// Override for CustomAction execute message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="customAction">Custom action request</param>
        /// <param name="targetRef">Target entity reference - null if custom action not entity related</param>
        public virtual void OnCustomOperation(IPluginExecutionContext context, OrganizationRequest customAction, EntityReference targetRef)
        {
        }

        /// <summary>
        /// Override for CustomAction execute message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="customAction">Custom action request</param>
        public virtual void OnCustomOperation(IPluginExecutionContext context, OrganizationRequest customAction)
        {
        }

        protected virtual Assembly GetProxyAssembly()
        {
            return Assembly.GetAssembly(GetType());
        }

        protected virtual PropertyInfo GetProxyTypesAssemblyProperty(Type type)
        {
            return type.GetProperty("ProxyTypesAssembly");
        }

        protected virtual void ReconfigureServices(IServiceProvider serviceProvider)
        {
        }

        private void OnExecute(IPluginExecutionContext executionContext)
        {
            var parameters = new ParametersWrapper<T>(executionContext);
            var parentContext = executionContext.ParentContext;
            var prop = executionContext.GetType().GetProperty("MessageCategory");
            var messageCategory = string.Empty;
            if (prop != null)
            {
                messageCategory = (string)prop.GetValue(executionContext);
            }

            string messageName;
            if (messageCategory == MessageCategory.CustomOperation)
            {
                messageName = MessageCategory.CustomOperation;
            }
            else if (parentContext != null &&
                     (parentContext.MessageName == PluginVirtualMessageName.ExportToExcel ||
                      parentContext.MessageName == PluginVirtualMessageName.ExportDynamicToExcel))
            {
                messageName = parentContext.MessageName;
            }
            else
            {
                messageName = executionContext.MessageName;
            }

            Action<IPluginExecutionContext, ParametersWrapper<T>> pluginAction;
            if (eventHandlers.TryGetValue(messageName, out pluginAction))
            {
                pluginAction(executionContext, parameters);
            }
        }

        private void FillEventHandlers()
        {
            eventHandlers.Clear();

            eventHandlers.Add(PluginVirtualMessageName.ExportToExcel, (ctx, p) => OnExportToExcel(ctx, p.Query, p.BusinessEntityCollection));
            eventHandlers.Add(PluginVirtualMessageName.ExportDynamicToExcel, (ctx, p) => OnExportToExcel(ctx, p.Query, p.BusinessEntityCollection));
            eventHandlers.Add(PluginMessageName.Create, (ctx, p) => OnCreate(ctx, p.TypedEntity(), p.Id, p.PostEntityImage));
            eventHandlers.Add(
                PluginMessageName.Update,
                (ctx, p) =>
                {
                    var typedEntity = p.TypedEntity();
                    OnUpdate(ctx, typedEntity, typedEntity.Id, p.PreEntityImage, p.PostEntityImage);
                });
            eventHandlers.Add(PluginMessageName.Delete, (ctx, p) => OnDelete(ctx, p.TargetRef.LogicalName, p.TargetRef.Id, p.PreEntityImage));
            eventHandlers.Add(PluginMessageName.SetState, (ctx, p) => OnSetState(ctx, p.EntityMoniker.LogicalName, p.EntityMoniker.Id, p.State, p.Status));
            eventHandlers.Add(PluginMessageName.SetStateDynamicEntity, (ctx, p) => OnSetState(ctx, p.EntityMoniker.LogicalName, p.EntityMoniker.Id, p.State, p.Status));
            eventHandlers.Add(PluginMessageName.Assign, (ctx, p) => OnAssign(ctx, p.TargetRef.LogicalName, p.TargetRef.Id, p.Assignee.LogicalName, p.Assignee.Id));
            eventHandlers.Add(PluginMessageName.Merge, (ctx, p) => OnMerge(ctx, p.TargetRef, p.SubordinateId, p.UpdateContent, p.PerformParentingChecks));
            eventHandlers.Add(PluginMessageName.AddMember, (ctx, p) => OnAddMember(ctx, p.ListId, p.EntityId, p.Id));
            eventHandlers.Add(PluginMessageName.RemoveMember, (ctx, p) => OnRemoveMember(ctx, p.ListId, p.EntityId));
            eventHandlers.Add(PluginMessageName.RetrieveMultiple, (ctx, p) => OnRetrieveMultiple(ctx, p.Query, p.BusinessEntityCollection));
            eventHandlers.Add(PluginMessageName.Close, (ctx, p) => OnClose(ctx, p.ClosedEntity, p.Status));
            eventHandlers.Add(PluginMessageName.Cancel, (ctx, p) => OnCancel(ctx, p.OrderClose, p.Status));
            eventHandlers.Add(
                PluginMessageName.GrantAccess,
                (ctx, p) =>
                    OnGrantAccess(
                        ctx,
                        p.TargetRef.LogicalName,
                        p.TargetRef.Id,
                        p.PrincipalAccess.Principal.LogicalName,
                        p.PrincipalAccess.Principal.Id,
                        p.PrincipalAccess.AccessMask));
            eventHandlers.Add(
                PluginMessageName.ModifyAccess,
                (ctx, p) =>
                    OnModifyAccess(
                        ctx,
                        p.TargetRef.LogicalName,
                        p.TargetRef.Id,
                        p.PrincipalAccess.Principal.LogicalName,
                        p.PrincipalAccess.Principal.Id,
                        p.PrincipalAccess.AccessMask));
            eventHandlers.Add(PluginMessageName.RevokeAccess, (ctx, p) => OnRevokeAccess(ctx, p.TargetRef.LogicalName, p.TargetRef.Id, p.Revokee.LogicalName, p.Revokee.Id));
            eventHandlers.Add(PluginMessageName.RetrieveFilteredForms, (ctx, p) => OnRetrieveFilteredForms(ctx, p.EntityLogicalName, p.SystemUserId, p.FormType, p.SystemForms));
            eventHandlers.Add(
                MessageCategory.CustomOperation,
                (ctx, p) =>
                {
                    var request = new OrganizationRequest(ctx.MessageName) { Parameters = ctx.InputParameters };
                    if (p.TargetRef == null)
                    {
                        OnCustomOperation(ctx, request);
                    }
                    else
                    {
                        OnCustomOperation(ctx, request, p.TargetRef);
                    }
                });
        }
    }
}
