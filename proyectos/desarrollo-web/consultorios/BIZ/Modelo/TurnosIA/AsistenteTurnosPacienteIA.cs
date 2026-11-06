using System;
using System.Collections.Generic;

namespace BIZ
{
    [Serializable]
    public class EstadoReservaTurnoIA
    {
        public string Paso { get; set; }

        public int IdPaciente { get; set; }

        public int? IdEspecialidad { get; set; }

        public string Especialidad { get; set; }

        public int? IdProfesional { get; set; }

        public string Profesional { get; set; }

        public bool CualquierProfesional { get; set; }

        public DateTime? FechaSeleccionada { get; set; }

        public DateTime? FechaHoraSeleccionada { get; set; }

        public int DuracionMinutos { get; set; }

        public int? IdTurnoCancelacion { get; set; }

        public string Motivo { get; set; }

        public string PreferenciaMomento { get; set; }

        public TimeSpan? HoraDesde { get; set; }

        public TimeSpan? HoraHasta { get; set; }


        public EstadoReservaTurnoIA()
        {
            Paso = "INICIO";
            Motivo = null;
        }
    }


    [Serializable]
    public class MensajeChatTurnoIA
    {
        public string Rol { get; set; }

        public string Texto { get; set; }


        public MensajeChatTurnoIA()
        {
        }


        public MensajeChatTurnoIA(
            string rol,
            string texto)
        {
            Rol = rol;
            Texto = texto;
        }
    }


    [Serializable]
    public class OpcionChatTurnoIA
    {
        public string Texto { get; set; }

        public string Valor { get; set; }


        public OpcionChatTurnoIA()
        {
        }


        public OpcionChatTurnoIA(
            string texto,
            string valor)
        {
            Texto = texto;
            Valor = valor;
        }
    }


    public class EspecialidadTurnoIA
    {
        public int IdEspecialidad { get; set; }

        public string Nombre { get; set; }
    }


    public class ProfesionalTurnoIA
    {
        public int IdProfesional { get; set; }

        public int IdEspecialidad { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Especialidad { get; set; }


        public string NombreCompleto
        {
            get
            {
                return
                    (Nombre + " " + Apellido)
                    .Trim();
            }
        }
    }


    public class SlotTurnoIA
    {
        public int IdProfesional { get; set; }

        public string Profesional { get; set; }

        public int IdEspecialidad { get; set; }

        public string Especialidad { get; set; }

        public DateTime FechaHora { get; set; }

        public int DuracionMinutos { get; set; }
    }


    public class TurnoPacienteResumenIA
    {
        public int IdTurno { get; set; }

        public DateTime FechaHora { get; set; }

        public int DuracionMinutos { get; set; }

        public string Estado { get; set; }

        public string Motivo { get; set; }

        public int IdProfesional { get; set; }

        public string Profesional { get; set; }

        public int IdEspecialidad { get; set; }

        public string Especialidad { get; set; }
    }


    public class ResultadoReservaTurnoIA
    {
        public bool Exito { get; set; }

        public string Mensaje { get; set; }

        public int IdTurno { get; set; }

        public TurnoPacienteResumenIA Turno { get; set; }
    }


    public class ResultadoOperacionTurnoIA
    {
        public bool Exito { get; set; }

        public string Mensaje { get; set; }
    }


    public class InterpretacionTurnoPacienteIA
    {
        public string Accion { get; set; }

        public string Especialidad { get; set; }

        public string Profesional { get; set; }

        public string Periodo { get; set; }

        public string Fecha { get; set; }

        public string Momento { get; set; }

        public string HoraDesde { get; set; }

        public string HoraHasta { get; set; }

        public string Motivo { get; set; }
    }


    internal class FranjaAgendaTurnoIA
    {
        public int IdProfesional { get; set; }

        public int IdEspecialidad { get; set; }

        public string Profesional { get; set; }

        public string Especialidad { get; set; }

        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public int DuracionMinutos { get; set; }
    }


    internal class TurnoOcupadoIA
    {
        public int IdProfesional { get; set; }

        public int IdPaciente { get; set; }

        public DateTime Inicio { get; set; }

        public int DuracionMinutos { get; set; }


        public DateTime Fin
        {
            get
            {
                return
                    Inicio.AddMinutes(
                        DuracionMinutos);
            }
        }
    }
}
