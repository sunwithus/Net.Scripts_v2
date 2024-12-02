//Program.cs
global using MudBlazorWeb2.Components.Methods;

using MudBlazor.Services;
using MudBlazorWeb2.Components;
using MudBlazorWeb2.Components.Modules.SettingsOper.Services;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.Services;
using MudBlazorWeb2.Components.Modules.Replicator.Services;
using MudBlazorWeb2.Components.Modules._Shared;

var builder = WebApplication.CreateBuilder(args);

// settingsApp.json settingsOper.json
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<UserSettingsService>();

//BackgroundService
builder.Services.AddHostedService<ReplBackgroundService>();
builder.Services.AddHostedService<AiBackgroundService>();

builder.Services.AddSingleton<StateService>();
builder.Services.AddSingleton<StateService2>();
//builder.Services.AddSingleton<ReplSingletonService>(); //for old vervion

// SignalR
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
// HttpClient
builder.Services.AddHttpClient<WhisperService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15); //
});

builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(15);
});

var app = builder.Build();

////////////////////////////////////////////
app.UseRouting();
app.UseAntiforgery();
app.MapHub<ReplicatorHub>("/replicatorhub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run("http://0.0.0.0:555");
//app.Run();




