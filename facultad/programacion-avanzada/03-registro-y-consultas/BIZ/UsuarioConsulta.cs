using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class UsuarioConsulta
    {


        private static string ConneccionString()
        {
            return ConfigurationManager.ConnectionStrings["Parcial"].ConnectionString;
        }

        public static DataSet UsuariosPorEstado(int ID)
        {
          DataSet ds = new DataSet();
            try
            {
                string query = "SELECT * FROM usuario WHERE id_estado = @ID";
                using (SqlConnection CN = new SqlConnection(ConneccionString()))
                {
                    CN.Open();
                    using(SqlDataAdapter adapter = new SqlDataAdapter(query, CN))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@ID", ID);
                        adapter.Fill(ds);
                    }
                }
                return ds;
            }catch (Exception)
            {
                ds = null;
                return ds;
            }
        }
    }
}
