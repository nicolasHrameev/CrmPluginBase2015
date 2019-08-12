using System;

using CrmPluginBase.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmPluginBase
{
    internal class ParametersWrapper<TEntity> where TEntity : Entity
    {
        public ParametersWrapper(IExecutionContext pluginExecutionContext, string preImageName, string postImageName)
        {
            InputParameters = pluginExecutionContext.InputParameters;
            OutputParameters = pluginExecutionContext.OutputParameters;
            PreImages = pluginExecutionContext.PreEntityImages;
            PostImages = pluginExecutionContext.PostEntityImages;
            PreImageName = preImageName;
            PostImageName = postImageName;
        }

        public TEntity Target => InputParameters["Target"].ToEntity<TEntity>();

        public TEntity PreEntityImage => PreImages.TryGetValue(PreImageName, out var image) ? image.ToEntity<TEntity>() : null;

        public TEntity PostEntityImage => PostImages.TryGetValue(PostImageName, out var image) ? image.ToEntity<TEntity>() : null;

        public EntityReference TargetRef => InputParameters["Target"] as EntityReference;

        public Guid Id => OutputParameters.TryGetValue("id", out var id) ? Guid.Parse(id.ToString()) : Guid.Empty;

        public EntityReference EntityMoniker => InputParameters["EntityMoniker"] as EntityReference;

        public OptionSetValue State => InputParameters["State"] as OptionSetValue;

        public OptionSetValue Status
        {
            get
            {
                var status = InputParameters["Status"];
                return status as OptionSetValue ?? new OptionSetValue((int)status);
            }
        }

        public EntityReference Assignee => InputParameters["Assignee"] as EntityReference;

        public Guid SubordinateId => InputParameters.TryGetValue("SubordinateId", out var subordinateId) ? (Guid)subordinateId : Guid.Empty;

        public Entity UpdateContent => InputParameters["UpdateContent"] as Entity;

        public bool PerformParentingChecks => (bool)InputParameters["PerformParentingChecks"];

        public Guid ListId => InputParameters.TryGetValue("ListId", out var listId) ? (Guid)listId : Guid.Empty;

        public Guid EntityId => InputParameters.TryGetValue("EntityId", out var entityId) ? (Guid)entityId : Guid.Empty;

        public QueryBase Query => InputParameters.TryGetValue("Query", out var query) ? (QueryBase)query : null;

        public EntityCollection BusinessEntityCollection => OutputParameters.TryGetValue("BusinessEntityCollection", out var businessEntityCollection) ? (EntityCollection)businessEntityCollection : null;

        public Entity ClosedEntity =>
            (InputParameters.TryGetValue("IncidentResolution", out var incidentResolution)
                ? incidentResolution
                : InputParameters["QuoteClose"]) as Entity;

        public Entity OrderClose => InputParameters["OrderClose"] as Entity;

        public PrincipalAccess PrincipalAccess => (PrincipalAccess)InputParameters["PrincipalAccess"];

        public EntityReference Revokee => InputParameters["Revokee"] as EntityReference;

        public string EntityLogicalName => (string)InputParameters["EntityLogicalName"];

        public Guid SystemUserId => (Guid)InputParameters["SystemUserId"];

        public OptionSetValue FormType => (OptionSetValue)InputParameters["FormType"];

        public EntityReferenceCollection SystemForms => OutputParameters["SystemForms"] as EntityReferenceCollection;

        private ParameterCollection InputParameters { get; }

        private ParameterCollection OutputParameters { get; }

        private EntityImageCollection PreImages { get; }

        private EntityImageCollection PostImages { get; }

        private string PreImageName { get; }

        private string PostImageName { get; }
    }
}
