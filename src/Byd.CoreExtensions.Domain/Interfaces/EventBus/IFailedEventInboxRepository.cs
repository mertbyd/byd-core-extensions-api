using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities.EventBus;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Basarisiz olay kutusu aggregate'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: Kalici kayit yeteneklerine kapsamli ve zamani gelmis yeniden deneme sorgusunu ekler.
// sistemdeki gorevi: Basarisiz olay domain zinciri ile EF Core uygulamasi arasindaki veri erisim siniridir.
public interface IFailedEventInboxRepository : IExtendedRepository<FailedEventInbox>
{
    // Kendi kapsamimizda, zamani gelmis ve henuz basarilamamis kayitlari en eskiden baslayarak getirir.
    Task<List<FailedEventInbox>> GetRetryableListAsync(
        string ownerModuleName,
        string ownerEnvironmentName,
        DateTime now,
        int maxCount,
        CancellationToken cancellationToken = default);
}
