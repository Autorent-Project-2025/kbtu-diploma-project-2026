namespace AutoRent.Messaging.Contracts;

public sealed record PartnerCarSearchDocumentChanged(
    int PartnerCarId,
    string ChangeType);
