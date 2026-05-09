using System.Text.Json;
using DipcServer.Data;
using DipcServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DipcServer.Pages;

public class ReportModel : PageModel
{
    private readonly ReportStore _store;

    public ReportModel(ReportStore store)
    {
        _store = store;
    }

    [BindProperty(SupportsGet = true)]
    public string MachineId { get; set; } = "";

    public StoredReport? Stored { get; private set; }
    public PcReport? Report { get; private set; }
    public string JsonPretty { get; private set; } = "";

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(MachineId))
        {
            return RedirectToPage("/Index");
        }

        Stored = await _store.GetLatestAsync(MachineId, cancellationToken);
        if (Stored is null)
        {
            return NotFound();
        }

        try
        {
            Report = JsonSerializer.Deserialize<PcReport>(Stored.Json);
            JsonPretty = JsonSerializer.Serialize(JsonDocument.Parse(Stored.Json), new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            JsonPretty = Stored.Json;
        }

        return Page();
    }

    public static string FormatBytes(ulong? bytes)
    {
        if (bytes is null)
        {
            return "";
        }

        const double k = 1024.0;
        var b = (double)bytes.Value;
        if (b < k) return $"{bytes.Value} B";
        if (b < k * k) return $"{b / k:0.##} KB";
        if (b < k * k * k) return $"{b / (k * k):0.##} MB";
        if (b < k * k * k * k) return $"{b / (k * k * k):0.##} GB";
        return $"{b / (k * k * k * k):0.##} TB";
    }
}

