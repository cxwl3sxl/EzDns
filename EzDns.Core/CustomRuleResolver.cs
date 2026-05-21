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
    private readonly IRuleRepository _repository;
    private readonly IRequestResolver _forwardResolver;

    public CustomRuleResolver(IRuleRepository repository, IRequestResolver forwardResolver)
    {
        _repository = repository;
        _forwardResolver = forwardResolver;
    }

    public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        var rules = await _repository.GetAllRules();
        var sortedRules = rules.OrderByDescending(r => r.Priority).ToList();

        var response = Response.FromRequest(request);

        foreach (var question in request.Questions)
        {
            var matchedRule = MatchRule(sortedRules, question.Name.ToString(), question.Type);
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

    private DnsRule? MatchRule(List<DnsRule> sortedRules, string domain, RecordType type)
    {
        string lowerDomain = domain.ToLowerInvariant();

        foreach (var rule in sortedRules.Where(r => r.IsEnabled))
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
                        int idx = domain.LastIndexOf(suffix, StringComparison.Ordinal);
                        if (idx < 0)
                            return null;
                        string subPrefix = domain.Substring(0, idx);
                        // Remove trailing dot if present
                        if (subPrefix.EndsWith("."))
                        {
                            subPrefix = subPrefix.Substring(0, subPrefix.Length - 1);
                        }
                        var parts = subPrefix.Split('.');
                        // Get the last part which should be the numeric value
                        if (parts.Length >= 1 && int.TryParse(parts[parts.Length - 1], out int lastOctet))
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