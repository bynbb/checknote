param(
    [Parameter(Mandatory = $true)]
    [string] $SiteZip,

    [Parameter(Mandatory = $true)]
    [string] $Stamp
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Net.Http

$resolvedSiteZip = (Resolve-Path -LiteralPath $SiteZip).Path
$stampParent = Split-Path -Parent $Stamp
$outDir = (Resolve-Path -LiteralPath $stampParent).Path
$smokeDir = Join-Path $outDir 'api-route-smoke'

Remove-Item -LiteralPath $smokeDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $smokeDir | Out-Null
Expand-Archive -Path $resolvedSiteZip -DestinationPath $smokeDir -Force

$apiExe = Join-Path $smokeDir 'checknote_api.exe'
$indexPath = Join-Path $smokeDir 'wwwroot\index.html'
$errorPagePath = Join-Path $smokeDir 'ErrorPages\error.html'
$errorPatternPath = Join-Path $smokeDir 'ErrorPages\friendly-error-test-pattern.png'
$errorBarsPath = Join-Path $smokeDir 'ErrorPages\smpte-color-bars.png'

if (!(Test-Path -LiteralPath $apiExe)) {
    throw "Expected checknote_api.exe was not found at $apiExe"
}

if (!(Test-Path -LiteralPath $indexPath)) {
    throw "Expected wwwroot/index.html was not found at $indexPath"
}

if (!(Test-Path -LiteralPath $errorPagePath)) {
    throw "Expected ErrorPages/error.html was not found at $errorPagePath"
}

if (!(Test-Path -LiteralPath $errorPatternPath)) {
    throw "Expected ErrorPages/friendly-error-test-pattern.png was not found at $errorPatternPath"
}

if (!(Test-Path -LiteralPath $errorBarsPath)) {
    throw "Expected ErrorPages/smpte-color-bars.png was not found at $errorBarsPath"
}

$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()

$oldUrls = $env:ASPNETCORE_URLS
$oldEnvironment = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_URLS = "http://127.0.0.1:$port"
$proc = $null
$client = $null

function Wait-ForSmokeServer {
    param(
        [Parameter(Mandatory = $true)]
        [string] $BaseUrl
    )

    foreach ($attempt in 1..30) {
        try {
            $health = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -TimeoutSec 2

            if ($health.StatusCode -eq 200) {
                return
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    throw 'Smoke server did not start.'
}

function Get-OpenApiOperation {
    param(
        [Parameter(Mandatory = $true)]
        [object] $OpenApi,

        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Method
    )

    $pathProperty = $OpenApi.paths.PSObject.Properties[$Path]

    if (-not $pathProperty) {
        throw "Expected Swagger JSON to document path '$Path'."
    }

    $operationProperty = $pathProperty.Value.PSObject.Properties[$Method]

    if (-not $operationProperty) {
        throw "Expected Swagger JSON to document $Method $Path."
    }

    return $operationProperty.Value
}

function Assert-OpenApiResponses {
    param(
        [Parameter(Mandatory = $true)]
        [object] $OpenApi,

        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Method,

        [Parameter(Mandatory = $true)]
        [string[]] $StatusCodes
    )

    $operation = Get-OpenApiOperation -OpenApi $OpenApi -Path $Path -Method $Method

    foreach ($statusCode in $StatusCodes) {
        if (-not $operation.responses.PSObject.Properties[$statusCode]) {
            throw "Expected Swagger JSON to document $statusCode for $Method $Path."
        }
    }
}

function Assert-OpenApiTag {
    param(
        [Parameter(Mandatory = $true)]
        [object] $OpenApi,

        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Method,

        [Parameter(Mandatory = $true)]
        [string] $ExpectedTag
    )

    $operation = Get-OpenApiOperation -OpenApi $OpenApi -Path $Path -Method $Method

    if ($operation.tags -notcontains $ExpectedTag) {
        throw "Expected Swagger JSON to tag $Method $Path as '$ExpectedTag'."
    }
}

function Assert-OpenApiRequestBodyContent {
    param(
        [Parameter(Mandatory = $true)]
        [object] $OpenApi,

        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Method,

        [Parameter(Mandatory = $true)]
        [string] $ContentType
    )

    $operation = Get-OpenApiOperation -OpenApi $OpenApi -Path $Path -Method $Method

    if (-not $operation.requestBody.content.PSObject.Properties[$ContentType]) {
        throw "Expected Swagger JSON to document $ContentType request body for $Method $Path."
    }
}

try {
    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    $proc = Start-Process -FilePath $apiExe -WorkingDirectory $smokeDir -WindowStyle Hidden -PassThru
    $base = "http://127.0.0.1:$port"

    Wait-ForSmokeServer -BaseUrl $base

    $index = Invoke-WebRequest -Uri "$base/" -UseBasicParsing -TimeoutSec 5
    $indexCache = [string] $index.Headers['Cache-Control']

    if ($indexCache -notmatch 'no-cache') {
        throw "Expected / to serve index.html with no-cache headers, got: $indexCache"
    }

    $mainScript = [regex]::Match($index.Content, 'src="(?<path>[^"]+\.js)"')

    if (-not $mainScript.Success) {
        throw 'Expected index.html to reference a JavaScript bundle.'
    }

    $scriptPath = $mainScript.Groups['path'].Value.TrimStart('/')
    $script = Invoke-WebRequest -Uri "$base/$scriptPath" -UseBasicParsing -TimeoutSec 5
    $scriptCache = [string] $script.Headers['Cache-Control']

    if ($scriptCache -notmatch 'immutable') {
        throw "Expected hashed JavaScript asset to be immutable, got: $scriptCache"
    }

    $errorPattern = Invoke-WebRequest -Uri "$base/error-assets/friendly-error-test-pattern.png" -UseBasicParsing -TimeoutSec 5

    if ($errorPattern.StatusCode -ne 200 -or [string] $errorPattern.Headers['Content-Type'] -notmatch 'image/png') {
        throw "Expected friendly error test pattern asset to be served as PNG, got: status=$($errorPattern.StatusCode) contentType=$($errorPattern.Headers['Content-Type'])"
    }

    $errorBars = Invoke-WebRequest -Uri "$base/error-assets/smpte-color-bars.png" -UseBasicParsing -TimeoutSec 5

    if ($errorBars.StatusCode -ne 200 -or [string] $errorBars.Headers['Content-Type'] -notmatch 'image/png') {
        throw "Expected friendly error SMPTE asset to be served as PNG, got: status=$($errorBars.StatusCode) contentType=$($errorBars.Headers['Content-Type'])"
    }

    $fallback = Invoke-WebRequest -Uri "$base/client/route" -UseBasicParsing -TimeoutSec 5
    $fallbackCache = [string] $fallback.Headers['Cache-Control']

    if ($fallbackCache -notmatch 'no-cache') {
        throw "Expected SPA fallback to serve index.html with no-cache headers, got: $fallbackCache"
    }

    try {
        Invoke-WebRequest -Uri "$base/api/missing" -UseBasicParsing -TimeoutSec 5 | Out-Null
        throw 'Expected unknown API route to return 404.'
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 404) {
            throw
        }
    }

    try {
        Invoke-WebRequest -Uri "$base/api/test-errors/throw-404" -Headers @{ Accept = 'text/html' } -UseBasicParsing -TimeoutSec 5 | Out-Null
        throw 'Expected test API 404 route to return 404.'
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 404) {
            throw
        }

        $body = $_.ErrorDetails.Message

        if ($body -match 'friendly-error-test-pattern') {
            throw 'Expected test API 404 route not to return the friendly HTML page.'
        }
    }

    try {
        Invoke-WebRequest -Uri "$base/swagger/index.html" -UseBasicParsing -TimeoutSec 5 | Out-Null
        throw 'Expected Swagger UI to be unavailable without an explicit enablement setting.'
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 404) {
            throw
        }
    }

    $hello = Invoke-WebRequest -Uri "$base/hello-world" -UseBasicParsing -TimeoutSec 5

    if ($hello.Content.Trim() -ne 'Hello from Checknote API') {
        throw "Unexpected /hello-world response: $($hello.Content)"
    }

    try {
        Invoke-WebRequest -Uri "$base/api/todos" -UseBasicParsing -TimeoutSec 5 | Out-Null
        throw 'Expected /api/todos to require authentication.'
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 401) {
            throw
        }
    }

    $authConfig = Invoke-WebRequest -Uri "$base/api/auth/config" -UseBasicParsing -TimeoutSec 5

    if ($authConfig.StatusCode -ne 200 -or $authConfig.Content -notmatch 'checknote-angular') {
        throw "Unexpected /api/auth/config response: $($authConfig.Content)"
    }

    $badBody = @{
        todos = @(
            @{
                id = 88888
                title = '88888'
                completed = $false
            }
        )
    } | ConvertTo-Json -Compress -Depth 4

    $client = [System.Net.Http.HttpClient]::new()
    $content = [System.Net.Http.StringContent]::new($badBody, [System.Text.Encoding]::UTF8, 'application/json')
    $bad = $client.PutAsync("$base/api/todos/task-list", $content).GetAwaiter().GetResult()
    $badStatus = [int] $bad.StatusCode
    $badBodyText = $bad.Content.ReadAsStringAsync().GetAwaiter().GetResult()

    if ($badStatus -ne 401) {
        throw "Expected unauthenticated /api/todos/task-list to return 401, got: status=$badStatus body=$badBodyText"
    }

    try {
        Invoke-WebRequest -Uri "$base/api/users/current" -UseBasicParsing -TimeoutSec 5 | Out-Null
        throw 'Expected /api/users/current to require authentication.'
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 401) {
            throw
        }
    }

    Stop-Process -Id $proc.Id -Force
    $proc = $null
    Start-Sleep -Milliseconds 500

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $proc = Start-Process -FilePath $apiExe -WorkingDirectory $smokeDir -WindowStyle Hidden -PassThru

    Wait-ForSmokeServer -BaseUrl $base

    $swaggerUi = Invoke-WebRequest -Uri "$base/swagger/index.html" -UseBasicParsing -TimeoutSec 5
    $swaggerJson = Invoke-WebRequest -Uri "$base/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 5

    if ($swaggerUi.StatusCode -ne 200 -or $swaggerUi.Content -notmatch 'Swagger UI') {
        throw 'Expected Swagger UI to be available in Development.'
    }

    if ($swaggerJson.StatusCode -ne 200 -or $swaggerJson.Content -notmatch '"/api/todos"') {
        throw 'Expected Swagger JSON to include Checknote API routes.'
    }

    $openApi = $swaggerJson.Content | ConvertFrom-Json
    Assert-OpenApiResponses -OpenApi $openApi -Path '/hello-world' -Method 'get' -StatusCodes @('200')
    Assert-OpenApiResponses -OpenApi $openApi -Path '/health' -Method 'get' -StatusCodes @('200')
    Assert-OpenApiResponses -OpenApi $openApi -Path '/api/auth/config' -Method 'get' -StatusCodes @('200')
    Assert-OpenApiResponses -OpenApi $openApi -Path '/api/todos' -Method 'get' -StatusCodes @('200', '400', '401')
    Assert-OpenApiResponses -OpenApi $openApi -Path '/api/todos/task-list' -Method 'put' -StatusCodes @('204', '400', '401')
    Assert-OpenApiResponses -OpenApi $openApi -Path '/api/users/current' -Method 'get' -StatusCodes @('200', '400', '401')
    Assert-OpenApiRequestBodyContent -OpenApi $openApi -Path '/api/todos/task-list' -Method 'put' -ContentType 'application/json'
    Assert-OpenApiTag -OpenApi $openApi -Path '/hello-world' -Method 'get' -ExpectedTag 'System'
    Assert-OpenApiTag -OpenApi $openApi -Path '/health' -Method 'get' -ExpectedTag 'System'
    Assert-OpenApiTag -OpenApi $openApi -Path '/api/auth/config' -Method 'get' -ExpectedTag 'Auth'
    Assert-OpenApiTag -OpenApi $openApi -Path '/api/todos' -Method 'get' -ExpectedTag 'Todos'
    Assert-OpenApiTag -OpenApi $openApi -Path '/api/todos/task-list' -Method 'put' -ExpectedTag 'Todos'
    Assert-OpenApiTag -OpenApi $openApi -Path '/api/users/current' -Method 'get' -ExpectedTag 'Users'

    Set-Content -LiteralPath $Stamp -Value 'ok' -NoNewline
}
finally {
    if ($client) {
        $client.Dispose()
    }

    if ($proc -and -not $proc.HasExited) {
        Stop-Process -Id $proc.Id -Force
    }

    $env:ASPNETCORE_URLS = $oldUrls
    $env:ASPNETCORE_ENVIRONMENT = $oldEnvironment
}
