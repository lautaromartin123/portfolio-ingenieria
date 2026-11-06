# Permisos y respaldos

Ejemplo preparado con un directorio de práctica:

```sh
mkdir -p laboratorio-permisos
chmod 700 laboratorio-permisos
cd laboratorio-permisos
umask 077
printf 'contenido de prueba
' > ejemplo.txt
chmod 600 ejemplo.txt
stat -c '%A %a %n' ejemplo.txt
sha256sum ejemplo.txt > integridad.sha256
sha256sum -c integridad.sha256
tar -czf ../respaldo-laboratorio.tar.gz ejemplo.txt integridad.sha256
tar -tzf ../respaldo-laboratorio.tar.gz
```

Estas instrucciones acompañan prácticas de permisos, integridad y compresión; no incluyen datos ni configuración del servidor de la facultad. Un hash permite comprobar integridad del contenido, no sustituye autenticación o cifrado.
