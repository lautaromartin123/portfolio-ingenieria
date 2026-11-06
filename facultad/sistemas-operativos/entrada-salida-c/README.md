# Entrada/salida en C

| Archivo | Concepto |
| --- | --- |
| `src/cat_simple.c` | Lectura/escritura con stdio |
| `src/cat_syscall.c` | read/write sobre stdin/stdout |
| `src/leer_archivo.c` | open, descriptor, lectura por bloques y close |
| `src/redirigir_salida.c` | Creación de archivo y dup2 |

Desde esta carpeta, con GCC instalado:

```sh
mkdir -p bin
gcc -Wall -Wextra -Wpedantic src/cat_simple.c -o bin/cat_simple
gcc -Wall -Wextra -Wpedantic src/cat_syscall.c -o bin/cat_syscall
printf 'Ejemplo de entrada
' | ./bin/cat_simple
printf 'Ejemplo de entrada
' | ./bin/cat_syscall
gcc -Wall -Wextra -Wpedantic src/leer_archivo.c -o bin/leer_archivo
./bin/leer_archivo
gcc -Wall -Wextra -Wpedantic src/redirigir_salida.c -o bin/redirigir_salida
./bin/redirigir_salida
cat salida.txt
```

El lector usa `entrada.txt` en el directorio actual. El ejercicio de redirección crea o reemplaza `salida.txt`. Queda mejorar el tratamiento de escrituras parciales y los retornos de todas las llamadas. Estos fuentes no son ejercicios de fork, hilos o IPC.
