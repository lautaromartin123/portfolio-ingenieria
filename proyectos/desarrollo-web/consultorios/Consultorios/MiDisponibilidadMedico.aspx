<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MiDisponibilidadMedico.aspx.cs" Inherits="Consultorios.MiDisponibilidadMedico" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Mi disponibilidad semanal</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server" class="contenedor">
<div class="encabezado">
<h2>Mi disponibilidad semanal</h2>
<p class="ayuda">Revisa tus franjas de atencion y solicita los cambios al administrador.</p>
</div>
<p><asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" /></p>
<asp:Panel ID="pnlContenido" runat="server">
<div class="tarjeta">
    <h3>Horarios actuales</h3>
    <p class="ayuda">Los horarios actuales siguen vigentes hasta que un admin apruebe el cambio. Las bajas tambien requieren aprobacion. Solo puede haber una solicitud pendiente de datos u horarios a la vez.</p>
    <div class="tabla-contenedor"><asp:GridView ID="gvAgenda" runat="server" AutoGenerateColumns="false" CssClass="comparacion" EmptyDataText="No tenes horarios cargados.">
        <Columns>
            <asp:BoundField DataField="NombreDia" HeaderText="Dia" />
            <asp:BoundField DataField="Horario" HeaderText="Horario" />
            <asp:BoundField DataField="DuracionMinutos" HeaderText="Minutos por turno" />
            <asp:TemplateField HeaderText="Estado"><ItemTemplate><%#: Eval("Estado.Descripcion") %></ItemTemplate></asp:TemplateField>
        </Columns>
    </asp:GridView></div>
</div>
<div class="tarjeta formulario">
    <h3>Solicitar cambio de horario</h3>
    <div class="campos">
<p>
    <asp:Label runat="server" AssociatedControlID="ddlAgenda" Text="Horario a solicitar o modificar:" />
    <asp:DropDownList ID="ddlAgenda" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ElegirAgenda" CssClass="entrada" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="ddlDia" Text="Dia de la semana:" />
    <asp:DropDownList ID="ddlDia" runat="server" CssClass="entrada">
        <asp:ListItem Value="1">Lunes</asp:ListItem><asp:ListItem Value="2">Martes</asp:ListItem>
        <asp:ListItem Value="3">Miercoles</asp:ListItem><asp:ListItem Value="4">Jueves</asp:ListItem>
        <asp:ListItem Value="5">Viernes</asp:ListItem><asp:ListItem Value="6">Sabado</asp:ListItem><asp:ListItem Value="7">Domingo</asp:ListItem>
    </asp:DropDownList>
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtInicio" Text="Desde:" />
    <asp:TextBox ID="txtInicio" runat="server" TextMode="Time" CssClass="entrada" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtFin" Text="Hasta:" />
    <asp:TextBox ID="txtFin" runat="server" TextMode="Time" CssClass="entrada" />
    </p>
    <p>
    <asp:Label runat="server" AssociatedControlID="txtDuracion" Text="Duracion por turno (5 a 240 minutos):" />
    <asp:TextBox ID="txtDuracion" runat="server" TextMode="Number" Text="30" CssClass="entrada" />
    </p>
            </div>
    <p class="especialidades"><asp:CheckBox ID="chkBaja" runat="server" Text="Solicitar baja de esta franja" /></p>
    <asp:Button ID="btnSolicitar" runat="server" Text="Enviar solicitud de horario" OnClick="SolicitarHorario" CssClass="boton" />
</div>
<div class="tarjeta">
    <h3>Mis solicitudes de horarios</h3>
    <div class="tabla-contenedor"><asp:GridView ID="gvSolicitudes" runat="server" AutoGenerateColumns="false" CssClass="comparacion" EmptyDataText="No hay solicitudes de horarios.">
        <Columns>
            <asp:BoundField DataField="IdSolicitud" HeaderText="Solicitud" />
            <asp:BoundField DataField="Operacion" HeaderText="Operacion" />
            <asp:BoundField DataField="DiaSemana" HeaderText="Dia (1=Lunes)" />
            <asp:BoundField DataField="Horario" HeaderText="Horario propuesto" />
            <asp:BoundField DataField="Estado" HeaderText="Estado" />
            <asp:BoundField DataField="ObservacionRevision" HeaderText="Respuesta del admin" />
        </Columns>
    </asp:GridView></div>
</div>
</asp:Panel>
<p class="navegacion"><a href="MiAgendaMedico.aspx">Ver mi agenda</a><a href="PanelMedico.aspx">Volver al panel</a></p>
</form>
</body>
</html>
