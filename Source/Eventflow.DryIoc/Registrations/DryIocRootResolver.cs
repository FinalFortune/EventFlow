using System;
using System.Collections.Generic;
using System.Text;
using DryIoc;
using EventFlow.Configuration;

namespace EventFlow.DryIoc.Registrations
{
    internal class DryIocRootResolver : DryIocScopeResolver, IRootResolver
    {
        public IContainer Container { get; }

        public DryIocRootResolver(IContainer container) : base(container)
        {
            Container = container;
        }

        public override void Dispose()
        {
            base.Dispose();
            Container.Dispose();
        }
    }

}
