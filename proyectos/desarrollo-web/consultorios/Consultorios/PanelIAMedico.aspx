<%@ Page Language="C#"
    Async="true"
    AutoEventWireup="true"
    CodeBehind="PanelIAMedico.aspx.cs"
    Inherits="Consultorios.PanelIAMedico" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>
        IA y Reportes Médicos
    </title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />

    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>

<form
    id="form1"
    runat="server"
    class="contenedor">

    <div class="encabezado">
        <h2>
        IA y Reportes Médicos
    </h2>

    <asp:Label
        ID="lblUsuario"
        runat="server"

        CssClass="usuario">
    </asp:Label>
</div>

    <asp:Label
        ID="lblMensaje"
        runat="server"

        CssClass="mensaje">
    </asp:Label>

    <asp:Panel
        ID="pnlContenido"
        runat="server"
        Visible="false">

        <div class="resumen-ia">
            <asp:Panel
            ID="pnlDiaMasAtendido"
            runat="server"

            CssClass="tarjeta">

            <h3>
                Día con más pacientes atendidos
            </h3>

            <asp:Label
                ID="lblDiaMasAtendido"
                runat="server">
            </asp:Label>

        </asp:Panel>

        <asp:Panel
            ID="pnlHorarios"
            runat="server"

            CssClass="tarjeta">

            <h3>
                Días y horarios de trabajo
            </h3>

            <div class="resumen-texto">
                <asp:Literal
                ID="litHorarios"
                runat="server">
            </asp:Literal>
            </div>

        </asp:Panel>
        </div>

        <asp:Panel
            ID="pnlAgenda"
            runat="server"

            CssClass="tarjeta">

            <h3>
                Agenda de la próxima semana
            </h3>

            <asp:Label
                ID="lblPeriodo"
                runat="server">
            </asp:Label>

            <div class="resumen-texto">
                <asp:Literal
                ID="litAgenda"
                runat="server">
            </asp:Literal>
            </div>

        </asp:Panel>

        <asp:Panel
            ID="pnlIA"
            runat="server"

            CssClass="tarjeta formulario">

            <h3>
                Informe generado con IA
            </h3>

            <p>
                Ollama analizará las estadísticas
                y la agenda para generar un informe operativo.
            </p>

            <asp:Button
                ID="btnGenerarIA"
                runat="server"
                Text="Generar reporte con IA"
                CssClass="boton"
                OnClick="BtnGenerarIA_Click" />

            <asp:TextBox
                ID="txtReporteIA"
                runat="server"
                TextMode="MultiLine"
                ReadOnly="true"
                Rows="14"
                Width="100%"
                CssClass="reporte-ia">
            </asp:TextBox>

        </asp:Panel>

        <asp:Panel
            ID="pnlChatIA"
            runat="server"

            CssClass="tarjeta formulario">

            <h3>
                Asistente IA
            </h3>

            <p>
                Podés consultar sobre tus horarios,
                turnos, agenda y estadísticas.
            </p>

            <asp:TextBox
                ID="txtHistorialChat"
                runat="server"
                TextMode="MultiLine"
                ReadOnly="true"
                Rows="16"
                Width="100%"
                CssClass="reporte-ia">
            </asp:TextBox>

            <asp:Label
                ID="lblPreguntaIA"
                runat="server"
                Text="Pregunta:"
                CssClass="campo-label">
            </asp:Label>

            <asp:TextBox
                ID="txtPreguntaIA"
                runat="server"
                TextMode="MultiLine"
                Rows="3"
                Width="100%"
                CssClass="entrada">
            </asp:TextBox>

            <asp:Button
                ID="btnPreguntarIA"
                runat="server"
                Text="Preguntar a la IA"
                CssClass="boton"
                OnClick="BtnPreguntarIAEjecutivo_Click" />

            <asp:Button
                ID="btnLimpiarChat"
                runat="server"
                Text="Limpiar conversación"
                OnClick="BtnLimpiarChat_Click"
                CssClass="boton boton-secundario" />

        </asp:Panel>

    </asp:Panel>

    <p class="navegacion">
        <asp:HyperLink
        ID="lnkVolver"
        runat="server"
        NavigateUrl="~/PanelMedico.aspx"
        Text="Volver al panel médico">
    </asp:HyperLink>
    </p>

</form>

</body>

</html>
