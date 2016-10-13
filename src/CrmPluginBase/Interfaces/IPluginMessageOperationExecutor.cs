using System;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmPluginBase.Interfaces
{
    public interface IPluginMessageOperationExecutor<in T> where T : Entity
    {
        /// <summary>
        /// Override for Update plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the primary entity</param>
        /// <param name="preEntityImage">Entity image for pre operation</param>
        /// <param name="postEntityImage">Entity image for post operation</param>
        void OnUpdate(IPluginExecutionContext context, T entity, Guid primaryEntityId, T preEntityImage, T postEntityImage);

        /// <summary>
        /// Override for Create plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="postEntityImage">Entity image for post operation</param>
        void OnCreate(IPluginExecutionContext context, T entity, Guid primaryEntityId, T postEntityImage);

        /// <summary>
        /// Override for Delete plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="preEntityImage">Entity image for pre operation</param>
        void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId, T preEntityImage);

        /// <summary>
        /// Override for Update plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the primary entity</param>
        void OnUpdate(IPluginExecutionContext context, T entity, Guid primaryEntityId);

        /// <summary>
        /// Override for Create plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entity">Typed entity</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        void OnCreate(IPluginExecutionContext context, T entity, Guid primaryEntityId);

        /// <summary>
        /// Override for Delete plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        void OnDelete(IPluginExecutionContext context, string entityName, Guid primaryEntityId);

        /// <summary>
        /// Override for SetState plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="newState">New State of the entity</param>
        /// <param name="newStatus">New Status of the entity</param>
        void OnSetState(IPluginExecutionContext context, string entityName, Guid primaryEntityId, OptionSetValue newState, OptionSetValue newStatus);

        /// <summary>
        /// Override for Assign plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityLogicalName">Entity logical name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="assigneeEntityLogicalName">Assignee entity logical name</param>
        /// <param name="assigneeId">Assignee id</param>
        void OnAssign(IPluginExecutionContext context, string entityLogicalName, Guid primaryEntityId, string assigneeEntityLogicalName, Guid assigneeId);

        /// <summary>
        /// Override for Merge plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="targetId">Entity reference</param>
        /// <param name="subordinateId">Entity to merge</param>
        /// <param name="updateContent">Attributes to update from base entity</param>
        /// <param name="performParentingChecks">Perfor parenting checks</param>
        void OnMerge(IPluginExecutionContext context, EntityReference targetId, Guid subordinateId, Entity updateContent, bool performParentingChecks);

        /// <summary>
        /// Override for Close plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="closeEntity">Incident resolution entity</param>
        /// <param name="status">Status code</param>
        void OnClose(IPluginExecutionContext context, Entity closeEntity, OptionSetValue status);

        /// <summary>
        /// Override for Cancel plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="orderClose">Order close entity</param>
        /// <param name="status">Status code</param>
        void OnCancel(IPluginExecutionContext context, Entity orderClose, OptionSetValue status);

        /// <summary>
        /// Override for AddMember plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="listId">Id of the list entity</param>
        /// <param name="primaryEntityId">Id of the adding to list entity</param>
        /// <param name="listMemberId">Id of the created listmember entity</param>
        void OnAddMember(IPluginExecutionContext context, Guid listId, Guid primaryEntityId, Guid listMemberId);

        /// <summary>
        /// Override for RemoveMember plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="listId">Id of the list entity</param>
        /// <param name="primaryEntityId">Id of the removing from list entity</param>
        void OnRemoveMember(IPluginExecutionContext context, Guid listId, Guid primaryEntityId);

        /// <summary>
        /// Override for RetrieveMultiple plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="query">Query of RetrieveMultiple</param>
        /// <param name="entityCollection">Collection of entity</param>
        void OnRetrieveMultiple(IPluginExecutionContext context, QueryBase query, EntityCollection entityCollection);

        /// <summary>
        /// Override for ExportToExcel (RetrieveMultiple) plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="query">Query of RetrieveMultiple</param>
        /// <param name="entityCollection">Collection of entity</param>
        void OnExportToExcel(IPluginExecutionContext context, QueryBase query, EntityCollection entityCollection);

        /// <summary>
        /// Override for GrantAccess plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="principalEntityName">Principal entity name</param>
        /// <param name="principalId">Principal Id</param>
        /// <param name="mask">Principal access mask</param>
        void OnGrantAccess(IPluginExecutionContext context, string entityName, Guid primaryEntityId, string principalEntityName, Guid principalId, AccessRights mask);

        /// <summary>
        /// Override for GrantAccess plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="principalEntityName">Principal entity name</param>
        /// <param name="principalId">Principal Id</param>
        /// <param name="mask">Principal access mask</param>
        void OnModifyAccess(IPluginExecutionContext context, string entityName, Guid primaryEntityId, string principalEntityName, Guid principalId, AccessRights mask);

        /// <summary>
        /// Override for GrantAccess plugin message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="primaryEntityId">Id of the entity</param>
        /// <param name="revokeeEntityName">Revokee entity name</param>
        /// <param name="revokeeId">Revokee Id</param>
        void OnRevokeAccess(IPluginExecutionContext context, string entityName, Guid primaryEntityId, string revokeeEntityName, Guid revokeeId);

        /// <summary>
        /// Override for RetrieveFilteredForms message
        /// </summary>
        /// <param name="context">Crm Context</param>
        /// <param name="entityName">Current entity name</param>
        /// <param name="userId">System user Id</param>
        /// <param name="formType">Form type</param>
        /// <param name="systemForms">System forms for current entity</param>
        void OnRetrieveFilteredForms(IPluginExecutionContext context, string entityName, Guid userId, OptionSetValue formType, EntityReferenceCollection systemForms);
    }
}
