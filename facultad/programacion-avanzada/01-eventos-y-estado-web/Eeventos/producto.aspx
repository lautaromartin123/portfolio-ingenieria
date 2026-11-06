<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="producto.aspx.cs" Inherits="Eeventos.producto" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Vista del Producto</h2>
    <div class="row">
        <div class="col-md-3">
            <asp:TextBox ID="TdId" runat="server"></asp:TextBox>
        </div>
        <div class="col-md-3">
            <asp:TextBox ID="TxDescripcion" runat="server"></asp:TextBox>
        </div>
        <div class="col-md-3">
            <asp:TextBox ID="TxCodigo" runat="server"></asp:TextBox>
        </div>
        <div class="col-md-3">
            <asp:TextBox ID="TxCosto" runat="server"></asp:TextBox>
        </div>
    </div>
</asp:Content>
