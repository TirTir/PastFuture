using PastFuture.Domain.Entities;

namespace PastFuture.Application.Entities
{
    /// <summary>
    /// Something in the presentation layer that shows one entity (a GameObject view, later a pooled
    /// instance). Engine-free so the registry stays in Application. It holds no authoritative state.
    /// </summary>
    public interface IEntityPresentation
    {
        EntityId EntityId { get; }

        /// <summary>Name for logs and debugging only.</summary>
        string DebugName { get; }
    }
}
