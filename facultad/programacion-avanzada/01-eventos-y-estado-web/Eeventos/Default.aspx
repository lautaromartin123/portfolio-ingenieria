<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Eeventos._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Ejemplo de Eventos</h1>


    <div class="row border border-1">
        <div class="col-4">
            <label for="TxIngresar">Ingrese su Nombre:</label>
            <asp:TextBox ID="TxIngresar" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:Button ID="BtSaludar" runat="server" CssClass="btn btn-primary text-white" Text="Saludar" OnClick="BtSaludar_Click" />
        </div>
        <div class="col-4">
            <asp:Label ID="LbMensaje" runat="server" Text=""></asp:Label>
            <asp:TextBox ID="txborrar" runat="server"></asp:TextBox>
        </div>
        <div class="col-4">
            <label for="TxCalendario">Fecha de Nacimiento:</label>
            <asp:TextBox ID="TxCalendario" CssClass="form-control" TextMode="Date" runat="server"></asp:TextBox>
            <asp:Button ID="BtCalcularEdad" runat="server" Text="Calcular Edad" CssClass="btn btn-success text-white" OnClick="BtCalcularEdad_Click" />
        </div>
    </div>
    <hr />

    <div class="row">
        <div class="col-4">
            <asp:UpdatePanel ID="UpNumero1" runat="server">
                <ContentTemplate>
                    <label for="TxTimer1">Número 1:</label>
                    <asp:TextBox ID="TxTimer1" CssClass="form-control" runat="server"></asp:TextBox>
                    <asp:Timer ID="Timer1" runat="server" OnTick="Timer1_Tick"></asp:Timer>

                    <br />
                    <asp:Button ID="BtInicio1" runat="server" Text="Inicio"   CssClass="btn bg-primary text-white" OnClick="BtInicio1_Click"/>
                    <asp:Button ID="BtFin1" runat="server" Text="Fin"   CssClass="btn bg-primary text-white" OnClick="BtFin1_Click"/>
                </ContentTemplate>

            </asp:UpdatePanel>
        </div>
        <div class="col-4"></div>
        <div class="col-4">
            <asp:UpdatePanel ID="UpNumero2" runat="server">
                <ContentTemplate>
                    <label for="TxTimer2">Número 2:</label>
                    <asp:TextBox ID="TxTimer2" CssClass="form-control" runat="server"></asp:TextBox><br />
                    <asp:Button ID="BtInicio2" runat="server" Text="Inicio"  CssClass="btn bg-primary text-white" OnClick="BtInicio2_Click" />
                    <asp:Button ID="BtFin2" runat="server" Text="Fin" CssClass="btn bg-primary text-white" OnClick="BtFin2_Click" />
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Timer2" EventName="Tick" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <asp:Timer ID="Timer2" runat="server" OnTick="Timer2_Tick1"></asp:Timer>
    <asp:Timer ID="Timer3" runat="server" OnTick="Timer3_Tick"></asp:Timer>
    <hr />
    <h2>JAVASCRIPT</h2>
    <div class="row">
        <div class="col-4">
            <asp:TextBox ID="TxJava" runat="server"></asp:TextBox>
        </div>
        <div class="col-4"></div>
        <div class="col-4">
            <asp:Button ID="BtJava" runat="server" Text="Button" OnClientClick="mostrarMensaje(); return false" />
        </div>
    </div>
    <div class="row">
        <div class="col-4"></div>
        <div class="col-4"></div>
        <div class="col-4"></div>
    </div>
    <div class="row">
        <div class="col-4"></div>
        <div class="col-4"></div>
        <div class="col-4"></div>
    </div>
    <script src="JAVA/Ejemplo.js"></script>
<%--    <script type="text/javascript">
        function mostrarMensaje()
        {
            var texto = document.getElementById('<%= TxJava.ClientID %>').value;
            alert("¡Hola! " + texto + " Has hecho clic en el botón.");
        }
    </script>--%>
</asp:Content>
