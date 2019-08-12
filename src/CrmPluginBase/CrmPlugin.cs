using Microsoft.Xrm.Sdk;

namespace CrmPluginBase
{
    public abstract class CrmPlugin<TEntity> : CrmPluginBase<TEntity> where TEntity : Entity
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
