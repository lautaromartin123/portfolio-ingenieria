using System;
using System.Collections.Generic;

namespace BIZ
{
    public class EstadoTurnoCantidadIA
    {
        public string Estado
        {
            get;
            set;
        }


        public int Cantidad
        {
            get;
            set;
        }
    }


    public class DiaAgendaIA
    {
        public DateTime Fecha
        {
            get;
            set;
        }


        public int CantidadTurnos
        {
            get;
            set;
        }


        public DateTime PrimerTurno
        {
            get;
            set;
        }


        public DateTime UltimoTurno
        {
            get;
            set;
        }
    }


    public class ResumenPeriodoIA
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


        public int DiasPeriodo
        {
            get;
            set;
        }


        public int TotalTurnos
        {
            get;
            set;
        }


        public int TurnosNoCancelados
        {
            get;
            set;
        }


        public int Atendidos
        {
            get;
            set;
        }


        public int Cancelados
        {
            get;
            set;
        }


        public int DiasConTurnos
        {
            get;
            set;
        }


        public decimal PromedioTurnosPorDiaConActividad
        {
            get;
            set;
        }


        public DateTime? DiaMasCargado
        {
            get;
            set;
        }


        public int CantidadDiaMasCargado
        {
            get;
            set;
        }


        public DateTime? DiaMenosCargadoConTurnos
        {
            get;
            set;
        }


        public int CantidadDiaMenosCargadoConTurnos
        {
            get;
            set;
        }


        public DateTime? PrimerTurnoPeriodo
        {
            get;
            set;
        }


        public DateTime? UltimoTurnoPeriodo
        {
            get;
            set;
        }


        public List<EstadoTurnoCantidadIA> Estados
        {
            get;
            set;
        }


        public List<DiaAgendaIA> Dias
        {
            get;
            set;
        }


        public ResumenPeriodoIA()
        {
            Estados =
                new List<EstadoTurnoCantidadIA>();


            Dias =
                new List<DiaAgendaIA>();
        }
    }
}
