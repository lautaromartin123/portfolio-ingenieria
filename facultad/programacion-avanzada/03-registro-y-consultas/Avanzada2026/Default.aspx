<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Avanzada2026.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
    <link href="Content/StyleMisitio.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <section class="vh-100 gradient-custom">
        <div class="container py-5 h-100">
            <div class="row d-flex justify-content-center align-items-center h-100">
                <div class="col-12 col-md-8 col-lg-6 col-xl-5">
                    <div class="card bg-dark text-white" style="border-radius: 1rem;">
                        <div class="card-body p-5 text-center">

                            <div class="mb-md-5 mt-md-4 pb-5 text-center">

                                <h2 class="fw-bold mb-2 text-uppercase">Login</h2>
                                <p class="text-white-50 mb-5">Ingrese su Usuario y contraseña!</p>

                                <div class=" align-content-center">
                                    <asp:TextBox ID="Txusuario" runat="server" CssClass="form-control w-100" placeholder="Ingrese su usuario"></asp:TextBox>
                                    <label class="form-label" for="typeEmailX">Email</label>
                                </div>

                                <div class="">
                                    <asp:TextBox ID="TxPassword" runat="server" TextMode="Password" CssClass="form-control w-100" placeholder="Ingrese su password"></asp:TextBox>
                                    <label class="form-label" for="typePasswordX">Password</label>
                                </div>

                                <p class="small mb-5 pb-lg-2"><a class="text-white-50" href="Recupera.aspx">Recuperar Contraseña?</a></p>
                                <asp:Button ID="BtLogin" runat="server" Text="Ingresar" CssClass="btn btn-outline-light btn-lg px-5" OnClick="BtLogin_Click" />
                                <asp:Literal ID="ltMensaje"  runat="server"></asp:Literal>
                                <div class="d-flex justify-content-center text-center mt-4 pt-1">
                                    <a href="#!" class="text-white"><i class="fab fa-facebook-f fa-lg"></i></a>
                                    <a href="#!" class="text-white"><i class="fab fa-twitter fa-lg mx-4 px-2"></i></a>
                                    <a href="#!" class="text-white"><i class="fab fa-google fa-lg"></i></a>
                                </div>

                            </div>

                            <div>
                                <p class="mb-0">
                                    No tienes una cuenta? <a href="CrearCuenta.aspx" class="text-white-50 fw-bold">Crear Una Cuenta</a>
                                </p>
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
