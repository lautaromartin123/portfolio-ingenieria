using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class SolicitudModificacion
    {
        public int IdSolicitud { get; set; }

        public int IdUsuarioSolicitante { get; set; }

        public int? IdPaciente { get; set; }

        public int? IdProfesional { get; set; }

        public int IdEstadoSolicitud { get; set; }

        public EstadoSolicitud EstadoSolicitud { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public int? IdUsuarioRevisor { get; set; }

        public DateTime? FechaResolucion { get; set; }

        public string ObservacionRevision { get; set; }
    }
}
