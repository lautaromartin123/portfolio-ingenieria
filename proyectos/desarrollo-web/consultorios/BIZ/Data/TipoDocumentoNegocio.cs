using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class TipoDocumentoNegocio
    {
        public List<TipoDocumento> Listar()
        {
            List<TipoDocumento> lista = new List<TipoDocumento>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdTipoDocumento,
                        Descripcion
                    FROM TipoDocumento
                    ORDER BY Descripcion
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    TipoDocumento tipoDocumento = new TipoDocumento();

                    tipoDocumento.IdTipoDocumento =
                        Convert.ToInt32(lector["IdTipoDocumento"]);

                    tipoDocumento.Descripcion =
                        lector["Descripcion"].ToString();

                    lista.Add(tipoDocumento);
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
