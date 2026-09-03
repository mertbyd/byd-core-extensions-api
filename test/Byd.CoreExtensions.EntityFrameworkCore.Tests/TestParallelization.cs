using Xunit;

// islevi: Bu test derlemesinde xUnit'in test koleksiyonlarini paralel calistirmasini kapatir.
// sistemdeki gorevi: Test host'lari paylasilan onbellekli bellek ici SQLite veritabani kullanir; bu
// veritabanina birden fazla baglantidan ayni anda yazildiginda SQLite "database is locked" hatasi verir.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
