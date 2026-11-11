# Lautaro · Portfolio de Ingeniería Informática

Estudiante avanzado de Ingeniería Informática. Desarrollo web, datos y sistemas. Busco mi primera oportunidad en IT.

**Elegí una muestra para ver su código o descargar el estudio académico.** Cada trabajo conserva su carpeta y sus instrucciones.

[Proyectos](#proyectos) · [Estudio académico](#estudio-académico) · [Prácticas de C#](#prácticas-de-c) · [Simulaciones](#siete-simulaciones-python) · [Linux y C](#linux-y-c) · [English](docs/ABOUT-EN.md)

## Proyectos

| Proyecto y explicación | Tecnologías / área | Abrir el código |
| --- | --- | --- |
| [Consultorios](proyectos/desarrollo-web/consultorios) | C# · WebForms · SQL Server | [Pantallas](proyectos/desarrollo-web/consultorios/Consultorios) · [Lógica](proyectos/desarrollo-web/consultorios/BIZ) · [SQL](proyectos/desarrollo-web/consultorios/BD) |
| [Trading Journal](proyectos/desarrollo-web/trading-journal) | TypeScript · React · reglas y CSV | [Interfaz](proyectos/desarrollo-web/trading-journal/components/journal) · [Cálculos](proyectos/desarrollo-web/trading-journal/lib/domain.ts) · [Pruebas](proyectos/desarrollo-web/trading-journal/tests) |
| [Milagros Fabricantes](proyectos/desarrollo-web/milagros-fabricantes) | E-commerce · caso de estudio público | Caso de estudio; el código del sistema original se conserva fuera del portfolio |
| [BTC y SOL](proyectos/datos-y-automatizacion/senales-mercado) | Python · datos de mercado | [Bot BTC](proyectos/datos-y-automatizacion/senales-mercado/src/btc.py) · [Bot SOL](proyectos/datos-y-automatizacion/senales-mercado/src/sol.py) |
| [Importador Excel](proyectos/datos-y-automatizacion/importador-excel) | C# · ClosedXML · EF Core | [Normalización](proyectos/datos-y-automatizacion/importador-excel/Services/ImportacionExcelService.cs) · [Controlador](proyectos/datos-y-automatizacion/importador-excel/Controllers/ImportacionController.cs) |

## Estudio académico

**[Envejecimiento poblacional de Boa Vista](proyectos/datos-y-automatizacion/analisis-demografico)**: investigación con datos censales del IBGE de 2000, 2010 y 2022, análisis estadístico y contexto socioeconómico.

**[Descargar el estudio completo en Word (.docx)](https://github.com/lautaromartin123/portfolio-ingenieria/raw/refs/heads/main/proyectos/datos-y-automatizacion/analisis-demografico/Estudio_envejecimiento_poblacional_Boa_Vista.docx)**. Se puede leer directamente en Word; el script Python de esa carpeta es material auxiliar de cálculo.

## Prácticas de C#

| Solución | Contenido | Abrir el código |
| --- | --- | --- |
| [Eventos y estado web](facultad/programacion-avanzada/01-eventos-y-estado-web) | Eventos, Session, ViewState, cookies y Repeater | [Páginas](facultad/programacion-avanzada/01-eventos-y-estado-web/Eeventos) · [BIZ](facultad/programacion-avanzada/01-eventos-y-estado-web/BIZ) |
| [Consultas y filtros](facultad/programacion-avanzada/02-consultas-y-filtros) | Alumnos por curso y productos por estado | [Páginas](facultad/programacion-avanzada/02-consultas-y-filtros/Avanzada2026) · [BIZ](facultad/programacion-avanzada/02-consultas-y-filtros/BIZ) |
| [Registro y consultas](facultad/programacion-avanzada/03-registro-y-consultas) | Formularios, imágenes y consultas | [Páginas](facultad/programacion-avanzada/03-registro-y-consultas/Avanzada2026) · [BIZ](facultad/programacion-avanzada/03-registro-y-consultas/BIZ) |

## Siete simulaciones Python

| Simulación | Código completo |
| --- | --- |
| [Movimiento vertical](experimentos/01-movimiento-vertical) | [simulacion_simple.py](experimentos/01-movimiento-vertical/simulacion_simple.py) |
| [Pelota con rebote](experimentos/02-rebote) | [simulacion_rebote.py](experimentos/02-rebote/simulacion_rebote.py) |
| [Gravedad y rebote en 3D](experimentos/03-gravedad-3d) | [simulacion_3d_gravedad.py](experimentos/03-gravedad-3d/simulacion_3d_gravedad.py) |
| [Superficie gravitatoria con malla](experimentos/04-plano-con-malla) | [simulacion_plano_einstein.py](experimentos/04-plano-con-malla/simulacion_plano_einstein.py) |
| [Superficie gravitatoria coloreada](experimentos/05-plano-superficie) | [simulacion_plano_einstein_2.py](experimentos/05-plano-superficie/simulacion_plano_einstein_2.py) |
| [Superficie de dos masas](experimentos/06-dos-masas) | [simulacion_plano_einstein_3.py](experimentos/06-dos-masas/simulacion_plano_einstein_3.py) |
| [Dos masas configurables](experimentos/07-masas-configurables) | [simulacion_plano_einstein_4.py](experimentos/07-masas-configurables/simulacion_plano_einstein_4.py) |

**[Abrir el menú de las siete simulaciones](experimentos/README.md)**

## Linux y C

| Práctica | Código o guía |
| --- | --- |
| Entrada/salida con stdio | [cat_simple.c](facultad/sistemas-operativos/entrada-salida-c/src/cat_simple.c) |
| Entrada/salida con llamadas al sistema | [cat_syscall.c](facultad/sistemas-operativos/entrada-salida-c/src/cat_syscall.c) |
| Leer un archivo | [leer_archivo.c](facultad/sistemas-operativos/entrada-salida-c/src/leer_archivo.c) |
| Redirigir la salida con dup2 | [redirigir_salida.c](facultad/sistemas-operativos/entrada-salida-c/src/redirigir_salida.c) |
| Comandos y texto | [Guía reproducible](facultad/sistemas-operativos/comandos-y-texto) |
| Permisos y respaldos | [Guía reproducible](facultad/sistemas-operativos/permisos-y-respaldos) |
| Autómata finito | [Explicación](facultad/teoria-del-lenguaje/automata-jflap) · [Archivo JFLAP](facultad/teoria-del-lenguaje/automata-jflap/automata.jff) |

## Ejecutar una muestra

En GitHub se puede leer el código y descargar los archivos. Para ejecutar una aplicación o abrir sus gráficos se utiliza la copia local del equipo y las instrucciones del proyecto. En Windows, las simulaciones tienen INICIAR.cmd o VER_SIMULACIONES.cmd. El estudio de Boa Vista se descarga con su enlace directo a Word.

Si todavía no tenés una copia local, usá **Code → Download ZIP** en la portada del repositorio y descomprimí la carpeta completa. Los lanzadores necesitan los demás archivos de su proyecto.

Los paquetes Python se instalan en el entorno virtual de cada colección. No es necesario modificar el Python global administrado por uv.

## Alcance y procedencia

La colección incluye trabajos académicos —algunos grupales—, proyectos personales y experimentos con asistencia de IA. Los README de cada muestra describen su estado. Consultorios y las prácticas mantienen sus límites de integración; el importador Excel es un prototipo y los bots son analizadores de señales. Milagros comparte únicamente un caso de estudio.

[Verificaciones realizadas](docs/VERIFICACION.md) · [Procedencia y contribuciones](docs/PROCEDENCIA.md) · [Cambios de preparación](docs/CAMBIOS-PORTFOLIO.md)
