using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class AgendaNegocio
    {
        private const string SELECT_BASE = @"
            SELECT
                A.IdAgenda,
                A.IdProfesional,
                A.DiaSemana,
                A.HoraInicio,
                A.HoraFin,
                A.DuracionMinutos,
                A.AtencionConTurno,
                A.IdEstado,

                P.Nombre       AS ProfesionalNombre,
                P.Apellido     AS ProfesionalApellido,
                ES.Descripcion AS EstadoDescripcion

            FROM Agenda A

            INNER JOIN Profesional P
                ON A.IdProfesional = P.IdProfesional

            INNER JOIN Estado ES
                ON A.IdEstado = ES.IdEstado
        ";


        private Agenda Mapear(SqlDataReader lector)
        {
            Agenda agenda = new Agenda();

            agenda.IdAgenda =
                Convert.ToInt32(lector["IdAgenda"]);

            agenda.DiaSemana =
                Convert.ToInt32(lector["DiaSemana"]);

            agenda.HoraInicio =
                (TimeSpan)lector["HoraInicio"];

            agenda.HoraFin =
                (TimeSpan)lector["HoraFin"];

            agenda.DuracionMinutos =
                Convert.ToInt32(lector["DuracionMinutos"]);

            agenda.AtencionConTurno =
                Convert.ToBoolean(lector["AtencionConTurno"]);


            // PROFESIONAL
            agenda.IdProfesional =
                Convert.ToInt32(lector["IdProfesional"]);

            agenda.Profesional = new Profesional();

            agenda.Profesional.IdProfesional =
                Convert.ToInt32(lector["IdProfesional"]);

            agenda.Profesional.Nombre =
                lector["ProfesionalNombre"].ToString();

            agenda.Profesional.Apellido =
                lector["ProfesionalApellido"].ToString();


            // ESTADO
            agenda.IdEstado =
                Convert.ToInt32(lector["IdEstado"]);

            agenda.Estado = new Estado();

            agenda.Estado.IdEstado =
                Convert.ToInt32(lector["IdEstado"]);

            agenda.Estado.Descripcion =
                lector["EstadoDescripcion"].ToString();

            return agenda;
        }


        public List<Agenda> Listar()
        {
            List<Agenda> lista = new List<Agenda>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " ORDER BY P.Apellido, A.DiaSemana, A.HoraInicio";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(Mapear(lector));
                }

                lector.Close();

                return lista;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        /* Los horarios de atencion de un profesional, ordenados
           por dia de la semana. */
        public List<Agenda> ListarPorProfesional(int idProfesional)
        {
            List<Agenda> lista = new List<Agenda>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " WHERE A.IdProfesional = @IdProfesional " +
                    " ORDER BY A.DiaSemana, A.HoraInicio";

                comando.Parameters.AddWithValue(
                    "@IdProfesional",
                    idProfesional
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(Mapear(lector));
                }

                lector.Close();

                return lista;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        public Agenda BuscarPorId(int idAgenda)
        {
            Agenda agenda = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE + " WHERE A.IdAgenda = @IdAgenda";

                comando.Parameters.AddWithValue("@IdAgenda", idAgenda);

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    agenda = Mapear(lector);
                }

                lector.Close();

                return agenda;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        /* Indica si el profesional atiende en ese dia y horario.

           Busca una franja activa de la agenda que sea del mismo dia
           de la semana y que contenga al turno entero: tiene que
           empezar antes (o junto) y terminar despues (o junto).

           La agenda guarda el dia como 1=Lunes ... 7=Domingo, pero
           DateTime.DayOfWeek devuelve 0=Domingo ... 6=Sabado, asi que
           hay que convertirlo antes de comparar. */
        public bool CubreHorario(
            int idProfesional,
            DateTime fechaHora,
            int duracionMinutos)
        {
            int diaSemana = (int)fechaHora.DayOfWeek;

            if (diaSemana == 0)
                diaSemana = 7;          // domingo

            TimeSpan horaInicio = fechaHora.TimeOfDay;

            TimeSpan horaFin =
                fechaHora.AddMinutes(duracionMinutos).TimeOfDay;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT COUNT(*)

                    FROM Agenda A

                    INNER JOIN Estado E
                        ON A.IdEstado = E.IdEstado

                    WHERE A.IdProfesional = @IdProfesional
                      AND A.DiaSemana     = @DiaSemana
                      AND E.Descripcion   = 'Activo'
                      AND A.HoraInicio   <= @HoraInicio
                      AND A.HoraFin      >= @HoraFin
                ";

                comando.Parameters.AddWithValue(
                    "@IdProfesional", idProfesional);

                comando.Parameters.AddWithValue(
                    "@DiaSemana", diaSemana);

                comando.Parameters.AddWithValue(
                    "@HoraInicio", horaInicio);

                comando.Parameters.AddWithValue(
                    "@HoraFin", horaFin);

                conexion.Open();

                int cantidad = Convert.ToInt32(comando.ExecuteScalar());

                return cantidad > 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        /* True si el profesional no tiene ninguna franja horaria
           activa cargada. Sirve para avisar en vez de rechazar: si
           nadie le cargo la agenda todavia, no tiene sentido impedir
           que se le den turnos. */
        public bool SinAgendaCargada(int idProfesional)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT COUNT(*)

                    FROM Agenda A

                    INNER JOIN Estado E
                        ON A.IdEstado = E.IdEstado

                    WHERE A.IdProfesional = @IdProfesional
                      AND E.Descripcion   = 'Activo'
                ";

                comando.Parameters.AddWithValue(
                    "@IdProfesional", idProfesional);

                conexion.Open();

                int cantidad = Convert.ToInt32(comando.ExecuteScalar());

                return cantidad == 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        public int Agregar(Agenda agenda)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    INSERT INTO Agenda
                    (
                        IdProfesional,
                        DiaSemana,
                        HoraInicio,
                        HoraFin,
                        DuracionMinutos,
                        AtencionConTurno,
                        IdEstado
                    )

                    OUTPUT INSERTED.IdAgenda

                    VALUES
                    (
                        @IdProfesional,
                        @DiaSemana,
                        @HoraInicio,
                        @HoraFin,
                        @DuracionMinutos,
                        @AtencionConTurno,
                        @IdEstado
                    )
                ";

                comando.Parameters.AddWithValue(
                    "@IdProfesional", agenda.IdProfesional);

                comando.Parameters.AddWithValue(
                    "@DiaSemana", agenda.DiaSemana);

                comando.Parameters.AddWithValue(
                    "@HoraInicio", agenda.HoraInicio);

                comando.Parameters.AddWithValue(
                    "@HoraFin", agenda.HoraFin);

                comando.Parameters.AddWithValue(
                    "@DuracionMinutos",
                    agenda.DuracionMinutos == 0 ? 30 : agenda.DuracionMinutos);

                comando.Parameters.AddWithValue(
                    "@AtencionConTurno", agenda.AtencionConTurno);

                comando.Parameters.AddWithValue(
                    "@IdEstado",
                    agenda.IdEstado == 0 ? 1 : agenda.IdEstado);

                conexion.Open();

                return Convert.ToInt32(comando.ExecuteScalar());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        public void Modificar(Agenda agenda)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    UPDATE Agenda
                    SET
                        DiaSemana        = @DiaSemana,
                        HoraInicio       = @HoraInicio,
                        HoraFin          = @HoraFin,
                        DuracionMinutos  = @DuracionMinutos,
                        AtencionConTurno = @AtencionConTurno,
                        IdEstado         = @IdEstado
                    WHERE IdAgenda = @IdAgenda
                ";

                comando.Parameters.AddWithValue(
                    "@DiaSemana", agenda.DiaSemana);

                comando.Parameters.AddWithValue(
                    "@HoraInicio", agenda.HoraInicio);

                comando.Parameters.AddWithValue(
                    "@HoraFin", agenda.HoraFin);

                comando.Parameters.AddWithValue(
                    "@DuracionMinutos", agenda.DuracionMinutos);

                comando.Parameters.AddWithValue(
                    "@AtencionConTurno", agenda.AtencionConTurno);

                comando.Parameters.AddWithValue(
                    "@IdEstado", agenda.IdEstado);

                comando.Parameters.AddWithValue(
                    "@IdAgenda", agenda.IdAgenda);

                conexion.Open();

                comando.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        /* Baja logica: la franja horaria deja de estar vigente pero
           no se borra, porque puede haber turnos historicos que la
           usaron como referencia. */
        public void DarDeBaja(int idAgenda)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    UPDATE Agenda
                    SET IdEstado = (
                        SELECT IdEstado
                        FROM Estado
                        WHERE Descripcion = 'Inactivo'
                    )
                    WHERE IdAgenda = @IdAgenda
                ";

                comando.Parameters.AddWithValue("@IdAgenda", idAgenda);

                conexion.Open();

                comando.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }
    }
}
