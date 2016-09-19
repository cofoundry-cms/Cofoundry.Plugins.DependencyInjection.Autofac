using Autofac;
using Autofac.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cofoundry.Core.DependencyInjection;

namespace Cofoundry.Plugins.DependencyInjection.AutoFac
{
    public class AutoFacContainerBuilder : IContainerBuilder
    {
        #region Privates

        private readonly Dictionary<Type, Action> RegistrationOverrides = new Dictionary<Type, Action>();
        private readonly ContainerBuilder _builder;
        bool hasBuild = false;

        #endregion

        #region constructor

        public AutoFacContainerBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        #endregion

        #region properties

        internal ContainerBuilder Builder 
        {
            get
            {
                return _builder;
            } 
        }

        #endregion

        #region public methods

        public void Build()
        {
            CheckIsBuilt();
            RegisterFramework();

            var containerRegister = new AutoFacContainerRegister(this);

            var registrations = GetAllRegistrations(containerRegister);
            foreach (var registration in registrations)
            {
                registration.Register(containerRegister);
            }

            BuildOverrides();
        }

        private void RegisterFramework()
        {
            _builder
                .RegisterType<AutoFacResolutionContext>()
                .As<IResolutionContext>();
        }


        internal void QueueRegistration<TTo>(Action registration)
        {
            var typeToRegister = typeof(TTo);
            if (RegistrationOverrides.ContainsKey(typeToRegister))
            {
                throw new ArgumentException("Type already registered as an override. Multiple overrides currently not supported: " + typeToRegister);
            }

            RegistrationOverrides.Add(typeToRegister, registration);
        }

        #endregion

        #region helpers

        private void CheckIsBuilt()
        {
            if (hasBuild)
            {
                throw new InvalidOperationException("The container has already been built.");
            }
            hasBuild = true;
        }

        private void BuildOverrides()
        {
            foreach (var registrationOverride in RegistrationOverrides)
            {
                registrationOverride.Value();
            }
        }

        private IEnumerable<IDependencyRegistration> GetAllRegistrations(AutoFacContainerRegister register)
        {
            var registrations = register.GetAllTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IDependencyRegistration)) && t.GetConstructor(Type.EmptyTypes) != null)
                .Select(t => Activator.CreateInstance(t) as IDependencyRegistration);

            return registrations;
        }
        
        #endregion
    }
}
