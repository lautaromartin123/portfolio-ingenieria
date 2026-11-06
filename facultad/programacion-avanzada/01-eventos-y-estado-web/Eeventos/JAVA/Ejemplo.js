function mostrarMensaje() {
    var texto = document.getElementById('<%= TxJava.ClientID %>').value;
    alert("¡Hola! " + texto + " Has hecho clic en el botón.");
}
