using CollateralPledgeAgent.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace CollateralPledgeAgent.Services
{
    public class GmailSettings
    {
        public string ApplicationName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string UserEmail { get; set; }
    }

    public class PollingSettings
    {
        public int IntervalSeconds { get; set; }
    }

    public class EmailPollingService : BackgroundService, IEmailPollingService
    {
        private readonly ILogger<EmailPollingService> _logger;
        private readonly PollingSettings _polling;
        private readonly GmailSettings _gmailSettings;
        private readonly IOcrService _ocrService;
        private readonly IExtractionService _extractor;
        private readonly ISheetsService _sheets;
        private readonly IStorageService _storage;
        private readonly IEmailSenderService _emailSender;

        public EmailPollingService(
            IOptions<GmailSettings> gOpt,
            IOptions<PollingSettings> pOpt,
            IOcrService ocrService,
            IExtractionService extractor,
            ISheetsService sheets,
            IStorageService storage,
            IEmailSenderService emailSender,
            ILogger<EmailPollingService> logger)
        {
            _gmailSettings = gOpt.Value;
            _polling = pOpt.Value;
            _ocrService = ocrService;
            _extractor = extractor;
            _sheets = sheets;
            _storage = storage;
            _emailSender = emailSender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run once at startup, then every IntervalSeconds
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessGmailAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in polling loop");
                }

                await Task.Delay(_polling.IntervalSeconds * 1000, stoppingToken);
            }
        }

        private async Task ProcessGmailAsync()
        {
            // 1) Build a GoogleCredential from your OAuth2 data
            var cred = await BuildGoogleCredentialAsync();
            var gmail = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred,
                ApplicationName = _gmailSettings.ApplicationName
            });

            // 2) List unread messages
            var request = gmail.Users.Messages.List(_gmailSettings.UserEmail);
            request.LabelIds = new[] { "UNREAD" };
            request.IncludeSpamTrash = false;
            var response = await request.ExecuteAsync();

            if (response.Messages == null || response.Messages.Count == 0)
            {
                _logger.LogInformation("No new Gmail messages.");
                return;
            }

            foreach (var msgId in response.Messages)
            {
                var getReq = gmail.Users.Messages.Get(_gmailSettings.UserEmail, msgId.Id);
                getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                var msg = await getReq.ExecuteAsync();

                // Traverse MIME tree, find PDF attachments
                await HandleGmailMessagePayloadAsync(msg.Payload, gmail, _gmailSettings.UserEmail, msg.Id);

                // Mark as read
                var modReq = new ModifyMessageRequest { RemoveLabelIds = new[] { "UNREAD" } };
                await gmail.Users.Messages.Modify(modReq, _gmailSettings.UserEmail, msg.Id).ExecuteAsync();
            }
        }

        private async Task<GoogleCredential> BuildGoogleCredentialAsync()
        {
            // ── STUB ─────────────────────────────────────────────────────────────────
            // You need to either:
            // 1) Load a service-account JSON: GoogleCredential.FromFile("path-to-service-account.json")
            // 2) Perform OAuth2 flow using ClientId/ClientSecret/RefreshToken
            // For simplicity in a POC, you can do:
            //    return GoogleCredential.FromFile("path-to-service-account.json");
            throw new NotImplementedException("Implement OAuth2 flow or load service-account JSON here.");
        }

        private async Task HandleGmailMessagePayloadAsync(MessagePart payload, GmailService gmail, string userEmail, string messageId)
        {
            if (!string.IsNullOrEmpty(payload.Filename)
                && payload.Filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                var attachPart = payload.Body;
                var attachmentId = attachPart.AttachmentId;
                var attachReq = gmail.Users.Messages.Attachments.Get(userEmail, messageId, attachmentId);
                var attachData = await attachReq.ExecuteAsync();
                var fileBytes = Convert.FromBase64String(attachData.Data.Replace('-', '+').Replace('_', '/'));

                await ProcessFileAsync(fileBytes, payload.Filename, userEmail);
            }

            if (payload.Parts != null)
            {
                foreach (var sub in payload.Parts)
                {
                    await HandleGmailMessagePayloadAsync(sub, gmail, userEmail, messageId);
                }
            }
        }

        private async Task ProcessFileAsync(byte[] fileBytes, string fileName, string senderEmail)
        {
            // 1) OCR
            var rawText = await _ocrService.ExtractRawTextAsync(fileBytes);

            // 2) Extraction
            var rows = await _extractor.ExtractCollateralRowsAsync(rawText);

            // 3) Append to Google Sheets
            await _sheets.AppendRowsAsync(rows);

            // 4) Upload PDF to Google Drive
            var driveLink = await _storage.UploadFileAsync(fileBytes, fileName);

            // 5) Send confirmation email
            var subject = $"[AUTO] Received Collateral Pledge Report: {fileName}";
            var sb = new StringBuilder();
            sb.Append($"<p>We have received your Collateral Pledge Report <b>{fileName}</b>.</p>");
            sb.Append($"<p>Rows ingested: <b>{rows.Count}</b></p>");
            sb.Append($"<p>View archived PDF: <a href=\"{driveLink}\">{driveLink}</a></p>");
            sb.Append($"<p>If you see any discrepancies, please reply to this email.</p>");

            await _emailSender.SendConfirmationAsync(
                toEmail: senderEmail,
                subject: subject,
                bodyHtml: sb.ToString(),
                cc: new List<string> { "risk@yourbank.com" }
            );
        }
    }
}
