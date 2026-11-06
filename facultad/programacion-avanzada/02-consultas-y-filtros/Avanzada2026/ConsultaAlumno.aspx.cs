using System;
using BIZ;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Avanzada2026
{
    public partial class ConsultaAlumno : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarInicial();
            }

        }

        private void CargarInicial()
        {
            DdCurso.DataSource=BIZ.Curso.ListarCursos();
            DdCurso.DataTextField = "Nombre";
            DdCurso.DataValueField = "IdCurso";
            DdCurso.DataBind();

            CargarRepeater();
        }

        protected void DdCurso_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarRepeater();
        }
        private void CargarRepeater()
        {
            RepAlumnos.DataSource = BIZ.Alumnos.ListarAlumnosPorCurso(Convert.ToInt32(DdCurso.SelectedValue));
            RepAlumnos.DataBind();
        }

    }
}
