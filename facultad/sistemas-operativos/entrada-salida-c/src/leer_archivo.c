#include <fcntl.h>
#include <unistd.h>
#include <stdlib.h>

int main() {

   int fd;
   char buffer[100];
   int bytes_leidos;

   fd = open("entrada.txt",O_RDONLY);

   if(fd == -1){
      write(1,"Error al abrir archivo \n",sizeof("Error al abrir archivo \n")-1);
      return 1;}


   while((bytes_leidos = read(fd,buffer,sizeof(buffer)))>0){
     write(1,buffer,bytes_leidos);
     }
   if(bytes_leidos == -1){
     write(1,"Error al leer el archivo \n", sizeof("Error al leer el archivo \n") -1);
     close(fd);
     return 1;
}

 close(fd);
 return 0;



}
