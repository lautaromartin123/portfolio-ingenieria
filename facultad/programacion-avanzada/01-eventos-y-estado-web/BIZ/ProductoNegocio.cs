using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class ProductoNegocio
    {
        private string cadena = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;


        //SobreCarga  : trae productos por categoria general
        public List<Producto2> Listar(int IdDependencia)
        {
            List<Producto2> lista = new List<Producto2>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    string consulta = "select id_producto, descripcion, costo, fecha_modificacion from producto where " +
                        "id_dependencia = @idDependencia";

                    SqlCommand comando = new SqlCommand(consulta, conexion);
                    comando.Parameters.AddWithValue("@idDependencia", IdDependencia);
                    conexion.Open();
                    SqlDataReader lector = comando.ExecuteReader();
                    while (lector.Read())
                    {
                     Producto2 aux = new Producto2();

                        aux.IdProducto = (int)lector["id_producto"];
                        aux.Descripcion = lector["descripcion"].ToString();
                        aux.Costo = (decimal)lector["costo"];
                        aux.FechaModificacion = (DateTime)lector["fecha_modificacion"];

                        lista.Add(aux);
                    }
                }
                return lista;

            }
            catch (Exception) { lista = null; return lista; }

        }


        //SobreCarga 2 : trae productos por categoria general y subcategoria

        public List<Producto2> Listar(int idDependencia, int idCategoria)
        {
            List<Producto2> lista = new List<Producto2>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    string consulta = "select id_producto, descripcion, costo, fecha_modificacion " +
                    "from producto where id_dependencia = @idDependencia and id_categoria = @idCategoria";

                    SqlCommand comando = new SqlCommand(consulta, conexion);
                    comando.Parameters.AddWithValue("@idDependencia", idDependencia);
                    comando.Parameters.AddWithValue("@idCategoria", idCategoria);

                    conexion.Open();
                    SqlDataReader lector = comando.ExecuteReader();

                    while (lector.Read())
                    {
                        Producto2 aux = new Producto2();
                        aux.IdProducto = (int)lector["id_producto"];
                        aux.Descripcion = lector["descripcion"].ToString();
                        aux.Costo = (decimal)lector["costo"];
                        aux.FechaModificacion = (DateTime)lector["fecha_modificacion"];

                        lista.Add(aux);
                    }

                }
                return lista;
            }
            catch (Exception) { lista = null; return lista; }
        }











    }
}
