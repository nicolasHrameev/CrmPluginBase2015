using System;

namespace CrmPluginBase.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static TService GetService<TService>(this IServiceProvider serviceProvider) => (TService)serviceProvider.GetService(typeof(TService));
    }
}
