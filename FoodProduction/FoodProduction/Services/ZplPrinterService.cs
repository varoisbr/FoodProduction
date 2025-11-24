using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using System.Text;

namespace FoodProduction.Services;

public interface IZplPrinterService
{
    Task<string> RenderLabelAsync(LabelTemplate template, string productName, decimal weight, decimal price, DateTime date);

    Task<bool> PrintLabelAsync(string zplContent);

    Task<bool> PrintLabelToFileAsync(string zplContent, string filePath);

    Task<LabelTemplate?> GetTemplateAsync(int templateId);

    Task<List<LabelTemplate>> GetAllTemplatesAsync();
}

public class ZplPrinterService : IZplPrinterService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZplPrinterService> _logger;

    public ZplPrinterService(ApplicationDbContext context, IConfiguration configuration, ILogger<ZplPrinterService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string> RenderLabelAsync(LabelTemplate template, string productName, decimal weight, decimal price, DateTime date)
    {
        string zplContent = template.Content
            .Replace("{ProductName}", productName)
            .Replace("{Weight}", weight.ToString("F2") + "kg")
            .Replace("{Price}", price.ToString("C2"))
            .Replace("{Date}", date.ToString("yyyy-MM-dd HH:mm"));

        return Task.FromResult(zplContent);
    }

    public async Task<bool> PrintLabelAsync(string zplContent)
    {
        try
        {
            var printerIp = _configuration["Printer:IP"] ?? "localhost";
            var printerPort = int.Parse(_configuration["Printer:Port"] ?? "9100");

            using (var client = new TcpClient())
            {
                await client.ConnectAsync(printerIp, printerPort);

                using (var stream = client.GetStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(zplContent);
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }

            _logger.LogInformation("Label printed successfully to {PrinterIp}:{PrinterPort}", printerIp, printerPort);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing label to ZPL printer");
            return false;
        }
    }

    public async Task<bool> PrintLabelToFileAsync(string zplContent, string filePath)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, zplContent);
            _logger.LogInformation("Label saved to file: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving label to file");
            return false;
        }
    }

    public async Task<LabelTemplate?> GetTemplateAsync(int templateId)
    {
        return await _context.LabelTemplates.FindAsync(templateId);
    }

    public async Task<List<LabelTemplate>> GetAllTemplatesAsync()
    {
        return await _context.LabelTemplates.OrderBy(t => t.Name).ToListAsync();
    }
}
