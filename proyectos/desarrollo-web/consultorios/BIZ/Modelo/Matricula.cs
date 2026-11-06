using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Matricula
    {
        public int IdMatricula { get; set; }

        public string Numero { get; set; }

        public string EntidadEmisora { get; set; }

        public DateTime? FechaVencimiento { get; set; }


        // FK Profesional
        public int IdProfesional { get; set; }


        // FK TipoMatricula
        public int IdTipoMatricula { get; set; }

        public TipoMatricula TipoMatricula { get; set; }


        // FK Estado
        public int IdEstado { get; set; }

        public Estado Estado { get; set; }


        // True si la matricula ya vencio. No se guarda en la base:
        // se calcula en el momento a partir de FechaVencimiento.
        public bool EstaVencida
        {
            get
            {
                if (FechaVencimiento == null)
                    return false;

                return FechaVencimiento.Value.Date < DateTime.Today;
            }
        }
    }
}
