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

        public T PreEntityImage { get; private set; }

        public T PostEntityImage { get; private set; }

        public EntityReference TargetRef
        {
            get
            {
                return InputParameters["Target"] as EntityReference;
            }
        }

        public Guid Id
        {
            get
            {
                return OutputParameters.Contains("id") ? Guid.Parse(OutputParameters["id"].ToString()) : Guid.Empty;
            }
        }

        public EntityReference EntityMoniker
        {
            get
            {
                return InputParameters["EntityMoniker"] as EntityReference;
            }
        }

        public OptionSetValue State
        {
            get
            {
                return InputParameters["State"] as OptionSetValue;
            }
        }

        public OptionSetValue Status
        {
            get
            {
                var status = InputParameters["Status"];
                return status as OptionSetValue ?? new OptionSetValue((int)status);
            }
        }

        public EntityReference Assignee
        {
            get
            {
                return InputParameters["Assignee"] as EntityReference;
            }
        }

        public Guid SubordinateId
        {
            get
            {
                return InputParameters.Contains("SubordinateId") ? (Guid)InputParameters["SubordinateId"] : Guid.Empty;
            }
        }

        public Entity UpdateContent
        {
            get
            {
                return InputParameters["UpdateContent"] as Entity;
            }
        }

        public bool PerformParentingChecks
        {
            get
            {
                return (bool)InputParameters["PerformParentingChecks"];
            }
        }

        public Guid ListId
        {
            get
            {
                return InputParameters.Contains("ListId") ? (Guid)InputParameters["ListId"] : Guid.Empty;
            }
        }

        public Guid EntityId
        {
            get
            {
                return InputParameters.Contains("EntityId") ? (Guid)InputParameters["EntityId"] : Guid.Empty;
            }
        }

        public QueryBase Query
        {
            get
            {
                return InputParameters.Contains("Query") ? (QueryBase)InputParameters["Query"] : null;
            }
        }

        public EntityCollection BusinessEntityCollection
        {
            get
            {
                return OutputParameters.Contains("BusinessEntityCollection") ? (EntityCollection)OutputParameters["BusinessEntityCollection"] : null;
            }
        }

        public Entity ClosedEntity
        {
            get
            {
                return (InputParameters.Contains("IncidentResolution")
                           ? InputParameters["IncidentResolution"]
                           : InputParameters["QuoteClose"]) as Entity;
            }
        }

        public Entity OrderClose
        {
            get
            {
                return InputParameters["OrderClose"] as Entity;
            }
        }

        public PrincipalAccess PrincipalAccess
        {
            get
            {
                return (PrincipalAccess)InputParameters["PrincipalAccess"];
            }
        }

        public EntityReference Revokee
        {
            get
            {
                return InputParameters["Revokee"] as EntityReference;
            }
        }

        public string EntityLogicalName
        {
            get
            {
                return (string)InputParameters["EntityLogicalName"];
            }
        }

        public Guid SystemUserId
        {
            get
            {
                return (Guid)InputParameters["SystemUserId"];
            }
        }

        public OptionSetValue FormType
        {
            get
            {
                return (OptionSetValue)InputParameters["FormType"];
            }
        }

        public EntityReferenceCollection SystemForms
        {
            get
            {
                return OutputParameters["SystemForms"] as EntityReferenceCollection;
            }
        }

        private ParameterCollection InputParameters { get; set; }

        private ParameterCollection OutputParameters { get; set; }

        private object Target
        {
            get
            {
                return InputParameters["Target"];
            }
        }

        public T TypedEntity(object target = null)
        {
            target = target ?? Target;
            var typedEntity = target as T;
            if (typedEntity != null)
            {
                return typedEntity;
            }

            var entity = target as Entity;
            return entity != null ? entity.ToEntity<T>() : null;
        }
    }
}
