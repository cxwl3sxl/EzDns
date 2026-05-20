using DNS.Client.RequestResolver;
using DNS.Server;
using EzDns.Core;
using EzDns.WebApi.Services;
using Microsoft.Extensions.Options;
using System.Net;

namespace EzDns.WebApi.Services;

public class EzDnsHostedService : BackgroundService
{
    private DnsServer? _server;
    private readonly IRuleRepository _repository;
    private readonly DnsOptions _options;

    public EzDnsHostedService(IRuleRepository repository, IOptions<DnsOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rules = await _repository.GetAllRules();
        var forwardResolver = new UdpRequestResolver(new IPEndPoint(IPAddress.Parse(_options.ForwardDns), 53));
        var resolver = new CustomRuleResolver(rules, forwardResolver);
        _server = new DnsServer(resolver);
        await _server.Listen();
    }

    public override void Dispose()
    {
        _server?.Dispose();
        base.Dispose();
    }
}