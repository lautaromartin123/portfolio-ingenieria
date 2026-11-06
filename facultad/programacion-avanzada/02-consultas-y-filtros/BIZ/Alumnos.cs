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
    public class Alumnos
    {
        private static string CadenaConexion()
        {
            return ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;
        }

        public static DataSet ListarAlumnosPorCurso(int idCurso)
        {
            DataSet ds = new DataSet();
            try
            {
                string query = "SELECT IdAlumno, Apellido, Nombre, Email FROM Alumno WHERE IdCurso = @IdCurso";
                using (SqlConnection conexion = new SqlConnection(CadenaConexion()))
                {
                    conexion.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@idCurso", idCurso);
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
