namespace BookingService.Application.Interfaces.Integrations
{
    public interface IDamageEvaluationClient
    {
        /// <summary>
        /// Sends the five slot-labelled booking-completion photos to the AI
        /// inspection service and returns a structured assessment.
        ///
        /// Never throws for infrastructure failures — on timeout / 5xx /
        /// connection reset the method returns an assessment with
        /// <see cref="DamageEvaluationStatus.Error"/> or
        /// <see cref="DamageEvaluationStatus.Unavailable"/> so the caller
        /// can fall open and still create a ticket. It will only throw for
        /// genuinely unexpected conditions (e.g. misconfiguration).
        /// </summary>
        Task<DamageEvaluationAssessment> InspectSessionAsync(
            DamageEvaluationRequest request,
            CancellationToken cancellationToken = default);
    }

    public sealed record DamageEvaluationRequest(
        int PartnerCarId,
        string CarBrand,
        string CarModel,
        string CarColor,
        FileUploadPayload FrontPhoto,
        FileUploadPayload BackPhoto,
        FileUploadPayload SideLeftPhoto,
        FileUploadPayload SideRightPhoto,
        FileUploadPayload InteriorPhoto);

    public enum DamageEvaluationStatus
    {
        // AI returned a usable result (OK or DAMAGES_FOUND).
        Ok,

        // AI rejected the session because too many photos were invalid.
        // The caller should surface the rejected-photo list to the user
        // and refuse to create the ticket.
        InvalidSession,

        // AI did respond but with an application-level error we can
        // display (malformed response, 4xx that isn't InvalidSession, etc.).
        Error,

        // AI could not be reached — timeout, 5xx, connection refused.
        // Fail-open: create the ticket anyway so the manager can decide
        // manually.
        Unavailable,
    }

    public enum DamageEvaluationVerdict
    {
        Ok,
        DamagesFound,
        InvalidSession,
    }

    public sealed record DamageEvaluationDamage(
        string Type,
        double Confidence,
        IReadOnlyList<int> BoundingBox,
        string? Slot,
        string? SourceFile);

    public sealed record DamageEvaluationRejectedPhoto(
        string? Slot,
        string FileName,
        int Step,
        string Reason,
        IReadOnlyList<string> Details);

    public sealed record DamageEvaluationAssessment(
        DamageEvaluationStatus Status,
        DamageEvaluationVerdict? Verdict,
        int ValidPhotosCount,
        IReadOnlyList<DamageEvaluationDamage> Damages,
        IReadOnlyList<DamageEvaluationRejectedPhoto> RejectedPhotos,
        DateTimeOffset ProcessedAtUtc,
        string? ErrorMessage);
}
