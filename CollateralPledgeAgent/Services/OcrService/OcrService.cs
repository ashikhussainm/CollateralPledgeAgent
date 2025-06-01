using Microsoft.AspNetCore.Cors.Infrastructure;
using System.IO;
using Tesseract;

namespace CollateralPledgeAgent.Services
{
    public class OcrService : IOcrService
    {
        public OcrService() { }
        /// <summary>
        /// Extracts raw text from a byte array representing an image file (PNG or single-page PDF converted to PNG).
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        public async Task<string> ExtractRawTextAsync(byte[] fileBytes)
        {
            // 1) Save the byte[] to a temp file (assuming PNG or single-page PDF converted to PNG)
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            await File.WriteAllBytesAsync(tempPath, fileBytes);

            // 2) Initialize Tesseract (ensure your tessdata folder is at ./tessdata)
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(tempPath);
            using var page = engine.Process(img);
            var text = page.GetText();

            // 3) Clean up
            File.Delete(tempPath);

            return text;
        }
    }
}
