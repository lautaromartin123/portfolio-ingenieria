<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DemoEstado.aspx.cs" Inherits="Eeventos.DemoEstado" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Cookies, Session, ViewState y PostBack</title>
</head>
<body>
    <form id="form1" runat="server">

        <h2>Proyecto: Manejo de Estado en WebForms</h2>

        <asp:Label ID="lblNombre" runat="server" Text="Ingrese su nombre:"></asp:Label>
        <br />
        <asp:TextBox ID="txtNombre" runat="server"></asp:TextBox>

        <br /><br />

        <asp:Button ID="btnGuardar" runat="server" Text="Guardar datos" OnClick="btnGuardar_Click" />

        <hr />

        <asp:Label ID="lblPostBack" runat="server"></asp:Label>
        <br />
        <asp:Label ID="lblCookie" runat="server"></asp:Label>
        <br />
        <asp:Label ID="lblSession" runat="server"></asp:Label>
        <br />
        <asp:Label ID="lblViewState" runat="server"></asp:Label>

    </form>
</body>
</html>
