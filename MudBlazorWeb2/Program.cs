//Program.cs
global using MudBlazorWeb2.Components.Methods;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazorWeb2.Components.EntityFrameworkCore;

using MudBlazor.Services;
using MudBlazorWeb2.Components;
using MudBlazorWeb2.Components.Modules.SettingsOper.Services;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.Services;
using MudBlazorWeb2.Components.Modules.Replicator.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервиса для чтения/записи настроек из settingsApp.json и settingsOper.json
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<UserSettingsService>();

builder.Services.AddHostedService<AudioReplicationService>();

builder.Services.AddSingleton<StateService>();
builder.Services.AddSingleton<StateService2>();
builder.Services.AddSingleton<ReplSingletonService>();


// Oracle настройки "по-умолчанию"
var connectionString = builder.Configuration.GetConnectionString("OracleDbConnection");
builder.Services.AddDbContextFactory<OracleDbContext>(options =>
            options.UseOracle(connectionString, providerOptions => providerOptions
                                .CommandTimeout(60)
                                .UseRelationalNulls(true)
                                .MinBatchSize(2))
                 .EnableDetailedErrors(false)
                 .EnableSensitiveDataLogging(false)
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

//#####

// Настройки SignalR
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 512;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
    options.HandshakeTimeout = TimeSpan.FromMinutes(60);
});
//#####

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
// Добавление HttpClient сервисов с настройкой таймаута
builder.Services.AddHttpClient<WhisperService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15); // Установка таймаута
});

builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15);
});

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
//app.Run();




