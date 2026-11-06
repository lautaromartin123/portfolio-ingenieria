@echo off
setlocal
cd /d "%~dp0"

where node >nul 2>&1
if errorlevel 1 goto missing_node
where npm.cmd >nul 2>&1
if errorlevel 1 goto missing_node

echo Instalando las dependencias del Trading Journal...
call npm.cmd ci --no-audit --no-fund
if errorlevel 1 goto error

echo.
echo Ejecutando las pruebas...
call npm.cmd test
if errorlevel 1 goto error

echo.
echo Instalacion y pruebas completadas.
pause
exit /b 0

:missing_node
echo Hace falta Node.js 24 con npm. Instalalo y abre una terminal nueva.
pause
exit /b 1

:error
echo.
echo El proceso no termino correctamente. Revisa el mensaje anterior.
pause
exit /b 1
