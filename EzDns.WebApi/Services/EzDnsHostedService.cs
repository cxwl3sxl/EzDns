using DNS.Client.RequestResolver;
using DNS.Server;
using EzDns.Core;
using Microsoft.Extensions.Options;
using System.Net;

namespace EzDns.WebApi.Services;

public class EzDnsHostedService(
    IRuleRepository repository,
    IOptions<DnsOptions> options,
    ILogger<EzDnsHostedService> logger)
    : BackgroundService
{
    private DnsServer? _server;
    private readonly DnsOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rules = await repository.GetAllRules();

        var forwardResolver = new UdpRequestResolver(new IPEndPoint(IPAddress.Parse(_options.ForwardDns), 53));
        var resolver = new CustomRuleResolver(rules, forwardResolver);
        _server = new DnsServer(resolver);

        try
        {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            _server.Listen(_options.DnsPort);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            logger.LogInformation("DNS server started on port {Port}", _options.DnsPort);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to start DNS server on port {Port}. " +
                "Ensure the process is running with administrator privileges (ports below 1024 require admin on Windows).", _options.DnsPort);
            throw;
        }
    }

    public override void Dispose()
    {
        _server?.Dispose();
        base.Dispose();
    }
}