using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity.Mvc;
using BeYourMarket.Core;
using System.Reflection;
using System;
using BeYourMarket.Core.Plugins;

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

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            try
            {
                //http://stackoverflow.com/questions/699852/how-to-find-all-the-classes-which-implement-a-given-interface
                foreach (var assembly in assemblies)
                {
                    var instances = from t in assembly.GetTypes()
                                    where t.GetInterfaces().Contains(typeof(IDependencyRegister))
                                             && t.GetConstructor(Type.EmptyTypes) != null
                                    select Activator.CreateInstance(t) as IDependencyRegister;

                    foreach (var instance in instances.OrderBy(x => x.Order))
                    {
                        instance.Register(container);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
            http://stackoverflow.com/questions/1091853/error-message-unable-to-load-one-or-more-of-the-requested-types-retrieve-the-l
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    System.IO.FileNotFoundException exFileNotFound = exSub as System.IO.FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();

                throw new Exception(errorMessage, ex);
                //Display or log the error based on your application.
            }


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