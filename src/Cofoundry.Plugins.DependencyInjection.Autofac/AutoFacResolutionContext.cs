using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cofoundry.Core.DependencyInjection;

namespace Cofoundry.Plugins.DependencyInjection.AutoFac
{
    public class AutoFacResolutionContext : IResolutionContext
    {
        #region constructor

        private readonly ILifetimeScope _container;

        public AutoFacResolutionContext(ILifetimeScope container)
        {
            _container = container;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Returns the registered implementation of a service
        /// </summary>
        /// <typeparam name="TService">Type of service to resolve.</typeparam>
        /// <returns>TService instance.</returns>
        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        /// <summary>
        /// Returns the registered implementation of a service
        /// </summary>
        /// <param name="t">Type of service to resolve.</param>
        /// <returns>Service instance.</returns>
        public object Resolve(Type t)
        {
            return _container.Resolve(t);
        }

        /// <summary>
        /// Returns all registered implementation of a service
        /// </summary>
        /// <typeparam name="TService">Type of service to resolve.</typeparam>
        /// <returns>All registered instances of TService.</returns>
        public IEnumerable<TService> ResolveAll<TService>()
        {
            return _container.Resolve<IEnumerable<TService>>();
        }
        
        /// <summary>
        /// Creates a child context inheriting the parent contexts settings
        /// but can be used with a different lifetime scope.
        /// </summary>
        /// <returns>Child IResolutionContext</returns>
        public IChildResolutionContext CreateChildContext()
        {
            return new AutoFacChildResolutionContext(_container.BeginLifetimeScope());
        }

        /// <summary>
        /// Determins if the specified generic type is registered in the container
        /// </summary>
        /// <typeparam name="T">Type to check for registration</typeparam>
        /// <returns>True if the type is registered; otherwise false</returns>
        public bool IsRegistered<T>()
        {
            return _container.IsRegistered<T>();
        }
        
        /// <summary>
        /// Determins if the specified type is registered in the container
        /// </summary>
        /// <param name="typeToCheck">Type to check for registration</param>
        /// <returns>True if the type is registered; otherwise false</returns>
        public bool IsRegistered(Type typeToCheck)
        {
            return _container.IsRegistered(typeToCheck);
        }

        #endregion
    }
}
