namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Basarisiz olay kutusu filtre yuzeyinde disariya acilan alan adlari.
/// </summary>
// islevi: DataTable izin listesindeki tel alanlarini ve entity property yollarini derleme guvenli sabitler olarak tanimlar.
// sistemdeki gorevi: Filtre DTO'su ile repository sorgu motoru arasindaki kararli alan adi sozlesmesidir.
public static class FailedEventInboxFilterFields
{
    public const string MessageId = "messageId";
    public const string EventName = "eventName";
    public const string OwnerModuleName = "ownerModuleName";
    public const string OwnerEnvironmentName = "ownerEnvironmentName";
    public const string IsProcessed = "isProcessed";
    public const string RetryCount = "retryCount";
    public const string NotBefore = "notBefore";
    public const string CreationTime = "creationTime";

    public static class PropertyPaths
    {
        public const string MessageId = "MessageId";
        public const string EventName = "EventName";
        public const string OwnerModuleName = "OwnerModuleName";
        public const string OwnerEnvironmentName = "OwnerEnvironmentName";
        public const string IsProcessed = "IsProcessed";
        public const string RetryCount = "RetryCount";
        public const string NotBefore = "NotBefore";
        public const string CreationTime = "CreationTime";
    }
}
