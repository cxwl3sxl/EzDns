# EzDns Core Components
- **EzDns.WebApi**: ASP.NET Core API (backend & DNS server)
- **EzDns.Web**: Vue 3 SPA frontend
- **EzDns.Core**: Shared models

## Setup
```bash
# API
cd EzDns.WebApi && dotnet run
# Web UI
cd ../EzDns.Web && npm install && npm run dev
```

## Key Configs
- `appsettings.json`: DNS forwarder (`8.8.8.8`), port (`53`), API URL (`http://0.0.0.0:8080`)
- `rules.json`: Initial DNS rules
- `EzDns.slnx`: Solution linking core projects

## Runtime Notes
- API defaults to port 8080 (override in `appsettings.json`)
- Frontend refreshes fully on code changes (no HMR)
- Keep both WebApi and Web processes running for UI‑API interaction

## Common Gotchas
- Manual IP updates required if runtime IP changes
- No automatic API restart on code change; stop/start needed
- DNS server starts with API; ensure API is running for rule changes
- DNS port 53 requires administrator privileges on Windows (UDP sockets < 1024 are restricted)
  - Run `dotnet run` from an elevated terminal / Visual Studio "Run as Administrator"
  - If DNS server fails to start, the API (port 8080) will still be reachable — check startup logs for the binding error
  - Alternatively set `Dns.DnsPort` in `appsettings.json` to a port > 1024 for testing (clients must point to that port explicitly)

## Testing
- Use `/api/rules` endpoint (e.g., Postman) to verify rules
- Check `src/router.ts` for route definitions