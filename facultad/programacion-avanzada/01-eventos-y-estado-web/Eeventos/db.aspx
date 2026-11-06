<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="db.aspx.cs" Inherits="Eeventos.db" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-4">
            <asp:DropDownList ID="DdEstado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DdEstado_SelectedIndexChanged">
                <asp:ListItem Value="0" Text="Activo">Activo</asp:ListItem>
                <asp:ListItem Value="1" Text="Inactivo">Inactivo</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="col-md-4">
            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
            <asp:Button ID="Button1" runat="server" Text="Button" />
            <label for="DD">Estado</label>
            <asp:DropDownList ID="DDDinamico" runat="server"></asp:DropDownList>
        </div>
        <div class="col-md-4">
        </div>
    </div>
</asp:Content>
