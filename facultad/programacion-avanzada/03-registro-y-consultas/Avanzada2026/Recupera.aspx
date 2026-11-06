<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Recupera.aspx.cs" Inherits="Avanzada2026.Recupera" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-4 col-12"></div>
        <div class="col-md-4 col-12"><h2>Recuperar Contraseña</h2></div>
        <div class="col-md-4 col-12"></div>
    </div>
    <div class="row m-2">
        <div class="col-md-4 col-12"></div>
        <div class="col-md-4 col-12">
            <asp:TextBox ID="Txmail" runat="server" CssClass="form-control"></asp:TextBox>

        </div>
        <div class="col-md-4 col-12"></div>
    </div>
    <div class="row m-2">
        <div class="col-md-4 col-12"></div>
        <div class="col-md-4 col-12">
             <asp:Button ID="BtEnviar" runat="server" Text="Enviar" CssClass="btn bg-primary text-white w-100" />
        </div>
        <div class="col-md-4 col-12"></div>
    </div>
    <div class="row">
        <div class="col-md-4 col-12"></div>
        <div class="col-md-4 col-12"></div>
        <div class="col-md-4 col-12"></div>
    </div>
</asp:Content>
