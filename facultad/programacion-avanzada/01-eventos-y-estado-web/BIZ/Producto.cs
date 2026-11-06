using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Producto
    {
        public class ProductoFull
        {
            public int Id { get; set; }
            public string Descripcion { get; set; }

            public string Codigo { get; set; }
            public decimal Costo { get; set; }
            public int Stock { get; set; }
            public string StockMinimo { get; set; }
        }
        public static DataSet ObtenerProducto()
        {
            DataSet ds = new DataSet();
            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;
            string query = @"select top 1000 * from producto where id_estado=0";
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
        public static DataSet ObtenerProductoPorDescripcion(string descripcion)
        {
            DataSet ds = new DataSet();
            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;
            string query = @"select  * from producto where id_estado=0 and descripcion LIKE @descripcion";
            /*
             select  * from producto where id_estado=0 and id_dependencia=@id_dependencia
             */


            try
            {
                using (SqlConnection con = new SqlConnection(CN))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@descripcion", "%" + descripcion + "%");
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
        public static DataSet ObtenerProductoPorIdcategoria(string IdDependencia)
        {
            DataSet ds = new DataSet();
            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;
            string query = @"select  * from producto where id_estado=0 and id_dependencia=@id_dependencia";
                         try
            {
                using (SqlConnection con = new SqlConnection(CN))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id_dependencia", IdDependencia);
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
        public static ProductoFull ObtenerProductoById(int id)
        {
            ProductoFull producto = new ProductoFull();
            string CN = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;
            string query = @"select * from producto where id_estado=0 and id_producto=@id";
            try
            {
                using (SqlConnection con = new SqlConnection(CN))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                producto.Id = Convert.ToInt32(dr["id_producto"]);
                                producto.Descripcion = dr["descripcion"].ToString();
                                producto.Codigo = dr["codigo"].ToString();
                                producto.Costo = Convert.ToDecimal(dr["costo"]);
                                producto.Stock = Convert.ToInt32(dr["stock"]);
                                producto.StockMinimo = dr["stock_minimo"].ToString();
                            }
                        }
                    }
                }
                return producto;
            }
            catch (Exception)
            {
                producto = null;
                return producto;
            }
        }
    }
}
