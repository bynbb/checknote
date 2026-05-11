param(
  [Parameter(Mandatory = $true)]
  [string] $KeycloakBaseUrl,
  [Parameter(Mandatory = $true)]
  [string] $Realm,
  [Parameter(Mandatory = $true)]
  [string] $ClientId,
  [Parameter(Mandatory = $true)]
  [string] $Username,
  [Parameter(Mandatory = $true)]
  [string] $Password,
  [Parameter(Mandatory = $true)]
  [string] $CurrentUserUrl,
  [string] $ExpectedEmail = '',
  [string] $ExpectedName = ''
)

$ErrorActionPreference = 'Stop'

function Assert-Value {
  param(
    [bool] $Condition,
    [string] $Message
  )

  if (!$Condition) {
    throw $Message
  }
}

$baseUrl = $KeycloakBaseUrl.TrimEnd('/')
$tokenUrl = "$baseUrl/realms/$Realm/protocol/openid-connect/token"

$tokenResponse = Invoke-RestMethod -Method Post -Uri $tokenUrl -ContentType 'application/x-www-form-urlencoded' -Body @{
  grant_type = 'password'
  client_id = $ClientId
  username = $Username
  password = $Password
  scope = 'openid email profile'
}

Assert-Value (![string]::IsNullOrWhiteSpace($tokenResponse.access_token)) `
  'Keycloak did not return an access token.'

function Invoke-CurrentUser {
  Invoke-RestMethod -Method Get -Uri $CurrentUserUrl -Headers @{
    Authorization = "Bearer $($tokenResponse.access_token)"
  }
}

$apiResponse = Invoke-CurrentUser

Assert-Value (![string]::IsNullOrWhiteSpace($apiResponse.id)) `
  'Current-user response is missing id.'
Assert-Value (![string]::IsNullOrWhiteSpace($apiResponse.name)) `
  'Current-user response is missing name.'
Assert-Value (![string]::IsNullOrWhiteSpace($apiResponse.email)) `
  'Current-user response is missing email.'

if (![string]::IsNullOrWhiteSpace($ExpectedEmail)) {
  Assert-Value ($apiResponse.email -eq $ExpectedEmail) `
    "Current-user email did not match the expected smoke user email."
}

if (![string]::IsNullOrWhiteSpace($ExpectedName)) {
  Assert-Value ($apiResponse.name -eq $ExpectedName) `
    "Current-user name did not match the expected smoke user name."
}

$secondApiResponse = Invoke-CurrentUser

Assert-Value ($secondApiResponse.id -eq $apiResponse.id) `
  'Current-user response did not keep a stable Checknote user id across calls.'

Write-Output "Keycloak current-user smoke passed for stable Checknote user id '$($apiResponse.id)' and name '$($apiResponse.name)'."
