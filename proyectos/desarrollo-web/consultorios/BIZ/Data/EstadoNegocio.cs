using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class EstadoNegocio
    {
        public List<Estado> Listar()
        {
            List<Estado> lista = new List<Estado>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdEstado,
                        Descripcion
                    FROM Estado
                    ORDER BY IdEstado
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    Estado estado = new Estado();

                    estado.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

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
