<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="AltaTurno.aspx.cs"
    Inherits="Consultorios.AltaTurno" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <title>Turno</title>

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

            <a
                class="volver"
                href="ListadoTurnos.aspx">
                Volver al listado
            </a>

            <h1>

                <asp:Literal
                    ID="litTitulo"
                    runat="server"
                    Text="Nuevo turno">
                </asp:Literal>

            </h1>

            <asp:Label
                ID="lblMensaje"
                runat="server"

                CssClass="mensaje">
            </asp:Label>

        </div>

        <!-- ========================================= -->
        <!-- FORMULARIO -->
        <!-- ========================================= -->

        <div class="tarjeta">

            <!-- PACIENTE / PROFESIONAL -->

            <div class="fila">

                <div class="campo">

                    <label for="ddlPaciente">

                        Paciente

                        <span class="obligatorio">
                            *
                        </span>

                    </label>

                    <asp:DropDownList
                        ID="ddlPaciente"
                        runat="server"

                        CssClass="entrada">
                    </asp:DropDownList>

                </div>

                <div class="campo">

                    <label for="ddlProfesional">

                        Profesional

                        <span class="obligatorio">
                            *
                        </span>

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

            <!-- AGENDA -->

            <asp:Panel
                ID="pnlAgenda"
                runat="server"
                Visible="false">

                <p class="ayuda">

                    <strong>
                        Horarios de atención:
                    </strong>

                    <asp:Literal
                        ID="litAgenda"
                        runat="server">
                    </asp:Literal>

                </p>

            </asp:Panel>

            <!-- FECHA / HORA / DURACIÓN -->

            <div class="fila">

                <div class="campo">

                    <label for="txtFecha">

                        Fecha

                        <span class="obligatorio">
                            *
                        </span>

                    </label>

                    <asp:TextBox
                        ID="txtFecha"
                        runat="server"
                        CssClass="entrada"
                        TextMode="Date">
                    </asp:TextBox>

                </div>

                <div class="campo">

                    <label for="txtHora">

                        Hora

                        <span class="obligatorio">
                            *
                        </span>

                    </label>

                    <asp:TextBox
                        ID="txtHora"
                        runat="server"
                        CssClass="entrada"
                        TextMode="Time">
                    </asp:TextBox>

                </div>

                <div class="campo">

                    <label for="txtDuracion">
                        Duración (minutos)
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

            <!-- ESTADO -->

            <div class="fila">

                <div class="campo">

                    <label for="ddlEstadoTurno">
                        Estado
                    </label>

                    <asp:DropDownList
                        ID="ddlEstadoTurno"
                        runat="server"

                        CssClass="entrada">
                    </asp:DropDownList>

                </div>

            </div>

            <!-- MOTIVO -->

            <div class="fila">

                <div class="campo">

                    <label for="txtMotivo">
                        Motivo
                    </label>

                    <asp:TextBox
                        ID="txtMotivo"
                        runat="server"
                        CssClass="entrada"
                        MaxLength="300">
                    </asp:TextBox>

                </div>

            </div>

            <!-- OBSERVACIONES -->

            <div class="fila">

                <div class="campo">

                    <label for="txtObservaciones">
                        Observaciones
                    </label>

                    <asp:TextBox
                        ID="txtObservaciones"
                        runat="server"
                        CssClass="entrada"
                        TextMode="MultiLine"
                        Rows="3"
                        MaxLength="500">
                    </asp:TextBox>

                </div>

            </div>

            <!-- BOTONES -->

            <asp:Button
                ID="btnGuardar"
                runat="server"
                CssClass="boton"
                Text="Guardar"
                OnClick="BtnGuardar_Click" />

            <asp:Button
                ID="btnCancelar"
                runat="server"
                CssClass="boton boton-secundario"
                Text="Cancelar"
                CausesValidation="false"
                OnClick="BtnCancelar_Click" />

        </div>

    </div>

</form>

</body>

</html>
