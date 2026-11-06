using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{

    public class Producto
    {
        private static string CLASE()
        {
            return ConfigurationManager.ConnectionStrings["CLASE"].ConnectionString;
        }

        public static DataSet ProductosPorCategoria(int ID)
        {
            DataSet ds = new DataSet();

            try
            {

                string query = "SELECT * FROM producto WHERE id_categoria = @ID";



                using (SqlConnection CN = new SqlConnection(CLASE()))
                {
                    CN.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@ID", ID);
                        adapter.Fill(ds);
                    }
                }
                return ds;
            }
            //catch (Exception ex)
            //{
            //    throw new Exception("Error en Productos: " + ex.Message);
            //}
            catch (Exception)
            {
                ds = null;
                return ds;

            }
        }



            //cierre public class Producto
    }
}
