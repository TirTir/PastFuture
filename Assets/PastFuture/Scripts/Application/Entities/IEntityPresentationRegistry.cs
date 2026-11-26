using PastFuture.Domain.Entities;

namespace PastFuture.Application.Entities
{
    /// <summary>
    /// One presentation per entity (Handover 16-17 IEntityPresentationRegistry; ADR-493 title).
    /// Lookups go both ways. Main thread only.
    /// </summary>
    public interface IEntityPresentationRegistry
    {
        int Count { get; }

        /// <summary>
        /// Binds <paramref name="presentation"/> to <paramref name="entityId"/>. A second presentation for an
        /// entity that is already bound is rejected and the first keeps authority (no duplicate
        /// interaction/physics authority, 16-17).
        /// </summary>
        BindResult TryBind(EntityId entityId, IEntityPresentation presentation, out EntityBinding binding);

        /// <summary>Removes the binding only if it is still current. A stale token changes nothing.</summary>
        bool Unbind(EntityBinding binding);

        /// <summary>True only for the latest bind of that entity that has not been unbound.</summary>
        bool IsCurrent(EntityBinding binding);

        bool TryGetPresentation(EntityId entityId, out IEntityPresentation presentation);

        bool TryGetBinding(IEntityPresentation presentation, out EntityBinding binding);

        /// <summary>Drops every binding (e.g. loop reset). All earlier tokens stop being current.</summary>
        void UnbindAll();
    }
}
