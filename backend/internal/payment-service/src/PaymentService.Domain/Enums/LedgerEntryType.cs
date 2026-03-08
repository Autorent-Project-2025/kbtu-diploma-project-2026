namespace PaymentService.Domain.Enums;

public enum LedgerEntryType
{
    BookingPendingCredit,
    BookingPendingReversal,
    BookingPendingRelease,
    BookingAvailableCredit,
    PayoutAvailableDebit,
    PayoutReservedCredit,
    PayoutReservedRelease,
    PayoutReservedRollback,
    PayoutAvailableReturn
}
