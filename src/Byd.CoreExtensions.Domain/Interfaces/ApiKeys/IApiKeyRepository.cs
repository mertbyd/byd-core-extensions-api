using Byd.CoreExtensions.Entities;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// API anahtari aggregate'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: ApiKey CRUD ve sorgu yeteneklerini Domain katmanina soyutlar.
// sistemdeki gorevi: Anahtar okuyan taraf ile EF Core uygulamasi arasindaki veri erisim siniridir.
// Anahtarlar tek tek degil toplu okundugu icin ayri bir tekil arama metodu tasimaz; goruntuyu
// besleyen okuma miras gelen GetListAsync ile yapilir.
public interface IApiKeyRepository : IExtendedRepository<ApiKey>
{
}
