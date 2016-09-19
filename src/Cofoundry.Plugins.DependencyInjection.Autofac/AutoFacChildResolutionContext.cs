using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cofoundry.Core.DependencyInjection;

namespace Cofoundry.Plugins.DependencyInjection.AutoFac
{
    public class AutoFacChildResolutionContext : AutoFacResolutionContext, IChildResolutionContext
    {
        #region constructor

        private readonly ILifetimeScope _container;

        public AutoFacChildResolutionContext(ILifetimeScope container)
            : base(container)
        {
            _container = container;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        } 

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                _container.Dispose();
            }
        }

        #endregion
    }
}
