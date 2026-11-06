<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ejercicio3.aspx.cs" Inherits="Avanzada2026.Ejercicio3" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">


<h2> Consulta de usuarios por estado</h2>

    <div class="row">
        <div class="col-4">
            estado:
    </div>

        <div class="col-4">
            <asp:DropDownList
                ID="DdEstado"
                runat="server"
                AutoPostBack="true"
                OnSelectedIndexChanged="DdEstado_SelectedIndexChanged">

            </asp:DropDownList>
        </div>

        <div class ="col-4"></div>
        </div>

    <br />
    <asp:repeater ID="RpUsuario" runat="server">
        <ItemTemplate>
            <div class="row">
                <div classs="col-4">
                    <%# Eval("nombre")%>
                    <asp:HiddenField ID="HFId"  runat="server" Value='<%# Eval("id_usuario") %>' />
            </div>

                <div class="col-4">
                    <%#Eval("email") %>
                </div>

                <div class="col-4">
                    <%#Eval("id_estado") %>

                </div>

             </div>


        </ItemTemplate>
    </asp:repeater>






</asp:Content>
