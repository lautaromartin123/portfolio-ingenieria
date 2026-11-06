using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class TipoMatriculaNegocio
    {
        public List<TipoMatricula> Listar()
        {
            List<TipoMatricula> lista = new List<TipoMatricula>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdTipoMatricula,
                        Descripcion
                    FROM TipoMatricula
                    ORDER BY IdTipoMatricula
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    TipoMatricula tipo = new TipoMatricula();

                    tipo.IdTipoMatricula =
                        Convert.ToInt32(lector["IdTipoMatricula"]);

                    tipo.Descripcion =
                        lector["Descripcion"].ToString();

                    lista.Add(tipo);
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
