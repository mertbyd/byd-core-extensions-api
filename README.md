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
