using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Application.Exceptions;

/// <summary>
/// Thrown when the AI inspection service refuses to accept a session
/// because too many photos were unusable. The controller layer converts
/// this into an HTTP 400 with a structured body the client UI can render
/// per-slot (which slot failed and why). The booking remains Active —
/// the client must upload better photos.
/// </summary>
public sealed class BookingCompletionAiRejectedException : Exception
{
    public BookingCompletionAiRejectedException(
        int validPhotosCount,
        IReadOnlyList<DamageEvaluationRejectedPhoto> rejectedPhotos)
        : base("Completion photos were rejected by AI inspection.")
    {
        ValidPhotosCount = validPhotosCount;
        RejectedPhotos = rejectedPhotos;
    }

    public int ValidPhotosCount { get; }

    public IReadOnlyList<DamageEvaluationRejectedPhoto> RejectedPhotos { get; }
}
