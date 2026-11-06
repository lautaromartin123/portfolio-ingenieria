using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class EspecialidadNegocio
    {
        public List<Especialidad> Listar()
        {
            List<Especialidad> lista = new List<Especialidad>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdEspecialidad,
                        Nombre
                    FROM Especialidad
                    ORDER BY Nombre
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    Especialidad especialidad = new Especialidad();

                    especialidad.IdEspecialidad =
                        Convert.ToInt32(lector["IdEspecialidad"]);

                    especialidad.Nombre =
                        lector["Nombre"].ToString();

                    lista.Add(especialidad);
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


        public Especialidad BuscarPorId(int idEspecialidad)
        {
            Especialidad especialidad = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdEspecialidad,
                        Nombre
                    FROM Especialidad
                    WHERE IdEspecialidad = @IdEspecialidad
                ";

                comando.Parameters.AddWithValue(
                    "@IdEspecialidad",
                    idEspecialidad
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    especialidad = new Especialidad();

                    especialidad.IdEspecialidad =
                        Convert.ToInt32(lector["IdEspecialidad"]);

                    especialidad.Nombre =
                        lector["Nombre"].ToString();
                }

                lector.Close();

                return especialidad;
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

        public List<Especialidad> ListarPorProfesional(int idProfesional)
        {
            List<Especialidad> lista = new List<Especialidad>();
            string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;
            using (SqlConnection conexion = new SqlConnection(cadena))
            using (SqlCommand comando = new SqlCommand(@"
                SELECT E.IdEspecialidad,E.Nombre FROM dbo.Especialidad E
                WHERE EXISTS
                (
                    SELECT 1 FROM dbo.ProfesionalEspecialidad PE
                    WHERE PE.IdProfesional=@IdProfesional AND PE.IdEspecialidad=E.IdEspecialidad
                )
                OR EXISTS
                (
                    SELECT 1 FROM dbo.Profesional P
                    WHERE P.IdProfesional=@IdProfesional AND P.IdEspecialidad=E.IdEspecialidad
                )
                ORDER BY E.Nombre;", conexion))
            {
                comando.Parameters.AddWithValue("@IdProfesional", idProfesional);
                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        lista.Add(new Especialidad
                        {
                            IdEspecialidad = Convert.ToInt32(lector["IdEspecialidad"]),
                            Nombre = Convert.ToString(lector["Nombre"])
                        });
                }
            }
            return lista;
        }

        internal void GuardarPrincipal(int idProfesional, int idEspecialidad,
            SqlConnection conexion, SqlTransaction transaccion)
        {
            using (SqlCommand comando = new SqlCommand(@"
                INSERT dbo.ProfesionalEspecialidad(IdProfesional,IdEspecialidad)
                SELECT @IdProfesional,@IdEspecialidad
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM dbo.ProfesionalEspecialidad
                    WHERE IdProfesional=@IdProfesional AND IdEspecialidad=@IdEspecialidad
                );", conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdProfesional", idProfesional);
                comando.Parameters.AddWithValue("@IdEspecialidad", idEspecialidad);
                comando.ExecuteNonQuery();
            }
        }
    }
}
