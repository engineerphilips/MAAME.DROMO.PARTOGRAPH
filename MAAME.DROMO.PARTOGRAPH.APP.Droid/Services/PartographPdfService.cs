using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public interface IPartographPdfService
    {
        Task<string> GenerateAndSavePartographPdfAsync(Guid partographId, string patientName);
    }

    public class PartographPdfService : IPartographPdfService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public PartographPdfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Get API URL from Preferences (same as sync service)
            _apiBaseUrl = Preferences.Get("SyncApiUrl", "https://localhost:7193");
        }

        public async Task<string> GenerateAndSavePartographPdfAsync(Guid partographId, string patientName)
        {
            try
            {
                // Call API to generate PDF
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Partographs/{partographId}/pdf");
                response.EnsureSuccessStatusCode();

                // Read PDF bytes
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                // Generate filename
                var fileName = $"Partograph_{patientName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                // Save to device's Downloads folder
                var filePath = await SavePdfToDownloadsAsync(pdfBytes, fileName);

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate partograph PDF: {ex.Message}", ex);
            }
        }

        private async Task<string> SavePdfToDownloadsAsync(byte[] pdfBytes, string fileName)
        {
#if ANDROID
            // Android-specific implementation
            var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            var filePath = Path.Combine(downloadsPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // Notify media scanner to make file visible in Downloads
            var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
            mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(filePath)));
            Android.App.Application.Context.SendBroadcast(mediaScanIntent);

            return filePath;
#elif IOS || MACCATALYST
            // iOS-specific implementation
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
#elif WINDOWS
            // Windows-specific implementation
            var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadsPath = Path.Combine(downloadsPath, "Downloads");
            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
#else
            // Fallback for other platforms
            var tempPath = FileSystem.CacheDirectory;
            var filePath = Path.Combine(tempPath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
#endif
        }
    }
}
