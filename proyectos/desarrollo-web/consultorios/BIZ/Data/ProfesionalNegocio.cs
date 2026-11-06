using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class ProfesionalNegocio
    {
        /* Consulta base reutilizada por Listar y BuscarPorId.
           Se define una sola vez para que las dos devuelvan
           exactamente las mismas columnas. */
        private const string SELECT_BASE = @"
            SELECT
                P.IdProfesional,
                P.IdUsuario,

                P.Nombre,
                P.Apellido,
                P.Foto,
                P.IdEspecialidad,
                P.IdEstado,
                P.IdDomicilio,

                E.Nombre       AS EspecialidadNombre,
                ES.Descripcion AS EstadoDescripcion

            FROM Profesional P

            INNER JOIN Especialidad E
                ON P.IdEspecialidad = E.IdEspecialidad

            INNER JOIN Estado ES
                ON P.IdEstado = ES.IdEstado
        ";


        /* Arma un Profesional a partir de la fila actual del lector.
           Evita repetir el mismo bloque en cada metodo. */
        private Profesional Mapear(SqlDataReader lector)
        {
            Profesional profesional = new Profesional();

            profesional.IdProfesional =
                Convert.ToInt32(lector["IdProfesional"]);
            if (lector["IdUsuario"] != DBNull.Value)
            {
                profesional.IdUsuario =
                    Convert.ToInt32(lector["IdUsuario"]);
            }

            profesional.Nombre =
                lector["Nombre"].ToString();

            profesional.Apellido =
                lector["Apellido"].ToString();

            profesional.Foto =
                lector["Foto"].ToString();


            // ESPECIALIDAD
            profesional.IdEspecialidad =
                Convert.ToInt32(lector["IdEspecialidad"]);

            profesional.Especialidad = new Especialidad();

            profesional.Especialidad.IdEspecialidad =
                Convert.ToInt32(lector["IdEspecialidad"]);

            profesional.Especialidad.Nombre =
                lector["EspecialidadNombre"].ToString();


            // ESTADO
            profesional.IdEstado =
                Convert.ToInt32(lector["IdEstado"]);

            profesional.Estado = new Estado();

            profesional.Estado.IdEstado =
                Convert.ToInt32(lector["IdEstado"]);

            profesional.Estado.Descripcion =
                lector["EstadoDescripcion"].ToString();


            // DOMICILIO
            if (lector["IdDomicilio"] != DBNull.Value)
            {
                profesional.IdDomicilio =
                    Convert.ToInt32(lector["IdDomicilio"]);
            }

            return profesional;
        }


        /* Devuelve todos los profesionales, activos e inactivos.
           Para mostrar solo los activos usar ListarActivos. */
        public List<Profesional> Listar()
        {
            List<Profesional> lista = new List<Profesional>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE + " ORDER BY P.Apellido, P.Nombre";

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


        /* Solo los profesionales dados de alta.
           Es lo que hay que usar para cargar los combos de Turnos. */
        public List<Profesional> ListarActivos()
        {
            List<Profesional> lista = new List<Profesional>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " WHERE ES.Descripcion = 'Activo' " +
                    " ORDER BY P.Apellido, P.Nombre";

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


        public Profesional BuscarPorId(int idProfesional)
        {
            Profesional profesional = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE + " WHERE P.IdProfesional = @IdProfesional";

                comando.Parameters.AddWithValue(
                    "@IdProfesional",
                    idProfesional
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    profesional = Mapear(lector);
                }

                lector.Close();

                return profesional;
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

        public Profesional BuscarPorUsuario(int idUsuario)
        {
            Profesional profesional = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                 SELECT_BASE + " WHERE P.IdUsuario = @IdUsuario";

                comando.Parameters.Add(
                 "@IdUsuario",
                 System.Data.SqlDbType.Int
                 ).Value = idUsuario;

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    profesional = Mapear(lector);
                }

                lector.Close();

                return profesional;
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


        /* Busca por apellido, nombre o especialidad.
           Si el filtro viene vacio devuelve la lista completa. */
        public List<Profesional> Buscar(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Listar();

            List<Profesional> lista = new List<Profesional>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    @" WHERE P.Apellido LIKE @Filtro
                          OR P.Nombre   LIKE @Filtro
                          OR E.Nombre   LIKE @Filtro
                       ORDER BY P.Apellido, P.Nombre";

                comando.Parameters.AddWithValue(
                    "@Filtro",
                    "%" + filtro.Trim() + "%"
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


        public int Agregar(Profesional profesional)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            try
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int id = Agregar(profesional, conexion, transaccion);
                    transaccion.Commit();
                    return id;
                }
            }
            finally
            {
                conexion.Close();
            }
        }


        /* Sobrecarga para usar dentro de una transaccion, por ejemplo
           al dar de alta un profesional junto con su matricula. */
        internal int Agregar(
            Profesional profesional,
            SqlConnection conexion,
            SqlTransaction transaccion)
        {
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexion;
            comando.Transaction = transaccion;

            comando.CommandText = @"
    INSERT INTO Profesional
    (
        Nombre,
        Apellido,
        Foto,
        IdEspecialidad,
        IdEstado,
        IdDomicilio,
        IdUsuario
    )

    OUTPUT INSERTED.IdProfesional

    VALUES
    (
        @Nombre,
        @Apellido,
        @Foto,
        @IdEspecialidad,
        @IdEstado,
        @IdDomicilio,
        @IdUsuario
    )
";

            comando.Parameters.AddWithValue("@Nombre", profesional.Nombre);
            comando.Parameters.AddWithValue("@Apellido", profesional.Apellido);

            comando.Parameters.AddWithValue(
                "@Foto",
                (object)profesional.Foto ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdEspecialidad",
                profesional.IdEspecialidad
            );

            /* Si no se indica estado se da de alta como Activo (1),
               que es el mismo default que tiene la tabla. */
            comando.Parameters.AddWithValue(
                "@IdEstado",
                profesional.IdEstado == 0 ? 1 : profesional.IdEstado
            );

            comando.Parameters.AddWithValue(
                "@IdDomicilio",
                (object)profesional.IdDomicilio ?? DBNull.Value
            );
            comando.Parameters.Add(
              "@IdUsuario",
               System.Data.SqlDbType.Int
               ).Value = (object)profesional.IdUsuario ?? DBNull.Value;

            int idProfesional = Convert.ToInt32(comando.ExecuteScalar());
            new EspecialidadNegocio().GuardarPrincipal(
                idProfesional, profesional.IdEspecialidad, conexion, transaccion);
            return idProfesional;
        }


        public void Modificar(Profesional profesional)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            try
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    Modificar(profesional, conexion, transaccion);
                    transaccion.Commit();
                }
            }
            finally
            {
                conexion.Close();
            }
        }


        internal void Modificar(
            Profesional profesional,
            SqlConnection conexion,
            SqlTransaction transaccion)
        {
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexion;
            comando.Transaction = transaccion;

            comando.CommandText = @"
                UPDATE Profesional
                SET
                    Nombre         = @Nombre,
                    Apellido       = @Apellido,
                    Foto           = @Foto,
                    IdEspecialidad = @IdEspecialidad,
                    IdEstado       = @IdEstado,
                    IdDomicilio    = @IdDomicilio
                WHERE IdProfesional = @IdProfesional
            ";

            comando.Parameters.AddWithValue("@Nombre", profesional.Nombre);
            comando.Parameters.AddWithValue("@Apellido", profesional.Apellido);

            comando.Parameters.AddWithValue(
                "@Foto",
                (object)profesional.Foto ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdEspecialidad",
                profesional.IdEspecialidad
            );

            comando.Parameters.AddWithValue(
                "@IdEstado",
                profesional.IdEstado
            );

            comando.Parameters.AddWithValue(
                "@IdDomicilio",
                (object)profesional.IdDomicilio ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdProfesional",
                profesional.IdProfesional
            );

            comando.ExecuteNonQuery();
            new EspecialidadNegocio().GuardarPrincipal(
                profesional.IdProfesional, profesional.IdEspecialidad, conexion, transaccion);
        }


        /* Baja logica: el registro no se borra nunca, se pasa a
           Inactivo. Borrarlo de verdad romperia los turnos y las
           atenciones que lo referencian. */
        public void DarDeBaja(int idProfesional)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    UPDATE Profesional
                    SET IdEstado = (
                        SELECT IdEstado
                        FROM Estado
                        WHERE Descripcion = 'Inactivo'
                    )
                    WHERE IdProfesional = @IdProfesional
                ";

                comando.Parameters.AddWithValue(
                    "@IdProfesional",
                    idProfesional
                );

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


        /* Vuelve a poner Activo a un profesional dado de baja. */
        public void Reactivar(int idProfesional)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    UPDATE Profesional
                    SET IdEstado = (
                        SELECT IdEstado
                        FROM Estado
                        WHERE Descripcion = 'Activo'
                    )
                    WHERE IdProfesional = @IdProfesional
                ";

                comando.Parameters.AddWithValue(
                    "@IdProfesional",
                    idProfesional
                );

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
