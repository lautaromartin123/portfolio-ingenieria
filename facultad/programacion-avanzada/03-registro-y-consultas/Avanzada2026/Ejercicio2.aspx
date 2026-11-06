<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ejercicio2.aspx.cs" Inherits="Avanzada2026.Ejercicio2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Consulta de productos por categoria</h2>


    <div class="row">
        <div class="col-4">
            Categoria
    </div>

    <div class="col-4" >
        <asp:DropDownList
            ID="DdCategoria"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged ="DdCategoria_SelectedIndexChanged">

        </asp:DropDownList>
        </div>

        <div class ="col-4"></div>
        </div>

    <br />

    <asp:Repeater ID="RpProducto" runat="server">
        <HeaderTemplate>
            <div class="row">
                <div class="col-4"><b>Descripcion</b><div>
                <div class="col-4"><b>Costo</b><div>
                <div class="col-4"><b>Estado</b><div>
            </div>
        </HeaderTemplate>
        <ItemTemplate>
            <div class="row">
                <div class ="col-4">
                    <%# Eval("Descripcion") %>
                    <asp:HiddenField ID="HFId" runat ="server" Value=' <%# Eval("id_producto") %>' />
                </div>
                <div class="col-4">

                    <%# Eval("costo") %>
                </div>
                <div class ="col-4">
                    <%# Eval("id_estado") %>
                </div>


            </div>
        </ItemTemplate>
    </asp:Repeater>




</asp:Content>
