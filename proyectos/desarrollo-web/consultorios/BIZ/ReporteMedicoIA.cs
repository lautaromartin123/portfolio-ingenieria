using System;

namespace BIZ
{
    public class DiaMasAtendidoReporte
    {
        public DateTime Fecha { get; set; }

        public int CantidadAtendidos { get; set; }
    }


    public class HorarioTrabajoReporte
    {
        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }


        public string NombreDia
        {
            get
            {
                switch (DiaSemana)
                {
                    case 1:
                        return "Lunes";

                    case 2:
                        return "Martes";

                    case 3:
                        return "Miércoles";

                    case 4:
                        return "Jueves";

                    case 5:
                        return "Viernes";

                    case 6:
                        return "Sábado";

                    case 7:
                        return "Domingo";

                    default:
                        return "Día desconocido";
                }
            }
        }
    }


    public class AgendaDiaReporte
    {
        public DateTime Fecha { get; set; }

        public int CantidadTurnos { get; set; }

        public DateTime PrimerTurno { get; set; }

        public DateTime UltimoTurno { get; set; }
    }
}
