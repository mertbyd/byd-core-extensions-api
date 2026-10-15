# Byd.CoreExtensions

Ortak core yeteneklerinin gelistirilecegi ABP DDD module iskeletidir.

## Uretim kaynagi

Iskelet elle yazilmadi. ABP CLI 10.3.0 temiz bir tool dizinine kuruldu ve resmi `module` template'i calistirildi:

```powershell
dotnet tool install Volo.Abp.Cli --version 10.3.0 --tool-path <tool-directory>
<tool-directory>\abp.exe new Byd.CoreExtensions -t module --no-ui -v 10.3.0 -o <output-directory> --skip-installing-libs
```

## E-posta

- Olaylar `EmailEvents` lookup tablosunda durur; olayi gonderen modul kendi olayini tekil adla seed eder
  (`EmailEventManager.CreateAsync`). Sablon satiri olaya `EmailEventId` ile baglanir.
- Moduller e-postayi `EmailTemplateManager.SendAsync(olayAdi, alici, model)` ile gonderir. Sablon `EmailTemplates`
  tablosundan istegin diline ve kapsamina gore secilir: organizasyon, ust organizasyon, tenant, host. Sablon degiskenleri
  `{{ ad }}` biciminde yazilir ve modelin uye adlariyla eslesir.
- Her olayin host kapsamindaki varsayilan sablonu, olayi kullanan modulun seed'iyle gelir. Tenant ve organizasyon
  yoneticileri olaylari `api/core-extensions/email-events` ucundan gorur, kendi surumlerini
  `api/core-extensions/email-templates` uclarindan acar.
- Gonderici ABP'nin SMTP gondericisidir. Host `Abp.Mailing.DefaultFromAddress`, `Abp.Mailing.DefaultFromDisplayName`,
  `Abp.Mailing.Smtp.Host`, `Abp.Mailing.Smtp.Port`, `Abp.Mailing.Smtp.EnableSsl`, `Abp.Mailing.Smtp.UseDefaultCredentials`,
  `Abp.Mailing.Smtp.UserName` ve `Abp.Mailing.Smtp.Password` ayarlarini verir. Sifre sifreli ayardir; appsettings'e duz
  yazilmaz, ayar yonetiminden girilir.

## Elasticsearch

- Baglanti ABP ayarlarindan gelir: `CoreExtensions.Elasticsearch.IsEnabled`, `.Url`, `.UserName`, `.Password`,
  `.IsTraceMode`. Varsayilan kapalidir; kapaliyken istemci istenirse `CoreExtensions:060001` doner. Degerler
  appsettings'te `Settings` bolumunden ya da ayar yonetiminden global olarak verilir; ayar degisince istemci host yeniden
  baslatilmadan yeniden kurulur.
- Her modul `IElasticsearchRepository<TBelge, TKey>`'i dogrudan enjekte eder ya da `ElasticsearchRepository<TBelge, TKey>`'den
  turetip `IndexName`'i ezer. Yuzey: indeks olusturma/varlik/temizleme, baglanti kontrolu, tekil ve toplu yazma/guncelleme/silme,
  kimlikle, alanla ve tarih araligiyla okuma, sayim, `AnyAsync`, DataTable filtre/siralama/sayfalama ile liste ve sayim, liste ile
  toplami tek istekte donen `SearchAsync` ve sonucu kisa sureli onbellekleyen `CachedSearchAsync`.
- Filtre sozlesmesi veritabani listeleriyle aynidir (`DataTableListInput<TFilter>` + `[DataTable]`). Indekse ozel ek operatorler:
  `notContains`, `match` (kelime bazli tam metin) ve `fuzzy` (yazim hatasina toleransli). `contains` buyuk/kucuk harf duyarsizdir.
- Belge `IMultiTenant` ya da `IMultiOrganization` tasiyorsa her okuma, acik veri filtresine gore yururlukteki tenant ve
  organizasyonla sinirlanir. Yazmalar indeks yenilenene kadar bekler; yazilan kayit hemen okunur.

## Redis

- Host `Redis` bolumunu verir: `IsEnabled`, `Configuration` (StackExchange.Redis baglanti dizesi) ve `KeyPrefix`. Bolum yoksa ya da
  `IsEnabled` `true` degilse dagitik onbellek bellek icinde kalir. Ayni sistemin host'lari ayni `KeyPrefix`'i kullanir ki izin ve
  ayar onbellegi temizligi hepsine ulassin; farkli sistemler farkli on ek kullanir.
- Onbellekli repository'ler (`EfCachedRepository`) bu repository uzerinden yapilan her yazmada listeyi commit sonrasi siler.
  `CoreExtensions.Caching.IsEntityCachingEnabled` kapaliyken okumalar dogrudan tabloya gider.
- `[CachedResponse(grup)]` isaretli action'larin yaniti Redis'te saklanir. Anahtar on ek, modul, tenant, grup, organizasyon, kultur,
  yol ve action girdilerinden kurulur; `IsUserScoped` acikken kullanici da eklenir. `ICachedResponseSource` tasiyan bir entity
  degisince adiyla ayni gruptaki yanitlar commit sonrasi silinir; entity disi bir yoldan degisen veri icin
  `ICachedResponseStore.RemoveGroupAsync` cagrilir. `CoreExtensions.Caching.IsResponseCachingEnabled` kapaliyken ya da
  Redis kapaliyken yanit onbellegi devre disidir.

## Saglik ucu

- `UseCoreExtensions()` `/health` ucunu acar; cevap genel durumu ve veritabani, Redis, RabbitMQ, Elasticsearch yoklamalarini
  JSON olarak doner. Elasticsearch ayarlarda kapaliysa saglikli sayilir; Redis yoklamasi yalniz Redis acikken kaydedilir.
- Host bir yoklamayi ya da ucu `PreConfigure<CoreExtensionsHealthCheckOptions>(...)` ile kapatir.

## Sistem loglari

- Host Program.cs'te logger'i `new LoggerConfiguration().ConfigureSystemLog(builder.Environment.EnvironmentName).CreateBootstrapLogger()`
  ile kurar ve `UseSerilog()`'u parametresiz cagirir.
- `CoreExtensions.SystemLog.IsElasticsearchEnabled` ve Elasticsearch acikken loglar `logs-<indeks>-<ortam>` veri akisina da
  yazilir; indeks bossa host'un modul adi kullanilir. `MinimumLevel`, `IsSelfLogEnabled`, `FailureLogPath` ve
  `RefreshIntervalSeconds` ayarlari da vardir. Sistem log iscisi ayarlari periyodik okur; degisiklik host yeniden baslatilmadan
  uygulanir.
