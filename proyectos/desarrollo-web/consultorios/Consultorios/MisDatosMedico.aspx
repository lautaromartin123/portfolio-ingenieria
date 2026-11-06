<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MisDatosMedico.aspx.cs" Inherits="Consultorios.MisDatosMedico" ResponseEncoding="utf-8" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Mis datos de medico</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server" class="contenedor">

        <div class="encabezado">
            <h2>Mis datos de medico</h2>
            <p class="ayuda">Consulta tus datos, especialidades y matriculas registradas.</p>
        </div>

        <p>
            <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" />
        </p>

        <asp:Panel ID="pnlDatos" runat="server"
            Visible="false"
            CssClass="tarjeta datos">

            <h3>Datos actuales</h3>

            <div class="campos">
            <p>
                <strong>Nombre:</strong>
                <asp:Literal ID="litNombre" runat="server" Mode="Encode" />
            </p>
            <p>
                <strong>Apellido:</strong>
                <asp:Literal ID="litApellido" runat="server" Mode="Encode" />
            </p>
            <p class="ancho-completo">
                <strong>Especialidades:</strong>
                <asp:Literal ID="litEspecialidad" runat="server" Mode="Encode" />
            </p>
            </div>

            <h3>Matriculas</h3>
            <asp:Label ID="lblSinMatricula" runat="server" Visible="false" Text="NULL" />
            <asp:Repeater ID="rptMatriculas" runat="server">
                <ItemTemplate>
                    <div class="tarjeta">
                        <p>Numero: <%#: Eval("Numero") %></p>
                        <p>Tipo: <%#: Eval("TipoMatricula.Descripcion") %></p>
                        <p>Entidad emisora: <%#: MostrarValor(Eval("EntidadEmisora")) %></p>
                        <p>Vencimiento: <%#: MostrarFecha(Eval("FechaVencimiento")) %></p>
                        <p>Estado: <%#: Eval("Estado.Descripcion") %></p>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <p class="ayuda">Una matricula registrada solo puede ser cambiada por un administrador, con motivo e historial.</p>

        </asp:Panel>

        <asp:Panel ID="pnlSolicitud" runat="server"
            Visible="false"
            CssClass="tarjeta formulario">

            <h3>Solicitar modificacion</h3>

            <p class="ayuda">
                Los cambios quedaran pendientes de aprobacion.
                Tus datos actuales se conservaran hasta que se aprueben.
            </p>

            <div class="campos">
<p>
                <asp:Label runat="server"
                    AssociatedControlID="txtNombre" Text="Nombre:" />
                <asp:TextBox ID="txtNombre" runat="server"
                    MaxLength="100"
                    CssClass="entrada" />
            </p>
            <p>
                <asp:Label runat="server"
                    AssociatedControlID="txtApellido" Text="Apellido:" />
                <asp:TextBox ID="txtApellido" runat="server"
                    MaxLength="100"
                    CssClass="entrada" />
            </p>
            </div>
            <p>
                <asp:Label runat="server"
                    AssociatedControlID="cblEspecialidades" Text="Especialidades:" />
                <asp:CheckBoxList ID="cblEspecialidades" runat="server"
                    RepeatLayout="Flow" RepeatDirection="Vertical"
                    CssClass="especialidades" />
            </p>

            <asp:Panel ID="pnlMatriculaInicial" runat="server" Visible="false" CssClass="tarjeta">
                <h3>Cargar primera matricula</h3>
                <p class="ayuda">Si la informas, se registrara cuando un administrador apruebe esta solicitud.</p>
                <div class="campos">
<p>
                    <asp:Label runat="server" AssociatedControlID="txtNumeroMatricula" Text="Numero de matricula:" />
                    <asp:TextBox ID="txtNumeroMatricula" runat="server" MaxLength="50" CssClass="entrada" />
                </p>
                <p>
                    <asp:Label runat="server" AssociatedControlID="ddlTipoMatricula" Text="Tipo de matricula:" />
                    <asp:DropDownList ID="ddlTipoMatricula" runat="server" CssClass="entrada" />
                </p>
                <p>
                    <asp:Label runat="server" AssociatedControlID="txtEntidadEmisora" Text="Entidad emisora:" />
                    <asp:TextBox ID="txtEntidadEmisora" runat="server" MaxLength="100" CssClass="entrada" />
                </p>
                <p>
                    <asp:Label runat="server" AssociatedControlID="txtVencimiento" Text="Vencimiento (opcional):" />
                    <asp:TextBox ID="txtVencimiento" runat="server" TextMode="Date" CssClass="entrada" />
                </p>
            </div>
            </asp:Panel>

            <asp:Button ID="btnEnviarSolicitud" runat="server"
                Text="Enviar solicitud" CssClass="boton"
                OnClick="btnEnviarSolicitud_Click" />

        </asp:Panel>

        <p class="navegacion">
        <asp:HyperLink ID="lnkVolver" runat="server"
            NavigateUrl="~/PanelMedico.aspx" Text="Volver al panel" />
        </p>

    </form>
</body>
</html>
