using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class EstadoTurnoNegocio
    {
        public List<EstadoTurno> Listar()
        {
            List<EstadoTurno> lista = new List<EstadoTurno>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdEstadoTurno,
                        Descripcion
                    FROM EstadoTurno
                    ORDER BY IdEstadoTurno
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    EstadoTurno estado = new EstadoTurno();

                    estado.IdEstadoTurno =
                        Convert.ToInt32(lector["IdEstadoTurno"]);

                    estado.Descripcion =
                        lector["Descripcion"].ToString();

                    lista.Add(estado);
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
    }
}
