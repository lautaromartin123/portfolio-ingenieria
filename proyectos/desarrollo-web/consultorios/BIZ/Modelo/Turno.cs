using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Turno
    {
        public int IdTurno { get; set; }

        public DateTime FechaHora { get; set; }

        public int DuracionMinutos { get; set; }

        public string Motivo { get; set; }

        public string Observaciones { get; set; }


        // FK Paciente
        public int IdPaciente { get; set; }

        public Paciente Paciente { get; set; }


        // FK Profesional
        public int IdProfesional { get; set; }

        public Profesional Profesional { get; set; }


        // FK EstadoTurno
        public int IdEstadoTurno { get; set; }

        public EstadoTurno EstadoTurno { get; set; }


        /* Momento en que termina el turno. No se guarda en la base:
           se calcula con FechaHora + DuracionMinutos. */
        public DateTime FechaHoraFin
        {
            get { return FechaHora.AddMinutes(DuracionMinutos); }
        }
    }
}
