using EzDns.Core.Models;
using EzDns.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EzDns.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRuleRepository _repository;

    public RulesController(IRuleRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<DnsRule>>> GetAllRules()
    {
        var rules = await _repository.GetAllRules();
        return Ok(rules);
    }

    [HttpPost]
    public async Task<ActionResult> AddRule([FromBody] DnsRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Pattern))
            return BadRequest("Pattern is required");

        if (!IsValidDnsPattern(rule.Pattern))
            return BadRequest("Invalid pattern format. Use format like 'example.com' or '*.example.com'");

        await _repository.AddRule(rule);
        return Ok();
    }

    [HttpPut("{pattern}")]
    public async Task<ActionResult> UpdateRule(string pattern, [FromBody] DnsRule rule)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return BadRequest("Pattern is required");

        if (!IsValidDnsPattern(pattern))
            return BadRequest("Invalid pattern format. Use format like 'example.com' or '*.example.com'");

        await _repository.UpdateRule(pattern, rule);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteRule([FromQuery] string pattern)
    {
        await _repository.DeleteRule(pattern);
        return Ok();
    }

    private bool IsValidDnsPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var trimmed = pattern.Trim();
        if (trimmed.Contains(" "))
            return false;

        if (trimmed.StartsWith("*"))
        {
            var suffix = trimmed.TrimStart('*').TrimStart('.');
            return !string.IsNullOrWhiteSpace(suffix) && suffix.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-');
        }

        return trimmed.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-');
    }
}