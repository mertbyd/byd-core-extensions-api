using System;

namespace Byd.CoreExtensions.AntiDuplicate;

/// <summary>
/// Idempotency imzasinin cache kaydini tasir.
/// </summary>
// islevi: Cache anahtarinin olusturulma zamanini saklar.
// sistemdeki gorevi: Ayni istegin TTL suresince tekrar gonderildigini anlamak icin dagitik cache degeridir.
public sealed class IdempotencyRecord
{
    public DateTime CreationTime { get; set; }
}
