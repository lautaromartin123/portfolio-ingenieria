using System;
using System.Net;
using System.Net.Mail;

namespace Consultorios
{
    public static class EmailService
    {
        public static void EnviarRecuperacionPassword(
            string destinatario,
            string nombreUsuario,
            string urlRecuperacion)
        {
            string correoRemitente =
                Environment.GetEnvironmentVariable(
                    "CONSULTORIOS_GMAIL_USER");

            string passwordAplicacion =
                Environment.GetEnvironmentVariable(
                    "CONSULTORIOS_GMAIL_APP_PASSWORD");


            if (string.IsNullOrEmpty(correoRemitente))
            {
                throw new Exception(
                    "No está configurado el correo remitente.");
            }


            if (string.IsNullOrEmpty(passwordAplicacion))
            {
                throw new Exception(
                    "No está configurada la contraseña de aplicación de Gmail.");
            }


            MailMessage mensaje =
                new MailMessage();


            mensaje.From =
                new MailAddress(
                    correoRemitente,
                    "Sistema de Consultorios");


            mensaje.To.Add(destinatario);


            mensaje.Subject =
                "Recuperación de contraseña";


            mensaje.IsBodyHtml =
                true;


            mensaje.Body = @"
                <html>
                <body style='
                    font-family: Arial, Helvetica, sans-serif;
                    background-color: #f5f5f5;
                    padding: 30px;
                '>

                    <div style='
                        max-width: 520px;
                        margin: auto;
                        background: white;
                        border: 3px solid #111;
                        padding: 35px;
                        box-shadow: 8px 8px 0 #111;
                    '>

                        <h2 style='
                            text-align: center;
                            color: #111;
                        '>
                            Recuperación de contraseña
                        </h2>

                        <p>
                            Hola <strong>" +
                            WebUtility.HtmlEncode(nombreUsuario) +
                            @"</strong>.
                        </p>

                        <p>
                            Recibimos una solicitud para cambiar
                            la contraseña de tu cuenta.
                        </p>

                        <p style='text-align:center; margin:30px 0;'>

                            <a href='" +
                            WebUtility.HtmlEncode(urlRecuperacion) +
                            @"'
                               style='
                                   background:#111;
                                   color:#fff;
                                   padding:14px 22px;
                                   text-decoration:none;
                                   font-weight:bold;
                                   display:inline-block;
                               '>

                                RESTABLECER CONTRASEÑA

                            </a>

                        </p>

                        <p>
                            Este enlace tiene una validez
                            de 30 minutos y puede utilizarse
                            una sola vez.
                        </p>

                        <p>
                            Si no solicitaste este cambio,
                            podés ignorar este correo.
                        </p>

                        <hr style='
                            border:none;
                            border-top:1px solid #ddd;
                            margin-top:30px;
                        ' />

                        <p style='
                            font-size:12px;
                            color:#777;
                            text-align:center;
                        '>
                            Sistema de Consultorios
                        </p>

                    </div>

                </body>
                </html>";


            using (SmtpClient smtp =
                new SmtpClient(
                    "smtp.gmail.com",
                    587))
            {
                smtp.EnableSsl =
                    true;

                smtp.UseDefaultCredentials =
                    false;

                smtp.Credentials =
                    new NetworkCredential(
                        correoRemitente,
                        passwordAplicacion);

                smtp.Send(mensaje);
            }
        }
    }
}
