# Análisis demográfico de Boa Vista

Exploración de la estructura por edades del municipio de Boa Vista (Brasil), usando datos agregados de los censos de 2000, 2010 y 2022 del IBGE.

- [Fuentes y método](datos/boa-vista-censos-2000-2010-2022.md).
- [Código de regresión](scripts/calcular_regresion_ie.py).
- [Resultado](resultados/regresion_ie_2030.json).

## Ejecutar en Windows

Requiere Python 3.12 o posterior con acceso desde la terminal. Después de instalarlo, cerrar y volver a abrir la terminal. La preparación local se comprobó con Python 3.14.10 y un entorno virtual propio del proyecto.

Abrir la carpeta de este proyecto y hacer doble clic en **EJECUTAR.cmd**. El archivo entra en la carpeta correcta, crea un entorno `.venv`, instala NumPy si falta y ejecuta el análisis. El resultado se guarda en `resultados/regresion_ie_2030.json`.

También se puede ejecutar desde CMD, estando en esta carpeta:

```bat
python -m venv .venv
.venv\Scripts\python.exe -m pip install -r requirements.txt
.venv\Scripts\python.exe scripts\calcular_regresion_ie.py
```

Si Windows dice que no encontró Python y ofrece Microsoft Store, el comando está resolviendo al acceso directo de la tienda. Comprobar `python --version` en una terminal nueva y revisar que la instalación preceda al acceso de Microsoft Store en PATH. No basta con copiar el comando desde cualquier carpeta porque `requirements.txt` pertenece a este proyecto. Las dependencias se instalan dentro de `.venv`, también al usar Python administrado por uv.

## Linux / WSL

Desde esta carpeta, con Python 3 y el módulo `venv` disponibles:

```sh
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
.venv/bin/python scripts/calcular_regresion_ie.py
```

La regresión usa tres observaciones, índices redondeados y un grado de libertad residual. La proyección y sus intervalos son exploratorios; no constituyen un modelo predictivo robusto ni una explicación causal. Los datos son agregados públicos, no registros individuales. Fuentes, universo e intervalos temporales están documentados.
