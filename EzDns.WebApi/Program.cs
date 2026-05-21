using EzDns.WebApi;
using EzDns.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DnsOptions>(builder.Configuration.GetSection("Dns"));
builder.Services.AddSingleton<IRuleRepository>(_ =>
    new JsonRuleRepository(Path.Combine(AppContext.BaseDirectory, "rules.json")));
builder.Services.AddHostedService<EzDnsHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();