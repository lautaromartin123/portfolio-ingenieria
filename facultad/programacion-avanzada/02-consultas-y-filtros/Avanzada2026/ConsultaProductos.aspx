<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultaProductos.aspx.cs" Inherits="Avanzada2026.ConsultaProductos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>



<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2> Consulta de estados por producto</h2>

    <asp:Label ID="LblEstado" runat="server" Text="Seleccione un estado: "></asp:Label>

    <br />
    <br />
    <asp:DropDownList ID="DdEstado"
        runat="server"
        AutoPostBack="true"
        OnSelectedIndexChanged="DdEstado_SelectedIndexChanged"

        ></asp:DropDownList>

    <br />
    <br />

    <asp:Repeater



        ID="RepProductos"
        runat="server">

        <HeaderTemplate>
            <table class="table table-bordered">
            <tr>
                <th>Producto</th>
                <th>Precio</th>
            </tr>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%#Eval("Nombre") %></td>
                <td><%#Eval("Precio") %></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>



    </asp:Repeater>






</asp:Content>
