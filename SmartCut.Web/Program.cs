using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SmartCut.Shared.Data;
using SmartCut.Shared.Helpers;
using SmartCut.Shared.Interfaces;
using SmartCut.Shared.Services;
using SmartCut.Web.Components;
using SmartCut.Web.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the SmartCut.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

builder.Logging.AddConsole();

var config = builder.Configuration;
string? connectionString = string.Empty;
// Load configuration based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("Secrets.json", optional: true, reloadOnChange: true);
}
else
{
    builder.Configuration.AddEnvironmentVariables();
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5003);
    });
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
    });
}

connectionString = builder.Configuration["SmartCutConnectionString"] ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.CommandTimeout(10))
    );


builder.Services.AddLocalization();
builder.Services.AddControllers();
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<IExcelService, ClosedXMLService>();
builder.Services.AddScoped<BreadcrumbService>(); 
builder.Services.AddScoped<NotificationService>();

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddHttpClient<ApiClient>(client =>
{
    if (builder.Environment.IsDevelopment())
        client.BaseAddress = new Uri("https://localhost:7110/");
    else
        client.BaseAddress = new Uri("https://smartcut.smartifie.com/"); // Production URL
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SmartCut.Shared._Imports).Assembly);
app.MapControllers();
app.Run();
