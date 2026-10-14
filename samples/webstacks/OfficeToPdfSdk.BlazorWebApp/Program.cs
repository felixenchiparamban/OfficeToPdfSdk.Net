using FelixELabs.OfficeToPdfSdk.BlazorWebApp.Components;
using FelixELabs.OfficeToPdfSdk.BlazorWebApp.Services;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<HubOptions>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Register the conversion service client for HTTP-based document conversion
// Configuration: ConversionService:BaseUrl (default: http://localhost:5000)
var conversionServiceUrl = builder.Configuration["ConversionService:BaseUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient<IConversionServiceClient, ConversionServiceClient>(client =>
{
    client.BaseAddress = new Uri(conversionServiceUrl);
    client.Timeout = TimeSpan.FromMinutes(10);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

