using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity.Mvc;
using BeYourMarket.Core;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(BeYourMarket.Web.App_Start.UnityWebActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(BeYourMarket.Web.App_Start.UnityWebActivator), "Shutdown")]

namespace BeYourMarket.Web.App_Start
{
    /// <summary>Provides the bootstrapping for integrating Unity with ASP.NET MVC.</summary>
    public static class UnityWebActivator
    {
        /// <summary>Integrates Unity when the application starts.</summary>
        public static void Start()
        {
            var container = ContainerManager.GetConfiguredContainer();
            UnityConfig.RegisterTypes(container);

            FilterProviders.Providers.Remove(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().First());
            FilterProviders.Providers.Add(new UnityFilterAttributeFilterProvider(container));

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            //http://stackoverflow.com/questions/699852/how-to-find-all-the-classes-which-implement-a-given-interface
            //    var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
            //                    where t.GetInterfaces().Contains(typeof(ISomething))
            //                             && t.GetConstructor(Type.EmptyTypes) != null
            //                    select Activator.CreateInstance(t) as ISomething;

            //    foreach (var instance in instances)
            //    {
            //        instance.Foo(); // where Foo is a method of ISomething
            //    }

            // TODO: Uncomment if you want to use PerRequestLifetimeManager
            // Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(UnityPerRequestHttpModule));
        }

        /// <summary>Disposes the Unity container when the application is shut down.</summary>
        public static void Shutdown()
        {
            var container = ContainerManager.GetConfiguredContainer();
            container.Dispose();
        }
    }
}