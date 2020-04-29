using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DryIoc;
using EventFlow.Configuration;

namespace EventFlow.DryIoc.Registrations
{
    internal class DryIocResolver : EventFlow.Configuration.IResolver
    {
        public DryIocResolver(IContainer container)
        {
            _container = container;
        }

        private readonly IContainer _container;
        public IEnumerable<Type> GetRegisteredServices()
        {
            return _container.GetServiceRegistrations().Select(servInfo => servInfo.ServiceType);
        }

        public bool HasRegistrationFor<T>() where T : class
        {
            return _container.GetServiceRegistrations().Any(serviceInfo => serviceInfo.ServiceType == typeof(T));
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return _container.ResolveMany(serviceType);
        }
    }

}
