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
    public class Categoria2
    {
        private static string CLASE()
        {
            return ConfigurationManager.ConnectionStrings["CLASE"].ConnectionString;
        }

        public static DataSet TodasLasCategorias()
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "SELECT * FROM categoria";
                using (SqlConnection CN = new SqlConnection(CLASE()))
                {
                    CN.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
                    {
                        adapter.Fill(ds);
                    }
                }
                return ds;
            }
            //catch (Exception ex)
            //{
            //    throw new Exception("Error en Categorias: " + ex.Message);
            //}
            catch (Exception)
            {
                ds = null;
                return ds;
            }
        }



        //cierre public class Categoria 2
    }
}
