# EzDns

一个轻量级的 DNS 管理服务，提供 Web UI 和 REST API，支持跨平台部署为系统服务。

## 功能特性

- **DNS 规则管理** — 通过 Web UI 或 REST API 增删改查 DNS 解析规则
- **智能规则引擎** — 支持固定 IP / 自动 IP 生成两种模式，优先级排序匹配
- **通配符域名匹配** — 支持 `*.example.com` 格式，自动解析子域名
- **JWT 身份认证** — 登录后自动携带 Token，会话持久化
- **亮/暗主题切换** — 跟随系统偏好或手动切换
- **跨平台服务部署** — 支持安装为 Windows 服务或 Linux systemd 服务
- **CI/CD 自动构建** — GitHub Actions 自动发布 Windows 和 Linux 自包含单文件版本

## 项目架构

```
EzDns/
├── EzDns.WebApi/          # ASP.NET Core 8 Web API + DNS 服务器（后台服务）
│   ├── Controllers/       # AuthController, RulesController
│   ├── Services/          # EzDnsHostedService, JsonRuleRepository
│   ├── ServiceCommands.cs # 服务安装/卸载/启停命令
│   ├── Program.cs         # 应用入口（含 CLI 参数处理）
│   └── EzDnsWebApi.service # systemd 模板文件
├── EzDns.Core/            # 共享模型与 DNS 解析逻辑
│   ├── CustomRuleResolver.cs  # 自定义 DNS 规则解析器
│   ├── RuleSort.cs            # 规则优先级排序
│   └── Models/                # DnsRule, IRuleRepository
├── EzDns.Web/             # Vue 3 SPA 前端
│   └── src/
│       ├── views/         # LoginView, RulesView
│       ├── composables/   # useAuth, useTheme, useRefreshKey
│       └── router.ts      # 路由与导航守卫
└── .github/workflows/     # GitHub Actions 构建工作流
```

## 快速开始

### 前置要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) 和 npm
- Windows 下端口 < 1024（如 DNS 53 端口）需要管理员权限

### 启动后端 API

```bash
cd EzDns.WebApi
dotnet run
```

API 默认监听 `http://0.0.0.0:8080`（可在 `appsettings.json` 的 `Kestrel:Endpoints` 中配置）。

DNS 服务器随 API 一起启动，默认端口 53（需要管理员权限，可通过 `Dns:DnsPort` 配置）。

### 启动前端（开发模式）

```bash
cd EzDns.Web
npm install
npm run dev
```

### 构建前端并集成到 API

```bash
cd EzDns.Web
npm install
npm run build
# 将构建产物复制到 API 的 wwwroot 目录
mkdir -p ../EzDns.WebApi/wwwroot
cp -r dist/* ../EzDns.WebApi/wwwroot/
```

之后直接启动 API 即可访问 Web UI。

### 默认登录账号

| 用户名 | 密码 |
|--------|------|
| `admin` | `admin123` |

> **注意**：请在生产环境中修改 `appsettings.json` 中的 `Auth:Password` 和 `Auth:Jwt:Secret`。

## 服务管理命令

EzDns 支持安装为系统服务，提供以下 CLI 命令：

| 命令 | 说明 |
|------|------|
| *(无参数)* | 以控制台模式正常运行 |
| `install` | 安装为系统服务 |
| `uninstall` | 卸载系统服务 |
| `status` | 查看服务状态 |
| `start` | 启动服务 |
| `stop` | 停止服务 |
| `help` | 显示帮助信息 |

```bash
# 示例：Windows（需要管理员权限）
EzDns.WebApi.exe install
EzDns.WebApi.exe start

# 示例：Linux（需要 sudo）
./EzDns.WebApi install
./EzDns.WebApi start
```

- **Windows**：底层使用 `sc.exe` 管理服务
- **Linux**：底层使用 `systemctl` 管理服务，自动生成 systemd 单元文件

## API 端点

所有规则管理接口需要 JWT 认证（`Authorization: Bearer <token>`）。

| 方法 | 路径 | 说明 | 需要认证 |
|------|------|------|----------|
| `POST` | `/api/auth/login` | 登录获取 Token | 否 |
| `GET` | `/api/rules` | 获取所有规则 | 是 |
| `POST` | `/api/rules` | 添加规则 | 是 |
| `PUT` | `/api/rules/{pattern}` | 更新规则 | 是 |
| `DELETE` | `/api/rules?pattern=...` | 删除规则 | 是 |

### DNS 规则字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `pattern` | string | 域名模式，如 `example.com` 或 `*.example.com` |
| `type` | number | 记录类型：`1`(A)、`28`(AAAA)、`15`(MX)、`2`(NS)、`16`(TXT) |
| `mode` | string | `fixed`（固定 IP）或 `auto`（自动生成 IP） |
| `value` | string | 固定模式下的 IP 地址 |
| `ipBase` | string | 自动模式下 IP 前缀，如 `192.168.0.` |
| `priority` | number | 优先级，数字越大越优先 |
| `isEnabled` | boolean | 是否启用 |

## 配置

### appsettings.json

```json
{
  "Dns": {
    "ForwardDns": "8.8.8.8",     // 上游 DNS 服务器
    "DnsPort": 53                 // DNS 监听端口（<1024 需要管理员权限）
  },
  "Auth": {
    "Username": "admin",
    "Password": "admin123",
    "Jwt": {
      "Secret": "CHANGE_THIS_TO...",
      "Issuer": "EzDns",
      "Audience": "EzDnsClient",
      "ExpiryMinutes": 480
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

### 规则文件

初始规则存储在 `rules.json`（与 API 启动目录同级），运行时自动读写。

## CI/CD 自动构建

项目包含 GitHub Actions 工作流（`.github/workflows/build.yml`），当推送至 `main` 分支时自动执行：

1. 构建 Vue 前端
2. 将前端产物复制到 `wwwroot`
3. 使用 .NET 8 自包含发布 Windows (`win-x64`) 和 Linux (`linux-x64`) 版本
4. 附带 `appsettings.json`、`rules.json` 等配置文件
5. 上传构建产物到 GitHub Artifacts

发布产物为**单文件自包含应用**，无需安装 .NET 运行时即可运行。

## 常见问题

- **DNS 端口 53 绑定失败**：Windows 下需要以管理员身份运行。可临时修改 `Dns:DnsPort` 为大于 1024 的端口进行测试
- **前端页面空白**：请确保已构建前端或使用 `npm run dev` 启动开发服务器
- **401 未授权**：Token 过期后自动跳转登录页，请重新登录
- **API 修改后不生效**：需要重启 API 进程（不支持热重载）

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端框架 | ASP.NET Core 8 |
| DNS 协议 | [DNS](https://www.nuget.org/packages/DNS/) (NuGet) |
| 身份认证 | JWT Bearer |
| 前端框架 | Vue 3 + TypeScript |
| 构建工具 | Vite 5 |
| HTTP 客户端 | Axios |
| 服务管理 | sc.exe (Windows) / systemd (Linux) |
| CI/CD | GitHub Actions |

## License

MIT
