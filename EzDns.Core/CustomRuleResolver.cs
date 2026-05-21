using DNS;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client.RequestResolver;
using EzDns.Core.Models;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EzDns.Core;

public class CustomRuleResolver : IRequestResolver
{
    private readonly List<DnsRule> _rules;
    private readonly IRequestResolver _forwardResolver;

    public CustomRuleResolver(List<DnsRule> rules, IRequestResolver forwardResolver)
    {
        _rules = rules.OrderByDescending(r => r.Priority).ToList();
        _forwardResolver = forwardResolver;
    }

    public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        var response = Response.FromRequest(request);

        foreach (var question in request.Questions)
        {
            var matchedRule = MatchRule(question.Name.ToString(), question.Type);
            if (matchedRule != null)
            {
                var record = CreateRecord(question, matchedRule, question.Name.ToString().ToLowerInvariant());
                if (record != null)
                {
                    response.AnswerRecords.Add(record);
                    response.ResponseCode = ResponseCode.NoError;
                    return response;
                }
            }
        }

        return await _forwardResolver.Resolve(request, cancellationToken);
    }

    private DnsRule? MatchRule(string domain, RecordType type)
    {
        string lowerDomain = domain.ToLowerInvariant();

        foreach (var rule in _rules.Where(r => r.IsEnabled))
        {
            if (rule.Type != type && rule.Type != RecordType.ANY)
                continue;

            switch (rule.Mode)
            {
                case "fixed":
                    if (lowerDomain.Equals(rule.Pattern.ToLowerInvariant()) ||
                        lowerDomain.EndsWith("." + rule.Pattern.ToLowerInvariant()))
                        return rule;
                    break;

                case "auto":
                    if (rule.Pattern.StartsWith("*"))
                    {
                        string suffix = rule.Pattern[1..];
                        if (lowerDomain.EndsWith(suffix) || lowerDomain.Equals(suffix[1..]))
                            return rule;
                    }
                    break;
            }
        }

        return null;
    }

    private IResourceRecord? CreateRecord(Question question, DnsRule rule, string domain)
    {
        switch (rule.Mode)
        {
            case "fixed":
                if (IPAddress.TryParse(rule.Value, out IPAddress? ip))
                {
                    return new IPAddressResourceRecord(question.Name, ip, TimeSpan.FromMinutes(5));
                }
                break;

            case "auto":
                if (rule.Pattern.StartsWith("*"))
                {
                    string suffix = rule.Pattern[1..];
                    if (domain.EndsWith(suffix) || domain.Equals(suffix[1..]))
                    {
                        int idx = domain.LastIndexOf(suffix);
                        if (idx < 0)
                            return null;
                        string subPrefix = domain.Substring(0, idx);
                        var parts = subPrefix.Split('.');
                        if (parts.Length >= 1 && int.TryParse(parts[0], out int lastOctet))
                        {
                            string ipBase = string.IsNullOrEmpty(rule.IpBase) ? "192.168.0." : rule.IpBase;
                            var generatedIp = IPAddress.Parse($"{ipBase}{lastOctet}");
                            return new IPAddressResourceRecord(question.Name, generatedIp, TimeSpan.FromMinutes(5));
                        }
                    }
                }
                break;
        }

        return null;
    }
}