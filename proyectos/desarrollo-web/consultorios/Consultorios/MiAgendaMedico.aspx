<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MiAgendaMedico.aspx.cs" Inherits="Consultorios.MiAgendaMedico" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Mi agenda</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server" class="contenedor">
<div class="encabezado">
<h2><asp:Literal ID="litTitulo" runat="server" Mode="Encode" /></h2>
<p class="ayuda">Organiza tus turnos y consulta las atenciones que ya registraste.</p>
</div>
<p class="navegacion"><a href="MiAgendaMedico.aspx">Agenda</a><a href="MiAgendaMedico.aspx?vista=historial">Historial de turnos</a></p>
<p><asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" /></p>
<asp:Panel ID="pnlContenido" runat="server">
<div class="tarjeta">
    <h3><asp:Literal ID="litListado" runat="server" Mode="Encode" /></h3>
    <p class="ayuda">Las fechas y horas corresponden a Argentina. Los turnos cerrados se consultan en Historial de turnos, sin permitir cambios.</p>
    <div class="formulario">
    <asp:Label runat="server" AssociatedControlID="txtFiltro" Text="Filtrar por fecha:" />
    <asp:TextBox ID="txtFiltro" runat="server" TextMode="Date" CssClass="entrada" />
    <asp:Button ID="btnFiltrar" runat="server" Text="Ver dia" OnClick="Filtrar" CssClass="boton" />
    <asp:Button ID="btnTodos" runat="server" Text="Ver todos mis turnos" OnClick="VerTodos" CssClass="boton boton-secundario" />
    </div>
    <div class="tabla-contenedor">
    <asp:GridView ID="gvTurnos" runat="server" AutoGenerateColumns="false" CssClass="comparacion" EmptyDataText="No hay turnos para mostrar.">
        <Columns>
            <asp:BoundField DataField="IdTurno" HeaderText="Turno" />
            <asp:BoundField DataField="FechaHora" HeaderText="Fecha y hora" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
            <asp:BoundField DataField="Paciente" HeaderText="Paciente" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:BoundField DataField="ObraSocial" HeaderText="Obra social" />
            <asp:BoundField DataField="Afiliado" HeaderText="Afiliado" NullDisplayText="NULL" />
            <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
            <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
            <asp:HyperLinkField HeaderText="Detalle" Text="Ver turno"
                DataNavigateUrlFields="IdTurno" DataNavigateUrlFormatString="DetalleTurnoMedico.aspx?id={0}" />
        </Columns>
    </asp:GridView>
    </div>
</div>
<asp:Panel ID="pnlNuevoTurno" runat="server" CssClass="tarjeta formulario">
    <h3>Agregar turno</h3>
    <p class="ayuda">Solo se pueden usar tus horarios aprobados. El turno se carga como Pendiente.</p>
    <div class="campos">
<p>
    <asp:Label runat="server" AssociatedControlID="ddlPaciente" Text="Paciente y cobertura:" />
    <asp:DropDownList ID="ddlPaciente" runat="server" CssClass="entrada" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtFecha" Text="Fecha:" />
    <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="entrada" />
    <asp:Button ID="btnBuscar" runat="server" Text="Buscar horarios libres" OnClick="BuscarHorarios" CssClass="boton boton-secundario" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="ddlTurno" Text="Horario disponible:" />
    <asp:DropDownList ID="ddlTurno" runat="server" CssClass="entrada" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtMotivo" Text="Motivo de consulta (opcional):" />
    <asp:TextBox ID="txtMotivo" runat="server" MaxLength="300" CssClass="entrada" />
    </p>
            </div>
    <asp:Button ID="btnReservar" runat="server" Text="Agregar turno" OnClick="Reservar" CssClass="boton" />
</asp:Panel>
</asp:Panel>
<p class="navegacion"><a href="MiDisponibilidadMedico.aspx">Mi disponibilidad semanal</a><a href="PanelMedico.aspx">Volver al panel</a></p>
</form>
</body>
</html>
