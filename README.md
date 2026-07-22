# 🎓 ExamSchedular - Sınav Çizelgeleme ve Otomasyon Sistemi

ExamSchedular, üniversite ve fakültelerin sınav dönemlerindeki planlama karmaşasını çözmek için geliştirilmiş modern bir masaüstü (WPF) uygulamasıdır. Öğrenci, ders ve derslik verilerini analiz ederek sıfır çakışma (overlap) prensibiyle optimum sınav programları oluşturur.

Gelişmiş mimarisi (.NET 9, MVVM, N-Tier) ve veritabanı altyapısı (PostgreSQL & EF Core) sayesinde yüksek performanslı, ölçeklenebilir ve güvenli bir kullanım sunar.

---

## ✨ Öne Çıkan Özellikler

*   **🧠 Akıllı Çizelgeleme Algoritması:** Aynı öğrencinin aynı saatte birden fazla sınava girmesini engeller, dersleri günlere dengeli dağıtır ve derslik kapasitelerine göre otomatik yerleştirme yapar.
*   **👥 Rol Bazlı Erişim Kontrolü (RBAC):** 
    *   **Admin:** Tüm departmanları, kullanıcıları ve genel sistemi yönetir.
    *   **Koordinatör:** Sadece atandığı departmanın ders, öğrenci ve sınav programı süreçlerini yönetebilir.
*   **📊 Excel Entegrasyonu (İçe/Dışa Aktarım):** ClosedXML altyapısı sayesinde binlerce satırlık öğrenci ve ders listelerini saniyeler içinde sisteme aktarabilir (Import). Oluşturulan sınav programını Excel formatında dışa aktarabilir (Export).
*   **🏫 Gelişmiş Derslik Yönetimi:** Sınıfların satır/sütun (Rows x Columns) ve oturma düzeni (SeatGroupSize) parametrelerini görsel bir ızgara (SeatGrid) üzerinden planlama imkanı sunar.
*   **🔒 Güvenlik:** Şifreler `BCrypt` algoritması ile hashlenerek saklanır. 

---

## 🛠 Teknoloji Yığını (Tech Stack)

*   **Kullanıcı Arayüzü (UI):** WPF (Windows Presentation Foundation), XAML
*   **Çatı (Framework):** .NET 9.0
*   **Mimari:** MVVM (Model-View-ViewModel), Katmanlı Mimari (N-Tier), Dependency Injection
*   **Veritabanı & ORM:** PostgreSQL, Entity Framework Core 9.0
*   **Kütüphaneler:** 
    *   `ClosedXML` (Excel İşlemleri)
    *   `BCrypt.Net-Next` (Kriptografi)
    *   `QuestPDF` (PDF Çıktı İşlemleri)
    *   `Microsoft.Extensions.DependencyInjection` (DI Container)

---

## 🏗 Proje Mimarisi

Proje, sürdürülebilirliği sağlamak adına 4 ana katmana ayrılmıştır:

1.  **`ExamSchedular.UI` (Sunum Katmanı):** MVVM deseni kullanılarak tasarlanan, görünümlerin (Views) ve kullanıcı etkileşimlerinin (ViewModels) bulunduğu katman.
2.  **`ExamSchedular.Business` (İş Katmanı):** Çizelgeleme algoritması, Excel import servisleri, kimlik doğrulama (Auth) ve veri işleme kurallarının barındığı çekirdek katman.
3.  **`ExamSchedular.Data` (Veri Katmanı):** EF Core `DbContext` sınıfını, Entity (Model) tanımlarını, veritabanı migration'larını barındırır.
4.  **`ExamSchedular.Core` (Çekirdek/Ortak Katman):** Tüm katmanların kullanabileceği altyapı sınıflarını (`RelayCommand`, `ViewModelBase`) içerir.

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [PostgreSQL](https://www.postgresql.org/download/) (Localhost üzerinde çalışır durumda)

### Adım Adım Kurulum

1.  **Repoyu Klonlayın:**
    ```bash
    git clone [https://github.com/KULLANICI_ADINIZ/melihsaygili-exam-schedular.git](https://github.com/KULLANICI_ADINIZ/melihsaygili-exam-schedular.git)
    cd melihsaygili-exam-schedular/ExamSchedular
    ```

2.  **Veritabanı Bağlantısını Ayarlayın:**
    `ExamSchedular.UI` projesi altındaki `appsettings.json` dosyasını açarak `DefaultConnection` bilgisini kendi PostgreSQL yapılandırmanıza göre düzenleyin:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=Yazlab2.1;Username=postgres;Password=sifreniz"
      }
    }
    ```

3.  **Uygulamayı Çalıştırın:**
    Uygulama açılışında (`App.xaml.cs`) **Otomatik Migration ve Seed (Veri Tohumlama)** işlemi devreye girecektir. Manuel olarak terminalden migration çalıştırmanıza gerek yoktur. Veritabanı ve tablolar otomatik oluşacaktır.
    ```bash
    dotnet build
    dotnet run --project ExamSchedular/ExamSchedular.UI.csproj
    ```

---

## 🔑 Varsayılan Giriş Bilgileri

Sistem ilk kez ayağa kalktığında veritabanına otomatik olarak bir Admin hesabı tanımlanır.

*   **E-posta:** `admin@uni.edu.tr`
*   **Şifre:** `Admin123!`

*(Güvenliğiniz için sisteme giriş yaptıktan sonra yeni bir admin oluşturup bu varsayılan hesabı silmeniz/şifresini değiştirmeniz önerilir.)*

---

## 📸 Ekran Görüntüleri ve Modüller

*   **Sınıf Yönetimi (`ClassroomsPage`):** Sınav yapılacak alanların dinamik kapasite hesaplamalarıyla sisteme tanıtılması.
*   **İçe Aktarım (`CoursesImportPage` & `StudentsImportPage`):** Hazır Excel formatındaki listelerin sürükle/bırak ve önizleme desteğiyle DB'ye yazılması.
*   **Çizelgeleme Motoru (`ProgramCreatePage`):** Gün aralığı, sınav slot süresi (örn. 90dk), çatışma kontrolü gibi parametrelerin seçilip takvimin otomatik simüle edilmesi.

---

## 👨‍💻 Geliştirici

Bu proje **Melih Saygılı** tarafından akademik ve profesyonel gereksinimler gözetilerek geliştirilmiştir.

## 📄 Lisans

Bu proje MIT Lisansı altında lisanslanmıştır. Daha fazla bilgi için `LICENSE` dosyasına bakabilirsiniz.
