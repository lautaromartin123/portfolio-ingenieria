using System;
using System.Collections.Generic;

namespace BIZ
{
    public class AlertaAgendaIA
    {
        public string Tipo
        {
            get;
            set;
        }


        public DateTime? FechaHora
        {
            get;
            set;
        }


        public string Descripcion
        {
            get;
            set;
        }
    }


    public class AnalisisAgendaIA
    {
        public DateTime Desde
        {
            get;
            set;
        }


        public DateTime Hasta
        {
            get;
            set;
        }


        public int TurnosAnalizados
        {
            get;
            set;
        }


        public int TurnosFueraHorario
        {
            get;
            set;
        }


        public int TurnosEnDiaNoLaborable
        {
            get;
            set;
        }


        public int Superposiciones
        {
            get;
            set;
        }


        public List<AlertaAgendaIA> Alertas
        {
            get;
            set;
        }


        public AnalisisAgendaIA()
        {
            Alertas =
                new List<AlertaAgendaIA>();
        }
    }
}
