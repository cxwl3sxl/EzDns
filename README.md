# EzDns

A lightweight DNS management service with a web UI and API.

## Features
- Define DNS rules with fixed or auto IP generation.
- REST API (`/api/rules`) for CRUD operations.
- Vue 3 front‑end for rule management.
- Custom rule resolver that falls back to an upstream DNS server.

## Architecture
- **EzDns.WebApi** – ASP.NET Core API & background DNS server.
- **EzDns.Core** – Core models and custom DNS resolver.
- **EzDns.Web** – Vue 3 SPA built with Vite.

## Getting Started
### Prerequisites
- .NET 7 SDK
- Node.js 20+ and npm

### Run locally
```bash
# API
cd EzDns.WebApi
dotnet run
# Web UI
cd ../EzDns.Web
npm install
npm run dev
```
The API listens on `http://0.0.0.0:8080` (configurable in `appsettings.json`).

### Configuration
- `appsettings.json` – DNS forwarder and port.
- `rules.json` – Initial DNS rules.
- Environment variables can override `Dns:ForwardDns` and `Dns:DnsPort`.

## API Endpoints
- `GET /api/rules` – List all rules.
- `POST /api/rules` – Add a rule.
- `PUT /api/rules/{pattern}` – Update a rule.
- `DELETE /api/rules?pattern=...` – Delete a rule.

## Development
- Front‑end hot‑reload: `npm run dev`
- Build for production: `npm run build`
- Lint/formatting follows the project's ESLint and Prettier configs (if any).

## License
MIT
