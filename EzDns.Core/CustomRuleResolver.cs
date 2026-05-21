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
        var allRules = await _repository.GetAllRules();
        var sortedRules = RuleSort.BuildEvaluationOrder(allRules);

        var response = Response.FromRequest(request);

        foreach (var question in request.Questions)
        {
            var domainName = question.Name.ToString();
            var lowerDomain = domainName.ToLowerInvariant();

            var match = MatchRule(sortedRules, lowerDomain, question.Type);
            if (match.MatchedRule is not null)
            {
                var record = CreateRecord(question, match.MatchedRule, match.MatchedSubDomain!, lowerDomain);
                if (record is not null)
                {
                    response.AnswerRecords.Add(record);
                    response.ResponseCode = ResponseCode.NoError;
                    return response;
                }
            }
        }

        return await _forwardResolver.Resolve(request, cancellationToken);
    }

    /// <summary>Result of a rule match used by both the resolver and CreateRecord.</summary>
    /// <param name="MatchedRule">The DNS rule that matched the query (null if none matched).</param>
    /// <param name="MatchedSubDomain">
    ///   The leading (leftmost) subdomain fragments that triggered the match,
    ///   e.g. "a.84" for rule *.84.pj.cn matching a.84.pj.cn.
    ///   Null for fixed rules or when no match occurred.
    /// </param>
    private sealed record RuleMatch(DnsRule MatchedRule, string? MatchedSubDomain);

    private RuleMatch MatchRule(List<DnsRule> sortedRules, string lowerDomain, RecordType type)
    {
        foreach (var rule in sortedRules.Where(r => r.IsEnabled))
        {
            if (rule.Type != type && rule.Type != RecordType.ANY)
                continue;

            switch (rule.Mode)
            {
                case "fixed":
                    if (lowerDomain.Equals(rule.Pattern.ToLowerInvariant()) ||
                        lowerDomain.EndsWith("." + rule.Pattern.ToLowerInvariant()))
                        return new RuleMatch(rule, null);
                    break;

                case "auto":
                    if (rule.Pattern.StartsWith("*"))
                    {
                        string suffix = rule.Pattern[1..];
                        if (lowerDomain.EndsWith(suffix) || lowerDomain.Equals(suffix[1..]))
                        {
                            // matchedSubDomain = leading segments before the suffix (e.g. "a.84")
                            string stripped = lowerDomain.Substring(0, lowerDomain.Length - suffix.Length);
                            return new RuleMatch(rule, stripped);
                        }
                    }
                    break;
            }
        }

        return new RuleMatch(null, null);
    }

    private IResourceRecord? CreateRecord(Question question, DnsRule rule, string matchedSubDomain, string lowerDomain)
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
                    // Extract the last dot-separated segment from matchedSubDomain
                    // e.g. "a.84" → last segment = "84"
                    // e.g. "a"    → last segment = "a"
                    string cleanSub = matchedSubDomain;
                    if (cleanSub.EndsWith("."))
                        cleanSub = cleanSub[..^1];

                    var parts = cleanSub.Split('.');
                    if (parts.Length >= 1 && int.TryParse(parts[^1], out int lastOctet))
                    {
                        string ipBase = string.IsNullOrEmpty(rule.IpBase) ? "192.168.0." : rule.IpBase;
                        var generatedIp = IPAddress.Parse($"{ipBase}{lastOctet}");
                        return new IPAddressResourceRecord(question.Name, generatedIp, TimeSpan.FromMinutes(5));
                    }
                }
                break;
        }

        return null;
    }
}