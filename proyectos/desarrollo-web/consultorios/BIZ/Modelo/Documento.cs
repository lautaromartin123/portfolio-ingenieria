using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Documento
    {
        public int IdDocumento { get; set; }

        public string NumeroDocumento { get; set; }


        // FK Tipo Documento
        public int IdTipoDocumento { get; set; }

        public TipoDocumento TipoDocumento { get; set; }


        // FK Paciente
        public int? IdPaciente { get; set; }

        public Paciente Paciente { get; set; }


        // FK Profesional
        public int? IdProfesional { get; set; }

        // Cuando J cree Profesional creamos..
        // public Profesional Profesional { get; set; }
    }
}
