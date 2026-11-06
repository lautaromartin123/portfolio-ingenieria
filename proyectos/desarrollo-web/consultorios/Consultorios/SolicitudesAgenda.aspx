<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SolicitudesAgenda.aspx.cs" Inherits="Consultorios.SolicitudesAgenda" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Solicitudes de horarios</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server" class="contenedor">
<div class="encabezado">
<h2>Solicitudes de horarios de medicos</h2>
<p class="ayuda">Revisa la disponibilidad actual y la propuesta antes de resolver cada solicitud.</p>
</div>
<p><asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" /></p>
<asp:Button ID="btnActualizar" runat="server" Text="Actualizar listado" OnClick="Actualizar" CssClass="boton boton-secundario" />
<p class="ayuda">Una aprobacion modifica la disponibilidad semanal, sin cancelar ni reprogramar turnos existentes. Si quedan turnos fuera del horario, se impide aprobar.</p>
<asp:Repeater ID="rptSolicitudes" runat="server" OnItemCommand="Resolver">
<ItemTemplate><div class="solicitud formulario">
    <h3>Solicitud <%#: Eval("IdSolicitud") %> - <%#: Eval("Profesional") %></h3>
    <p>Operacion: <%#: Eval("Operacion") %> - Fecha de solicitud: <%#: Eval("FechaSolicitud","{0:dd/MM/yyyy HH:mm}") %> UTC</p>
    <div class="tabla-contenedor">
    <table class="comparacion">
        <tr><th>Dato</th><th>Actual</th><th>Propuesto</th></tr>
        <tr><td>Dia (1=Lunes, 7=Domingo)</td><td><%#: Eval("DiaActual") %></td><td><%#: Eval("DiaSemana") %></td></tr>
        <tr><td>Horario</td><td><%#: Eval("HorarioActual") %></td><td><%#: Eval("Horario") %></td></tr>
        <tr><td>Minutos por turno</td><td><%#: Eval("DuracionActual") %></td><td><%#: Eval("DuracionMinutos") %></td></tr>
    </table>
    </div>
    <p class="ayuda">Para una baja, la franja dejara de estar activa.</p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtMotivo" Text="Respuesta / motivo de rechazo:" />
    <asp:TextBox ID="txtMotivo" runat="server" TextMode="MultiLine" Rows="3" CssClass="entrada" />
    </p>
    <asp:Button ID="btnAprobar" runat="server" Text="Aprobar horario" CommandName="Aprobar" CommandArgument='<%# Eval("IdSolicitud") %>'
        CssClass="boton" OnClientClick="return confirm('Aprobar este cambio de horario?');" />
    <asp:Button ID="btnRechazar" runat="server" Text="Rechazar horario" CommandName="Rechazar" CommandArgument='<%# Eval("IdSolicitud") %>' CssClass="boton boton-rechazar" />
</div></ItemTemplate>
</asp:Repeater>
<asp:Label ID="lblSinSolicitudes" runat="server" Visible="false" CssClass="mensaje" Text="No hay solicitudes de horarios pendientes." />
<p class="navegacion"><a href="PanelAdmin.aspx">Volver al panel</a></p>
</form>
</body>
</html>
