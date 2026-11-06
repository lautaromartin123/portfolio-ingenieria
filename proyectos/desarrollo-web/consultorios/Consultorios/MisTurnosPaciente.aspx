<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MisTurnosPaciente.aspx.cs" Inherits="Consultorios.MisTurnosPaciente" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Mis turnos</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server" class="contenedor">
<div class="encabezado">
<h2>Mis turnos</h2>
<p class="ayuda">Consulta tus proximos turnos, solicita uno nuevo o revisa tu historial.</p>
</div>
<p class="navegacion"><a href="MisTurnosPaciente.aspx">Mi agenda y proximos turnos</a><a href="MisTurnosPaciente.aspx?vista=historial">Historial de atenciones</a></p>
<p><asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" /></p>
<asp:Panel ID="pnlContenido" runat="server">
<div class="tarjeta">
<h3><asp:Literal ID="litTitulo" runat="server" Mode="Encode" /></h3>
<p class="ayuda">Las fechas y horas corresponden a Argentina. El historial muestra las atenciones realizadas, con las observaciones del medico.</p>
<div class="tabla-contenedor">
<asp:GridView ID="gvTurnos" runat="server" AutoGenerateColumns="false" CssClass="comparacion" EmptyDataText="No hay turnos para mostrar.">
    <Columns>
        <asp:BoundField DataField="IdTurno" HeaderText="Turno" />
        <asp:BoundField DataField="FechaHora" HeaderText="Fecha y hora" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
        <asp:BoundField DataField="Profesional" HeaderText="Medico" />
        <asp:BoundField DataField="Estado" HeaderText="Estado" />
        <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
        <asp:BoundField DataField="Observaciones" HeaderText="Observaciones del medico" />
        <asp:BoundField DataField="ObraSocial" HeaderText="Obra social" />
        <asp:BoundField DataField="Afiliado" HeaderText="Afiliado" NullDisplayText="NULL" />
        <asp:BoundField DataField="TipoCobertura" HeaderText="Cobertura informada" />
    </Columns>
</asp:GridView></div>
</div>
<asp:Panel ID="pnlReserva" runat="server" CssClass="tarjeta formulario">
    <h3>Solicitar turno</h3>
    <p><asp:Literal ID="litCobertura" runat="server" Mode="Encode" /></p>
    <p class="ayuda">El turno quedara Pendiente y el horario reservado. La cobertura se conserva tal como figura al solicitarlo. Para corregir tus datos, entra a Mis datos.</p>
    <div class="campos">
<p>
    <asp:Label runat="server" AssociatedControlID="ddlProfesional" Text="Medico:" />
    <asp:DropDownList ID="ddlProfesional" runat="server" CssClass="entrada" AutoPostBack="true" OnSelectedIndexChanged="CambiarProfesional" />
    </p>
    <p class="ancho-completo"><asp:Literal ID="litEspecialidades" runat="server" Mode="Encode" /></p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtFecha" Text="Fecha:" />
    <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="entrada" />
    <asp:Button ID="btnBuscar" runat="server" Text="Buscar horarios libres" OnClick="BuscarHorarios" CssClass="boton boton-secundario" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="ddlHorario" Text="Horario disponible:" />
    <asp:DropDownList ID="ddlHorario" runat="server" CssClass="entrada" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtMotivo" Text="Motivo de consulta (opcional):" />
    <asp:TextBox ID="txtMotivo" runat="server" MaxLength="300" CssClass="entrada" />
    </p>
            </div>
    <asp:Button ID="btnSolicitar" runat="server" Text="Solicitar turno" OnClick="Solicitar" CssClass="boton" />
</asp:Panel>
</asp:Panel>
<p class="navegacion"><a href="PanelPaciente.aspx">Volver al panel</a></p>
</form>
</body>
</html>
