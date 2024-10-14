using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudBlazorWeb2.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
//
var oracleSettingsFilePath = Path.Combine(AppContext.BaseDirectory, "oracleSettings.json");
var oracleSettings = new OracleSettings();
oracleSettings.LoadSettingsFromJson(oracleSettingsFilePath);
builder.Services.AddSingleton(oracleSettings);

builder.Services.AddSingleton<DatabaseService>();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
// Добавление HttpClient сервисов с настройкой таймаута
builder.Services.AddHttpClient<WhisperService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(30); // Установка таймаута
});

builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10); // Установка таймаута
});

builder.Services.AddScoped<AudioProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run("http://0.0.0.0:555");


