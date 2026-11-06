<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CrearCuenta.aspx.cs" Inherits="Avanzada2026.CrearCuenta" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-4 col-12"></div>
        <div class="col-md-4 col-12">
            <h2>Crear Perfil</h2>
        </div>
        <div class="col-md-4 col-12"></div>
    </div>
    <div class="row">
        <div class="col-md-10 col-12">
            <div class="row">
                <div class="col-md-4 col-12">
                    <label for="TxApellido">Apellido</label>
                    <asp:TextBox ID="TxApellido" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:HiddenField ID="HfId" runat="server" />
                </div>
                <div class="col-md-4 col-12">
                    <label for="TxNombre">Nombre</label>
                    <asp:TextBox ID="TxNombre" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4 col-12">
                    <label for="TxCel">Celular</label>
                    <asp:TextBox ID="TxCel" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 col-12">
                    <label for="TxNombre">Correo Electronico</label>
                    <asp:TextBox ID="TxCorreo1" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4 col-12">
                    <label for="TxNombre">Correo Electronico Repetir</label>
                    <asp:TextBox ID="TxCorreo2" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4 col-12"></div>
            </div>
            <div class="row">
                <div class="col-md-4 col-12">
                    <label for="TxNombre">Password</label>
                    <asp:TextBox ID="Txpassword1" autocomplete="off" TextMode="Password" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4 col-12">
                    <label for="TxNombre">Password repetir</label>
                    <asp:TextBox ID="Txpassword2" autocomplete="off" TextMode="Password" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4 col-12"></div>
            </div>

        </div>
        <div class="col-md-2 col-12">
            <asp:Image ID="ImgFoto" runat="server" CssClass="img-fluid shadow rounded rounded-1" ImageUrl="~/Imagen/SinFoto.png" />
            <div class="m-2">
                <asp:FileUpload ID="FuFoto" runat="server" onchange="previaImagen(this)"  />
                <asp:Button ID="BtTomarFoto" runat="server" Text="Tomar Foto" CssClass="btn bg-primary text-white w-100" />
            </div>
        </div>

    </div>
    <br />
            <div class="row">
                <div class="col-md-4 col-12"></div>
                <div class="col-md-4 col-12"></div>
                <div class="col-md-4 col-12 align-content-end">
                    <asp:Button ID="BtEnviar" runat="server" Text="Enviar" CssClass="btn bg-primary text-white w-100" OnClick="BtEnviar_Click" />
                </div>
            </div>
<script type="text/javascript">
    function previaImagen(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                document.getElementById('<%= ImgFoto.ClientID %>').src = e.target.result;
            };
            reader.readAsDataURL(input.files[0]);         }
    }
</script>
</asp:Content>
