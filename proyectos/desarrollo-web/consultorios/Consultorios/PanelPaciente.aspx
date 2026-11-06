<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PanelPaciente.aspx.cs" Inherits="Consultorios.PanelPaciente" ResponseEncoding="utf-8" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Paciente</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>

<body>
    <form id="form1" runat="server" class="contenedor">
        <div class="encabezado">
            <h2>Modulo Paciente</h2>
            <asp:Label ID="lblUsuario" runat="server" CssClass="usuario">
            </asp:Label>
            <p class="ayuda">Consulta tus datos, solicita un turno y revisa tus atenciones.</p>
        </div>

        <div class="grupo-accesos">
            <div class="tarjeta menu">
                <h3>Mis datos</h3>
                <p class="ayuda">
                    <asp:Label ID="lblDatosPaciente" runat="server" />
                </p>
                <p>
                    <asp:HyperLink ID="lnkMisDatos" runat="server"
                        NavigateUrl="~/MisDatosPaciente.aspx" Text="Mis datos" />
                </p>
            </div>

            <div class="tarjeta menu">
                <h3>Turnos y atenciones</h3>
                <p>
                    <asp:HyperLink ID="lnkMisTurnos" runat="server"
                        NavigateUrl="~/MisTurnosPaciente.aspx" Text="Mi agenda y solicitar turno" />
                </p>
                <p>
                    <asp:HyperLink ID="lnkHistorial" runat="server"
                        NavigateUrl="~/MisTurnosPaciente.aspx?vista=historial" Text="Historial de atenciones" />
                </p>
            </div>
        </div>

        <div class="tarjeta acceso-ia">
            <h3>Asistente para turnos</h3>
            <p class="ayuda">Elegi las opciones del asistente para buscar y solicitar tu turno.</p>
            <a runat="server" href="~/TurnosIA/PanelIAPaciente.aspx" class="boton boton-ia">
                Sacar turno con asistente IA
            </a>
        </div>

        <asp:Button ID="btnCerrarSesion" runat="server" Text="Cerrar sesion"
            OnClick="btnCerrarSesion_Click"
            CssClass="boton boton-secundario" />
    </form>
</body>
</html>
