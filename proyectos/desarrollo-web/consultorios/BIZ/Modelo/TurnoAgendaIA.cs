using System;

namespace BIZ
{
    public class TurnoAgendaIA
    {
        public int IdTurno
        {
            get;
            set;
        }


        public DateTime FechaHora
        {
            get;
            set;
        }


        public int DuracionMinutos
        {
            get;
            set;
        }


        public string Estado
        {
            get;
            set;
        }


        public DateTime FechaFin
        {
            get
            {
                return FechaHora.AddMinutes(
                    DuracionMinutos);
            }
        }
    }
}
