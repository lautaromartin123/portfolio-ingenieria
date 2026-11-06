using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class TurnoNegocio
    {
        private const string SELECT_BASE = @"
            SELECT
                T.IdTurno,
                T.FechaHora,
                T.DuracionMinutos,
                T.Motivo,
                T.Observaciones,
                T.IdPaciente,
                T.IdProfesional,
                T.IdEstadoTurno,

                PA.Nombre      AS PacienteNombre,
                PA.Apellido    AS PacienteApellido,

                PR.Nombre      AS ProfesionalNombre,
                PR.Apellido    AS ProfesionalApellido,

                ET.Descripcion AS EstadoTurnoDescripcion

            FROM Turno T

            INNER JOIN Paciente PA
                ON T.IdPaciente = PA.IdPaciente

            INNER JOIN Profesional PR
                ON T.IdProfesional = PR.IdProfesional

            INNER JOIN EstadoTurno ET
                ON T.IdEstadoTurno = ET.IdEstadoTurno
        ";


        private Turno Mapear(SqlDataReader lector)
        {
            Turno turno = new Turno();

            turno.IdTurno =
                Convert.ToInt32(lector["IdTurno"]);

            turno.FechaHora =
                Convert.ToDateTime(lector["FechaHora"]);

            turno.DuracionMinutos =
                Convert.ToInt32(lector["DuracionMinutos"]);

            turno.Motivo =
                lector["Motivo"].ToString();

            turno.Observaciones =
                lector["Observaciones"].ToString();


            // PACIENTE
            turno.IdPaciente =
                Convert.ToInt32(lector["IdPaciente"]);

            turno.Paciente = new Paciente();

            turno.Paciente.IdPaciente =
                Convert.ToInt32(lector["IdPaciente"]);

            turno.Paciente.Nombre =
                lector["PacienteNombre"].ToString();

            turno.Paciente.Apellido =
                lector["PacienteApellido"].ToString();


            // PROFESIONAL
            turno.IdProfesional =
                Convert.ToInt32(lector["IdProfesional"]);

            turno.Profesional = new Profesional();

            turno.Profesional.IdProfesional =
                Convert.ToInt32(lector["IdProfesional"]);

            turno.Profesional.Nombre =
                lector["ProfesionalNombre"].ToString();

            turno.Profesional.Apellido =
                lector["ProfesionalApellido"].ToString();


            // ESTADO DEL TURNO
            turno.IdEstadoTurno =
                Convert.ToInt32(lector["IdEstadoTurno"]);

            turno.EstadoTurno = new EstadoTurno();

            turno.EstadoTurno.IdEstadoTurno =
                Convert.ToInt32(lector["IdEstadoTurno"]);

            turno.EstadoTurno.Descripcion =
                lector["EstadoTurnoDescripcion"].ToString();

            return turno;
        }


        public List<Turno> Listar()
        {
            List<Turno> lista = new List<Turno>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE + " ORDER BY T.FechaHora";

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


        public List<Turno> ListarPorProfesional(int idProfesional)
        {
            List<Turno> lista = new List<Turno>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " WHERE T.IdProfesional = @IdProfesional " +
                    " ORDER BY T.FechaHora";

                comando.Parameters.AddWithValue(
                    "@IdProfesional", idProfesional);

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


        public List<Turno> ListarPorPaciente(int idPaciente)
        {
            List<Turno> lista = new List<Turno>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " WHERE T.IdPaciente = @IdPaciente " +
                    " ORDER BY T.FechaHora";

                comando.Parameters.AddWithValue(
                    "@IdPaciente", idPaciente);

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


        /* Todos los turnos de un dia. Se compara solo la parte de
           fecha para que no influya la hora. */
        public List<Turno> ListarPorFecha(DateTime fecha)
        {
            List<Turno> lista = new List<Turno>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " WHERE CAST(T.FechaHora AS DATE) = @Fecha " +
                    " ORDER BY T.FechaHora";

                comando.Parameters.AddWithValue(
                    "@Fecha", fecha.Date);

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


        /* La agenda del dia de un profesional: es la consulta mas
           usada de la pantalla de turnos. */
        public List<Turno> ListarPorProfesionalYFecha(
            int idProfesional, DateTime fecha)
        {
            List<Turno> lista = new List<Turno>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    @" WHERE T.IdProfesional = @IdProfesional
                         AND CAST(T.FechaHora AS DATE) = @Fecha " +
                    " ORDER BY T.FechaHora";

                comando.Parameters.AddWithValue(
                    "@IdProfesional", idProfesional);

                comando.Parameters.AddWithValue(
                    "@Fecha", fecha.Date);

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


        public Turno BuscarPorId(int idTurno)
        {
            Turno turno = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE + " WHERE T.IdTurno = @IdTurno";

                comando.Parameters.AddWithValue("@IdTurno", idTurno);

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    turno = Mapear(lector);
                }

                lector.Close();

                return turno;
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


        /* Indica si el profesional ya tiene otro turno que se pisa
           con el que se quiere dar.

           Dos turnos se superponen cuando uno empieza antes de que
           termine el otro. Con inicio A/fin A e inicio B/fin B:

               se pisan  si  A_inicio < B_fin  Y  B_inicio < A_fin

           Los turnos cancelados no cuentan: ese horario quedo libre.

           idTurnoExcluir sirve al modificar un turno, para que no se
           choque consigo mismo. Al dar uno nuevo se pasa 0. */
        public bool HaySolapamiento(
            int idProfesional,
            DateTime fechaHora,
            int duracionMinutos,
            int idTurnoExcluir)
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

                    FROM Turno T

                    INNER JOIN EstadoTurno ET
                        ON T.IdEstadoTurno = ET.IdEstadoTurno

                    WHERE T.IdProfesional = @IdProfesional
                      AND T.IdTurno <> @IdTurnoExcluir
                      AND ET.Descripcion <> 'Cancelado'
                      AND @Inicio < DATEADD(MINUTE, T.DuracionMinutos, T.FechaHora)
                      AND T.FechaHora < @Fin
                ";

                comando.Parameters.AddWithValue(
                    "@IdProfesional", idProfesional);

                comando.Parameters.AddWithValue(
                    "@IdTurnoExcluir", idTurnoExcluir);

                comando.Parameters.AddWithValue(
                    "@Inicio", fechaHora);

                comando.Parameters.AddWithValue(
                    "@Fin", fechaHora.AddMinutes(duracionMinutos));

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


        public int Agregar(Turno turno)
        {
            return new AgendaTurnosNegocio().GuardarAdministrativo(turno, false);
        }


        public void Modificar(Turno turno)
        {
            new AgendaTurnosNegocio().GuardarAdministrativo(turno, true);
        }


        /* Cambia solo el estado del turno: Confirmado, Atendido,
           Ausente, etc. Sirve para los botones del listado sin tener
           que reescribir el turno entero. */
        public void CambiarEstado(int idTurno, int idEstadoTurno)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    UPDATE Turno
                    SET IdEstadoTurno = @IdEstadoTurno
                    WHERE IdTurno = @IdTurno
                ";

                comando.Parameters.AddWithValue(
                    "@IdEstadoTurno", idEstadoTurno);

                comando.Parameters.AddWithValue(
                    "@IdTurno", idTurno);

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


        /* La baja de un turno es pasarlo a Cancelado. No se borra:
           el turno cancelado es informacion que hay que conservar. */
        public void Cancelar(int idTurno)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    UPDATE Turno
                    SET IdEstadoTurno = (
                        SELECT IdEstadoTurno
                        FROM EstadoTurno
                        WHERE Descripcion = 'Cancelado'
                    )
                    WHERE IdTurno = @IdTurno
                ";

                comando.Parameters.AddWithValue("@IdTurno", idTurno);

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
