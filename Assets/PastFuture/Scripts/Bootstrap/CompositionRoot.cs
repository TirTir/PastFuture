using System;
using PastFuture.Application.Entities;
using PastFuture.Application.Lifecycle;
using PastFuture.UnityAdapters.Entities;

namespace PastFuture.Bootstrap
{
    /// <summary>
    /// Manual constructor DI composition root (Handover 16-1, 16-17; ADR-481, ADR-482, ADR-569).
    /// The only place that constructs services. Planned order (16-17):
    /// App registry/save/profile → menu → Session → Loop → Scene → bindings → simulation.
    /// M1 creates the Application scope and the entity presentation registry (ADR-573); the scene
    /// binder starts only when <see cref="StartSceneBindings"/> is called from the running game.
    /// </summary>
    public sealed class CompositionRoot : IDisposable
    {
        private SceneEntityBinder _sceneEntityBinder;

        public CompositionRoot()
        {
            ApplicationScope = new LifetimeScope(ScopeKind.Application);
            EntityPresentations = new EntityPresentationRegistry();
        }

        public LifetimeScope ApplicationScope { get; }

        /// <summary>Entity ↔ presentation bindings. Not WorldState.</summary>
        public IEntityPresentationRegistry EntityPresentations { get; }

        /// <summary>
        /// Binds the views of every loaded scene and of each scene loaded later. Separate from the
        /// constructor so that creating a root in EditMode tests does not touch the open scenes.
        /// Calling it again does nothing.
        /// </summary>
        public void StartSceneBindings()
        {
            if (_sceneEntityBinder != null)
            {
                return;
            }

            _sceneEntityBinder = ApplicationScope.Register(new SceneEntityBinder(EntityPresentations));
        }

        public void Dispose()
        {
            ApplicationScope.Dispose();
        }
    }
}
