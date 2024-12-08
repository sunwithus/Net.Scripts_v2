//Program.cs
global using MudBlazorWeb2.Components.Methods;

using MudBlazor.Services;
using MudBlazorWeb2.Components;
using MudBlazorWeb2.Components.Modules.SettingsOper.Services;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.Services;
using MudBlazorWeb2.Components.EntityFrameworkCore.SqliteModel;
using MudBlazorWeb2.Components.Modules._Shared;
using MudBlazorWeb2.Components.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// settingsApp.json settingsOper.json
builder.Services.AddSingleton<UserSettingsService>();

builder.Services.AddDbContextFactory<SqliteDbContext>();
/*builder.Services.AddDbContextFactory<SqliteDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "todos.db")}")
);*/

//BackgroundService
builder.Services.AddHostedService<ReplBackgroundService>();
builder.Services.AddHostedService<AiBackgroundService>();

builder.Services.AddSingleton<StateService>();
builder.Services.AddSingleton<StateService2>();
//builder.Services.AddSingleton<ProgressService>();
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
app.MapHub<TodoHub>("/todohub");

app.Map("about", () => "About page");
app.Map("todolist", () =>
{
    return "sdsds";
});

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