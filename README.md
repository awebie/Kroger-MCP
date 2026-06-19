# Kroger MCP Server

A local .NET 10 Model Context Protocol (MCP) server for Kroger. It supports
location lookup, store-specific product search, product details, and adding
items to an authenticated customer's cart.

This server uses the MCP **stdio transport**. It is launched as a child process
by an MCP client and exchanges JSON-RPC messages through standard input and
standard output. It is not an HTTP service and does not listen on a public MCP
port.

See [SPEC.md](SPEC.md) for the architecture, interfaces, validation rules, and
Mermaid diagrams.

## Requirements

- .NET 10 SDK or runtime
- A Kroger developer application
- A Kroger API client ID and client secret
- A Kroger customer account for cart operations
- A local browser for the interactive cart authorization flow

## MCP Tools

| Tool | Purpose | Authentication |
| --- | --- | --- |
| `lookup_locations` | Find stores by ZIP code, coordinates, or location ID | Client credentials |
| `get_location` | Get one store by location ID | Client credentials |
| `search_products` | Search products at a specific store | Client credentials |
| `get_product` | Get product details, optionally scoped to a store | Client credentials |
| `add_to_cart` | Add one or more products to the customer's Kroger cart | Customer authorization |

`search_products` requires an eight-character Kroger location ID. A normal
workflow is to call `lookup_locations`, select a store, and pass its
`locationId` into subsequent product searches.

The cart API adds products to the customer's existing Kroger cart. It does not
change the account's selected pickup or delivery store.

## Environment Variables

### Required

`KROGER_CLIENT_ID`

The client ID from the Kroger developer application. Products and locations
use it for the OAuth client-credentials flow. The interactive cart flow also
uses it.

`KROGER_CLIENT_SECRET`

The client secret from the Kroger developer application. Treat it as a secret;
do not commit it to source control or place it in shared MCP configuration.

### Optional

`KROGER_CUSTOMER_ACCESS_TOKEN`

A Kroger customer OAuth access token with cart write permission. When present,
`add_to_cart` uses this token and skips the interactive browser authorization
flow. Access tokens expire, so this is mainly useful for temporary local
development and testing.

`KROGER_DEFAULT_LOCATION_ID`

Reserved for a future default-location feature. The current implementation does
not consume this variable; callers must provide `LocationId` to
`search_products`.

### PowerShell Examples

Set variables for the current PowerShell process:

```powershell
$env:KROGER_CLIENT_ID = "your-client-id"
$env:KROGER_CLIENT_SECRET = "your-client-secret"
```

Persist the values for the current Windows user:

```powershell
[Environment]::SetEnvironmentVariable("KROGER_CLIENT_ID", $env:KROGER_CLIENT_ID, "User")
[Environment]::SetEnvironmentVariable("KROGER_CLIENT_SECRET", $env:KROGER_CLIENT_SECRET, "User")
```

Newly persisted user variables are inherited only by processes started after
the change. Restart the MCP client before testing them.

Verify presence without printing secret values:

```powershell
@("KROGER_CLIENT_ID", "KROGER_CLIENT_SECRET", "KROGER_CUSTOMER_ACCESS_TOKEN") |
    ForEach-Object {
        [pscustomobject]@{
            Name = $_
            IsSet = -not [string]::IsNullOrWhiteSpace(
                [Environment]::GetEnvironmentVariable($_, "Process"))
        }
    }
```

## .NET Configuration

Non-secret defaults are stored in
`src/KrogerMcp.Host/appsettings.json`. The current local cart callback is:

```json
{
  "Kroger": {
    "OAuthRedirectUri": "http://localhost:5000"
  }
}
```

The redirect URI registered in the Kroger developer application must exactly
match this value.

.NET configuration can also be supplied with double-underscore environment
variable names:

```powershell
$env:Kroger__BaseUrl = "https://api.kroger.com"
$env:Kroger__AuthorizationUrl = "https://api.kroger.com/v1/connect/oauth2/authorize"
$env:Kroger__TokenUrl = "https://api.kroger.com/v1/connect/oauth2/token"
$env:Kroger__OAuthRedirectUri = "http://localhost:5000"
```

The named `KROGER_CLIENT_ID`, `KROGER_CLIENT_SECRET`, and
`KROGER_CUSTOMER_ACCESS_TOKEN` variables take precedence over corresponding
values under `Kroger:OAuth`.

Do not put real credentials in `appsettings.json`. For local development, an
ignored `src/KrogerMcp.Host/appsettings.Development.json` may be based on
`appsettings.example.json`; it is loaded only when the .NET environment is
`Development`.

## Build And Test

Restore and build:

```powershell
dotnet restore KrogerMcp.sln --configfile NuGet.Config
dotnet build KrogerMcp.sln --no-restore
```

Run tests:

```powershell
dotnet test KrogerMcp.sln --no-build --no-restore
```

Publish a stable executable layout for an MCP client:

```powershell
dotnet publish src/KrogerMcp.Host/KrogerMcp.Host.csproj `
    --configuration Release `
    --output artifacts/KrogerMcp.Host
```

## Running As A Stdio MCP Server

For manual diagnostics, the host can be started with:

```powershell
dotnet run --project src/KrogerMcp.Host/KrogerMcp.Host.csproj
```

The process will appear to wait without showing an interactive prompt. That is
expected: it is waiting for MCP JSON-RPC messages on standard input. Normally,
the MCP client starts and manages this process.

Application logs are written to standard error. Standard output is reserved for
MCP protocol messages and must not be redirected into ordinary logging.

## Generic MCP Client Configuration

Exact configuration keys vary by MCP client. A typical stdio registration has
this shape:

```json
{
  "mcpServers": {
    "kroger": {
      "command": "dotnet",
      "args": [
        "C:\\absolute\\path\\to\\artifacts\\KrogerMcp.Host\\KrogerMcp.Host.dll"
      ],
      "cwd": "C:\\absolute\\path\\to\\artifacts\\KrogerMcp.Host"
    }
  }
}
```

Use absolute paths. If the MCP client supports `cwd`, set it to the published
directory so `appsettings.json` is discovered. If it does not support `cwd`,
provide the required .NET configuration through environment variables.

The server inherits environment variables from the MCP client process. Prefer
OS user-level variables over duplicating secrets in the MCP client's JSON file.
If client-specific `env` configuration is necessary, its generic shape is:

```json
{
  "env": {
    "KROGER_CLIENT_ID": "your-client-id",
    "KROGER_CLIENT_SECRET": "your-client-secret",
    "Kroger__OAuthRedirectUri": "http://localhost:5000"
  }
}
```

Protect any configuration file containing those values with appropriate file
permissions and keep it out of source control.

After changing the server binary, configuration, or inherited environment,
restart the MCP client so it launches a fresh server process.

## Cart Authorization

When `KROGER_CUSTOMER_ACCESS_TOKEN` is not set, `add_to_cart` performs an OAuth
authorization-code flow:

1. The MCP server starts a temporary local callback listener.
2. It opens the Kroger authorization page in the default browser.
3. The customer signs in and grants `cart.basic:write` permission.
4. Kroger redirects to the configured local callback URI.
5. The server exchanges the authorization code and adds the requested items.

The current implementation keeps the resulting token only for that operation;
it does not persist refresh tokens. A later cart call may require authorization
again.

## Troubleshooting

**The server says credentials are required**

The MCP client did not inherit `KROGER_CLIENT_ID` or
`KROGER_CLIENT_SECRET`. Verify the variables, then fully restart the client.

**The server starts but no prompt appears**

This is normal for a stdio MCP server. An MCP client must send the initialization
request over standard input.

**The cart callback fails or times out**

Confirm that the Kroger application and `Kroger__OAuthRedirectUri` use the same
URI, that the callback port is available, and that local firewall rules allow
the listener.

**Products do not match the intended store**

Call `lookup_locations` again and pass the selected eight-character
`locationId` to `search_products`.

**Cart contents use a different pickup store**

The Kroger cart endpoint does not set the fulfillment location. Select the
intended store in the Kroger account before checkout.

## Regenerating Kiota Clients

The canonical Kroger OpenAPI documents are under `openapi/`. Regenerate the
isolated Kiota projects with:

```powershell
dotnet tool restore --configfile NuGet.Config
.\eng\generate-kiota.ps1
```

The MCP tools depend on application interfaces rather than generated Kiota
types, so regeneration does not change the public MCP tool names.
