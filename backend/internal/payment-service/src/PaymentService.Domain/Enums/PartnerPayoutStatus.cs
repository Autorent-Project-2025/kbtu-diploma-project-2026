namespace PaymentService.Domain.Enums;

public enum PartnerPayoutStatus
{
    Requested,
    Processing,
    Paid,
    Failed,
    Canceled
}
