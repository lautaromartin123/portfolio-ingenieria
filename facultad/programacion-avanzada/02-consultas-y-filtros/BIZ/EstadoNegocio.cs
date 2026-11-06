using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BIZ
{
    public class EstadoNegocio
    {

        private static string CadenaConexion()
        {
            return ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;
        }

        public static DataSet ListarEstados()
        {
         DataSet ds = new DataSet();
            try
            {
                string query = "SELECT IdEstado, nombre FROM Estado";

                using(SqlConnection CN = new SqlConnection(CadenaConexion()))
                {
                    CN.Open();
                    using(SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
                    {
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
