# Byd.CoreExtensions

Ortak core yeteneklerinin gelistirilecegi ABP DDD module iskeletidir.

## Uretim kaynagi

Iskelet elle yazilmadi. ABP CLI 10.3.0 temiz bir tool dizinine kuruldu ve resmi `module` template'i calistirildi:

```powershell
dotnet tool install Volo.Abp.Cli --version 10.3.0 --tool-path <tool-directory>
<tool-directory>\abp.exe new Byd.CoreExtensions -t module --no-ui -v 10.3.0 -o <output-directory> --skip-installing-libs
```
