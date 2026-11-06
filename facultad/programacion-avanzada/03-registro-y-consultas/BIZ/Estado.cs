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
    public class Estado
    {
        private static string ConneccionString()
        {
            return ConfigurationManager.ConnectionStrings["Parcial"].ConnectionString;
        }

        public static DataSet TodosLosEstados()
        {
            DataSet ds = new DataSet();

            try
            {
                string query = "SELECT * FROM estado";
                    using(SqlConnection CN = new SqlConnection(ConneccionString()))
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
