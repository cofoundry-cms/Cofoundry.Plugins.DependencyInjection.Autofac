using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Cofoundry.Core.EmbeddedResources;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;

namespace Cofoundry.Plugins.DependencyInjection.AutoFac.Web
{
    public static class AutoFacOwinMiddlewareExtensions
    {
        #region registration

        /// <summary>
        /// Configures the dependency resolver for Cofoundry to use AutoFac and
        /// registers all the services, repositories and modules setup for auto-registration.
        /// </summary>
        public static void UseCofoundryAutoFacIntegration(this IAppBuilder app, Action<ContainerBuilder> additionalConfiguration = null)
        {
            var autoFacBuilder = new ContainerBuilder();
            var cofoundryBuilder = new AutoFacContainerBuilder(autoFacBuilder);

            cofoundryBuilder.Build();

            if (additionalConfiguration != null)
            {
                additionalConfiguration(autoFacBuilder);
            }

            var container = autoFacBuilder.Build();
            RegisterMvc(container);
            SetServiceLocators(container);
        }

        /// <summary>
        /// Here we need to use the existing registration to find other assemblies
        /// with IAssemblyResourceRegistration, which might contain other
        /// controllers that need registering.
        /// </summary>
        private static void RegisterMvc(IContainer container)
        {
            // loadedAssemblies should at least always include the main entry assembly
            var loadedAssemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => !a.FullName.StartsWith("System"))
                .Where(a => !a.FullName.StartsWith("Microsoft"))
                .Where(a => !a.FullName.StartsWith("mscorlib"));

            // modular resource assemblies
            var registrations = container.Resolve<IEnumerable<IAssemblyResourceRegistration>>();

            var allAssemblies = registrations
                .Select(r => r.GetType().Assembly)
                .Union(loadedAssemblies)
                .ToArray();

            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(allAssemblies);
            builder.RegisterControllers(allAssemblies);

            builder.Update(container);
        }

        private static void SetServiceLocators(IContainer container)
        {
            var serviceLocator = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        #endregion
    }
}
