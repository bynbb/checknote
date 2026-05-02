@echo off
setlocal

set "OUTDIR=%~1"
set "ZIPFILE=%~2"

if exist "%OUTDIR%" rmdir /s /q "%OUTDIR%"
if exist "%ZIPFILE%" del /f /q "%ZIPFILE%"

call node_modules\.bin\ng.cmd build --configuration production --output-path="%OUTDIR%"
if errorlevel 1 exit /b %ERRORLEVEL%

powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%OUTDIR%\*' -DestinationPath '%ZIPFILE%' -Force"
if errorlevel 1 exit /b %ERRORLEVEL%

if not exist "%ZIPFILE%" exit /b 1
