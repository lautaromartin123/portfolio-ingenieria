# Señales de mercado · BTC y SOL

Dos prototipos Python que consumen velas OHLCV y calculan indicadores con pandas y NumPy. Se mantienen como muestras de análisis y automatización; no implementan envío de órdenes ni un backtester.

## Estructura

- `src/btc.py`: indicadores y señales para BTC.
- `src/sol.py`: indicadores y señales para SOL, con cálculo teórico de tamaño y avisos opcionales.
- `examples/`: dos CSV de velas SOL, de 500 y 800 filas, usados como datos de ejemplo históricos.
- `tests/test_sol_filter.py`: comprobación offline del filtro de volatilidad.

## Prueba sin conexiones

```sh
python tests/test_sol_filter.py
```

Esta prueba usa la función real aislada mediante AST y la biblioteca estándar. Comprueba valores debajo, en y por encima del umbral y recorre el CSV de 4 horas. No inicia el bucle del bot ni realiza solicitudes externas. Para esta comprobación se usan niveles de soporte/resistencia vacíos: no valida todos los filtros de la estrategia.

## Preparar dependencias

```sh
python -m venv .venv
# Activar el entorno según el sistema operativo.
python -m pip install -r requirements.txt
```

Ejecutar un archivo de `src/` inicia consultas periódicas a datos públicos y puede escribir un CSV local. Telegram está desactivado por defecto y su configuración se obtiene de variables de entorno, sin valores incluidos.

## Corrección documentada

El percentil Bollinger se produce en 0–1. SOL lo comparaba directamente con 40. Se convirtió a 0–100 antes de la comparación. De 582 filas con percentil válido, 205 se filtran por baja volatilidad y 377 continúan evaluándose. Se conserva el comportamiento previo para NaN (valor sustituto 50).

Pendientes: revisar velas cerradas, alineación de pivotes, período inicial de indicadores, casos límite y validación histórica independiente. Los ejemplos no demuestran rentabilidad.
