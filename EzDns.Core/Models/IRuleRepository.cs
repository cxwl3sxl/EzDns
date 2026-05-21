using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzDns.Core.Models;

public interface IRuleRepository
{
    Task<List<DnsRule>> GetAllRules();
    Task SaveRules(List<DnsRule> rules);
    Task AddRule(DnsRule rule);
    Task UpdateRule(string pattern, DnsRule updatedRule);
    Task DeleteRule(string pattern);
}
