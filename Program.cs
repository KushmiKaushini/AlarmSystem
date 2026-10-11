using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using AlarmSystem.Services;
using AlarmSystem.Configuration;
using AlarmSystem.Services.Interfaces;
using AlarmSystem.Services.Implementations;
using AlarmSystem.Services.EmailSender;
using AlarmSystem.Services.EmailReporting;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------------
// 1. Authentication & Azure AD Services
// ------------------------------------------------------------------------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SaveTokens = true;
    options.Scope.Add("User.Read.All");
    options.Scope.Add("User.Read");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    var originalHandler = options.Events?.OnRedirectToIdentityProvider;
    options.Events ??= new OpenIdConnectEvents();
    options.Events.OnRedirectToIdentityProvider = async context =>
    {
        context.ProtocolMessage.Prompt = "select_account";
        if (originalHandler is not null)
            await originalHandler(context);
    };
});

// ------------------------------------------------------------------------
// 2. Authorization Policies
// ------------------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    var authenticatedPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.DefaultPolicy = authenticatedPolicy;
    options.FallbackPolicy = authenticatedPolicy;
});

// ------------------------------------------------------------------------
// 3. Controllers & UI
// ------------------------------------------------------------------------
builder.Services
    .AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// ------------------------------------------------------------------------
// 4. SQL Connection & Dependency Injection (Services)
// ------------------------------------------------------------------------
builder.Services.AddScoped<SqlConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("TMS")));

// App Services Registration
builder.Services.AddScoped<IWarRoomService, WarRoomDashboardService>();
builder.Services.AddSingleton<FaultReportService>(); // ✅ FaultReportService Registered Here
builder.Services.AddScoped<EscalationService>();
builder.Services.AddScoped<FaultService>();
builder.Services.AddScoped<AlarmService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<NewTicketService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<FaultDocketService>();
builder.Services.AddScoped<WorkOrderService>();
builder.Services.AddScoped<WorkGroupService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<FaultDetailsService>();
builder.Services.AddScoped<EventNewService>();
builder.Services.AddScoped<CarrierBusinessSummaryService>();
builder.Services.AddScoped<InboxService>();
builder.Services.AddScoped<PowerAcService>();
builder.Services.AddScoped<EscFaultService>();
builder.Services.AddScoped<CurrentMsanOutageService>();
builder.Services.AddScoped<MsanHistoryService>();
builder.Services.AddScoped<CeaHistoryService>();
builder.Services.AddScoped<AlarmDelayService>();
builder.Services.AddTransient<TroubleTicketService>();
builder.Services.AddScoped<SearchNodeService>();
builder.Services.AddScoped<PowerAlarmService>();
builder.Services.AddScoped<TodayNodeDownService>();
builder.Services.AddScoped<SummaryNetworkFaultService>();
builder.Services.AddScoped<SummaryCarrierFaultService>();
builder.Services.AddScoped<WarRoomDashboardService>();
builder.Services.AddScoped<NodeDownReportService>();
builder.Services.AddScoped<ACMainFailureService>();
builder.Services.AddScoped<IAlarmHistoryEngineerService, AlarmHistoryEngineerService>();
builder.Services.AddScoped<IAlarmHistoryProvinceService, AlarmHistoryProvinceService>();
builder.Services.AddScoped<IAlarmHistoryRegionService, AlarmHistoryRegionService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();
builder.Services.AddScoped<IRpbSummaryService, RpbSummaryService>();
builder.Services.AddScoped<IReportCeaPortDownService, ReportCeaPortDownService>();
builder.Services.AddScoped<ICarrierFaultExportService, CarrierFaultExportService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Email Reporting Services
builder.Services.AddSingleton<IEmailSenderService, EmailSenderService>();
builder.Services.AddHostedService<DailyEmailReportSchedulerService>();

// Configuration
builder.Services.Configure<EmailReportingOptions>(builder.Configuration.GetSection("EmailReporting"));
builder.Services.Configure<StarlinkOptions>(builder.Configuration.GetSection("Starlink"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<AzureGraphService>();
builder.Services.AddScoped<ConfigService>();

// ------------------------------------------------------------------------
// 5. Starlink & External Services
// ------------------------------------------------------------------------
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("starlink");

// ------------------------------------------------------------------------
// 6. Application Pipeline Configuration
// ------------------------------------------------------------------------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Disable Caching Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// MVC Controller Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ------------------------------------------------------------------------
// 7. Dynamic Email Preview Endpoint (Type 1, 2, 3, 4 Supported)
// ------------------------------------------------------------------------
app.MapGet("/preview-email", async (FaultReportService service, int type = 1, DateTime? date = null) =>
{
    var html = await service.GetEmailPreviewHtmlAsync(type, date);
    return Results.Content(html, "text/html");
}).AllowAnonymous();

// ------------------------------------------------------------------------
// 8. Graceful Shutdown Handling (Prevents Port Binding Issues)
// ------------------------------------------------------------------------
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    // Allow in-flight requests to complete
    Thread.Sleep(500);
});

app.Run();