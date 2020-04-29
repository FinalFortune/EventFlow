using DryIoc;
using EventFlow.DryIoc.Extensions;
using EventFlow.Configuration;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using NUnit.Framework;

namespace EventFlow.DryIoc.Tests.IntegrationTests
{
    [Category(Categories.Integration)]
    public class DryIocServiceRegistrationIntegrationTests : IntegrationTestSuiteForServiceRegistration
    {
        protected override IEventFlowOptions Options(IEventFlowOptions eventFlowOptions)
        {
            return base.Options(eventFlowOptions
                .UseDryIocContainer(new Container()));
        }

        protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
        {
            return eventFlowOptions
                .CreateResolver();
        }
    }
}

