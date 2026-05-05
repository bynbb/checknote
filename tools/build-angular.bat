@echo off
setlocal

set "OUTDIR=%~f1"
set "ZIPFILE=%~f2"
set "REPO_ROOT=%~dp0.."

if exist "%OUTDIR%" rmdir /s /q "%OUTDIR%"
if exist "%ZIPFILE%" del /f /q "%ZIPFILE%"

pushd "%REPO_ROOT%\cdev"
node "%REPO_ROOT%\tools\build-angular.mjs" --workspace "%REPO_ROOT%\cdev" --target checknote:build:production --output "%OUTDIR%"
set "BUILD_EXIT=%ERRORLEVEL%"
popd
if not "%BUILD_EXIT%"=="0" exit /b %BUILD_EXIT%

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%OUTDIR%\*' -DestinationPath '%ZIPFILE%' -Force"
if errorlevel 1 exit /b %ERRORLEVEL%

if not exist "%ZIPFILE%" exit /b 1
