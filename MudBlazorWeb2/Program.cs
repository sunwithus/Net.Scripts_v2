 //Program.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazorWeb2.Components.EntityFrameworkCore;

using MudBlazor.Services;
using MudBlazorWeb2.Components;
using static MudBlazorWeb2.Components.Pages.ReplicatorOra;
using MudBlazorWeb2.Components.Methods;
using MudBlazorWeb2.Components.Modules.ProcessingDB;

var builder = WebApplication.CreateBuilder(args);

// ����������� ������� ��� ������/������ �������� �� appsettings.json
builder.Services.AddSingleton<SettingsService>();
// ����������� ������� ��� ������/������ �������� �� settingsOper.json
builder.Services.AddSingleton<OperSettingsService>();

// Oracle ��������� "��-���������"
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

// ��������� SignalR
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
// ���������� HttpClient �������� � ���������� ��������
builder.Services.AddHttpClient<WhisperService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15); // ��������� ��������
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




