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
        var (sortedRules, entries) = RuleSort.BuildEvaluationOrder(allRules);

        var response = Response.FromRequest(request);

        foreach (var question in request.Questions)
        {
            var domainName = question.Name.ToString();
            var lowerDomain = domainName.ToLowerInvariant();

            var match = MatchRule(entries, lowerDomain, question.Type);
            if (match.MatchedRule is not null)
            {
                var record = CreateRecord(question, match.MatchedRule, match.MatchedSubDomain, lowerDomain);
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

    /// <summary>
    /// Result of a rule match consumed by both the resolver and CreateRecord.
    /// </summary>
    private readonly struct MatchInfo(DnsRule MatchedRule, string MatchedSubDomain)
    {
        public readonly DnsRule  MatchedRule     = MatchedRule;
        public readonly string MatchedSubDomain = MatchedSubDomain;
        public static readonly MatchInfo None = default;
    }

    private MatchInfo MatchRule(IReadOnlyList<RuleSort.Entry> entries, string lowerDomain, RecordType type)
    {
        foreach (var entry in entries)
        {
            var rule = entry.Rule;
            if (!rule.IsEnabled) continue;
            if (rule.Type != type && rule.Type != RecordType.ANY) continue;

            switch (rule.Mode)
            {
                case "fixed":
                    string? fixedPattern = entry.LowerPattern;
                    if (fixedPattern is null) continue;
                    if (lowerDomain.Equals(fixedPattern, StringComparison.Ordinal) ||
                        lowerDomain.EndsWith("." + fixedPattern, StringComparison.Ordinal))
                        return new MatchInfo(rule, string.Empty);
                    break;

                case "auto":
                    ReadOnlySpan<char> pat = rule.Pattern.AsSpan();
                    if (pat.Length > 1 && pat[0] == '*')
                    {
                        ReadOnlySpan<char> suffix = pat[1..];
                        if (lowerDomain.AsSpan().EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            // matchedSubDomain = leading segments before the suffix (e.g. "a.84")
                            string stripped = lowerDomain.Substring(0, lowerDomain.Length - suffix.Length);
                            return new MatchInfo(rule, stripped);
                        }
                    }
                    break;
            }
        }

        return MatchInfo.None;
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
                    // matchedSubDomain is guaranteed to be non-null here (MatchRule only reaches this
                    // branch for auto-mode rules and always returns a populated MatchedSubDomain).

                    // Clean trailing dot first: "a.84." → "a.84"
                    int td = matchedSubDomain.Length - 1;
                    string sub = matchedSubDomain[td] == '.' ? matchedSubDomain[..td] : matchedSubDomain;

                    // Extract the LAST dot-separated segment using LastIndexOf — avoids allocating string[].
                    // "a.84"   → lastIdx=1  → segment="84"
                    // "84"     → lastIdx=-1 → segment="84"
                    int lastIdx = sub.LastIndexOf('.');
                    string lastSegment = lastIdx >= 0 ? sub.Substring(lastIdx + 1) : sub;

                    if (int.TryParse(lastSegment, out int lastOctet))
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