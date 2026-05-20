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
        await _repository.AddRule(rule);
        return Ok();
    }

    [HttpPut("{pattern}")]
    public async Task<ActionResult> UpdateRule(string pattern, [FromBody] DnsRule rule)
    {
        await _repository.UpdateRule(pattern, rule);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteRule([FromQuery] string pattern)
    {
        await _repository.DeleteRule(pattern);
        return Ok();
    }
}