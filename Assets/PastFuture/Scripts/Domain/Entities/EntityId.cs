using System;

namespace PastFuture.Domain.Entities
{
    /// <summary>
    /// Stable identity of a world entity (Handover 16-2: every entity has a stable ID; ADR-008, ADR-018
    /// titles only). Positive values only; <see cref="None"/> (0) means "no entity".
    /// The split between content-fixed IDs and per-loop dynamic IDs (ADR-018) and the loop-qualified
    /// reference <c>LoopEntityRef(SessionId, LoopId, EntityId)</c> (16-15) come with the M2 IdAllocator.
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>, IComparable<EntityId>
    {
        public static readonly EntityId None = default;

        public EntityId(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "EntityId must be positive.");
            }

            Value = value;
        }

        public long Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(EntityId other) => Value == other.Value;

        public override bool Equals(object obj) => obj is EntityId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(EntityId other) => Value.CompareTo(other.Value);

        public override string ToString() => IsValid ? $"Entity#{Value}" : "Entity#None";

        public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);

        public static bool operator !=(EntityId left, EntityId right) => !left.Equals(right);
    }
}
