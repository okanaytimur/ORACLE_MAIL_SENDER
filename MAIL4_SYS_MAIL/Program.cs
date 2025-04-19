using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;


class Program
{
    private static string _connectionString;
    private static string _smtpUser;
    private static string _smtpPass;
    private static string _smtpHost;
    private static int _smtpPort;

    static async Task Main()
    {
        Console.WriteLine("Uygulama başlatılıyor...");
        LoadConfiguration();
        await LoadMailSettingsAsync();

        while (true)
        {
            await CheckMailQueueAndSendMailAsync();
            await Task.Delay(60000); // 60 saniye bekle
        }
    }

    static void LoadConfiguration()
    {
        Console.WriteLine("Yapılandırma dosyası yükleniyor...");
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _connectionString = config["Database:ConnectionString"];
        Console.WriteLine("Veritabanı bağlantı dizesi yüklendi.");
    }

    static async Task LoadMailSettingsAsync()
    {
        Console.WriteLine("SMTP bilgileri veritabanından alınıyor...");
        using (var conn = new OracleConnection(_connectionString))
        {
            try
            {
                await conn.OpenAsync();
                Console.WriteLine("Veritabanına bağlantı sağlandı.");
                string query = "SELECT BELEDIYE_MAIL_ADRESI, YETKILI_KULLANICI_SIFRESI, MAIL_SERVER_ADI, SMTP_PORT FROM SYS_MAIL_SABITLERI WHERE REFERANS = 1 FETCH FIRST 1 ROWS ONLY";

                using (var cmd = new OracleCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        _smtpUser = reader.GetString(0);
                        _smtpPass = reader.GetString(1);
                        _smtpHost = reader.GetString(2);
                        _smtpPort = reader.GetInt32(3);

                        Console.WriteLine($"SMTP Kullanıcı: {_smtpUser}");
                        Console.WriteLine($"SMTP Sunucu: {_smtpHost}");
                        Console.WriteLine($"SMTP Port: {_smtpPort}");
                        Console.WriteLine("SMTP bilgileri başarıyla yüklendi.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SMTP bilgileri alınırken hata oluştu: " + ex.Message);
            }
        }
    }

    static async Task CheckMailQueueAndSendMailAsync()
    {
        Console.WriteLine("E-posta kuyruğu kontrol ediliyor...");
        using (var conn = new OracleConnection(_connectionString))
        {
            try
            {
                await conn.OpenAsync();
                Console.WriteLine("Bağlantı sağlandı.");
                string query = "SELECT ID,ALICILAR,MAIL_BASLIK,MAIL_TEXT FROM SYS_MAIL_GONDERILENLER WHERE DURUM IS NULL";

                using (var cmd = new OracleCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(0);
                        string aliciAdres = reader.GetString(1);
                        string mailBaslik = reader.GetString(2);

                        Stream blobStream = reader.GetStream(3);
                        string mailMetin;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            blobStream.CopyTo(ms);
                            mailMetin = Encoding.UTF8.GetString(ms.ToArray());
                            Console.WriteLine(mailMetin);
                        }
                        
                        Console.WriteLine($"E-posta gönderiliyor: {mailBaslik} - {aliciAdres}- {mailMetin}");

                        bool success = await SendEmailAsync(aliciAdres, mailBaslik, mailMetin);
                        if (success)
                        {
                            await UpdateMailStatusAsync(conn, id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }
    }

    static async Task UpdateMailStatusAsync(OracleConnection conn, int id)
    {
        Console.WriteLine($"E-posta durumu güncelleniyor: {id}");
        string updateQuery = "UPDATE SYS_MAIL_GONDERILENLER SET DURUM=1 WHERE ID = :id";
        using (var updateCmd = new OracleCommand(updateQuery, conn))
        {
            updateCmd.Parameters.Add(new OracleParameter("id", id));
            await updateCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"E-posta durumu güncellendi: {id}");
        }
    }

    static async Task<bool> SendEmailAsync(string recipient, string subject, string body)
    {
        Console.WriteLine($"E-posta gönderme işlemi başlatıldı: {recipient}");
        try
        {
            var message = new MimeMessage();
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;
            message.From.Add(new MailboxAddress("", _smtpUser));
            message.To.Add(new MailboxAddress("", recipient));
            message.Subject = subject;
            message.Body = bodyBuilder.ToMessageBody(); ;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUser, _smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            Console.WriteLine($"E-posta başarıyla gönderildi: {recipient}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("E-posta gönderme hatası: " + ex.Message);
            return false;
        }
    }
}
