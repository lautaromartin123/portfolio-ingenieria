<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SolicitudesModificacion.aspx.cs" Inherits="Consultorios.SolicitudesModificacion" ResponseEncoding="utf-8" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Solicitudes de modificacion</title>
    <link runat="server" href="~/Estilos/consultorios.css?v=20261026" rel="stylesheet" />

</head>
<body>
    <form id="form1" runat="server" class="contenedor">

        <div class="encabezado">
            <h2>Solicitudes pendientes de modificacion</h2>
            <p class="ayuda">Compara los datos actuales con los propuestos antes de aprobar o rechazar.</p>
        </div>

        <asp:Button ID="btnActualizar"
            runat="server"
            Text="Actualizar listado"
            OnClick="btnActualizar_Click"

            CssClass="boton boton-secundario"/>

        <p>
            <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje" />
        </p>

        <h3>Pacientes</h3>

        <asp:Label ID="lblSinSolicitudesPaciente" runat="server"
            Visible="false" CssClass="mensaje" Text="No hay solicitudes pendientes de pacientes." />

        <asp:Repeater ID="rptSolicitudes" runat="server"  OnItemCommand="rptSolicitudes_ItemCommand">
            <ItemTemplate>

            <div class="solicitud formulario">

                    <h3>
                        Solicitud <%#: Eval("IdSolicitud") %>
                    </h3>

                    <p>
                        Paciente: <%#: Eval("IdPaciente") %>
                        - Cuenta solicitante:
                        <%#: Eval("IdUsuarioSolicitante") %>
                    </p>

                    <p>
                        Fecha:
                        <%#: Eval("FechaSolicitud", "{0:dd/MM/yyyy HH:mm}") %>
                        UTC
                    </p>

                    <div class="tabla-contenedor">
                    <table class="comparacion">
                        <thead>
                            <tr>
                                <th scope="col">Campo</th>
                                <th scope="col">Dato actual</th>
                                <th scope="col">Dato propuesto</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr>
                                <th scope="row">Nombre</th>
                                <td><%#: MostrarValor(Eval("NombreActual")) %></td>
                                <td><%#: MostrarValor(Eval("NombrePropuesto")) %></td>
                            </tr>

                            <tr>
                                <th scope="row">Apellido</th>
                                <td><%#: MostrarValor(Eval("ApellidoActual")) %></td>
                                <td><%#: MostrarValor(Eval("ApellidoPropuesto")) %></td>
                            </tr>

                            <tr>
                                <th scope="row">Telefono</th>
                                <td><%#: MostrarValor(Eval("TelefonoActual")) %></td>
                                <td><%#: MostrarValor(Eval("TelefonoPropuesto")) %></td>
                            </tr>

                            <tr>
                                <th scope="row">Obra social</th>
                                <td><%#: MostrarValor(Eval("ObraSocialActual")) %></td>
                                <td><%#: MostrarValor(Eval("ObraSocialPropuesta")) %></td>
                            </tr>

                            <tr>
                                <th scope="row">Numero de afiliado</th>
                                <td><%#: MostrarValor(Eval("NumeroObraSocialActual")) %></td>
                                <td><%#: MostrarValor(Eval("NumeroObraSocialPropuesto")) %></td>
                            </tr>
                        </tbody>
                    </table>
                    </div>
                    <p>
                        <asp:Label runat="server"
                            AssociatedControlID="txtObservacion"
                            Text="Motivo del rechazo:" />

                        <asp:TextBox ID="txtObservacion"
                            runat="server"
                            TextMode="MultiLine"
                            Rows="3"
                            Columns="60"

                            CssClass="entrada" />
                    </p>

                    <asp:Button ID="btnAprobar"
                        runat="server"
                        Text="Aprobar solicitud"
                        CommandName="Aprobar"
                        CommandArgument='<%# Eval("IdSolicitud") %>'
                        OnClientClick="return confirm('Aprobar esta solicitud y aplicar los datos propuestos?');"

                        CssClass="boton"/>

                    <asp:Button ID="btnRechazar"
                        runat="server"
                        Text="Rechazar solicitud"
                        CommandName="Rechazar"
                        CommandArgument='<%# Eval("IdSolicitud") %>'
                        OnClientClick="return confirm('Rechazar esta solicitud?');"

                        CssClass="boton boton-rechazar"/>

                </div>

            </ItemTemplate>
        </asp:Repeater>

        <h3>Medicos</h3>

        <asp:Label ID="lblSinSolicitudesMedico" runat="server"
            Visible="false" CssClass="mensaje" Text="No hay solicitudes pendientes de medicos." />

        <asp:Repeater ID="rptSolicitudesMedico" runat="server"
            OnItemCommand="rptSolicitudesMedico_ItemCommand">
            <ItemTemplate>

                <div class="solicitud formulario">
                    <h3>Solicitud <%#: Eval("IdSolicitud") %></h3>

                    <p>
                        Profesional: <%#: Eval("IdProfesional") %>
                        - Cuenta solicitante: <%#: Eval("IdUsuarioSolicitante") %>
                    </p>
                    <p>
                        Fecha: <%#: Eval("FechaSolicitud", "{0:dd/MM/yyyy HH:mm}") %> UTC
                    </p>

                    <div class="tabla-contenedor">
                    <table class="comparacion">
                        <thead>
                            <tr>
                                <th scope="col">Campo</th>
                                <th scope="col">Dato actual</th>
                                <th scope="col">Dato propuesto</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <th scope="row">Nombre</th>
                                <td><%#: MostrarValor(Eval("NombreActual")) %></td>
                                <td><%#: MostrarValor(Eval("NombrePropuesto")) %></td>
                            </tr>
                            <tr>
                                <th scope="row">Apellido</th>
                                <td><%#: MostrarValor(Eval("ApellidoActual")) %></td>
                                <td><%#: MostrarValor(Eval("ApellidoPropuesto")) %></td>
                            </tr>
                            <tr>
                                <th scope="row">Especialidades</th>
                                <td><%#: MostrarValor(Eval("EspecialidadActual")) %></td>
                                <td><%#: MostrarValor(Eval("EspecialidadPropuesta")) %></td>
                            </tr>
                            <tr>
                                <th scope="row">Numero de matricula</th>
                                <td><%#: Eval("MatriculaActual") %></td>
                                <td><%#: Eval("MatriculaPropuesta") %></td>
                            </tr>
                        </tbody>
                    </table>
                    </div>

                    <p>Tipo de matricula propuesto: <%#: Eval("TipoMatriculaPropuesto") %></p>
                    <p>Entidad emisora propuesta: <%#: Eval("EntidadPropuesta") %></p>
                    <p>Vencimiento propuesto: <%#: Eval("VencimientoPropuesto") %></p>
                    <p>
                        <asp:HyperLink runat="server"
                            NavigateUrl='<%# "~/Matriculas.aspx?idProfesional=" + Eval("IdProfesional") %>'
                            Text="Administrar matriculas y consultar historial" />
                    </p>

                    <p>
                        <asp:Label runat="server"
                            AssociatedControlID="txtObservacion" Text="Motivo del rechazo:" />
                        <asp:TextBox ID="txtObservacion" runat="server"
                            TextMode="MultiLine" Rows="3" Columns="60"
                            CssClass="entrada" />
                    </p>

                    <asp:Button ID="btnAprobar" runat="server"
                        Text="Aprobar solicitud" CommandName="Aprobar"
                        CommandArgument='<%# Eval("IdSolicitud") %>'
                        OnClientClick="return confirm('Aprobar esta solicitud y aplicar los datos propuestos?');"

                        CssClass="boton" />

                    <asp:Button ID="btnRechazar" runat="server"
                        Text="Rechazar solicitud" CommandName="Rechazar"
                        CommandArgument='<%# Eval("IdSolicitud") %>'
                        OnClientClick="return confirm('Rechazar esta solicitud?');"

                        CssClass="boton boton-rechazar" />
                </div>

            </ItemTemplate>
        </asp:Repeater>

        <p class="navegacion">
        <asp:HyperLink ID="lnkVolver"
            runat="server"
            NavigateUrl="~/PanelAdmin.aspx"
            Text="Volver al panel" />
        </p>

    </form>
</body>
</html>
