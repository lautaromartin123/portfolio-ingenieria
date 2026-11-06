using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class ObraSocialNegocio
    {
        public List<ObraSocial> Listar()
        {
            List<ObraSocial> lista = new List<ObraSocial>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        IdObraSocial,
                        Nombre
                    FROM ObraSocial
                    ORDER BY Nombre
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    ObraSocial obraSocial = new ObraSocial();

                    obraSocial.IdObraSocial =
                        Convert.ToInt32(lector["IdObraSocial"]);

                    obraSocial.Nombre =
                        lector["Nombre"].ToString();

                    lista.Add(obraSocial);
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
