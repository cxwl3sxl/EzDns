namespace EzDns.WebApi;

public class AuthOptions
{
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "admin123";
    public JwtOptions Jwt { get; set; } = new();
}

public class JwtOptions
{
    public string Secret      { get; set; } = string.Empty;
    public string Issuer      { get; set; } = "EzDns";
    public string Audience    { get; set; } = "EzDnsClient";
    public int    ExpiryMinutes { get; set; } = 480;
}
