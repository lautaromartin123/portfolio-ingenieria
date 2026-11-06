<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MisDatosPaciente.aspx.cs" Inherits="Consultorios.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Mis datos de paciente</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server" class="contenedor">

        <div class="encabezado">
            <h2>Mis datos de paciente</h2>
            <p class="ayuda">Revisa tu informacion y solicita los cambios que necesites.</p>
        </div>

        <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" />

        <asp:Panel ID="pnlDatos"
            runat="server"
            Visible="false"

            CssClass="tarjeta datos">

            <h3>Datos actuales</h3>

            <div class="campos">
            <p>
                <strong>Nombre:</strong>
                <asp:Literal ID="litNombre"
                    runat="server" Mode="Encode" />
            </p>

            <p>
                <strong>Apellido:</strong>
                <asp:Literal ID="litApellido"
                    runat="server" Mode="Encode" />
            </p>

            <p>
                <strong>Telefono:</strong>
                <asp:Literal ID="litTelefono"
                    runat="server" Mode="Encode" />
            </p>

            <p>
                <strong>Obra social:</strong>
                <asp:Literal ID="litObraSocial"
                    runat="server" Mode="Encode" />
            </p>

            <p>
                <strong>Numero de afiliado:</strong>
                <asp:Literal ID="litNumeroObraSocial"
                    runat="server" Mode="Encode" />
            </p>
            </div>

        </asp:Panel>

      <asp:Panel ID="pnlSolicitud"
        runat="server"
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
            AssociatedControlID="txtNombre"
            Text="Nombre:" />

        <asp:TextBox ID="txtNombre"
            runat="server"
            MaxLength="100"

            CssClass="entrada" />
    </p>

    <p>
        <asp:Label runat="server"
            AssociatedControlID="txtApellido"
            Text="Apellido:" />

        <asp:TextBox ID="txtApellido"
            runat="server"
            MaxLength="100"

            CssClass="entrada" />

    </p>

    <p>
        <asp:Label runat="server"
            AssociatedControlID="txtTelefono"
            Text="Telefono:" />

        <asp:TextBox ID="txtTelefono"
            runat="server"
            MaxLength="30"

            CssClass="entrada" />
    </p>

    <p>
        <asp:Label runat="server"
            AssociatedControlID="ddlObraSocial"
            Text="Obra social:" />

        <asp:DropDownList ID="ddlObraSocial"
            runat="server"

            CssClass="entrada" />
    </p>

    <p>
        <asp:Label runat="server"
            AssociatedControlID="txtNumeroObraSocial"
            Text="Numero de afiliado:" />

        <asp:TextBox ID="txtNumeroObraSocial"
            runat="server"
            MaxLength="100"

            CssClass="entrada" />
    </p>
            </div>

    <asp:Button ID="btnEnviarSolicitud"
        runat="server"
        Text="Enviar solicitud"
        OnClick="btnEnviarSolicitud_Click"

        CssClass="boton"/>

</asp:Panel>

        <p class="navegacion">
        <asp:HyperLink ID="lnkVolver"
            runat="server"
            NavigateUrl="~/PanelPaciente.aspx"
            Text="Volver al panel" />
        </p>

    </form>
</body>
</html>
