# Experimentos · siete simulaciones Python

**Cada simulación tiene ahora su propia carpeta y su código visible al abrirla.** Las primeras tres son de movimiento; las otras cuatro son variantes de superficies ilustrativas.

| Simulación | Qué vas a ver | Código completo |
| --- | --- | --- |
| [Movimiento vertical](01-movimiento-vertical) | Altura en función del tiempo, velocidad inicial y aceleración gravitatoria. | [simulacion_simple.py](01-movimiento-vertical/simulacion_simple.py) |
| [Pelota con rebote](02-rebote) | Animación de una pelota que pierde energía al rebotar. | [simulacion_rebote.py](02-rebote/simulacion_rebote.py) |
| [Gravedad y rebote en 3D](03-gravedad-3d) | Movimiento sobre tres ejes con gravedad y choque contra un plano. | [simulacion_3d_gravedad.py](03-gravedad-3d/simulacion_3d_gravedad.py) |
| [Superficie gravitatoria con malla](04-plano-con-malla) | Una masa central representada con una malla tridimensional. | [simulacion_plano_einstein.py](04-plano-con-malla/simulacion_plano_einstein.py) |
| [Superficie gravitatoria coloreada](05-plano-superficie) | Versión de una masa central con una superficie continua y mapa de color. | [simulacion_plano_einstein_2.py](05-plano-superficie/simulacion_plano_einstein_2.py) |
| [Superficie de dos masas](06-dos-masas) | Superposición de dos contribuciones sobre un fondo oscuro. | [simulacion_plano_einstein_3.py](06-dos-masas/simulacion_plano_einstein_3.py) |
| [Dos masas configurables](07-masas-configurables) | Masas, posiciones y suavizado editables en el código. | [simulacion_plano_einstein_4.py](07-masas-configurables/simulacion_plano_einstein_4.py) |

## Ejecutar desde Windows

Abrir la carpeta experimentos y hacer doble clic en **VER_SIMULACIONES.cmd**. Aparece un menú con las siete opciones. También hay un INICIAR.cmd dentro de cada simulación.

El lanzador crea un entorno virtual en esta carpeta. Las dependencias se instalan dentro de ese entorno; no se usa pip sobre el Python administrado por uv.

## Ver código desde GitHub

Los enlaces de la columna Código completo abren directamente cada archivo Python. GitHub permite leer y descargar los archivos; para abrir sus ventanas de gráficos hay que ejecutarlos en el equipo.

## Linux / WSL

Desde esta carpeta:

```sh
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
.venv/bin/python abrir_simulaciones.py
```

Hace falta un entorno gráfico; en WSL puede requerirse WSLg y Tkinter. Las superficies llamadas plano Einstein son ilustraciones, no simulaciones de relatividad general.

[Volver al portfolio](../README.md)
