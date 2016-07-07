using System.Reflection;

using Microsoft.Xrm.Sdk;

namespace Crm.PluginBase.Extensions
{
    public static class IPluginExecutionContextExtensions
    {
        public static void EnableProxyTypes(this IPluginExecutionContext pluginExecutionContext, Assembly proxyAssembly)
        {
            pluginExecutionContext.GetType().GetProperty("ProxyTypesAssembly").SetValue(pluginExecutionContext, proxyAssembly, new object[0]);
        }
    }
}
