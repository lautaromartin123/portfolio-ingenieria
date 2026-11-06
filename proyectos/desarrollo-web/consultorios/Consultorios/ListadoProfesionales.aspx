<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ListadoProfesionales.aspx.cs" Inherits="Consultorios.ListadoProfesionales" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Profesionales</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="contenedor">

            <div class="encabezado">
                <a class="volver" href="PanelAdmin.aspx">Volver al panel</a>
                <h1>Profesionales</h1>
                <asp:Label ID="lblUsuario" runat="server" CssClass="usuario" />
                <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" />
            </div>

            <div class="tarjeta">
                <div class="fila">
                    <div class="campo">
                        <label for="txtBuscar">Buscar por apellido, nombre o especialidad</label>
                        <asp:TextBox
                            ID="txtBuscar"
                            runat="server"

                            CssClass="entrada" />
                    </div>

                    <div class="campo acciones-filtro">
                        <asp:Button
                            ID="btnBuscar"
                            runat="server"
                            CssClass="boton"
                            Text="Buscar"
                            OnClick="btnBuscar_Click" />

                        <asp:Button
                            ID="btnLimpiar"
                            runat="server"
                            CssClass="boton boton-secundario"
                            Text="Ver todos"
                            OnClick="btnLimpiar_Click" />

                        <asp:Button
                            ID="btnNuevo"
                            runat="server"
                            CssClass="boton"
                            Text="Nuevo profesional"
                            OnClick="btnNuevo_Click" />
                    </div>
                </div>
            </div>

            <div class="tarjeta">
                <h2>Listado</h2>

                <div class="tabla-contenedor">
                    <asp:GridView
                        ID="gvProfesionales"
                        runat="server"
                        CssClass="tabla"
                        AutoGenerateColumns="false"
                        DataKeyNames="IdProfesional"
                        GridLines="None"
                        OnRowCommand="gvProfesionales_RowCommand"
                        EmptyDataText="No hay profesionales para mostrar.">

                        <Columns>

                            <asp:BoundField
                                DataField="IdProfesional"
                                HeaderText="ID" />

                            <asp:BoundField
                                DataField="Apellido"
                                HeaderText="Apellido" />

                            <asp:BoundField
                                DataField="Nombre"
                                HeaderText="Nombre" />

                            <asp:TemplateField HeaderText="Especialidad">
                                <ItemTemplate>
                                    <%# Eval("Especialidad.Nombre") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate>
                                    <asp:Label
                                        ID="lblEstado"
                                        runat="server"
                                        CssClass='<%# Eval("Estado.Descripcion").ToString() == "Inactivo" ? "inactivo" : "" %>'
                                        Text='<%# Eval("Estado.Descripcion") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>

                                    <asp:LinkButton
                                        ID="lnkEditar"
                                        runat="server"
                                        Text="Editar"
                                        CommandName="Editar"
                                        CommandArgument='<%# Eval("IdProfesional") %>' />

                                    &nbsp;|&nbsp;

                                    <asp:LinkButton
                                        ID="lnkMatriculas"
                                        runat="server"
                                        Text="Matrículas"
                                        CommandName="Matriculas"
                                        CommandArgument='<%# Eval("IdProfesional") %>' />

                                    &nbsp;|&nbsp;

                                    <asp:LinkButton
                                        ID="lnkBaja"
                                        runat="server"
                                        Text="Dar de baja"
                                        CommandName="Baja"
                                        CommandArgument='<%# Eval("IdProfesional") %>'
                                        Visible='<%# Eval("Estado.Descripcion").ToString() == "Activo" %>'
                                        OnClientClick="return confirm('¿Dar de baja a este profesional?');" />

                                    <asp:LinkButton
                                        ID="lnkReactivar"
                                        runat="server"
                                        Text="Reactivar"
                                        CommandName="Reactivar"
                                        CommandArgument='<%# Eval("IdProfesional") %>'
                                        Visible='<%# Eval("Estado.Descripcion").ToString() == "Inactivo" %>' />

                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>
                </div>
            </div>

        </div>
    </form>
</body>
</html>
