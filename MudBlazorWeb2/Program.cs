//Program.cs
global using MudBlazorWeb2.Components.Methods;

using MudBlazor.Services;
using MudBlazorWeb2.Components;
using MudBlazorWeb2.Components.EntityFrameworkCore.SqliteModel;
using MudBlazorWeb2.Components.Modules._Shared;
using MudBlazorWeb2.Components.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<SqliteDbContext>();

//BackgroundService
builder.Services.AddHostedService<AiBackgroundService>();

builder.Services.AddSingleton<IDbContextFactory, DbContextFactory>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 512;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
    options.HandshakeTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<WhisperService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15); //
});

builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15);
});

SelectDb.Configure(builder.Configuration); //for static Toolkit.cs

var app = builder.Build();

app.UseRouting();
app.UseAntiforgery();
app.MapHub<ReplicatorHub>("/replicatorhub");
app.MapHub<TodoHub>("/todohub");

app.Map("about", () => "About page");
app.Map("todolist", () =>
{
    return "sdsds";
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run("http://0.0.0.0:555");