using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class ProductoNegocio
    {
        private static string CadenaConexion()
        {
            return ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;
        }

        public static   DataSet ListarProductosPorEstado(int idEstado)
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "SELECT IdProducto, nombre, Precio FROM Producto WHERE IdEstado=@IdEstado";

                using(SqlConnection CN = new SqlConnection(CadenaConexion()))
                {
                    CN.Open();
                        using(SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@IdEstado", idEstado);
                        adapter.Fill(ds);
                    }
                }
                return ds;

            }
            catch (Exception)
            {
                ds = null;
                return ds;
            }


        }









    }
}
