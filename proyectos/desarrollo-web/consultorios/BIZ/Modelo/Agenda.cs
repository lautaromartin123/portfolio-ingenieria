using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Agenda
    {
        public int IdAgenda { get; set; }


        // FK Profesional
        public int IdProfesional { get; set; }

        public Profesional Profesional { get; set; }


        // 1 = Lunes ... 7 = Domingo
        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public int DuracionMinutos { get; set; }


        // true = atiende con turno, false = por orden de llegada
        public bool AtencionConTurno { get; set; }


        // FK Estado
        public int IdEstado { get; set; }

        public Estado Estado { get; set; }


        /* Nombre del dia para mostrar en pantalla, en vez del numero. */
        public string NombreDia
        {
            get
            {
                switch (DiaSemana)
                {
                    case 1: return "Lunes";
                    case 2: return "Martes";
                    case 3: return "Miercoles";
                    case 4: return "Jueves";
                    case 5: return "Viernes";
                    case 6: return "Sabado";
                    case 7: return "Domingo";
                    default: return "Dia " + DiaSemana;
                }
            }
        }


        /* Franja horaria lista para mostrar: "08:00 a 12:00" */
        public string Horario
        {
            get
            {
                return HoraInicio.ToString(@"hh\:mm")
                     + " a "
                     + HoraFin.ToString(@"hh\:mm");
            }
        }
    }
}
