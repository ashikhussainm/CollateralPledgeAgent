using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using System.IO;

namespace CollateralPledgeAgent.Services
{
    public class GoogleDriveSettings
    {
        public string FolderId { get; set; }
        public string CredentialsJsonPath { get; set; }
    }

    public class StorageService : IStorageService
    {
        private readonly DriveService _driveService;
        private readonly string _folderId;

        public StorageService(IOptions<GoogleDriveSettings> options)
        {
            var s = options.Value;
            _folderId = s.FolderId;

            GoogleCredential credential;
            using (var stream = new FileStream(s.CredentialsJsonPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                              .CreateScoped(DriveService.Scope.DriveFile);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Bank-CollateralAgent"
            });
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new List<string> { _folderId }
            };

            using var ms = new MemoryStream(fileBytes);
            var request = _driveService.Files.Create(fileMetadata, ms, "application/pdf");
            request.Fields = "id, webViewLink";
            var uploadResult = await request.UploadAsync();
            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw new Exception("Upload to Google Drive failed");
            }

            var uploaded = await _driveService.Files.Get(request.ResponseBody.Id).ExecuteAsync();
            return uploaded.WebViewLink!;
        }
    }
}
