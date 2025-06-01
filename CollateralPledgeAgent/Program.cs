using CollateralPledgeAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure strongly-typed settings
builder.Services.Configure<GmailSettings>(builder.Configuration.GetSection("Gmail"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<GoogleSheetsSettings>(builder.Configuration.GetSection("GoogleSheets"));
builder.Services.Configure<GoogleDriveSettings>(builder.Configuration.GetSection("GoogleDrive"));
builder.Services.Configure<PollingSettings>(builder.Configuration.GetSection("Polling"));

// 2) Register all services
builder.Services.AddSingleton<IOcrService, OcrService>();
builder.Services.AddSingleton<IExtractionService, ExtractionService>();
builder.Services.AddSingleton<ISheetsService, SheetsService>();
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<IEmailSenderService, EmailSenderService>();

// 3) Email polling service (Gmail only)
builder.Services.AddHostedService<EmailPollingService>();

// 4) Add Controllers + Swagger (optional)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5) (Optional) Swagger + health endpoint
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();
