using System;
using DryIoc;
using EventFlow.Configuration;
using EventFlow.Configuration.Bootstraps;
using EventFlow.Configuration.Decorators;
using EventFlow.Extensions;

namespace EventFlow.DryIoc.Registrations
{
    internal class DecoratorServiceInjector<TService> where TService : class
    {
        private DecoratorService DecoratorFactory { get; }

        public DecoratorServiceInjector(DecoratorService service)
        {
            DecoratorFactory = service;
        }

        public TService Decorate(TService instance, global::DryIoc.IContainer context)
        {
            return DecoratorFactory.Decorate(instance,
                                    new EventFlow.Configuration.ResolverContext(
                                        new DryIocResolver(context)));

        }
    }

    internal class DryIocServiceRegistration : IServiceRegistration
    {
        private readonly IContainer _container;
        private readonly DecoratorService _decoratorService = new DecoratorService();

        public DryIocServiceRegistration() : this(null) { }

        public DryIocServiceRegistration(IContainer container)
        {
            _container = container ?? new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());
            _container.Register<IBootstrapper, Bootstrapper>(Reuse.Singleton);
            _container.Register<EventFlow.Configuration.IResolver, DryIocResolver>();
            _container.RegisterDelegate<IDecoratorService>(_ => _decoratorService, Reuse.Singleton);
            _container.RegisterDelegate<IScopeResolver>(c => new DryIocScopeResolver(c.OpenScope()), Reuse.Transient, setup: Setup.With(allowDisposableTransient:true));
        }

        public IRootResolver CreateResolver(bool validateRegistrations)
        {
            var resolver = new DryIocRootResolver(_container);

            if (validateRegistrations)
                resolver.ValidateRegistrations();

            var bootstrapper = resolver.Resolve<IBootstrapper>();
            bootstrapper.Start();

            return resolver;
        }

        public void Decorate<TService>(Func<EventFlow.Configuration.IResolverContext, TService, TService> factory)
        {
            _decoratorService.AddDecorator(factory);
        }

        private IReuse GetLifeTime(Lifetime lifetime)
        {
            switch (lifetime)
            {
                case Lifetime.Singleton:
                    return Reuse.Singleton;
                case Lifetime.Scoped:
                    return Reuse.Scoped;
                case Lifetime.AlwaysUnique:
                    return Reuse.Singleton;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Register<TService>(
            Func<EventFlow.Configuration.IResolverContext, TService> factory,
            Lifetime lifetime = Lifetime.AlwaysUnique,
            bool keepDefault = false)
            where TService : class
        {
            var reuse = GetLifeTime(lifetime);

            _container.RegisterDelegate(cxt =>
            {
                var instance = factory(
                    new Configuration.ResolverContext(
                        new DryIocResolver(cxt as IContainer)));

                var result = _decoratorService.Decorate(
                                    instance,
                                    new EventFlow.Configuration.ResolverContext(
                                        new DryIocResolver(cxt as IContainer)));

                return result;

            }, reuse, Setup.With(allowDisposableTransient:true));

            //_container.Register(
            //    reuse: reuse,
            //    made: Made.Of(() => factory(new EventFlow.Configuration.ResolverContext(
            //          new DryIocResolver(_container)))));


            //_container.RegisterDelegateDecorator<TService>(cxt => service =>
            //{
            //    var instance = _decoratorService.Decorate(
            //        service,
            //        new EventFlow.Configuration.ResolverContext(
            //            new DryIocResolver(cxt as IContainer)));

            //    return instance;
            //});
        }

        public void Register(
            Type serviceType,
            Type implementationType,
            Lifetime lifetime = Lifetime.AlwaysUnique,
            bool keepDefault = false)
        {
            var reuse = GetLifeTime(lifetime);

            //_container.Register(serviceType, made: Made.Of(
            //    () => _decoratorService.Decorate(
            //        Activator.CreateInstance(implementationType), 
            //            new EventFlow.Configuration.ResolverContext(
            //                new DryIocResolver(_container)))
            //        ));

            _container.Register(serviceType, implementationType, reuse);
            _container.Register(
                serviceType,
                reuse: reuse,
                made: Made.Of(
                    r => ServiceInfo.Of<IDecoratorService>(),
                    i => i.Decorate(
                        Arg.Of(serviceType, IfUnresolved.ReturnDefault),
                        Arg.Of(implementationType, IfUnresolved.ReturnDefault),
                        Arg.Of<Configuration.IResolverContext>(IfUnresolved.ReturnDefault))),
                        //Arg.Of(
                        //    new Configuration.ResolverContext(
                        //        new DryIocResolver(_container)), 
                        //    IfUnresolved.ReturnDefault))),
                setup: Setup.Decorator);

            //_container.RegisterDelegate(serviceType, cxt =>
            //{
            //    var instance = cxt.Resolve(implementationType);
            //    var decoratorService = cxt.Resolve<IDecoratorService>();

            //    return decoratorService.Decorate(
            //        instance,
            //            new EventFlow.Configuration.ResolverContext(
            //                new DryIocResolver(cxt as IContainer)));
            //});

            //_container.Register(serviceType, implementationType, reuse,
            //    made:Made.Of(() =>
            //    {
            //        var instance = _container.Resolve(implementationType);
            //        var decoratorService = _container.Resolve<IDecoratorService>();

            //        var result = decoratorService.Decorate(
            //            instance,
            //            new EventFlow.Configuration.ResolverContext(
            //                new DryIocResolver(_container)));

            //        return result;
            //    }));

            //_container.RegisterInitializer<object>((instance, cxt) =>
            //{
            //    var decoratorService = cxt.Resolve<IDecoratorService>();

            //    var result = decoratorService.Decorate(
            //        instance,
            //        new EventFlow.Configuration.ResolverContext(
            //            new DryIocResolver(cxt as IContainer)));

            //    cxt.UseInstance(result);
            //}, r => r.ServiceType == serviceType && r.ImplementationType == implementationType);
        }

        public void Register<TService, TImplementation>(
            Lifetime lifetime,
            bool keepDefault)
            where TService : class
            where TImplementation : class, TService
        {
            var reuse = GetLifeTime(lifetime);

            _container.Register<TService, TImplementation>(
                reuse:reuse, 
                setup:Setup.With(allowDisposableTransient:true));

            _container.RegisterInitializer<TService>((instance, cxt) =>
            {
                var result = _decoratorService.Decorate(
                    instance,
                    new EventFlow.Configuration.ResolverContext(
                        new DryIocResolver(cxt as IContainer)));

                cxt.UseInstance(result);
            }, r => r.ImplementationType == typeof(TImplementation));
        }

        public void RegisterGeneric(Type serviceType, Type implementationType, Lifetime lifetime = Lifetime.AlwaysUnique, bool keepDefault = false)
        {
            var reuse = lifetime == Lifetime.Singleton
                ? Reuse.Singleton
                : lifetime == Lifetime.Scoped
                    ? Reuse.Scoped
                    : Reuse.Transient;

            _container.Register(serviceType, implementationType, setup: Setup.With(allowDisposableTransient: true));
        }

        public void RegisterIfNotRegistered<TService, TImplementation>(
            Lifetime lifetime = Lifetime.AlwaysUnique)
            where TService : class
            where TImplementation : class, TService
        {
            Register<TService, TImplementation>(lifetime, true);
        }

        public void RegisterType(
                    Type serviceType,
            Lifetime lifetime = Lifetime.AlwaysUnique,
            bool keepDefault = false)
        {
            var reuse = lifetime == Lifetime.Singleton
                ? Reuse.Singleton
                : lifetime == Lifetime.Scoped
                    ? Reuse.Scoped
                    : Reuse.Transient;

            _container.Register(
                serviceType, 
                reuse: reuse, 
                setup: Setup.With(allowDisposableTransient:true));

            _container.RegisterInitializer<object>((instance, cxt) =>
            {
                var result = _decoratorService.Decorate(
                    instance,
                    new EventFlow.Configuration.ResolverContext(
                        new DryIocResolver(cxt as IContainer)));

                cxt.UseInstance(result);
            }, r => r.ServiceType == serviceType);
        }
    }
}