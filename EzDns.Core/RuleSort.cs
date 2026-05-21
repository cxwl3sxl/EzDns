using System;
using System.Collections.Generic;
using System.Linq;
using EzDns.Core.Models;

internal static class RuleSort
{
    /// <summary>
    /// Sorts rules into deterministic evaluation order:
    ///   1. Descending by <see cref="DnsRule.Priority"/> so higher numbers win.
    ///   2. Within equal priorities, fixed rules before auto rules.
    ///   3. Among auto rules at the same priority, longest suffix wins (more specific = more specific domain segment).
    /// This guarantees that for three rules at priority 0 —
    ///   fixed: b.pj.cn, auto: *.84.pj.cn, auto: *.pj.cn —
    /// *.84.pj.cn is always evaluated before *.pj.cn regardless of the order in rules.json.
    /// </summary>
    internal static List<DnsRule> BuildEvaluationOrder(IEnumerable<DnsRule> allRules)
    {
        return allRules
            .Select(r =>
            {
                string suffix = r.Mode == "auto" && r.Pattern.StartsWith("*")
                    ? (r.Pattern.Length > 1 ? r.Pattern[1..] : string.Empty)
                    : (r.Mode == "fixed" ? r.Pattern.ToLowerInvariant() : string.Empty);

                // Specificity key: fixed = very high, auto with long suffix = higher
                int specificityKey = r.Mode == "fixed"
                    ? 999999                  // fixed rules always win in their priority tier
                    : suffix.Length;          // auto rules: longer suffix = more specific

                return new { Rule = r, SuffixLen = specificityKey };
            })
            .OrderByDescending(x => x.Rule.Priority)
            .ThenByDescending(x => x.SuffixLen)
            .Select(x => x.Rule)
            .ToList();
    }
}
