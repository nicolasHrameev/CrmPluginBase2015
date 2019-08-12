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
    /// <summary>
    /// Provides ability for easy CRM plugins creation with focus on what you need to do and out of pain InputParameters/OutputParameters parsing
    /// </summary>
    /// <typeparam name="TEntity">Early-bound entity type or even just Entity (late-bound)</typeparam>
    public abstract class CrmPluginBase<TEntity> : IPluginMessageOperationExecutor<TEntity>, IPlugin where TEntity : Entity
    {
        private static readonly IDictionary<string, Action<CrmPluginBase<TEntity>, IPluginExecutionContext, ParametersWrapper<TEntity>>> EventHandlers =
            new Dictionary<string, Action<CrmPluginBase<TEntity>, IPluginExecutionContext, ParametersWrapper<TEntity>>>();

        protected CrmPluginBase(string unsecure, string secure = null)
        {
            Unsecure = unsecure;
            Secure = secure;

            InitEventHandlers();
        }

        protected string Unsecure { get; }

        protected string Secure { get; }

        protected virtual string PreEntityImageName => "preimage";

        protected virtual string PostEntityImageName => "postimage";

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

        /// <summary>
        /// You don't need to call this method directly - rather override one (or even more) OnXXX methods
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            ReconfigureServices(ServiceProvider);

            var executionContext = ServiceProvider.GetService<IPluginExecutionContext>();

            try
            {
                OnExecute(executionContext);
            }
            catch (Exception ex)
            {
                OnException(ex);
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
        public virtual void OnUpdate(IPluginExecutionContext context, TEntity entity, Guid primaryEntityId, TEntity preEntityImage, TEntity postEntityImage)
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
        public virtual void OnCreate(IPluginExecutionContext context, TEntity entity, Guid primaryEntityId, TEntity postEntityImage)
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
        public virtual void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId, TEntity preEntityImage)
        {
            OnDelete(context, entityName, primaryEntityId);
        }

        /// <summary>
        /// Override for Update plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the primary entity</param>
        public virtual void OnUpdate(IPluginExecutionContext context, TEntity entity, Guid primaryEntityId)
        {
        }

        /// <summary>
        /// Override for Create plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        public virtual void OnCreate(IPluginExecutionContext context, TEntity entity, Guid primaryEntityId)
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
        /// <param name="targetRef">Target entity reference - <langword name="null"/> if custom action not entity related</param>
        public virtual void OnCustomOperation(IPluginExecutionContext context, OrganizationRequest customAction, EntityReference targetRef = null)
        {
        }

        protected virtual Assembly GetProxyAssembly() => Assembly.GetAssembly(GetType());

        protected virtual PropertyInfo GetProxyTypesAssemblyProperty(Type type) => type.GetProperty("ProxyTypesAssembly");

        protected virtual void ReconfigureServices(IServiceProvider serviceProvider)
        {
        }

        private void OnExecute(IPluginExecutionContext executionContext)
        {
            var parameters = new ParametersWrapper<TEntity>(executionContext, PreEntityImageName, PostEntityImageName);
            var parentContext = executionContext.ParentContext;
            var prop = executionContext.GetType().GetProperty("MessageCategory");
            var messageCategory = prop == null ? string.Empty : (string)prop.GetValue(executionContext);

            string messageName;
            switch (messageCategory)
            {
                case MessageCategory.CustomOperation:
                    messageName = MessageCategory.CustomOperation;
                    break;
                default:
                {
                    messageName =
                        parentContext != null &&
                        (parentContext.MessageName == PluginVirtualMessageName.ExportToExcel ||
                         parentContext.MessageName == PluginVirtualMessageName.ExportDynamicToExcel)
                            ? parentContext.MessageName
                            : executionContext.MessageName;

                    break;
                }
            }

            if (EventHandlers.TryGetValue(messageName, out var pluginAction))
            {
                pluginAction(this, executionContext, parameters);
            }
        }

        protected virtual void OnException(Exception ex)
        {
            // ToDo: use Polly
            if (ex is CrmException crmEx)
            {
                var exceptionMessage = $"Error occured:\n{ex.Message}";
                if (!crmEx.Expected)
                {
                    exceptionMessage += "\nStackTrace:\n" + ex.StackTrace;
                }

                throw new InvalidPluginExecutionException(exceptionMessage, crmEx);
            }

            throw new InvalidPluginExecutionException($"Error occured:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}", ex);
        }

        private static void InitEventHandlers()
        {
            EventHandlers.Clear();

            EventHandlers.Add(PluginVirtualMessageName.ExportToExcel, (plugin, ctx, p) => plugin.OnExportToExcel(ctx, p.Query, p.BusinessEntityCollection));
            EventHandlers.Add(PluginVirtualMessageName.ExportDynamicToExcel, (plugin, ctx, p) => plugin.OnExportToExcel(ctx, p.Query, p.BusinessEntityCollection));
            EventHandlers.Add(PluginMessageName.Create, (plugin, ctx, p) => plugin.OnCreate(ctx, p.Target, p.Id, p.PostEntityImage));
            EventHandlers.Add(
                PluginMessageName.Update,
                (plugin, ctx, p) =>
                {
                    var target = p.Target;
                    plugin.OnUpdate(ctx, target, target.Id, p.PreEntityImage, p.PostEntityImage);
                });
            EventHandlers.Add(PluginMessageName.Delete, (plugin, ctx, p) => plugin.OnDelete(ctx, p.TargetRef.LogicalName, p.TargetRef.Id, p.PreEntityImage));
            EventHandlers.Add(PluginMessageName.SetState, (plugin, ctx, p) => plugin.OnSetState(ctx, p.EntityMoniker.LogicalName, p.EntityMoniker.Id, p.State, p.Status));
            EventHandlers.Add(PluginMessageName.SetStateDynamicEntity, (plugin, ctx, p) => plugin.OnSetState(ctx, p.EntityMoniker.LogicalName, p.EntityMoniker.Id, p.State, p.Status));
            EventHandlers.Add(PluginMessageName.Assign, (plugin, ctx, p) => plugin.OnAssign(ctx, p.TargetRef.LogicalName, p.TargetRef.Id, p.Assignee.LogicalName, p.Assignee.Id));
            EventHandlers.Add(PluginMessageName.Merge, (plugin, ctx, p) => plugin.OnMerge(ctx, p.TargetRef, p.SubordinateId, p.UpdateContent, p.PerformParentingChecks));
            EventHandlers.Add(PluginMessageName.AddMember, (plugin, ctx, p) => plugin.OnAddMember(ctx, p.ListId, p.EntityId, p.Id));
            EventHandlers.Add(PluginMessageName.RemoveMember, (plugin, ctx, p) => plugin.OnRemoveMember(ctx, p.ListId, p.EntityId));
            EventHandlers.Add(PluginMessageName.RetrieveMultiple, (plugin, ctx, p) => plugin.OnRetrieveMultiple(ctx, p.Query, p.BusinessEntityCollection));
            EventHandlers.Add(PluginMessageName.Close, (plugin, ctx, p) => plugin.OnClose(ctx, p.ClosedEntity, p.Status));
            EventHandlers.Add(PluginMessageName.Cancel, (plugin, ctx, p) => plugin.OnCancel(ctx, p.OrderClose, p.Status));
            EventHandlers.Add(
                PluginMessageName.GrantAccess,
                (plugin, ctx, p) =>
                    plugin.OnGrantAccess(
                        ctx,
                        p.TargetRef.LogicalName,
                        p.TargetRef.Id,
                        p.PrincipalAccess.Principal.LogicalName,
                        p.PrincipalAccess.Principal.Id,
                        p.PrincipalAccess.AccessMask));
            EventHandlers.Add(
                PluginMessageName.ModifyAccess,
                (plugin, ctx, p) =>
                    plugin.OnModifyAccess(
                        ctx,
                        p.TargetRef.LogicalName,
                        p.TargetRef.Id,
                        p.PrincipalAccess.Principal.LogicalName,
                        p.PrincipalAccess.Principal.Id,
                        p.PrincipalAccess.AccessMask));
            EventHandlers.Add(PluginMessageName.RevokeAccess, (plugin, ctx, p) => plugin.OnRevokeAccess(ctx, p.TargetRef.LogicalName, p.TargetRef.Id, p.Revokee.LogicalName, p.Revokee.Id));
            EventHandlers.Add(PluginMessageName.RetrieveFilteredForms, (plugin, ctx, p) => plugin.OnRetrieveFilteredForms(ctx, p.EntityLogicalName, p.SystemUserId, p.FormType, p.SystemForms));
            EventHandlers.Add(
                MessageCategory.CustomOperation,
                (plugin, ctx, p) =>
                {
                    var request = new OrganizationRequest(ctx.MessageName) { Parameters = ctx.InputParameters };
                    plugin.OnCustomOperation(ctx, request, p.TargetRef);
                });
        }
    }
}
