using EzDns.Core.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace EzDns.WebApi.Services;

public interface IRuleRepository
{
    Task<List<DnsRule>> GetAllRules();
    Task SaveRules(List<DnsRule> rules);
    Task AddRule(DnsRule rule);
    Task UpdateRule(string pattern, DnsRule updatedRule);
    Task DeleteRule(string pattern);
}

public class JsonRuleRepository : IRuleRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonRuleRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<DnsRule>> GetAllRules()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
                return new List<DnsRule>();

            var json = await File.ReadAllTextAsync(_filePath);
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            return JsonSerializer.Deserialize<List<DnsRule>>(json, options) ?? new List<DnsRule>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveRules(List<DnsRule> rules)
    {
        await _semaphore.WaitAsync();
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(rules, options);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task AddRule(DnsRule rule)
    {
        var rules = await GetAllRules();
        rules.Add(rule);
        await SaveRules(rules);
    }

    public async Task UpdateRule(string pattern, DnsRule updatedRule)
    {
        var rules = await GetAllRules();
        var index = rules.FindIndex(r => r.Pattern.Equals(pattern, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            rules[index] = updatedRule;
            await SaveRules(rules);
        }
    }

    public async Task DeleteRule(string pattern)
    {
        var rules = await GetAllRules();
        rules.RemoveAll(r => r.Pattern.Equals(pattern, StringComparison.OrdinalIgnoreCase));
        await SaveRules(rules);
    }
}