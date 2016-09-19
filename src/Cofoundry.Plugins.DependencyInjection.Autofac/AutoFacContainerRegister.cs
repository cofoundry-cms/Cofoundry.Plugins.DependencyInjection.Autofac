using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cofoundry.Core.DependencyInjection;

namespace Cofoundry.Plugins.DependencyInjection.AutoFac
{
    public class AutoFacContainerRegister : IContainerRegister
    {
        #region Privates

        private readonly AutoFacContainerBuilder _containerBuilder;
        private readonly ContainerBuilder _builder;
        private Type[] _allTypes = null;
        private readonly object _lock = new object();

        #endregion

        #region constructor

        public AutoFacContainerRegister(AutoFacContainerBuilder containerBuilder)
        {
            _containerBuilder = containerBuilder;
            _builder = containerBuilder.Builder;
        }

        #endregion

        #region IContainerRegister implementation

        public IContainerRegister RegisterInstance<TRegisterAs>(TRegisterAs instance, RegistrationOptions options = null)
        {
            var fn = new Action(() =>
            {
                _builder
                    .Register<TRegisterAs>(c => instance)
                    .AsSelf()
                    ;
            });

            Register<TRegisterAs>(fn, options);

            return this;
        }

        public IContainerRegister RegisterInstance<TRegisterAs>(RegistrationOptions options = null)
        {
            return RegisterInstance<TRegisterAs, TRegisterAs>(options);
        }

        public IContainerRegister RegisterInstance<TRegisterAs, TConcrete>(RegistrationOptions options = null) where TConcrete : TRegisterAs
        {
            if (options == null)
            {
                options = new RegistrationOptions();
            }
            options.InstanceScope = InstanceScope.Singleton;
            return RegisterType<TRegisterAs, TConcrete>(options);
        }

        public IContainerRegister RegisterType<TConcrete>(RegistrationOptions options = null)
        {
            return RegisterType<TConcrete, TConcrete>(options);
        }

        public IContainerRegister RegisterType<TConcrete>(Type[] types, RegistrationOptions options = null)
        {
            var fn = new Action(() =>
            {
                _builder
                    .RegisterType<TConcrete>()
                    .AsSelf()
                    .As(types)
                    .ScopedTo(options);
            });

            Register<TConcrete>(fn, options);

            return this;
        }

        public IContainerRegister RegisterType<TRegisterAs, TConcrete>(RegistrationOptions options = null) where TConcrete : TRegisterAs
        {
            var fn = new Action(() =>
            {
                _builder
                    .RegisterType<TConcrete>()
                    .AsSelf()
                    .As<TRegisterAs>()
                    .ScopedTo(options);
            });

            Register<TRegisterAs>(fn, options);

            return this;
        }

        public IContainerRegister RegisterTypeInCollection<TRegisterAs, TConcrete>() where TConcrete : TRegisterAs
        {
            _builder
                .RegisterType<TConcrete>()
                .AsSelf()
                .As<TRegisterAs>()
                .AsDefaultScope();

            return this;
        }

        public IContainerRegister RegisterAll<TToRegister>()
        {
            var typeDef = typeof(TToRegister);

            var at = GetAllTypes()
                .Where(t => !t.ContainsGenericParameters
                         && typeDef.IsAssignableFrom(t)
                         && t != typeDef
                    );

            foreach (var type in at)
            {
                _builder
                    .RegisterType(type)
                    .AsSelf()
                    .As(typeDef)
                    .AsDefaultScope();
            }

            return this;
        }

        public IContainerRegister RegisterAllGenericImplementations(Type typeDef)
        {
            if (!typeDef.IsGenericTypeDefinition)
            {
                throw new ArgumentException("TGeneric should be generic");
            }

            var handlerRegistrations =
                from implementation in GetAllTypes()
                where !implementation.IsAbstract
                where !implementation.ContainsGenericParameters
                let services =
                    from iface in implementation.GetInterfaces()
                    where iface.IsGenericType
                    where iface.GetGenericTypeDefinition() == typeDef
                    select iface
                from service in services
                select new { service, implementation };

            foreach (var handler in handlerRegistrations)
            {
                _builder
                    .RegisterType(handler.implementation)
                    .AsSelf()
                    .As(handler.service)
                    .AsDefaultScope();
            }

            return this;
        }

        public IContainerRegister RegisterFactory<TConcrete, TFactory>(RegistrationOptions options = null) where TFactory : IInjectionFactory<TConcrete>
        {
            return RegisterFactory<TConcrete, TConcrete, TFactory>(options);
        }

        public IContainerRegister RegisterFactory<TRegisterAs, TConcrete, TFactory>(RegistrationOptions options = null)
            where TFactory : IInjectionFactory<TConcrete>
            where TRegisterAs : TConcrete
        {
            var fn = new Action(() =>
            {
                _builder
                    .RegisterType<TFactory>()
                    .AsDefaultScope();
                _builder.Register<TConcrete>(c => c.Resolve<TFactory>().Create()).As<TRegisterAs>().ScopedTo(options);
            });

            Register<TRegisterAs>(fn, options);

            return this;
        }

        public IContainerRegister RegisterGeneric(Type registerAs, Type typeToRegister)
        {
            _builder
                .RegisterGeneric(typeToRegister)
                .As(registerAs)
                .AsDefaultScope();

            return this;
        }

        #endregion

        #region internal helpers

        /// <remarks>
        /// internal access to the type library
        /// </remarks>
        internal Type[] GetAllTypes()
        {
            if (_allTypes == null)
            {
                lock (_lock)
                {
                    if (_allTypes == null)
                    {
                        var loadedAssemblies = AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .ToList();

                        var loadedPaths = loadedAssemblies
                            .Select(a => new { Location = a.Location }).ToArray();

                        var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

                        var assembliesToLoad = referencedPaths
                            .Where(p => !loadedAssemblies.Any(la => la.Location.Equals(p, StringComparison.OrdinalIgnoreCase)))
                            .Select(p => AssemblyName.GetAssemblyName(p))
                            .Where(a => !loadedAssemblies.Any(la => la.FullName == a.FullName))
                            .GroupBy(a => a.FullName)
                            .Select(a => a.First())
                            .ToList();

                        foreach (var assemblyToLoad in assembliesToLoad
                            .Where(a => !a.FullName.StartsWith("System"))
                            .Where(a => !a.FullName.StartsWith("Microsoft"))
                            .Where(a => !a.FullName.StartsWith("mscorlib")))
                        {
                            var assembly = AppDomain.CurrentDomain.Load(assemblyToLoad);
                            Trace.TraceInformation("Dynamically loaded assembly: {0}", assembly.Location);
                            loadedAssemblies.Add(assembly);
                        }

                        _allTypes = loadedAssemblies
                            // Remove BCL assemblies - we won't ever be registering them here so can speed up this up
                            .Where(a => !a.FullName.StartsWith("System"))
                            .Where(a => !a.FullName.StartsWith("Microsoft"))
                            .Where(a => !a.FullName.StartsWith("mscorlib"))
                            // Get Types
                            .SelectMany(a => a.ExportedTypes)
                            // Remove base classes
                            .Where(t => !t.IsAbstract)
                            // De-duplicate
                            .GroupBy(a => a.FullName)
                            .Select(a => a.First())
                            .ToArray();
                    }
                }
            }

            return _allTypes;
        }

        #endregion

        #region helpers

        private void Register<TTo>(Action register, RegistrationOptions options = null)
        {
            if (options != null && options.ReplaceExisting)
            {
                _containerBuilder.QueueRegistration<TTo>(register);
            }
            else
            {
                register();
            }
        }

        #endregion
    }
}
