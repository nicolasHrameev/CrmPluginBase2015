using Microsoft.Xrm.Sdk;

namespace Crm.PluginBase
{
    public abstract class CrmPlugin<T> : CrmPluginBase<T> where T : Entity
    {
        protected CrmPlugin()
            : this(null, null)
        {
        }

        protected CrmPlugin(string unsecure, string secure)
            : base(unsecure, secure)
        {
        }
    }
}
