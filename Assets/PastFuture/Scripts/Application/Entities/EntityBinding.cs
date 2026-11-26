using System;
using PastFuture.Domain.Entities;

namespace PastFuture.Application.Entities
{
    /// <summary>
    /// Token for one bind of an entity to a presentation (Handover 16-17 <c>EntityBindingContext</c>, subset).
    /// M1 carries only the entity and its <see cref="BindingGeneration"/>. Session/Loop/Definition/
    /// ExecutionGeneration/Asset/Region are added when those exist (M2+). A callback that carries a token
    /// whose generation is no longer current must be ignored (16-17, ADR-494 title).
    /// </summary>
    public readonly struct EntityBinding : IEquatable<EntityBinding>
    {
        public EntityBinding(EntityId entityId, long bindingGeneration)
        {
            EntityId = entityId;
            BindingGeneration = bindingGeneration;
        }

        public EntityId EntityId { get; }

        /// <summary>Unique per bind within a registry, increasing. 0 = no binding.</summary>
        public long BindingGeneration { get; }

        public bool IsNone => BindingGeneration == 0;

        public bool Equals(EntityBinding other) =>
            EntityId == other.EntityId && BindingGeneration == other.BindingGeneration;

        public override bool Equals(object obj) => obj is EntityBinding other && Equals(other);

        public override int GetHashCode() => (EntityId.GetHashCode() * 397) ^ BindingGeneration.GetHashCode();

        public override string ToString() => $"{EntityId}@gen{BindingGeneration}";
    }
}
