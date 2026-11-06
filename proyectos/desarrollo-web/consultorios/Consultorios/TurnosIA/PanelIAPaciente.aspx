<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="PanelIAPaciente.aspx.cs" Inherits="Consultorios.PanelIAPaciente" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Asistente de turnos</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />

</head>

<body>
<form id="form1" runat="server">
    <div class="pagina-chat">

        <div class="cabecera-chat">
            <a runat="server" class="volver" href="~/PanelPaciente.aspx">Volver al panel</a>
            <h1>Asistente de turnos</h1>
            <p class="subtitulo">
                Podés sacar, consultar o cancelar tus turnos desde el chat.
            </p>
            <asp:Label ID="lblUsuario" runat="server" CssClass="usuario" />
            <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje-error" />
        </div>

        <asp:Panel ID="pnlChat" runat="server" CssClass="panel-chat">

            <div id="historialChat" class="historial">
                <asp:Repeater ID="rptMensajes" runat="server">
                    <ItemTemplate>
                        <div class='<%# Eval("Rol").ToString() == "Paciente" ? "fila-mensaje paciente" : "fila-mensaje asistente" %>'>
                            <div class="burbuja-mensaje"><%#: Eval("Texto") %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:Panel ID="pnlOpciones" runat="server" CssClass="opciones">
                <asp:Repeater
                    ID="rptOpciones"
                    runat="server"
                    OnItemCommand="rptOpciones_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton
                            ID="btnOpcion"
                            runat="server"
                            CssClass="opcion-chat"
                            CommandName="Elegir"
                            CommandArgument='<%# Eval("Valor") %>'
                            CausesValidation="false"
                            Text='<%# Eval("Texto") %>' />
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>

            <div class="entrada-chat">
                <asp:TextBox
                    ID="txtMensaje"
                    runat="server"
                    TextMode="MultiLine"
                    MaxLength="500"
                    placeholder="Escribí, por ejemplo: Quiero sacar un turno la semana que viene..." AutoPostBack="true" OnTextChanged="txtMensaje_TextChanged" />

                <asp:Button
                    ID="btnEnviar"
                    runat="server"
                    Text="Enviar"
                    CssClass="btn-enviar"
                    OnClick="btnEnviar_Click" />
            </div>

            <div class="nota-seguridad">
                Este asistente gestiona turnos. No realiza diagnósticos ni indica medicamentos o tratamientos.
            </div>

        </asp:Panel>
    </div>

    <script type="text/javascript">
        (function () {
            var historial = document.getElementById('historialChat');
            if (historial) {
                historial.scrollTop = historial.scrollHeight;
            }
        })();
    </script>
</form>
</body>
</html>
