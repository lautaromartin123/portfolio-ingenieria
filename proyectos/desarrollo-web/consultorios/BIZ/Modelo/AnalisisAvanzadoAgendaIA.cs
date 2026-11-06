using System;
using System.Collections.Generic;

namespace BIZ
{
    public class AlertaAvanzadaAgendaIA
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


    public class AnalisisAvanzadoAgendaIA
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


        public int DiasLaborables
        {
            get;
            set;
        }


        public int DiasLaborablesSinTurnos
        {
            get;
            set;
        }


        public int CapacidadTeoricaTurnos
        {
            get;
            set;
        }


        public int TurnosDentroHorarioLaboral
        {
            get;
            set;
        }


        public decimal PorcentajeOcupacion
        {
            get;
            set;
        }


        public int HuecosLargos
        {
            get;
            set;
        }


        public int MinutosHuecosTotales
        {
            get;
            set;
        }


        public int MayorHuecoMinutos
        {
            get;
            set;
        }


        public DateTime? FechaMayorHueco
        {
            get;
            set;
        }


        public List<AlertaAvanzadaAgendaIA> Alertas
        {
            get;
            set;
        }


        public AnalisisAvanzadoAgendaIA()
        {
            Alertas =
                new List<AlertaAvanzadaAgendaIA>();
        }
    }
}
