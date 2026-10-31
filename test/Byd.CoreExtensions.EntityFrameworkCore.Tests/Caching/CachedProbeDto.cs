using System;

namespace Byd.CoreExtensions.Caching;

// islevi: Test servisinin sonucunu ve kacinci gercek calismada uretildigini tasir.
// sistemdeki gorevi: Onbellekten donen sonucun yeniden hesaplanmadigini olcmeye yarar.
public class CachedProbeDto
{
    public Guid Id { get; set; }
    public int CallNumber { get; set; }
}
