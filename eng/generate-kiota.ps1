param(
    [string[]]$KiotaPrefix = @("dotnet", "tool", "run", "kiota", "--")
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Invoke-Kiota {
    param([string[]]$Arguments)

    $command = $KiotaPrefix[0]
    $prefixArgs = if ($KiotaPrefix.Length -gt 1) { $KiotaPrefix[1..($KiotaPrefix.Length - 1)] } else { @() }
    & $command @prefixArgs @Arguments
}

Invoke-Kiota @(
    "generate",
    "--language", "CSharp",
    "--class-name", "KrogerProductsKiotaClient",
    "--namespace-name", "KrogerMcp.Generated.Products.Kiota",
    "--openapi", "$repoRoot/openapi/kroger-products.openapi.json",
    "--output", "$repoRoot/src/KrogerMcp.Generated.Products/Kiota"
)

Invoke-Kiota @(
    "generate",
    "--language", "CSharp",
    "--class-name", "KrogerCartKiotaClient",
    "--namespace-name", "KrogerMcp.Generated.Cart.Kiota",
    "--openapi", "$repoRoot/openapi/kroger-cart.openapi.json",
    "--output", "$repoRoot/src/KrogerMcp.Generated.Cart/Kiota"
)

Invoke-Kiota @(
    "generate",
    "--language", "CSharp",
    "--class-name", "KrogerLocationsKiotaClient",
    "--namespace-name", "KrogerMcp.Generated.Locations.Kiota",
    "--openapi", "$repoRoot/openapi/kroger-locations.openapi.json",
    "--output", "$repoRoot/src/KrogerMcp.Generated.Locations/Kiota"
)
