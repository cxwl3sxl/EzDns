using System;
using System.Collections.Generic;
using EzDns.Core.Models;

namespace EzDns.Core;

/// <summary>
/// Deterministic sort key that eliminates priority-equality ambiguity.
/// Sorted (highest → lowest): <see cref="Priority"/> first, then <see cref="Specificity"/>.
/// </summary>
internal struct RuleSortKey : IComparable<RuleSortKey>
{
    public readonly DnsRule Rule;
    public readonly int     Priority;
    public readonly int     Specificity; // int.MaxValue = fixed; auto = suffix length

    public RuleSortKey(DnsRule rule, int priority, int specificity)
    {
        Rule       = rule;
        Priority   = priority;
        Specificity = specificity;
    }

    /// <summary>Higher priority first, then higher specificity.</summary>
    public int CompareTo(RuleSortKey other)
    {
        int c = other.Priority.CompareTo(Priority);
        return c != 0 ? c : other.Specificity.CompareTo(Specificity);
    }
}

/// <summary>
/// Evaluation-order service for DNS rules.
/// </summary>
internal static class RuleSort
{
    /// <summary>
    /// Applies the complete evaluation ordering in a single sort pass:
    /// <list type="number">
    ///   <item><description>Higher priority first (always covers same domain, higher wins).</description></item>
    ///   <item><description>More specific first at the same priority: fixed rules outrank auto rules; among auto, longer wildcard suffix wins.</description></item>
    /// </list>
    ///
    /// Uses <see cref="RuleSortKey"/> (a struct) and <c>List.Sort</c> rather than anonymous-type
    /// projection, eliminating two heap allocations per rule per query (anonymous element + .ToList()).
    ///
    /// Returns an <see cref="Entry"/> list whose <see cref="Entry.LowerPattern"/> already holds the
    /// lower-cased form of each fixed rule's pattern.  The <see cref="CustomRuleResolver.MatchRule"/> hot
    /// loop reads that field — never calling <c>.ToLowerInvariant()</c> inside the per-query loop.
    /// </summary>
    internal static (List<DnsRule> OrderedRules, IReadOnlyList<Entry> Entries) BuildEvaluationOrder(
        IEnumerable<DnsRule> allRules)
    {
        var keys = new List<RuleSortKey>(64);
        foreach (var r in allRules)
        {
            int spec = r.Mode == "fixed"
                ? int.MaxValue
                : r.Pattern.AsSpan() is var p && p.Length > 1 && p[0] == '*'
                    ? p[1..].Length
                    : 0;
            keys.Add(new RuleSortKey(r, r.Priority, spec));
        }

        keys.Sort(); // in-place, zero extra allocation

        var ordered = new List<DnsRule>(keys.Count);
        var entries = new List<Entry> (keys.Count);
        for (int i = 0; i < keys.Count; i++)
        {
            var rule = keys[i].Rule;
            ordered.Add(rule);

            // Pre-normalise pattern once; the hot loop reads Entry.LowerPattern directly.
            // null = auto rule (not compared by name); non-null = fixed rule (already lower-cased).
            string? lower = rule.Mode == "fixed" ? rule.Pattern.ToLowerInvariant() : null;
            entries.Add(new Entry(rule, lower));
        }

        return (ordered, entries);
    }

    /// <summary>A read-only, pre-computed view of a rule used by <see cref="CustomRuleResolver.MatchRule"/>.</summary>
    internal readonly struct Entry(DnsRule Rule, string? LowerPattern)
    {
        public readonly DnsRule Rule       = Rule;
        /// <summary>Non-null when <see cref="DnsRule.Mode"/> is "fixed" — the ready-to-compare lower-cased pattern.</summary>
        public readonly string? LowerPattern = LowerPattern;
    }
}
