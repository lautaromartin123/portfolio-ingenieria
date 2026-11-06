<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetalleTurnoMedico.aspx.cs" Inherits="Consultorios.DetalleTurnoMedico" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Detalle del turno</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server" class="contenedor">
    <div class="encabezado">
        <h2>Detalle del turno</h2>
        <p class="ayuda">Consulta los datos de la cita y registra el resultado de la atencion.</p>
    </div>
    <p><asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" /></p>
    <asp:Panel ID="pnlContenido" runat="server" Visible="false">
        <div class="grupo-accesos">
<div class="tarjeta datos">
            <h3>Turno <asp:Literal ID="litIdTurno" runat="server" Mode="Encode" /></h3>
            <p><strong>Fecha y hora:</strong> <asp:Literal ID="litFecha" runat="server" Mode="Encode" /> (Argentina)</p>
            <p><strong>Duracion:</strong> <asp:Literal ID="litDuracion" runat="server" Mode="Encode" /> minutos</p>
            <p><strong>Estado:</strong> <asp:Literal ID="litEstado" runat="server" Mode="Encode" /></p>
            <p><strong>Motivo del turno:</strong> <asp:Literal ID="litMotivo" runat="server" Mode="Encode" /></p>
        </div>
        <div class="tarjeta datos">
            <h3>Datos del paciente</h3>
            <p><strong>Paciente:</strong> <asp:Literal ID="litPaciente" runat="server" Mode="Encode" /></p>
            <p><strong>IdPaciente:</strong> <asp:Literal ID="litIdPaciente" runat="server" Mode="Encode" /></p>
            <p><strong>Telefono:</strong> <asp:Literal ID="litTelefono" runat="server" Mode="Encode" /></p>
            <p><strong>Cobertura del turno:</strong> <asp:Literal ID="litCobertura" runat="server" Mode="Encode" /></p>
            <p><strong>Numero de afiliado:</strong> <asp:Literal ID="litAfiliado" runat="server" Mode="Encode" /></p>
            <p><strong>Cobertura actual:</strong> <asp:Literal ID="litCoberturaActual" runat="server" Mode="Encode" /></p>
        </div>
        </div>
        <asp:Panel ID="pnlResultadoGuardado" runat="server" Visible="false" CssClass="tarjeta">
            <h3>Resultado registrado</h3>
            <p class="ayuda">Este turno esta cerrado. El resultado y las observaciones son de solo lectura.</p>
            <h4>Observaciones</h4>
            <p class="observaciones"><asp:Literal ID="litObservacionGuardada" runat="server" Mode="Encode" /></p>
        </asp:Panel>
        <asp:Panel ID="pnlRegistro" runat="server" CssClass="tarjeta formulario">
            <h3>Resultado de la atencion</h3>
            <p><asp:Label ID="lblAviso" runat="server" /></p>
            <asp:Label runat="server" AssociatedControlID="rblResultado" Text="Indica si el turno se realizo:" />
            <asp:RadioButtonList ID="rblResultado" runat="server" RepeatLayout="Flow" CssClass="especialidades">
                <asp:ListItem Value="Atendido">Se realizo (OK)</asp:ListItem>
                <asp:ListItem Value="Ausente">No se realizo (Ausente)</asp:ListItem>
            </asp:RadioButtonList>
            <p>
                <asp:Label runat="server" AssociatedControlID="txtObservacion" Text="Observaciones (obligatorias, hasta 500 caracteres):" />
                <asp:TextBox ID="txtObservacion" runat="server" TextMode="MultiLine" Rows="6" CssClass="entrada" />
            </p>
            <p class="ayuda">Al guardar, el turno se cierra y las observaciones ya no se podran modificar. Si se realizo, quedan visibles para el paciente en su historial de atenciones.</p>
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar resultado" CssClass="boton" OnClick="GuardarResultado"
                OnClientClick="return confirm('Guardar y cerrar este turno? Despues solo podras consultarlo.');" />
        </asp:Panel>
        <asp:Button ID="btnActualizar" runat="server" Text="Actualizar turno" CssClass="boton boton-secundario" OnClick="Actualizar" />
    </asp:Panel>
    <p class="navegacion"><asp:HyperLink ID="lnkVolver" runat="server" NavigateUrl="~/MiAgendaMedico.aspx" Text="Volver a mi agenda" /></p>
</form>
</body>
</html>
