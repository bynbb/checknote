[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string] $KeycloakHome
)

$ErrorActionPreference = 'Stop'

$themeSource = Join-Path $PSScriptRoot 'themes\checknote'
$themesRoot = Join-Path $KeycloakHome 'themes'
$themeDestination = Join-Path $themesRoot 'checknote'

if (!(Test-Path -LiteralPath $themeSource)) {
  throw "Checknote Keycloak theme source was not found at '$themeSource'."
}

if (!(Test-Path -LiteralPath $themesRoot)) {
  throw "Keycloak themes directory was not found at '$themesRoot'."
}

Remove-Item -LiteralPath $themeDestination -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item -LiteralPath $themeSource -Destination $themeDestination -Recurse -Force

Write-Output "Installed Checknote Keycloak theme to '$themeDestination'."
Write-Output "Select login theme 'checknote' for the Keycloak 'checknote' realm, then restart or clear the theme cache if the old login page remains visible."
