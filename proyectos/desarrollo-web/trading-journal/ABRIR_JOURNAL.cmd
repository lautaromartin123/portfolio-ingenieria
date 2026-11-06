@echo off
setlocal
cd /d "%~dp0"
where npm.cmd >nul 2>&1
if errorlevel 1 goto missing_node

if not exist "node_modules\.bin\vinext.cmd" (
    echo Primera preparacion: puede tardar varios minutos. Deja terminar la instalacion.
    call npm.cmd ci --no-audit --no-fund
    if errorlevel 1 goto error
)

echo Abriendo Trading Journal. La direccion aparecera debajo como Local.
echo Mantene esta ventana abierta mientras uses la aplicacion.
call npm.cmd run dev
if errorlevel 1 goto error
exit /b 0

:missing_node
echo Hace falta Node.js 24 con npm. Instalalo y abre una terminal nueva.
pause
exit /b 1

:error
echo.
echo El arranque no termino correctamente. Revisa el mensaje anterior.
pause
exit /b 1
