using System.IO.Compression;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Pea.Meter.Helpers;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class LogViewModel(MemoryLogSink sink, ILogger<LogViewModel> logger, IConfiguration configuration)
    : ObservableObject
{
    [ObservableProperty] private string logContent = string.Empty;


    [RelayCommand]
    private void Clear()
    {
        sink.Clear();
        LogContent = string.Empty;
    }

    [RelayCommand]
    private void Refresh()
    {
        LogContent = sink.GetContent();
    }

    [RelayCommand]
    private async Task RefreshFromFile()
    {
        var logDir = Path.Combine(FileSystem.AppDataDirectory, "logs");
        var today = DateTime.Today.ToString("yyyyMMdd");
        var logPath = Path.Combine(logDir, $"pea{today}.log");

        if (!File.Exists(logPath) && Directory.Exists(logDir))
        {
            logPath = Directory.GetFiles(logDir, "pea*.log")
                .OrderByDescending(f => f)
                .FirstOrDefault() ?? logPath;
        }

        if (File.Exists(logPath))
        {
            await using var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            LogContent = await reader.ReadToEndAsync();
        }
        else
        {
            LogContent = "No log file found.";
        }
    }

    [RelayCommand]
    private async Task SendLog()
    {
        try
        {
            logger.LogInformation("Sending log email");

            var password = configuration["MailLoggPassword"] ?? Environment.GetEnvironmentVariable("MailLoggPassword");
            if (password is null)
            {
                logger.LogError("MailLoggPassword is not set in the app. Both Secret and environment variable is null");
                PopupHelper.ShowErrorPopup("ERROR!!!", "No password set for sending log file by email... Aborting.");

                return;
            }

            var logDir = Path.Combine(FileSystem.AppDataDirectory, "logs");
            var today = DateTime.Today.ToString("yyyyMMdd");
            var logPath = Path.Combine(logDir, $"pea{today}.log");

            if (!File.Exists(logPath) && Directory.Exists(logDir))
            {
                logPath = Directory.GetFiles(logDir, "pea*.log")
                    .OrderByDescending(f => f)
                    .FirstOrDefault() ?? logPath;
            }

            if (!File.Exists(logPath))
            {
                PopupHelper.ShowErrorPopup("ERROR!!!", "No log file found to send.");
                return;
            }

            var zipPath = Path.Combine(FileSystem.CacheDirectory, "pea_log.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                var entry = zip.CreateEntry(Path.GetFileName(logPath));
                await using var entryStream = entry.Open();
                await using var logStream =
                    new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                await logStream.CopyToAsync(entryStream);
            }

            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("Pea", "jompa67th@gmail.com"));
            mail.To.Add(new MailboxAddress("", "jompa67th@gmail.com"));
            mail.Subject = "Pea Log File";
            var body = new BodyBuilder { TextBody = "Log file attached." };
            await body.Attachments.AddAsync(zipPath);
            mail.Body = body.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (sender, cert, chain, errors) =>
                errors == System.Net.Security.SslPolicyErrors.None ||
                (chain?.ChainStatus.All(s =>
                     s.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags
                         .RevocationStatusUnknown ||
                     s.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags
                         .OfflineRevocation) ??
                 false);
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("jompa67th@gmail.com", password);
            await smtp.SendAsync(mail);
            await smtp.DisconnectAsync(true);

            logger.LogInformation("Log email sent");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send log email");
            PopupHelper.ShowErrorPopup("ERROR!!!", "Failed to send email with log file.");
        }
    }
}