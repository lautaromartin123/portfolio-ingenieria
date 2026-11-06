<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultaAlumno.aspx.cs" Inherits="Avanzada2026.ConsultaAlumno" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Consulta Alumnos Por Curso</h2>


    <asp:Label ID="LblCurso" runat="server" Text="Seleccione un curso: "></asp:Label>


    <br />
    <br />
    <asp:DropDownList ID="DdCurso" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DdCurso_SelectedIndexChanged"></asp:DropDownList>


    <br />
    <br />
    <asp:Repeater ID="RepAlumnos" runat="server">

        <HeaderTemplate>
                <table class="table table-bordered">
                    <tr>
                        <th>ID</th>
                        <th>Apellido</th>
                        <th>Nombre</th>
                        <th>Email</th>
                    </tr>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%#Eval("IdAlumno") %></td>
                 <td><%#Eval("Apellido") %></td>
                 <td><%#Eval("Nombre") %></td>
                 <td><%#Eval("Email") %></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>

    </asp:Repeater>




</asp:Content>
