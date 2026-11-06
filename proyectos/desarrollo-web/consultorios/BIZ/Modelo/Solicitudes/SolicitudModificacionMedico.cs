using System;
using System.Collections.Generic;

namespace BIZ
{
    public class SolicitudModificacionMedico
    {
        public int IdSolicitud { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public int IdEspecialidad { get; set; }

        public List<int> IdsEspecialidades { get; set; } = new List<int>();

        public string NumeroMatricula { get; set; }

        public int? IdTipoMatricula { get; set; }

        public string EntidadEmisora { get; set; }

        public DateTime? FechaVencimiento { get; set; }
    }
}
