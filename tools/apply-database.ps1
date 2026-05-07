$ErrorActionPreference = 'Stop'

$bazel = Join-Path $PSScriptRoot '..\node_modules\.bin\bazelisk.cmd'
$target = '//deployment/database:checknote_db_tool_publish'

& $bazel build $target
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$executionRoot = (& $bazel info execution_root).Trim()
$toolFile = & $bazel cquery $target --output=files |
    Where-Object { $_ -match 'checknote_db_tool\.exe$' } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($toolFile)) {
    throw 'Could not locate the published Checknote database tool.'
}

$toolPath = Join-Path $executionRoot $toolFile

# TODO: In production deployment automation, back up the target database before this apply step.
& $toolPath apply
exit $LASTEXITCODE
