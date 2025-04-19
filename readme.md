📧 Oracle Tabanlı Mail Kuyruğu Gönderici
Bu proje, Oracle veritabanındaki e-posta kuyruğunu kontrol edip SMTP ayarlarına göre sıradaki e-postaları gönderen basit bir .NET Core konsol uygulamasıdır. Mail gönderimi sonrasında ilgili kayıt güncellenerek tekrar gönderilmesi engellenir.

🚀 Özellikler
Oracle veritabanı bağlantısı

SMTP ayarlarını veritabanından dinamik olarak çekme

Blob (CLOB/BLOB) içerikli HTML e-posta desteği

Gönderilen e-postaların durum bilgisini güncelleme

60 saniyelik döngüyle sürekli kuyruk kontrolü

🛠️ Kullanılan Teknolojiler
.NET Core / C#

Oracle Managed Data Access (ODP.NET)

MailKit & MimeKit (SMTP desteği için)

Microsoft.Extensions.Configuration (JSON yapılandırma desteği)

🧩 Kurulum
1. Bağımlılıkları yükleyin
Projeyi açtıktan sonra aşağıdaki NuGet paketlerini yükleyin:

bash
Copy
Edit
dotnet add package Oracle.ManagedDataAccess
dotnet add package MailKit
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
2. appsettings.json dosyasını oluşturun
Aşağıdaki formatta bir appsettings.json dosyası oluşturun:

json
Copy
Edit
{
  "Database": {
    "ConnectionString": "User Id=KULLANICI_ADI;Password=ŞİFRE;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));"
  }
}
SMTP ayarları, veritabanında SYS_MAIL_SABITLERI tablosundan dinamik olarak çekilmektedir.

3. Veritabanı Gereksinimleri
SMTP Ayarları Tablosu:

Alan	Açıklama
BELEDIYE_MAIL_ADRESI	SMTP kullanıcı adresi
YETKILI_KULLANICI_SIFRESI	SMTP kullanıcı şifresi
MAIL_SERVER_ADI	SMTP sunucu adresi
SMTP_PORT	SMTP port numarası
Mail Kuyruğu Tablosu:

Alan	Açıklama
ID	Mail kayıt ID
ALICILAR	Alıcı e-posta adresleri
MAIL_BASLIK	E-posta başlığı
MAIL_TEXT	HTML içeriği (CLOB/BLOB)
DURUM	Gönderim durumu (NULL = bekliyor, 1 = gönderildi)
▶️ Uygulamayı Başlat
bash
Copy
Edit
dotnet run
Uygulama 60 saniyede bir e-posta kuyruğunu kontrol eder ve gönderilecek e-postaları işler.

📎 Notlar
Mail içeriği HTML olarak işlenmektedir. MAIL_TEXT alanı HTML formatında yazılmalıdır.

Çoklu alıcı desteği için ALICILAR alanında virgül , ayracı kullanılabilir.

SMTP bağlantısı STARTTLS kullanılarak sağlanmaktadır.

🧑‍💻 Katkı Sağlama
Pull request'ler açıktır. Hataları bildirmek veya yeni özellikler önermek için Issue açabilirsin.
