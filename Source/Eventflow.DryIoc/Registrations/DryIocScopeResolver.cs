using System;
using System.Collections.Generic;
using System.Text;
using EventFlow.Configuration;
using DryIoc;

namespace EventFlow.DryIoc.Registrations
{
    internal class DryIocScopeResolver : DryIocResolver, IScopeResolver
    {
        private readonly global::DryIoc.IResolverContext _resolverContext;

        public DryIocScopeResolver(global::DryIoc.IResolverContext cxt) : base(cxt as global::DryIoc.IContainer)
        {

            _resolverContext = cxt;
        }

        public IScopeResolver BeginScope()
        {
            return new DryIocScopeResolver(_resolverContext.OpenScope());
        }

        public virtual void Dispose()
        {
            _resolverContext.Dispose();
        }
    }
}
