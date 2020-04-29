using System;
using System.Collections.Generic;
using System.Text;
using DryIoc;
using EventFlow.DryIoc.Registrations;

namespace EventFlow.DryIoc.Extensions
{
    public static class EventFlowOptionsDryIocExtensions
    {
        public static IEventFlowOptions UseDryIocContainer(
            this IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .UseDryIocContainer(new Container());
        }

        public static IEventFlowOptions UseDryIocContainer(
            this IEventFlowOptions eventFlowOptions,
            IContainer container)
        {
            return eventFlowOptions
                .UseServiceRegistration(new DryIocServiceRegistration(container));
        }

        public static IContainer CreateContainer(
            this IEventFlowOptions eventFlowOptions,
            bool validateRegistrations = true)
        {
            var rootResolver = eventFlowOptions.CreateResolver(validateRegistrations);
            var dryiocRootResolver = rootResolver as DryIocRootResolver;
            if (dryiocRootResolver == null)
            {
                throw new InvalidOperationException(
                    "Make sure to configure the EventFlowOptions for DryIoc using the .UseDryIocContainer(...)");
            }

            return dryiocRootResolver.Container;
        }
    }
}
