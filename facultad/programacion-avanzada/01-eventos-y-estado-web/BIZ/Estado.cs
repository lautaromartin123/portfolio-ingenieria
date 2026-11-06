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
    public class Estado
    {
        public static DataSet ConsultarUnEstado(string ID)
        {
            DataSet ds = new DataSet();

            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;

            string query = @"SELECT * FROM estado WHERE id_objeto = @ID";

            try
            {
                using (SqlConnection con = new SqlConnection(CN))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", ID);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
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
