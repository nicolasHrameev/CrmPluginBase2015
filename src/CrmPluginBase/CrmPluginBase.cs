using System;
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
        private IOrganizationService systemService;

        protected CrmPluginBase(string unsecure, string secure = null)
        {
            Unsecure = unsecure;
            Secure = secure;
        }

        protected string Unsecure { get; private set; }

        protected string Secure { get; private set; }

        protected IServiceProvider ServiceProvider { get; set; }

        protected IOrganizationService SystemOrgService
        {
            get
            {
                if (systemService == null)
                {
                    var context = ServiceProvider.GetService<IPluginExecutionContext>();
                    context.EnableProxyTypes(GetProxyAssembly());
                    var serviceFactory = ServiceProvider.GetService<IOrganizationServiceFactory>();
                    systemService = serviceFactory.CreateOrganizationService(null);
                }

                return systemService;
            }
        }

        protected IOrganizationService UserOrgService
        {
            get
            {
                var serviceFactory = ServiceProvider.GetService<IOrganizationServiceFactory>();
                var context = ServiceProvider.GetService<IPluginExecutionContext>();
                context.EnableProxyTypes(GetProxyAssembly());
                return serviceFactory.CreateOrganizationService(context.InitiatingUserId);
            }
        }

        protected ITracingService TracingService
        {
            get
            {
                return ServiceProvider.GetService<ITracingService>();
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
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
                exceptionMessage = string.Format("Error occured:\n{0}", ex.Message);
                if (!ex.Expected)
                {
                    exceptionMessage += "\nStackTrace:\n" + ex.StackTrace;
                }

                originException = ex;
            }
            catch (Exception ex)
            {
                exceptionMessage = string.Format("Error occured:\n{0}\nStackTrace:\n{1}", ex.Message, ex.StackTrace);
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

        protected virtual Assembly GetProxyAssembly()
        {
            return Assembly.GetAssembly(GetType());
        }

        private void OnExecute(IPluginExecutionContext executionContext)
        {
            var parameters = new ParametersWrapper<T>(executionContext);
            var parentContext = executionContext.ParentContext;
            if (parentContext != null)
            {
                switch (parentContext.MessageName)
                {
                    case PluginVirtualMessageName.ExportToExcel:
                    case PluginVirtualMessageName.ExportDynamicToExcel:
                        OnExportToExcel(executionContext, parameters.Query, parameters.BusinessEntityCollection);
                        break;
                }
            }

            // ToDo: implement methods dictionary using and/or Visitor pattern
            switch (executionContext.MessageName)
            {
                case PluginMessageName.Create:
                    OnCreate(executionContext, parameters.TypedEntity(), parameters.Id, parameters.PostEntityImage);
                    break;
                case PluginMessageName.Update:
                    var convertedEntity = parameters.TypedEntity();
                    OnUpdate(executionContext, convertedEntity, convertedEntity.Id, parameters.PreEntityImage, parameters.PostEntityImage);
                    break;
                case PluginMessageName.Delete:
                    OnDelete(executionContext, parameters.TargetRef.LogicalName, parameters.TargetRef.Id, parameters.PreEntityImage);
                    break;
                case PluginMessageName.SetState:
                case PluginMessageName.SetStateDynamicEntity:
                    OnSetState(executionContext, parameters.EntityMoniker.LogicalName, parameters.EntityMoniker.Id, parameters.State, parameters.Status);
                    break;
                case PluginMessageName.Assign:
                    OnAssign(executionContext, parameters.TargetRef.LogicalName, parameters.TargetRef.Id, parameters.Assignee.LogicalName, parameters.Assignee.Id);
                    break;
                case PluginMessageName.Merge:
                    OnMerge(executionContext,
                            parameters.TargetRef,
                            parameters.SubordinateId,
                            parameters.UpdateContent,
                            parameters.PerformParentingChecks);
                    break;
                case PluginMessageName.AddMember:
                    OnAddMember(executionContext, parameters.ListId, parameters.EntityId, parameters.Id);
                    break;
                case PluginMessageName.RemoveMember:
                    OnRemoveMember(executionContext, parameters.ListId, parameters.EntityId);
                    break;
                case PluginMessageName.RetrieveMultiple:
                    OnRetrieveMultiple(executionContext, parameters.Query, parameters.BusinessEntityCollection);
                    break;
                case PluginMessageName.Close:
                    OnClose(executionContext, parameters.ClosedEntity, parameters.Status);
                    break;
                case PluginMessageName.Cancel:
                    OnCancel(executionContext, parameters.OrderClose, parameters.Status);
                    break;
                case PluginMessageName.GrantAccess:
                    OnGrantAccess(
                        executionContext,
                        parameters.TargetRef.LogicalName,
                        parameters.TargetRef.Id,
                        parameters.PrincipalAccess.Principal.LogicalName,
                        parameters.PrincipalAccess.Principal.Id,
                        parameters.PrincipalAccess.AccessMask);
                    break;
                case PluginMessageName.ModifyAccess:
                    OnModifyAccess(
                        executionContext,
                        parameters.TargetRef.LogicalName,
                        parameters.TargetRef.Id,
                        parameters.PrincipalAccess.Principal.LogicalName,
                        parameters.PrincipalAccess.Principal.Id,
                        parameters.PrincipalAccess.AccessMask);
                    break;
                case PluginMessageName.RevokeAccess:
                    OnRevokeAccess(executionContext, parameters.TargetRef.LogicalName, parameters.TargetRef.Id, parameters.Revokee.LogicalName, parameters.Revokee.Id);
                    break;
                case PluginMessageName.RetrieveFilteredForms:
                    OnRetrieveFilteredForms(executionContext, parameters.EntityLogicalName, parameters.SystemUserId, parameters.FormType, parameters.SystemForms);
                    break;
            }
        }
    }
}
