using System;
using EventFlow.DryIoc.Registrations;
using EventFlow.Configuration;
using EventFlow.TestHelpers.Suites;
using NUnit.Framework;
using EventFlow.TestHelpers;

namespace EventFlow.DryIoc.Tests.UnitTests
{
    [Category(Categories.Unit)]
    public class DryIocServiceRegistrationTests : TestSuiteForServiceRegistration
    {
        protected override IServiceRegistration CreateSut()
        {
            return new DryIocServiceRegistration();
        }
    }
}
