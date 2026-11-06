using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Curso
    {
        private static string CadenaConexion()
        {

            return ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;
        }


        public static DataSet ListarCursos()
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT IdCurso, Nombre FROM Curso";
                using (SqlConnection conexion = new SqlConnection(CadenaConexion()))
                {
                    conexion.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
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
