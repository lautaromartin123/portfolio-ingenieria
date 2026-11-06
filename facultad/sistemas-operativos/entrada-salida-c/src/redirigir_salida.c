#include <fcntl.h>
#include <unistd.h>
#include <stdlib.h>

int main() {

    int fd;

    // 1. abrir/crear archivo
    fd = open("salida.txt", O_CREAT | O_WRONLY | O_TRUNC, 0644);

    if (fd == -1) {
        write(1, "Error al abrir archivo\n", sizeof("Error al abrir archivo\n") - 1);
        return 1;
    }

    // 2. redirigir stdout al archivo
    dup2(fd, 1);

    // 3. escribir (ahora va al archivo)
    write(1, "Hola, esto ya no se ve en pantalla\n",
          sizeof("Hola, esto ya no se ve en pantalla\n") - 1);

    write(1, "Linea 2\n", sizeof("Linea 2\n") - 1);

    // 4. cerrar archivo
    close(fd);

    return 0;
}
