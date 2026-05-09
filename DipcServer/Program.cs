using System.Security.Cryptography;
using System.Text;
using DipcServer.Data;
using DipcServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var dbPath = builder.Configuration["Dipc:DatabasePath"];
if (string.IsNullOrWhiteSpace(dbPath))
{
    dbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "dipc.sqlite");
}

builder.Services.AddSingleton(new ReportStore(dbPath));

var app = builder.Build();

await app.Services.GetRequiredService<ReportStore>().InitializeAsync(CancellationToken.None);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapPost("/api/reports", async (HttpRequest request, ReportStore store, PcReport report, CancellationToken cancellationToken) =>
{
    var expectedKey = app.Configuration["Dipc:ApiKey"];
    if (!string.IsNullOrWhiteSpace(expectedKey) && !string.Equals(expectedKey.Trim(), "CHANGE_ME", StringComparison.OrdinalIgnoreCase))
    {
        var got = request.Headers["X-Api-Key"].ToString();
        if (!IsApiKeyValid(got, expectedKey))
        {
            return Results.Unauthorized();
        }
    }

    var receivedAtUtc = DateTimeOffset.UtcNow;
    await store.InsertAsync(report, receivedAtUtc, cancellationToken);
    return Results.Ok(new { ok = true, receivedAtUtc });
});

app.MapRazorPages();

app.Run();

static bool IsApiKeyValid(string provided, string expected)
{
    if (string.IsNullOrEmpty(provided))
    {
        return false;
    }

    var a = Encoding.UTF8.GetBytes(provided.Trim());
    var b = Encoding.UTF8.GetBytes(expected.Trim());
    return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
}
