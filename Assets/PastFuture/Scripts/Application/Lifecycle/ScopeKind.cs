namespace PastFuture.Application.Lifecycle
{
    /// <summary>
    /// Lifetime scopes from Handover 16-1 (ADR-010, ADR-395).
    /// Session: time-loop experience that continues across program restarts.
    /// Loop: the actual current world. Scene: presentation lifetime.
    /// Prediction: isolated temporary computation.
    /// </summary>
    public enum ScopeKind
    {
        Application,
        Profile,
        Session,
        Loop,
        Scene,
        Prediction,
    }
}
