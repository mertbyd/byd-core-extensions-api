using System;

namespace Byd.CoreExtensions.Attributes;

/// <summary>
/// Bir controller veya action'in gecerli bir API anahtari gerektirdigini bildiren oznitelik.
/// </summary>
// islevi: Endpoint'i "API anahtari zorunlu" olarak isaretler.
// sistemdeki gorevi: API anahtari dogrulama middleware'inin hangi endpoint'leri koruyacagini okudugu tek kaynak; isaret yoksa endpoint serbesttir.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireApiKeyAttribute : Attribute
{
}
