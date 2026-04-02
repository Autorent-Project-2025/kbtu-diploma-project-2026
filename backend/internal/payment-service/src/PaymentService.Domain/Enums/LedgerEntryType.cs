namespace PaymentService.Domain.Enums;

public enum LedgerEntryType
{
    BookingPendingCredit,
    BookingPendingReversal,
    BookingPendingRelease,
    BookingAvailableCredit,
    BookingChargeAvailableCredit,
    PayoutAvailableDebit,
    PayoutReservedCredit,
    PayoutReservedRelease,
    PayoutReservedRollback,
    PayoutAvailableReturn
}
