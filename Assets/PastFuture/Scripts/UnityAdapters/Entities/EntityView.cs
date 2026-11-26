using PastFuture.Application.Entities;
using PastFuture.Domain.Entities;

namespace PastFuture.UnityAdapters.Entities
{
    /// <summary>
    /// Marks a GameObject as the presentation of one entity (Handover 16-17 Entity binding).
    /// The ID is authored data; the registry is handed in by <see cref="SceneEntityBinder"/> (Manual DI,
    /// ADR-569). A view whose bind was rejected (duplicate ID) stays unbound and has no authority (UNITY_T024).
    /// Destroying the view only unbinds it: it is not the entity's death (16-17).
    /// </summary>
    [UnityEngine.DisallowMultipleComponent]
    public sealed class EntityView : UnityEngine.MonoBehaviour, IEntityPresentation
    {
        private const string LogPrefix = "[PastFuture.UnityAdapters]";

        [UnityEngine.SerializeField] private long entityId;

        private IEntityPresentationRegistry _registry;
        private EntityBinding _binding;

        public EntityId EntityId => entityId > 0 ? new EntityId(entityId) : EntityId.None;

        public string DebugName => name;

        /// <summary>Current binding token; <c>default</c> when not bound.</summary>
        public EntityBinding Binding => IsBound ? _binding : default;

        /// <summary>True while this view holds the entity's current binding.</summary>
        public bool IsBound => _registry != null && _registry.IsCurrent(_binding);

        /// <summary>
        /// Binds to <paramref name="registry"/>. Binding again while still bound does nothing.
        /// A rejection is logged as an error: two views for one entity is a content or spawn bug.
        /// </summary>
        public BindResult BindTo(IEntityPresentationRegistry registry)
        {
            if (registry == null)
            {
                throw new System.ArgumentNullException(nameof(registry));
            }

            if (ReferenceEquals(registry, _registry) && IsBound)
            {
                return BindResult.Bound;
            }

            var result = registry.TryBind(EntityId, this, out var binding);
            if (result == BindResult.Bound)
            {
                _registry = registry;
                _binding = binding;
                return result;
            }

            var holder = registry.TryGetPresentation(EntityId, out var current) ? current.DebugName : "-";
            UnityEngine.Debug.LogError(
                $"{LogPrefix} Entity bind rejected ({result}): {EntityId} on '{name}', current holder '{holder}'.", this);
            return result;
        }

        private void OnDestroy()
        {
            // Unbind only our own, still-current binding; a stale token is ignored by the registry.
            if (_registry != null)
            {
                _registry.Unbind(_binding);
                _registry = null;
                _binding = default;
            }
        }
    }
}
