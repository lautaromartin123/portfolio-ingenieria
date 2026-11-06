<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Repeater.aspx.cs" Inherits="Eeventos.Repeater" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Manejo Repeater</h2>
    <div class="row">
        <div class="col-md-6">
            <asp:TextBox ID="txbuscar" AutoPostBack="true" OnTextChanged="txbuscar_TextChanged" runat="server"></asp:TextBox>
        </div>
        <div class="col-md-6">
            <asp:DropDownList ID="Ddcategoria" AutoPostBack="true" OnSelectedIndexChanged="Ddcategoria_SelectedIndexChanged" runat="server"></asp:DropDownList>
        </div>
    </div>

    <div class="row bg-info text-white rounded rounded-2 m-2">
        <div class="col-md-3">Descripcion</div>
        <div class="col-md-3">Codigo</div>
        <div class="col-md-3">Costo</div>
        <div class="col-md-3"></div>
    </div>
    <asp:Repeater ID="RpProducto" runat="server" OnItemCommand="RpProducto_ItemCommand">
        <ItemTemplate>
            <div class="row ">
                <div class="col-md-3">
                    <asp:LinkButton ID="LkProducto" runat="server"><%# Eval("descripcion") %></asp:LinkButton>
                    <asp:HiddenField ID="HfId" Value='<%# Eval("id_producto") %>' runat="server" />
                </div>

                <div class="col-md-3">
                    <asp:TextBox ID="TxtCodigo" runat="server" Text='<%# Eval("codigo") %>'></asp:TextBox>
                </div>
                <div class="col-md-3"><%# Eval("costo", "{0:C}") %></div>
                <div class="col-md-3"></div>
            </div>

        </ItemTemplate>
    </asp:Repeater>
</asp:Content>
