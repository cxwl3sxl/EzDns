using System.IO;
using EzDns.Core.Models;
using EzDns.WebApi;
using EzDns.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// ── Service control commands (install/uninstall/status/start/stop) ────────
if (ServiceCommands.HandleCommand(args))
    return;
// ─────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DnsOptions>(builder.Configuration.GetSection("Dns"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.AddSingleton<IRuleRepository>(sp =>
    new JsonRuleRepository(Path.Combine(AppContext.BaseDirectory, "rules.json")));

// ── JWT Authentication ───────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken            = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = "EzDns",    // mirrors Auth:Jwt:Issuer in appsettings.json
        ValidAudience            = "EzDnsClient",
        IssuerSigningKey         = new SymmetricSecurityKey(
                                        System.Text.Encoding.UTF8.GetBytes(
                                            "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_IN_PRODUCTION")),
        ClockSkew                = TimeSpan.FromSeconds(30),
    };
});

builder.Services.AddAuthorization();
// ─────────────────────────────────────────────────────────────────────────

builder.Services.AddHostedService<EzDnsHostedService>();

// Enable running as Windows Service or Linux systemd service
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "EzDns";
});

builder.Host.UseSystemd();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();