using System;
using PastFuture.Application.Lifecycle;

namespace PastFuture.Bootstrap
{
    /// <summary>
    /// Manual constructor DI composition root (Handover 16-1, 16-17; ADR-481, ADR-482, ADR-569).
    /// The only place that constructs services. Planned order (16-17):
    /// App registry/save/profile → menu → Session → Loop → Scene → bindings → simulation.
    /// M0 creates only the Application scope; nothing else in that order exists yet.
    /// </summary>
    public sealed class CompositionRoot : IDisposable
    {
        public CompositionRoot()
        {
            ApplicationScope = new LifetimeScope(ScopeKind.Application);
        }

        public LifetimeScope ApplicationScope { get; }

        public void Dispose()
        {
            ApplicationScope.Dispose();
        }
    }
}
