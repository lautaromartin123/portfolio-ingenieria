<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DropDownAnidado.aspx.cs" Inherits="Eeventos.DropDownAnidado" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Anidados</h2>
    <label for="DdPadrea">Categoria</label>
    <asp:DropDownList ID="DdPadrea" runat="server" CssClass=" form-select" AutoPostBack="true" OnSelectedIndexChanged="DdPadrea_SelectedIndexChanged"></asp:DropDownList>
    <label for="DdHijo">Sub Categoria</label>
    <asp:DropDownList ID="DdHijo" runat="server" CssClass=" form-select"></asp:DropDownList>
    <hr />
    <h2>Repeater</h2>
    <asp:Repeater ID="RpEjemplo" runat="server">
        <HeaderTemplate></HeaderTemplate>
        <ItemTemplate>
            <%# Eval("descripcion") %>
        </ItemTemplate>
        <FooterTemplate></FooterTemplate>
    </asp:Repeater>
</asp:Content>
