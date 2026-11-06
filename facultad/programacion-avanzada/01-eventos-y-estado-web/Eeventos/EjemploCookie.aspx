<%@ Page Title="Ejemplo Cookie" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EjemploCookie.aspx.cs" Inherits="Eeventos.EjemploCookie" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Ejemplo Cookie - Idioma</h2>

    <asp:Label
        ID="Label1"
        runat="server"
        Text="Seleccione un idioma:">
    </asp:Label>

    <br />
    <br />

    <asp:DropDownList
        ID="DdIdioma"
        runat="server"
        AutoPostBack="true"
        OnSelectedIndexChanged="DdIdioma_SelectedIndexChanged">

        <asp:ListItem Value="0">Español</asp:ListItem>
        <asp:ListItem Value="1">Ingles</asp:ListItem>
        <asp:ListItem Value="2">Chino</asp:ListItem>

    </asp:DropDownList>

    <br />
    <br />


    <asp:Label
        ID="lbmensaje"
        runat="server"
        Text="Label">
    </asp:Label>

</asp:Content>
