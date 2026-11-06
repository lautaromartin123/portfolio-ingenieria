#include <unistd.h>

int main(){
char buffer [200];
int bytes;
while ((bytes = read(0,buffer,sizeof(buffer)))>0){
write(1,buffer,	bytes);
}
return 0;

}
