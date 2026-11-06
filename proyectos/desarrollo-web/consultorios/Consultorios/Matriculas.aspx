<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Matriculas.aspx.cs" Inherits="Consultorios.Matriculas" ResponseEncoding="utf-8" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Matriculas</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="contenedor">

            <div class="encabezado">
                <a class="volver" href="ListadoProfesionales.aspx">Volver al listado</a>
                <h1>Matriculas</h1>
                <asp:Label ID="lblProfesional" runat="server" />
                <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" />
            </div>

            <asp:Panel ID="pnlContenido" runat="server">

                <div class="tarjeta">
                    <h2><asp:Literal ID="litTituloForm" runat="server" Text="Nueva matricula" /></h2>

                    <div class="fila">
                        <div class="campo">
                            <label for="txtNumero">Numero <span class="obligatorio">*</span></label>
                            <asp:TextBox
                                ID="txtNumero"
                                runat="server"
                                CssClass="entrada"
                                MaxLength="50" />
                        </div>

                        <div class="campo">
                            <label for="ddlTipo">Tipo <span class="obligatorio">*</span></label>
                            <asp:DropDownList
                                ID="ddlTipo"
                                runat="server"

                                CssClass="entrada">
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="fila">
                        <div class="campo">
                            <label for="txtEntidadEmisora">Entidad emisora</label>
                            <asp:TextBox
                                ID="txtEntidadEmisora"
                                runat="server"
                                CssClass="entrada"
                                MaxLength="100" />
                        </div>

                        <div class="campo">
                            <label for="txtVencimiento">Vencimiento</label>
                            <asp:TextBox
                                ID="txtVencimiento"
                                runat="server"
                                CssClass="entrada"
                                TextMode="Date" />
                        </div>

                        <div class="campo">
                            <label for="ddlEstado">Estado</label>
                            <asp:DropDownList
                                ID="ddlEstado"
                                runat="server"

                                CssClass="entrada">
                            </asp:DropDownList>
                        </div>
                    </div>

                    <p>
                        <asp:Label runat="server" AssociatedControlID="txtMotivo"
                            Text="Motivo de la operacion (obligatorio para guardar o dar de baja):" />
                        <br />
                        <asp:TextBox ID="txtMotivo" runat="server" TextMode="MultiLine"
                            Rows="3" MaxLength="500"
                            CssClass="entrada" />
                    </p>

                    <asp:Button
                        ID="btnGuardar"
                        runat="server"
                        CssClass="boton"
                        Text="Guardar matricula"
                        OnClick="btnGuardar_Click" />

                    <asp:Button
                        ID="btnCancelarEdicion"
                        runat="server"
                        CssClass="boton boton-secundario"
                        Text="Cancelar edicion"
                        Visible="false"
                        OnClick="btnCancelarEdicion_Click" />
                </div>

                <div class="tarjeta">
                    <h2>Matriculas cargadas</h2>

                    <div class="tabla-contenedor">
                        <asp:GridView
                            ID="gvMatriculas"
                            runat="server"
                            CssClass="tabla"
                            AutoGenerateColumns="false"
                            DataKeyNames="IdMatricula"
                            GridLines="None"
                            OnRowCommand="gvMatriculas_RowCommand"
                            EmptyDataText="Este profesional no tiene matriculas cargadas.">

                            <Columns>

                                <asp:BoundField
                                    DataField="Numero"
                                    HeaderText="Numero" />

                                <asp:TemplateField HeaderText="Tipo">
                                    <ItemTemplate>
                                        <%# Eval("TipoMatricula.Descripcion") %>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField
                                    DataField="EntidadEmisora"
                                    HeaderText="Entidad emisora" />

                                <asp:TemplateField HeaderText="Vencimiento">
                                    <ItemTemplate>
                                        <asp:Label
                                            ID="lblVencimiento"
                                            runat="server"
                                            CssClass='<%# (bool)Eval("EstaVencida") ? "vencida" : "" %>'
                                            Text='<%# Eval("FechaVencimiento", "{0:dd/MM/yyyy}") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Estado">
                                    <ItemTemplate>
                                        <%# Eval("Estado.Descripcion") %>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Acciones">
                                    <ItemTemplate>

                                        <asp:LinkButton
                                            ID="lnkEditar"
                                            runat="server"
                                            Text="Editar"
                                            CommandName="Editar"
                                            CommandArgument='<%# Eval("IdMatricula") %>' />

                                        &nbsp;|&nbsp;

                                        <asp:LinkButton
                                            ID="lnkBaja"
                                            runat="server"
                                            Text="Dar de baja"
                                            CommandName="Baja"
                                            CommandArgument='<%# Eval("IdMatricula") %>'
                                            OnClientClick="return confirm('Dar de baja esta matricula?');" />

                                    </ItemTemplate>
                                </asp:TemplateField>

                            </Columns>

                        </asp:GridView>
                    </div>
                </div>

                <div class="tarjeta">
                    <h2>Historial de matriculas</h2>
                    <div class="tabla-contenedor">
                        <asp:GridView ID="gvHistorial" runat="server" CssClass="tabla"
                            AutoGenerateColumns="false" EmptyDataText="No hay cambios registrados.">
                            <Columns>
                                <asp:BoundField DataField="FechaCambio" HeaderText="Fecha (UTC)" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                <asp:BoundField DataField="Operacion" HeaderText="Operacion" />
                                <asp:BoundField DataField="NumeroAnterior" HeaderText="Numero anterior" NullDisplayText="NULL" />
                                <asp:BoundField DataField="NumeroNuevo" HeaderText="Numero nuevo" />
                                <asp:BoundField DataField="Administrador" HeaderText="Administrador" />
                                <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                                <asp:BoundField DataField="IdSolicitud" HeaderText="Solicitud" NullDisplayText="NULL" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </asp:Panel>

        </div>
    </form>
</body>
</html>
