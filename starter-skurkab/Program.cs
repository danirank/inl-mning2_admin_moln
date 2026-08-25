// Program.cs — startpunkten för Skurk AB-portalen
// Samma mönster som MinGram — Blazor + HttpClient mot ditt API

using SkurkAB.Components;

var builder = WebApplication.CreateBuilder(args);

// Registrera Blazor med InteractiveServer-rendering
// C#-koden körs på servern, UI uppdateras i realtid via WebSocket
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient konfigurerad med bas-URL från appsettings.json
builder.Services.AddHttpClient("SkurkApi", client =>
{
    var apiUrl = builder.Configuration["ApiUrl"]
        ?? throw new InvalidOperationException("ApiUrl saknas i appsettings.json");
    client.BaseAddress = new Uri(apiUrl);
});

// v35 — CORS-notering:
// När portalen och API:t körs på olika domäner i Azure
// (t.ex. frontend på https://skurkab-ui.azurewebsites.net och API på https://skurkab-api.azurewebsites.net)
// måste API:t tillåta anrop från frontend-URL:en — annars blockerar webbläsaren svaren.
// Det kallas CORS (Cross-Origin Resource Sharing) och konfigureras på API:ts App Service i Azure.
// Vi går igenom det den här veckan — Skurk AB godkänner inga obehöriga anslutningar.

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
