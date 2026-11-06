using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Profesional
    {
        public int IdProfesional { get; set; }


        // FOREING KEY A la cuenta del usuario.
        public int? IdUsuario { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Foto { get; set; }


        // FK Especialidad
        public int IdEspecialidad { get; set; }

        public Especialidad Especialidad { get; set; }


        // FK Estado
        public int IdEstado { get; set; }

        public Estado Estado { get; set; }


        // FK Domicilio - sacar el ? cuando exista la clase Domicilio
        public int? IdDomicilio { get; set; }

        // public Domicilio Domicilio { get; set; }
    }
}
