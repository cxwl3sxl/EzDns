namespace EzDns.WebApi;

public class DnsOptions
{
    public string ForwardDns { get; set; } = "8.8.8.8";
    public int DnsPort { get; set; } = 53;
}