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
