using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class DocumentoNegocio
    {
        public void Agregar(Documento documento)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            try
            {
                conexion.Open();

                Agregar(documento, conexion, null);
            }
            finally
            {
                conexion.Close();
            }
        }


        internal void Agregar(
    Documento documento,
    SqlConnection conexion,
    SqlTransaction transaccion)
        {
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexion;
            comando.Transaction = transaccion;

            comando.CommandText = @"
        INSERT INTO Documento
        (
            NumeroDocumento,
            IdTipoDocumento,
            IdPaciente,
            IdProfesional
        )
        VALUES
        (
            @NumeroDocumento,
            @IdTipoDocumento,
            @IdPaciente,
            @IdProfesional
        )
    ";

            comando.Parameters.AddWithValue(
                "@NumeroDocumento",
                documento.NumeroDocumento
            );

            comando.Parameters.AddWithValue(
                "@IdTipoDocumento",
                documento.IdTipoDocumento
            );

            comando.Parameters.AddWithValue(
                "@IdPaciente",
                (object)documento.IdPaciente ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdProfesional",
                (object)documento.IdProfesional ?? DBNull.Value
            );

            comando.ExecuteNonQuery();
        }




        public List<Documento> ListarPorPaciente(int idPaciente)
        {
            List<Documento> lista = new List<Documento>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
            SELECT
                D.IdDocumento,
                D.NumeroDocumento,
                D.IdTipoDocumento,
                D.IdPaciente,
                D.IdProfesional,

                TD.Descripcion AS TipoDocumentoDescripcion,

                P.Nombre,
                P.Apellido

            FROM Documento D

            INNER JOIN TipoDocumento TD
                ON D.IdTipoDocumento = TD.IdTipoDocumento

            LEFT JOIN Paciente P
                ON D.IdPaciente = P.IdPaciente

            WHERE D.IdPaciente = @IdPaciente
        ";

                comando.Parameters.AddWithValue(
                    "@IdPaciente",
                    idPaciente
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    Documento documento = new Documento();

                    documento.IdDocumento =
                        Convert.ToInt32(lector["IdDocumento"]);

                    documento.NumeroDocumento =
                        lector["NumeroDocumento"].ToString();

                    documento.IdTipoDocumento =
                        Convert.ToInt32(lector["IdTipoDocumento"]);


                    // Tipo Documento
                    documento.TipoDocumento = new TipoDocumento();

                    documento.TipoDocumento.IdTipoDocumento =
                        Convert.ToInt32(lector["IdTipoDocumento"]);

                    documento.TipoDocumento.Descripcion =
                        lector["TipoDocumentoDescripcion"].ToString();


                    // Paciente
                    if (lector["IdPaciente"] != DBNull.Value)
                    {
                        documento.IdPaciente =
                            Convert.ToInt32(lector["IdPaciente"]);

                        documento.Paciente = new Paciente();

                        documento.Paciente.IdPaciente =
                            Convert.ToInt32(lector["IdPaciente"]);

                        documento.Paciente.Nombre =
                            lector["Nombre"].ToString();

                        documento.Paciente.Apellido =
                            lector["Apellido"].ToString();
                    }


                    // Profesional
                    if (lector["IdProfesional"] != DBNull.Value)
                    {
                        documento.IdProfesional =
                            Convert.ToInt32(lector["IdProfesional"]);
                    }


                    lista.Add(documento);
                }

                lector.Close();

                return lista;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }

        public Documento BuscarPorPaciente(int idPaciente)
        {
            Documento documento = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
            SELECT
                D.IdDocumento,
                D.NumeroDocumento,
                D.IdTipoDocumento,
                D.IdPaciente,
                D.IdProfesional,

                TD.Descripcion AS TipoDocumentoDescripcion

            FROM Documento D

            INNER JOIN TipoDocumento TD
                ON D.IdTipoDocumento = TD.IdTipoDocumento

            WHERE D.IdPaciente = @IdPaciente
        ";

                comando.Parameters.AddWithValue(
                    "@IdPaciente",
                    idPaciente
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    documento = new Documento();

                    documento.IdDocumento =
                        Convert.ToInt32(lector["IdDocumento"]);

                    documento.NumeroDocumento =
                        lector["NumeroDocumento"].ToString();

                    documento.IdTipoDocumento =
                        Convert.ToInt32(lector["IdTipoDocumento"]);


                    documento.TipoDocumento = new TipoDocumento();

                    documento.TipoDocumento.IdTipoDocumento =
                        Convert.ToInt32(lector["IdTipoDocumento"]);

                    documento.TipoDocumento.Descripcion =
                        lector["TipoDocumentoDescripcion"].ToString();


                    if (lector["IdPaciente"] != DBNull.Value)
                    {
                        documento.IdPaciente =
                            Convert.ToInt32(lector["IdPaciente"]);
                    }

                    if (lector["IdProfesional"] != DBNull.Value)
                    {
                        documento.IdProfesional =
                            Convert.ToInt32(lector["IdProfesional"]);
                    }
                }

                lector.Close();

                return documento;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conexion.Close();
            }
        }


        public void Modificar(Documento documento)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            try
            {
                conexion.Open();

                Modificar(documento, conexion, null);
            }
            finally
            {
                conexion.Close();
            }
        }


        internal void Modificar(
    Documento documento,
    SqlConnection conexion,
    SqlTransaction transaccion)
        {
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexion;
            comando.Transaction = transaccion;

            comando.CommandText = @"
        UPDATE Documento
        SET
            NumeroDocumento = @NumeroDocumento,
            IdTipoDocumento = @IdTipoDocumento
        WHERE IdDocumento = @IdDocumento
    ";

            comando.Parameters.AddWithValue(
                "@NumeroDocumento",
                documento.NumeroDocumento
            );

            comando.Parameters.AddWithValue(
                "@IdTipoDocumento",
                documento.IdTipoDocumento
            );

            comando.Parameters.AddWithValue(
                "@IdDocumento",
                documento.IdDocumento
            );

            comando.ExecuteNonQuery();
        }



















    }
}
