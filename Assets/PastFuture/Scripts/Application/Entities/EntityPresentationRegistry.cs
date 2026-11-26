using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PastFuture.Domain.Entities;

namespace PastFuture.Application.Entities
{
    /// <summary>
    /// Default <see cref="IEntityPresentationRegistry"/>. Holds presentation references only; it is not
    /// WorldState and decides nothing about the entity itself. Not thread-safe (main thread).
    /// </summary>
    public sealed class EntityPresentationRegistry : IEntityPresentationRegistry
    {
        private readonly Dictionary<EntityId, Entry> _byEntity = new Dictionary<EntityId, Entry>();
        private readonly Dictionary<IEntityPresentation, EntityBinding> _byPresentation =
            new Dictionary<IEntityPresentation, EntityBinding>(ReferenceComparer.Instance);

        private long _lastGeneration;

        public int Count => _byEntity.Count;

        public BindResult TryBind(EntityId entityId, IEntityPresentation presentation, out EntityBinding binding)
        {
            if (presentation == null)
            {
                throw new ArgumentNullException(nameof(presentation));
            }

            binding = default;
            if (!entityId.IsValid)
            {
                return BindResult.RejectedInvalidEntityId;
            }

            if (_byPresentation.ContainsKey(presentation))
            {
                return BindResult.RejectedPresentationAlreadyBound;
            }

            if (_byEntity.ContainsKey(entityId))
            {
                return BindResult.RejectedDuplicateEntity;
            }

            binding = new EntityBinding(entityId, ++_lastGeneration);
            _byEntity.Add(entityId, new Entry(presentation, binding));
            _byPresentation.Add(presentation, binding);
            return BindResult.Bound;
        }

        public bool Unbind(EntityBinding binding)
        {
            if (!IsCurrent(binding))
            {
                return false;
            }

            var entry = _byEntity[binding.EntityId];
            _byEntity.Remove(binding.EntityId);
            _byPresentation.Remove(entry.Presentation);
            return true;
        }

        public bool IsCurrent(EntityBinding binding)
        {
            return !binding.IsNone
                   && _byEntity.TryGetValue(binding.EntityId, out var entry)
                   && entry.Binding.BindingGeneration == binding.BindingGeneration;
        }

        public bool TryGetPresentation(EntityId entityId, out IEntityPresentation presentation)
        {
            if (_byEntity.TryGetValue(entityId, out var entry))
            {
                presentation = entry.Presentation;
                return true;
            }

            presentation = null;
            return false;
        }

        public bool TryGetBinding(IEntityPresentation presentation, out EntityBinding binding)
        {
            if (presentation != null && _byPresentation.TryGetValue(presentation, out binding))
            {
                return true;
            }

            binding = default;
            return false;
        }

        public void UnbindAll()
        {
            // Generations keep increasing, so tokens from before stay stale after rebinding.
            _byEntity.Clear();
            _byPresentation.Clear();
        }

        private readonly struct Entry
        {
            public Entry(IEntityPresentation presentation, EntityBinding binding)
            {
                Presentation = presentation;
                Binding = binding;
            }

            public IEntityPresentation Presentation { get; }

            public EntityBinding Binding { get; }
        }

        /// <summary>Identity comparison: Unity objects override Equals/==, the registry must not.</summary>
        private sealed class ReferenceComparer : IEqualityComparer<IEntityPresentation>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public bool Equals(IEntityPresentation x, IEntityPresentation y) => ReferenceEquals(x, y);

            public int GetHashCode(IEntityPresentation obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
