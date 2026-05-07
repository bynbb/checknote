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

if (!(Test-Path -LiteralPath $apiExe)) {
    throw "Expected checknote_api.exe was not found at $apiExe"
}

if (!(Test-Path -LiteralPath $indexPath)) {
    throw "Expected wwwroot/index.html was not found at $indexPath"
}

$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()

$oldUrls = $env:ASPNETCORE_URLS
$env:ASPNETCORE_URLS = "http://127.0.0.1:$port"
$proc = $null
$client = $null

try {
    $proc = Start-Process -FilePath $apiExe -WorkingDirectory $smokeDir -WindowStyle Hidden -PassThru
    $base = "http://127.0.0.1:$port"
    $started = $false

    foreach ($attempt in 1..30) {
        try {
            $health = Invoke-WebRequest -Uri "$base/health" -UseBasicParsing -TimeoutSec 2

            if ($health.StatusCode -eq 200) {
                $started = $true
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $started) {
        throw 'Smoke server did not start.'
    }

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

    $hello = Invoke-WebRequest -Uri "$base/hello-world" -UseBasicParsing -TimeoutSec 5

    if ($hello.Content.Trim() -ne 'Hello from Checknote API') {
        throw "Unexpected /hello-world response: $($hello.Content)"
    }

    $todos = Invoke-WebRequest -Uri "$base/api/todos" -UseBasicParsing -TimeoutSec 5

    if ($todos.StatusCode -ne 200 -or $todos.Content -notmatch 'Build the app with Bazel') {
        throw "Unexpected /api/todos response: $($todos.Content)"
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

    if ($badStatus -ne 400 -or $badBodyText -notmatch 'Todos.ReservedTitle') {
        throw "Unexpected reserved todo response: status=$badStatus body=$badBodyText"
    }

    $user = Invoke-WebRequest -Uri "$base/api/users/current" -UseBasicParsing -TimeoutSec 5

    if ($user.StatusCode -ne 200 -or $user.Content -notmatch 'Ada Lovelace') {
        throw "Unexpected /api/users/current response: $($user.Content)"
    }

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
}
