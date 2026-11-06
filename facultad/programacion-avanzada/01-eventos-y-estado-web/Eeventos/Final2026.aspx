<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Final2026.aspx.cs" Inherits="Eeventos.Final2026" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2> Final Programacion Practica</h2>

    <asp:DropDownList
        ID="DdCategoria"
        runat ="server"
        AutoPostBack="true"
        OnSelectedIndexChanged="DdCategoria_SelectedIndexChanged"
        >

    </asp:DropDownList>

    <br />
    <br />

    <asp:DropDownList
        ID="DdSubCategoria"
        runat="server"
        AutoPostBack="true"
        OnSelectedIndexChanged="DdSubCategoria_SelectedIndexChanged">

    </asp:DropDownList>

    <br />
    <br />

    <h3> Listado De Productos</h3>

    <table>
        <tr>
            <th>ID</th>
            <th>Descripcion</th>
            <th>Costo</th>
            <th>Fecha</th>
        </tr>

    <asp:Repeater ID="RepProductos" runat="server">
        <ItemTemplate>
            <tr>
                <td><%#Eval("IdProducto") %></td>
                <td><%#Eval("Descripcion") %></td>
                <td><%#Eval("Costo")%></td>
                <td><%#Eval("FechaModificacion","{0:dd/MM/yyyy}") %></td>
            </tr>
        </ItemTemplate>
   </asp:Repeater>
      </table>




</asp:Content>
