using CollateralPledgeAgent.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Options;
using System.IO;

namespace CollateralPledgeAgent.Services
{
    public class GoogleSheetsSettings
    {
        public string SpreadsheetId { get; set; }
        public string CredentialsJsonPath { get; set; }
    }

    public class SheetsService : ISheetsService
    {
        private readonly Google.Apis.Sheets.v4.SheetsService _sheetsClient;
        private readonly string _spreadsheetId;

        public SheetsService(IOptions<GoogleSheetsSettings> options)
        {
            var s = options.Value;
            _spreadsheetId = s.SpreadsheetId;

            GoogleCredential credential;
            using (var stream = new FileStream(s.CredentialsJsonPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                              .CreateScoped(Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets);
            }

            _sheetsClient = new Google.Apis.Sheets.v4.SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Bank-CollateralAgent"
            });
        }

        public async Task AppendRowsAsync(IList<CollateralRow> rows)
        {
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>>()
            };

            foreach (var r in rows)
            {
                valueRange.Values.Add(new List<object>
                {
                    r.MemberId,
                    r.ReportDate.ToString("yyyy-MM-dd"),
                    r.Cusip,    
                    r.CollateralType,
                    r.MarketValue,
                    r.HaircutPercent,
                    r.EligibleAdvanceAmount
                });
            }

            var request = _sheetsClient.Spreadsheets.Values.Append(valueRange, _spreadsheetId, "Sheet1!A:G");
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();
        }
    }
}
