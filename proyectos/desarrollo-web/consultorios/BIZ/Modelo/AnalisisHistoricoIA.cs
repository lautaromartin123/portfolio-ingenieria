using System;
using System.Collections.Generic;
using System.Globalization;

namespace BIZ
{
    public class MesActividadHistoricaIA
    {
        public int Anio
        {
            get;
            set;
        }


        public int Mes
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


        public string NombreMes
        {
            get
            {
                if (Mes < 1 ||
                    Mes > 12)
                {
                    return "";
                }


                return new DateTime(
                    2000,
                    Mes,
                    1)
                    .ToString(
                        "MMMM",
                        new CultureInfo("es-AR"));
            }
        }


        public string NombrePeriodo
        {
            get
            {
                return
                    NombreMes
                    +
                    " "
                    +
                    Anio;
            }
        }
    }


    public class DiaSemanaHistoricoIA
    {
        public int DiaSemana
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


        public string NombreDia
        {
            get
            {
                switch (DiaSemana)
                {
                    case 1:
                        return "lunes";

                    case 2:
                        return "martes";

                    case 3:
                        return "miércoles";

                    case 4:
                        return "jueves";

                    case 5:
                        return "viernes";

                    case 6:
                        return "sábado";

                    case 7:
                        return "domingo";

                    default:
                        return "";
                }
            }
        }
    }


    public class ResumenHistoricoIA
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


        public int OtrosEstados
        {
            get;
            set;
        }


        public decimal TasaAtencion
        {
            get;
            set;
        }


        public decimal TasaCancelacion
        {
            get;
            set;
        }


        public decimal TasaAtencionSobreNoCancelados
        {
            get;
            set;
        }


        public decimal PromedioTurnosPorMes
        {
            get;
            set;
        }


        public decimal PromedioAtendidosPorMes
        {
            get;
            set;
        }


        public MesActividadHistoricaIA MesMasActivo
        {
            get;
            set;
        }


        public MesActividadHistoricaIA MesConMasAtendidos
        {
            get;
            set;
        }


        public DiaSemanaHistoricoIA DiaSemanaMasActivo
        {
            get;
            set;
        }


        public DiaSemanaHistoricoIA DiaSemanaConMasAtendidos
        {
            get;
            set;
        }


        public List<MesActividadHistoricaIA> Meses
        {
            get;
            set;
        }


        public List<DiaSemanaHistoricoIA> DiasSemana
        {
            get;
            set;
        }


        public ResumenHistoricoIA()
        {
            Meses =
                new List<MesActividadHistoricaIA>();


            DiasSemana =
                new List<DiaSemanaHistoricoIA>();
        }
    }
}
