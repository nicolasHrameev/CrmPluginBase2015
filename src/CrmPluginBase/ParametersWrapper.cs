using System;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmPluginBase
{
    internal class ParametersWrapper<T> where T : Entity
    {
        public ParametersWrapper(IExecutionContext pluginExecutionContext)
        {
            InputParameters = pluginExecutionContext.InputParameters;
            OutputParameters = pluginExecutionContext.OutputParameters;

            if (pluginExecutionContext.PreEntityImages.ContainsKey("preimage"))
            {
                PreEntityImage = TypedEntity(pluginExecutionContext.PreEntityImages["preimage"]);
            }

            if (pluginExecutionContext.PostEntityImages.ContainsKey("postimage"))
            {
                PostEntityImage = TypedEntity(pluginExecutionContext.PostEntityImages["postimage"]);
            }
        }

        public T PreEntityImage { get; }

        public T PostEntityImage { get; }

        public EntityReference TargetRef => InputParameters["Target"] as EntityReference;

        public Guid Id => OutputParameters.Contains("id") ? Guid.Parse(OutputParameters["id"].ToString()) : Guid.Empty;

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

        public Guid SubordinateId => InputParameters.Contains("SubordinateId") ? (Guid)InputParameters["SubordinateId"] : Guid.Empty;

        public Entity UpdateContent => InputParameters["UpdateContent"] as Entity;

        public bool PerformParentingChecks => (bool)InputParameters["PerformParentingChecks"];

        public Guid ListId => InputParameters.Contains("ListId") ? (Guid)InputParameters["ListId"] : Guid.Empty;

        public Guid EntityId => InputParameters.Contains("EntityId") ? (Guid)InputParameters["EntityId"] : Guid.Empty;

        public QueryBase Query => InputParameters.Contains("Query") ? (QueryBase)InputParameters["Query"] : null;

        public EntityCollection BusinessEntityCollection => OutputParameters.Contains("BusinessEntityCollection") ? (EntityCollection)OutputParameters["BusinessEntityCollection"] : null;

        public Entity ClosedEntity => (InputParameters.Contains("IncidentResolution")
            ? InputParameters["IncidentResolution"]
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

        private object Target => InputParameters["Target"];

        public T TypedEntity(object target = null)
        {
            target = target ?? Target;
            var typedEntity = target as T;
            if (typedEntity != null)
            {
                return typedEntity;
            }

            var entity = target as Entity;
            return entity?.ToEntity<T>();
        }
    }
}
