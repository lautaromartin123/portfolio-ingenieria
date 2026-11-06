@echo off
setlocal
cd /d "%~dp0"

if not exist ".venv\Scripts\python.exe" (
    python -m venv .venv
    if errorlevel 1 goto python_error
)

".venv\Scripts\python.exe" -c "import numpy" >nul 2>&1
if errorlevel 1 (
    ".venv\Scripts\python.exe" -m pip install -r requirements.txt
    if errorlevel 1 goto dependency_error
)

".venv\Scripts\python.exe" scripts\calcular_regresion_ie.py
if errorlevel 1 goto analysis_error
echo.
echo Resultado guardado en resultados\regresion_ie_2030.json
pause
exit /b 0

:python_error
echo No se pudo crear el entorno. Instala Python 3 con acceso desde la terminal y vuelve a abrirla.
pause
exit /b 1

:dependency_error
echo No se pudo instalar NumPy. Revisa la conexion y vuelve a ejecutar este archivo.
pause
exit /b 1

:analysis_error
echo El analisis no termino correctamente. Revisa el mensaje anterior.
pause
exit /b 1
