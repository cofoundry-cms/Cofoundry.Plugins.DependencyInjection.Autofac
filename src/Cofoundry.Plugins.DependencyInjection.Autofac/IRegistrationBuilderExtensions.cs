using Autofac;
using Autofac.Builder;
using Cofoundry.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cofoundry.Plugins.DependencyInjection.AutoFac
{
    public static class IRegistrationBuilderExtensions
    {
        private static InstanceScope DEFAULT_INSTANCE_SCOPE = InstanceScope.PerLifetimeScope;

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AsDefaultScope<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.ScopedTo(DEFAULT_INSTANCE_SCOPE);
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ScopedTo<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, RegistrationOptions options)
        {
            if (options == null)
            {
                return builder.AsDefaultScope();
            }
            else
            {
                return builder.ScopedTo(options.InstanceScope);
            }
        }

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ScopedTo<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, InstanceScope scope)
        {
            if (scope == null)
            {
                scope = DEFAULT_INSTANCE_SCOPE;
            }

            if (scope == InstanceScope.PerLifetimeScope)
            {
                return builder.InstancePerLifetimeScope();
            }

            if (scope == InstanceScope.Transient)
            {
                return builder.InstancePerDependency();
            }

            if (scope == InstanceScope.Singleton)
            {
                return builder.SingleInstance();
            }

            if (scope == InstanceScope.PerWebRequest)
            {
                return builder.InstancePerRequest();
            }

            throw new ArgumentException("InstanceScope '" + scope.GetType().FullName + "' not recognised");
        }
    }
}
