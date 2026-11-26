namespace PastFuture.Application.Entities
{
    public enum BindResult
    {
        Bound = 0,

        /// <summary>The entity already has a current presentation; the new one gets no authority (UNITY_T024).</summary>
        RejectedDuplicateEntity = 1,

        /// <summary>This presentation is already bound (to the same or another entity).</summary>
        RejectedPresentationAlreadyBound = 2,

        RejectedInvalidEntityId = 3,
    }
}
