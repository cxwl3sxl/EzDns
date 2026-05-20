using DNS.Protocol;

namespace EzDns.Core.Models;

public class DnsRule
{
    public string Pattern { get; set; } = string.Empty;
    public RecordType Type { get; set; } = RecordType.A;
    public string Mode { get; set; } = "fixed";
    public string Value { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 0;
    public string IpBase { get; set; } = string.Empty;
}