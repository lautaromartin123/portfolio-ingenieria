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
    public class Categoria
    {
        public static DataSet CategoriaPadre()
        {
            DataSet ds = new DataSet();
            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;
            string query = @"select * from categoria
                            where id_categoria= id_dependencia";
            try
            {
                using (SqlConnection con = new SqlConnection(CN))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
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
        public static DataSet CategoriaHijo(string idPadre)
        {
            DataSet ds = new DataSet();
            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;
            string query = @"
                        select * from categoria where id_dependencia=@idcategoria
                        and id_categoria <> id_dependencia";

            try
            {
                using (SqlConnection con = new SqlConnection(CN))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@idcategoria", idPadre);
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
