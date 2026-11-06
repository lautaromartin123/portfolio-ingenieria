<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="ListadoTurnos.aspx.cs"
    Inherits="Consultorios.ListadoTurnos" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Turnos</title>

    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>

<form id="form1" runat="server">

    <div class="contenedor">

        <!-- ========================================= -->
        <!-- ENCABEZADO -->
        <!-- ========================================= -->

        <div class="encabezado">

            <asp:HyperLink
                ID="lnkVolver"
                runat="server"
                CssClass="volver"
                Text="Volver al panel">
            </asp:HyperLink>

            <h1>
                Turnos
            </h1>

            <asp:Label
                ID="lblUsuario"
                runat="server"

                CssClass="usuario">
            </asp:Label>

            <asp:Label
                ID="lblMensaje"
                runat="server"

                CssClass="mensaje">
            </asp:Label>

        </div>

        <!-- ========================================= -->
        <!-- FILTROS -->
        <!-- ========================================= -->

        <div class="tarjeta">

            <div class="fila">

                <div class="campo">

                    <label for="ddlProfesional">
                        Profesional
                    </label>

                    <asp:DropDownList
                        ID="ddlProfesional"
                        runat="server"

                        CssClass="entrada">
                    </asp:DropDownList>

                </div>

                <div class="campo">

                    <label for="txtFecha">
                        Fecha
                    </label>

                    <asp:TextBox
                        ID="txtFecha"
                        runat="server"
                        CssClass="entrada"
                        TextMode="Date">
                    </asp:TextBox>

                </div>

                <div class="campo acciones-filtro">

                    <asp:Button
                        ID="btnFiltrar"
                        runat="server"
                        CssClass="boton"
                        Text="Filtrar"
                        OnClick="BtnFiltrar_Click" />

                    <asp:Button
                        ID="btnVerTodos"
                        runat="server"
                        CssClass="boton boton-secundario"
                        Text="Ver todos"
                        OnClick="BtnVerTodos_Click" />

                    <asp:Button
                        ID="btnNuevo"
                        runat="server"
                        CssClass="boton"
                        Text="Nuevo turno"
                        OnClick="BtnNuevo_Click" />

                </div>

            </div>

        </div>

        <!-- ========================================= -->
        <!-- LISTADO -->
        <!-- ========================================= -->

        <div class="tarjeta">

            <h2>
                Listado
            </h2>

            <div class="tabla-contenedor">

                <asp:GridView
                    ID="gvTurnos"
                    runat="server"
                    CssClass="tabla"
                    AutoGenerateColumns="false"
                    DataKeyNames="IdTurno"
                    GridLines="None"
                    EmptyDataText="No hay turnos para los filtros elegidos."
                    OnRowCommand="GvTurnos_RowCommand"
                    OnRowDataBound="GvTurnos_RowDataBound">

                    <Columns>

                        <asp:BoundField
                            DataField="FechaHora"
                            HeaderText="Fecha y hora"
                            DataFormatString="{0:dd/MM/yyyy HH:mm}" />

                        <asp:TemplateField
                            HeaderText="Paciente">

                            <ItemTemplate>

                                <%# Eval("Paciente.Apellido") %>,
                                <%# Eval("Paciente.Nombre") %>

                            </ItemTemplate>

                        </asp:TemplateField>

                        <asp:TemplateField
                            HeaderText="Profesional">

                            <ItemTemplate>

                                <%# Eval("Profesional.Apellido") %>,
                                <%# Eval("Profesional.Nombre") %>

                            </ItemTemplate>

                        </asp:TemplateField>

                        <asp:BoundField
                            DataField="Motivo"
                            HeaderText="Motivo" />

                        <asp:BoundField
                            DataField="DuracionMinutos"
                            HeaderText="Min." />

                        <asp:TemplateField
                            HeaderText="Estado">

                            <ItemTemplate>

                                <asp:Label
                                    ID="lblEstado"
                                    runat="server"
                                    CssClass='<%# "estado-" + Eval("EstadoTurno.Descripcion") %>'
                                    Text='<%# Eval("EstadoTurno.Descripcion") %>'>
                                </asp:Label>

                            </ItemTemplate>

                        </asp:TemplateField>

                        <%-- ACCIONES --%>

                        <asp:TemplateField
                            HeaderText="Acciones">

                            <ItemTemplate>

                                <asp:LinkButton
                                    ID="lnkEditar"
                                    runat="server"
                                    CssClass="accion"
                                    Text="Editar"
                                    CommandName="Editar"
                                    CommandArgument='<%# Eval("IdTurno") %>'>
                                </asp:LinkButton>

                                <asp:LinkButton
                                    ID="lnkConfirmar"
                                    runat="server"
                                    CssClass="accion"
                                    Text="Confirmar"
                                    CommandName="Confirmar"
                                    CommandArgument='<%# Eval("IdTurno") %>'>
                                </asp:LinkButton>

                                <asp:LinkButton
                                    ID="lnkAtendido"
                                    runat="server"
                                    CssClass="accion"
                                    Text="Atendido"
                                    CommandName="Atendido"
                                    CommandArgument='<%# Eval("IdTurno") %>'>
                                </asp:LinkButton>

                                <asp:LinkButton
                                    ID="lnkAusente"
                                    runat="server"
                                    CssClass="accion"
                                    Text="Ausente"
                                    CommandName="Ausente"
                                    CommandArgument='<%# Eval("IdTurno") %>'>
                                </asp:LinkButton>

                                <asp:LinkButton
                                    ID="lnkCancelar"
                                    runat="server"
                                    CssClass="accion"
                                    Text="Cancelar"
                                    CommandName="Cancelar"
                                    CommandArgument='<%# Eval("IdTurno") %>'
                                    OnClientClick="return confirm('¿Cancelar este turno?');">
                                </asp:LinkButton>

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
