using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BIZ;


namespace Consultorios
{
    public partial class AltaPaciente : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarTipoDocumento();
                CargarObrasSociales();
            }
        }

        private void CargarTipoDocumento()
        {
            TipoDocumentoNegocio negocio =
                new TipoDocumentoNegocio();

            DdTipoDocumento.DataSource = negocio.Listar();

            DdTipoDocumento.DataTextField = "Descripcion";
            DdTipoDocumento.DataValueField = "IdTipoDocumento";

            DdTipoDocumento.DataBind();

            DdTipoDocumento.Items.Insert(
                0,
                new ListItem("-- Seleccione --", "")
            );
        }

        private void CargarObrasSociales()
        {
            ObraSocialNegocio negocio =
                new ObraSocialNegocio();

            DdObraSocial.DataSource = negocio.Listar();

            DdObraSocial.DataTextField = "Nombre";
            DdObraSocial.DataValueField = "IdObraSocial";

            DdObraSocial.DataBind();

            DdObraSocial.Items.Insert(
                0,
                new ListItem("-- Sin obra social --", "")
            );
        }

        protected void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtNombre.Text))
                {
                    LblMensaje.Text = "Ingrese el nombre.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtApellido.Text))
                {
                    LblMensaje.Text = "Ingrese el apellido.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(DdTipoDocumento.SelectedValue))
                {
                    LblMensaje.Text = "Seleccione un tipo de documento.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtNumeroDocumento.Text))
                {
                    LblMensaje.Text = "Ingrese el número de documento.";
                    return;
                }


                Paciente paciente = new Paciente();

                paciente.Nombre = TxtNombre.Text.Trim();
                paciente.Apellido = TxtApellido.Text.Trim();
                paciente.Telefono = TxtTelefono.Text.Trim();

                paciente.Foto = null;

                paciente.NumeroObraSocial =
                    string.IsNullOrWhiteSpace(TxtNumeroObraSocial.Text)
                    ? null
                    : TxtNumeroObraSocial.Text.Trim();


                if (string.IsNullOrWhiteSpace(DdObraSocial.SelectedValue))
                {
                    paciente.IdObraSocial = null;
                }
                else
                {
                    paciente.IdObraSocial =
                        Convert.ToInt32(DdObraSocial.SelectedValue);
                }


                paciente.IdDomicilio = null;


                Documento documento = new Documento();

                documento.NumeroDocumento =
                    TxtNumeroDocumento.Text.Trim();

                documento.IdTipoDocumento =
                    Convert.ToInt32(DdTipoDocumento.SelectedValue);

                documento.IdPaciente = null;
                documento.IdProfesional = null;


                AltaPacienteNegocio negocio =
                    new AltaPacienteNegocio();

                negocio.Agregar(paciente, documento);


                LblMensaje.Text =
                    "Paciente agregado correctamente.";

                LimpiarCampos();
            }
            catch (Exception ex)
            {
                LblMensaje.Text =
                    "Error al agregar el paciente: " + ex.Message;
            }
        }

        private void LimpiarCampos()
        {
            TxtNombre.Text = "";
            TxtApellido.Text = "";
            TxtTelefono.Text = "";

            TxtNumeroDocumento.Text = "";
            TxtNumeroObraSocial.Text = "";

            DdTipoDocumento.SelectedIndex = 0;
            DdObraSocial.SelectedIndex = 0;
        }






    }
}
