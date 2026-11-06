using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class CategoriaNegocio
    {

        private string cadena = ConfigurationManager.ConnectionStrings["usal"].ConnectionString;

        public List<Categoria2> ListarCategoriasPrincipales()
        {


            List<Categoria2> lista = new List <Categoria2>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    string consulta = "select id_categoria, descripcion, id_dependencia " +
                                      "from categoria " +
                                      "where id_dependencia = id_categoria";



                    SqlCommand comando = new SqlCommand(consulta, conexion);

                    conexion.Open();

                    SqlDataReader lector = comando.ExecuteReader();

                    while (lector.Read())
                    {
                        Categoria2 aux = new Categoria2();
                        aux.IdCategoria = (int)lector["id_categoria"];
                        aux.Descripcion = lector["descripcion"].ToString();
                        aux.IdDependencia = (int)lector["id_dependencia"];


                        lista.Add(aux);
                    }


                }
                return lista;
            }
            catch (Exception) { lista = null; return lista; }
            }



        public List<Categoria2> ListarSubcategorias (int id_dependencia)
        {
            List<Categoria2> lista = new List<Categoria2>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena)) {
                    string consulta = " select id_categoria, descripcion, id_dependencia from categoria where id_dependencia = @IdDependencia and " +
                        "id_categoria <> id_dependencia";

                    SqlCommand comando = new SqlCommand(consulta, conexion);

                    comando.Parameters.AddWithValue("@IdDependencia", id_dependencia);

                    conexion.Open();

                    SqlDataReader lector = comando.ExecuteReader();

                    while (lector.Read()){
                        Categoria2 aux = new Categoria2();
                        aux.IdCategoria = (int)lector["id_categoria"];
                        aux.Descripcion = lector["descripcion"].ToString();
                        aux.IdDependencia = (int)lector["id_dependencia"];

                        lista.Add(aux);

                    }




                }return lista;

            }
            catch (Exception) { lista=null; return lista; }

        }









    }
}
