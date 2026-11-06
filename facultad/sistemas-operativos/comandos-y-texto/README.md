# Comandos y procesamiento de texto

Guía reproducible preparada para acompañar las prácticas de terminal. No es una transcripción del servidor de clase.

```sh
mkdir -p laboratorio-texto
cd laboratorio-texto
printf 'nombre,nota
Ana,8
Luis,6
Eva,9
' > notas.csv
cat notas.csv
grep ',9$' notas.csv
tail -n +2 notas.csv | sort -t, -k2,2n
cut -d, -f1 notas.csv
find . -type f -name '*.csv'
ps -u "$(id -un)"
```

Conceptos: archivos de texto, patrones, pipelines, delimitadores y procesos. Comparar una salida con otra y explicar por qué cambia es parte del ejercicio.
