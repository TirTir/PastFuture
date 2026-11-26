using System;
using System.Collections.Generic;
using PastFuture.Application.Entities;

namespace PastFuture.UnityAdapters.Entities
{
    /// <summary>
    /// Binds every <see cref="EntityView"/> of a scene when it loads (Handover 16-17 "Scene → bindings").
    /// Created by the composition root and disposed with its scope. Walks the scene's own hierarchy,
    /// including inactive objects; it does not look up services.
    /// </summary>
    public sealed class SceneEntityBinder : IDisposable
    {
        private readonly IEntityPresentationRegistry _registry;
        private readonly List<EntityView> _buffer = new List<EntityView>();
        private bool _disposed;

        public SceneEntityBinder(IEntityPresentationRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    BindScene(scene);
                }
            }
        }

        /// <summary>Binds the scene's views and returns how many are bound afterwards.</summary>
        public int BindScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SceneEntityBinder));
            }

            var bound = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                _buffer.Clear();
                root.GetComponentsInChildren(true, _buffer);
                foreach (var view in _buffer)
                {
                    if (view.BindTo(_registry) == BindResult.Bound)
                    {
                        bound++;
                    }
                }
            }

            _buffer.Clear();
            return bound;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            BindScene(scene);
        }
    }
}
