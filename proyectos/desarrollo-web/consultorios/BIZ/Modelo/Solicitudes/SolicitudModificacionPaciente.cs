using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class SolicitudModificacionPaciente
    {
        public int IdSolicitud { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Telefono { get; set; }

        public int? IdObraSocial { get; set; }

        public string NumeroObraSocial { get; set; }
    }
}
