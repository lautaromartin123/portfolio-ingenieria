<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="AgendaProfesional.aspx.cs"
    Inherits="Consultorios.AgendaProfesional" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Agenda de profesionales</title>

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
                Agenda de profesionales
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
        <!-- SELECCIÓN DE PROFESIONAL -->
        <!-- ========================================= -->

        <div class="tarjeta">

            <div class="campo">

                <label for="ddlProfesional">
                    Profesional
                </label>

                <asp:DropDownList
                    ID="ddlProfesional"
                    runat="server"
                    CssClass="entrada"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="DdlProfesional_SelectedIndexChanged">
                </asp:DropDownList>

            </div>

        </div>

        <!-- ========================================= -->
        <!-- AVISO SOLO LECTURA -->
        <!-- ========================================= -->

        <asp:Panel
            ID="pnlSoloLectura"
            runat="server"
            CssClass="solo-lectura"
            Visible="false">

            Tiene acceso de solo lectura a la agenda.

        </asp:Panel>

        <!-- ========================================= -->
        <!-- CONTENIDO -->
        <!-- ========================================= -->

        <asp:Panel
            ID="pnlContenido"
            runat="server"
            Visible="false">

            <!-- ===================================== -->
            <!-- FORMULARIO DE MODIFICACIÓN -->
            <!-- ===================================== -->

            <asp:Panel
                ID="pnlFormulario"
                runat="server">

                <div class="tarjeta">

                    <h2>

                        <asp:Literal
                            ID="litTituloForm"
                            runat="server"
                            Text="Nueva franja horaria">
                        </asp:Literal>

                    </h2>

                    <div class="fila">

                        <!-- DÍA -->

                        <div class="campo">

                            <label for="ddlDiaSemana">
                                Día
                            </label>

                            <asp:DropDownList
                                ID="ddlDiaSemana"
                                runat="server"

                                CssClass="entrada">

                                <asp:ListItem Value="1">
                                    Lunes
                                </asp:ListItem>

                                <asp:ListItem Value="2">
                                    Martes
                                </asp:ListItem>

                                <asp:ListItem Value="3">
                                    Miércoles
                                </asp:ListItem>

                                <asp:ListItem Value="4">
                                    Jueves
                                </asp:ListItem>

                                <asp:ListItem Value="5">
                                    Viernes
                                </asp:ListItem>

                                <asp:ListItem Value="6">
                                    Sábado
                                </asp:ListItem>

                                <asp:ListItem Value="7">
                                    Domingo
                                </asp:ListItem>

                            </asp:DropDownList>

                        </div>

                        <!-- HORA INICIO -->

                        <div class="campo">

                            <label for="txtHoraInicio">
                                Desde
                            </label>

                            <asp:TextBox
                                ID="txtHoraInicio"
                                runat="server"
                                CssClass="entrada"
                                TextMode="Time">
                            </asp:TextBox>

                        </div>

                        <!-- HORA FIN -->

                        <div class="campo">

                            <label for="txtHoraFin">
                                Hasta
                            </label>

                            <asp:TextBox
                                ID="txtHoraFin"
                                runat="server"
                                CssClass="entrada"
                                TextMode="Time">
                            </asp:TextBox>

                        </div>

                        <!-- DURACIÓN -->

                        <div class="campo">

                            <label for="txtDuracion">
                                Duración turno (min)
                            </label>

                            <asp:TextBox
                                ID="txtDuracion"
                                runat="server"
                                CssClass="entrada"
                                TextMode="Number"
                                Text="30">
                            </asp:TextBox>

                        </div>

                    </div>

                    <!-- MODALIDAD / ESTADO -->

                    <div class="fila">

                        <div class="campo">

                            <label for="ddlAtencion">
                                Modalidad
                            </label>

                            <asp:DropDownList
                                ID="ddlAtencion"
                                runat="server"

                                CssClass="entrada">

                                <asp:ListItem Value="1">
                                    Con turno
                                </asp:ListItem>

                                <asp:ListItem Value="0">
                                    Por orden de llegada
                                </asp:ListItem>

                            </asp:DropDownList>

                        </div>

                        <div class="campo">

                            <label for="ddlEstado">
                                Estado
                            </label>

                            <asp:DropDownList
                                ID="ddlEstado"
                                runat="server"

                                CssClass="entrada">
                            </asp:DropDownList>

                        </div>

                    </div>

                    <!-- BOTONES -->

                    <asp:Button
                        ID="btnGuardar"
                        runat="server"
                        CssClass="boton"
                        Text="Guardar franja"
                        OnClick="BtnGuardar_Click" />

                    <asp:Button
                        ID="btnCancelarEdicion"
                        runat="server"
                        CssClass="boton boton-secundario"
                        Text="Cancelar edición"
                        Visible="false"
                        OnClick="BtnCancelarEdicion_Click" />

                </div>

            </asp:Panel>

            <!-- ===================================== -->
            <!-- LISTADO -->
            <!-- ===================================== -->

            <div class="tarjeta">

                <h2>
                    Horarios cargados
                </h2>

                <div class="tabla-contenedor">

                    <asp:GridView
                        ID="gvAgenda"
                        runat="server"
                        CssClass="tabla"
                        AutoGenerateColumns="false"
                        DataKeyNames="IdAgenda"
                        GridLines="None"
                        EmptyDataText="Este profesional no tiene horarios cargados."
                        OnRowCommand="GvAgenda_RowCommand"
                        OnRowDataBound="GvAgenda_RowDataBound">

                        <Columns>

                            <asp:BoundField
                                DataField="NombreDia"
                                HeaderText="Día" />

                            <asp:BoundField
                                DataField="Horario"
                                HeaderText="Horario" />

                            <asp:BoundField
                                DataField="DuracionMinutos"
                                HeaderText="Min. por turno" />

                            <asp:TemplateField
                                HeaderText="Modalidad">

                                <ItemTemplate>

                                    <%#
                                        (bool)Eval("AtencionConTurno")
                                        ? "Con turno"
                                        : "Por orden de llegada"
                                    %>

                                </ItemTemplate>

                            </asp:TemplateField>

                            <asp:TemplateField
                                HeaderText="Estado">

                                <ItemTemplate>

                                    <asp:Label
                                        ID="lblEstado"
                                        runat="server"
                                        CssClass='<%#
                                            Eval("Estado.Descripcion")
                                            .ToString() == "Inactivo"
                                            ? "inactivo"
                                            : ""
                                        %>'
                                        Text='<%#
                                            Eval("Estado.Descripcion")
                                        %>'>
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
                                        Text="Editar"
                                        CommandName="Editar"
                                        CommandArgument='<%# Eval("IdAgenda") %>'>
                                    </asp:LinkButton>

                                    <asp:Literal
                                        ID="litSeparador"
                                        runat="server"
                                        Text=" &nbsp;|&nbsp; ">
                                    </asp:Literal>

                                    <asp:LinkButton
                                        ID="lnkBaja"
                                        runat="server"
                                        Text="Dar de baja"
                                        CommandName="Baja"
                                        CommandArgument='<%# Eval("IdAgenda") %>'
                                        OnClientClick="return confirm('¿Dar de baja esta franja horaria?');">
                                    </asp:LinkButton>

                                </ItemTemplate>

                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </asp:Panel>

    </div>

</form>

</body>

</html>
