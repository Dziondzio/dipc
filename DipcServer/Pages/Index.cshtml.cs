using Microsoft.AspNetCore.Mvc.RazorPages;
using DipcServer.Data;

namespace DipcServer.Pages;

public class IndexModel : PageModel
{
    private readonly ReportStore _store;

    public IndexModel(ReportStore store)
    {
        _store = store;
    }

    public IReadOnlyList<MachineSummary> Machines { get; private set; } = Array.Empty<MachineSummary>();

    public async Task OnGet(CancellationToken cancellationToken)
    {
        Machines = await _store.ListMachinesAsync(cancellationToken);
    }
}
