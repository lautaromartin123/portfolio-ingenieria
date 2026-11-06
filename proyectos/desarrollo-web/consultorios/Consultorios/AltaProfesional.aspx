<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AltaProfesional.aspx.cs" Inherits="Consultorios.AltaProfesional" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Profesional</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="contenedor">

            <div class="encabezado">
                <a class="volver" href="ListadoProfesionales.aspx">Volver al listado</a>
                <h1><asp:Literal ID="litTitulo" runat="server" Text="Nuevo profesional" /></h1>
                <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" />
            </div>

            <div class="tarjeta">

                <div class="fila">
                    <div class="campo">
                        <label for="txtNombre">Nombre <span class="obligatorio">*</span></label>
                        <asp:TextBox
                            ID="txtNombre"
                            runat="server"
                            CssClass="entrada"
                            MaxLength="100" />
                    </div>

                    <div class="campo">
                        <label for="txtApellido">Apellido <span class="obligatorio">*</span></label>
                        <asp:TextBox
                            ID="txtApellido"
                            runat="server"
                            CssClass="entrada"
                            MaxLength="100" />
                    </div>
                </div>

                <div class="fila">
                    <div class="campo">
                        <label for="ddlEspecialidad">Especialidad <span class="obligatorio">*</span></label>
                        <asp:DropDownList
                            ID="ddlEspecialidad"
                            runat="server"

                            CssClass="entrada">
                        </asp:DropDownList>
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

                <div class="fila">
                    <div class="campo">
                        <label for="txtFoto">Foto (URL o ruta)</label>
                        <asp:TextBox
                            ID="txtFoto"
                            runat="server"
                            CssClass="entrada"
                            MaxLength="500" />
                    </div>
                </div>

                <asp:Button
                    ID="btnGuardar"
                    runat="server"
                    CssClass="boton"
                    Text="Guardar"
                    OnClick="btnGuardar_Click" />

                <asp:Button
                    ID="btnCancelar"
                    runat="server"
                    CssClass="boton boton-secundario"
                    Text="Cancelar"
                    CausesValidation="false"
                    OnClick="btnCancelar_Click" />

            </div>

            <asp:Panel ID="pnlMatriculaInicial" runat="server">
                <div class="tarjeta">
                    <h2>Primera matrícula (opcional)</h2>

                    <p>
                        Si cargás los datos, el profesional y su matrícula se dan
                        de alta juntos. Si algo falla, no se guarda ninguno de los dos.
                    </p>

                    <div class="fila">
                        <div class="campo">
                            <label for="txtNumeroMatricula">Número</label>
                            <asp:TextBox
                                ID="txtNumeroMatricula"
                                runat="server"
                                CssClass="entrada"
                                MaxLength="50" />
                        </div>

                        <div class="campo">
                            <label for="ddlTipoMatricula">Tipo</label>
                            <asp:DropDownList
                                ID="ddlTipoMatricula"
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
                    </div>
                </div>
            </asp:Panel>

        </div>
    </form>
</body>
</html>
