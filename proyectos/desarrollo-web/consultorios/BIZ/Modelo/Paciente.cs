using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BIZ
{
    public class Paciente
    {
        public int IdPaciente { get; set; }

        // FOREING KEY A la cuenta del usuario.
        public int? IdUsuario { get; set; }


        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Telefono { get; set; }

        public string Foto { get; set; }

        public string NumeroObraSocial { get; set; }



        // FK Obra Social
        public int? IdObraSocial { get; set; }

        public ObraSocial ObraSocial { get; set; }


        // FK Estado
        public int IdEstado { get; set; }

        public Estado Estado { get; set; }

        // Sacar el null cuando cree domicilio
        public int? IdDomicilio { get; set; }

        // public Domicilio Domicilio {get; set;}

    }
}
