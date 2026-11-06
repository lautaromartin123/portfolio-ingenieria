using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class PacienteNegocio
    {
        public List<Paciente> Listar()
        {
            List<Paciente> lista = new List<Paciente>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
                    SELECT
                        P.IdPaciente,
                        P.IdUsuario,

                        P.Nombre,
                        P.Apellido,
                        P.Telefono,
                        P.Foto,
                        P.NumeroObraSocial,
                        P.IdObraSocial,
                        P.IdEstado,
                        P.IdDomicilio,

                        E.Descripcion AS EstadoDescripcion,
                        O.Nombre AS ObraSocialNombre
                    FROM Paciente P

                    INNER JOIN Estado E
                        ON P.IdEstado = E.IdEstado

                    LEFT JOIN ObraSocial O
                        ON P.IdObraSocial = O.IdObraSocial
                ";

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    Paciente paciente = new Paciente();

                    paciente.IdPaciente =
                        Convert.ToInt32(lector["IdPaciente"]);
                    if (lector["IdUsuario"] != DBNull.Value)
                    {
                        paciente.IdUsuario =
                            Convert.ToInt32(lector["IdUsuario"]);
                    }

                    paciente.Nombre =
                        lector["Nombre"].ToString();

                    paciente.Apellido =
                        lector["Apellido"].ToString();

                    paciente.Telefono =
                        lector["Telefono"].ToString();

                    paciente.Foto =
                        lector["Foto"].ToString();

                    paciente.NumeroObraSocial =
                        lector["NumeroObraSocial"].ToString();


                    // ESTADO
                    paciente.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

                    paciente.Estado = new Estado();

                    paciente.Estado.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

                    paciente.Estado.Descripcion =
                        lector["EstadoDescripcion"].ToString();


                    // OBRA SOCIAL
                    if (lector["IdObraSocial"] != DBNull.Value)
                    {
                        paciente.IdObraSocial =
                            Convert.ToInt32(lector["IdObraSocial"]);

                        paciente.ObraSocial = new ObraSocial();

                        paciente.ObraSocial.IdObraSocial =
                            Convert.ToInt32(lector["IdObraSocial"]);

                        paciente.ObraSocial.Nombre =
                            lector["ObraSocialNombre"].ToString();
                    }


                    // DOMICILIO
                    if (lector["IdDomicilio"] != DBNull.Value)
                    {
                        paciente.IdDomicilio =
                            Convert.ToInt32(lector["IdDomicilio"]);
                    }


                    lista.Add(paciente);
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


        public int Agregar(Paciente paciente)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            try
            {
                conexion.Open();

                return Agregar(paciente, conexion, null);
            }
            finally
            {
                conexion.Close();
            }
        }

        internal int Agregar(
    Paciente paciente,
    SqlConnection conexion,
    SqlTransaction transaccion)
        {
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexion;
            comando.Transaction = transaccion;

            comando.CommandText = @"
    INSERT INTO Paciente
    (
        Nombre,
        Apellido,
        Telefono,
        Foto,
        NumeroObraSocial,
        IdObraSocial,
        IdDomicilio,
        IdUsuario
    )

    OUTPUT INSERTED.IdPaciente

    VALUES
    (
        @Nombre,
        @Apellido,
        @Telefono,
        @Foto,
        @NumeroObraSocial,
        @IdObraSocial,
        @IdDomicilio,
        @IdUsuario
    )
";

            comando.Parameters.AddWithValue("@Nombre", paciente.Nombre);
            comando.Parameters.AddWithValue("@Apellido", paciente.Apellido);

            comando.Parameters.AddWithValue(
                "@Telefono",
                (object)paciente.Telefono ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@Foto",
                (object)paciente.Foto ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@NumeroObraSocial",
                (object)paciente.NumeroObraSocial ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdObraSocial",
                (object)paciente.IdObraSocial ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdDomicilio",
                (object)paciente.IdDomicilio ?? DBNull.Value
            );
            comando.Parameters.Add(
                 "@IdUsuario",
                 System.Data.SqlDbType.Int
                 ).Value = (object)paciente.IdUsuario ?? DBNull.Value;

            return Convert.ToInt32(comando.ExecuteScalar());
        }


        public Paciente BuscarPorId(int idPaciente)
        {
            Paciente paciente = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
            SELECT
                P.IdPaciente,
                P.IdUsuario,

                P.Nombre,
                P.Apellido,
                P.Telefono,
                P.Foto,
                P.NumeroObraSocial,
                P.IdObraSocial,
                P.IdEstado,
                P.IdDomicilio,

                E.Descripcion AS EstadoDescripcion,

                O.Nombre AS ObraSocialNombre

            FROM Paciente P

            INNER JOIN Estado E
                ON P.IdEstado = E.IdEstado

            LEFT JOIN ObraSocial O
                ON P.IdObraSocial = O.IdObraSocial

            WHERE P.IdPaciente = @IdPaciente
        ";

                comando.Parameters.AddWithValue(
                    "@IdPaciente",
                    idPaciente
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    paciente = new Paciente();

                    paciente.IdPaciente =
                        Convert.ToInt32(lector["IdPaciente"]);
                    if (lector["IdUsuario"] != DBNull.Value)
                    {
                        paciente.IdUsuario =
                            Convert.ToInt32(lector["IdUsuario"]);
                    }

                    paciente.Nombre =
                        lector["Nombre"].ToString();

                    paciente.Apellido =
                        lector["Apellido"].ToString();

                    paciente.Telefono =
                        lector["Telefono"].ToString();

                    paciente.Foto =
                        lector["Foto"].ToString();

                    paciente.NumeroObraSocial =
                        lector["NumeroObraSocial"].ToString();


                    // ESTADO
                    paciente.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

                    paciente.Estado = new Estado();

                    paciente.Estado.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

                    paciente.Estado.Descripcion =
                        lector["EstadoDescripcion"].ToString();


                    // OBRA SOCIAL
                    if (lector["IdObraSocial"] != DBNull.Value)
                    {
                        paciente.IdObraSocial =
                            Convert.ToInt32(lector["IdObraSocial"]);

                        paciente.ObraSocial = new ObraSocial();

                        paciente.ObraSocial.IdObraSocial =
                            Convert.ToInt32(lector["IdObraSocial"]);

                        paciente.ObraSocial.Nombre =
                            lector["ObraSocialNombre"].ToString();
                    }


                    // DOMICILIO
                    if (lector["IdDomicilio"] != DBNull.Value)
                    {
                        paciente.IdDomicilio =
                            Convert.ToInt32(lector["IdDomicilio"]);
                    }
                }

                lector.Close();

                return paciente;
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


        public Paciente BuscarPorUsuario(int idUsuario)
        {
            Paciente paciente = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
            SELECT
                P.IdPaciente,
                P.IdUsuario,

                P.Nombre,
                P.Apellido,
                P.Telefono,
                P.Foto,
                P.NumeroObraSocial,
                P.IdObraSocial,
                P.IdEstado,
                P.IdDomicilio,

                E.Descripcion AS EstadoDescripcion,

                O.Nombre AS ObraSocialNombre

            FROM Paciente P

            INNER JOIN Estado E
                ON P.IdEstado = E.IdEstado

            LEFT JOIN ObraSocial O
                ON P.IdObraSocial = O.IdObraSocial

            WHERE P.IdUsuario = @IdUsuario
        ";

                comando.Parameters.Add(
                 "@IdUsuario",
                System.Data.SqlDbType.Int
                ).Value = idUsuario;

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    paciente = new Paciente();

                    paciente.IdPaciente =
                        Convert.ToInt32(lector["IdPaciente"]);
                    if (lector["IdUsuario"] != DBNull.Value)
                    {
                        paciente.IdUsuario =
                            Convert.ToInt32(lector["IdUsuario"]);
                    }

                    paciente.Nombre =
                        lector["Nombre"].ToString();

                    paciente.Apellido =
                        lector["Apellido"].ToString();

                    paciente.Telefono =
                        lector["Telefono"].ToString();

                    paciente.Foto =
                        lector["Foto"].ToString();

                    paciente.NumeroObraSocial =
                        lector["NumeroObraSocial"].ToString();


                    // ESTADO
                    paciente.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

                    paciente.Estado = new Estado();

                    paciente.Estado.IdEstado =
                        Convert.ToInt32(lector["IdEstado"]);

                    paciente.Estado.Descripcion =
                        lector["EstadoDescripcion"].ToString();


                    // OBRA SOCIAL
                    if (lector["IdObraSocial"] != DBNull.Value)
                    {
                        paciente.IdObraSocial =
                            Convert.ToInt32(lector["IdObraSocial"]);

                        paciente.ObraSocial = new ObraSocial();

                        paciente.ObraSocial.IdObraSocial =
                            Convert.ToInt32(lector["IdObraSocial"]);

                        paciente.ObraSocial.Nombre =
                            lector["ObraSocialNombre"].ToString();
                    }


                    // DOMICILIO
                    if (lector["IdDomicilio"] != DBNull.Value)
                    {
                        paciente.IdDomicilio =
                            Convert.ToInt32(lector["IdDomicilio"]);
                    }
                }

                lector.Close();

                return paciente;
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






        public void Modificar(Paciente paciente)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            try
            {
                conexion.Open();

                Modificar(paciente, conexion, null);
            }
            finally
            {
                conexion.Close();
            }
        }

        internal void Modificar(
    Paciente paciente,
    SqlConnection conexion,
    SqlTransaction transaccion)
        {
            SqlCommand comando = new SqlCommand();

            comando.Connection = conexion;
            comando.Transaction = transaccion;

            comando.CommandText = @"
        UPDATE Paciente
        SET
            Nombre = @Nombre,
            Apellido = @Apellido,
            Telefono = @Telefono,
            Foto = @Foto,
            NumeroObraSocial = @NumeroObraSocial,
            IdObraSocial = @IdObraSocial,
            IdDomicilio = @IdDomicilio
        WHERE IdPaciente = @IdPaciente
    ";

            comando.Parameters.AddWithValue(
                "@Nombre",
                paciente.Nombre
            );

            comando.Parameters.AddWithValue(
                "@Apellido",
                paciente.Apellido
            );

            comando.Parameters.AddWithValue(
                "@Telefono",
                (object)paciente.Telefono ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@Foto",
                (object)paciente.Foto ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@NumeroObraSocial",
                (object)paciente.NumeroObraSocial ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdObraSocial",
                (object)paciente.IdObraSocial ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdDomicilio",
                (object)paciente.IdDomicilio ?? DBNull.Value
            );

            comando.Parameters.AddWithValue(
                "@IdPaciente",
                paciente.IdPaciente
            );

            comando.ExecuteNonQuery();
        }


        public void DarDeBaja(int idPaciente)
        {
            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText = @"
            UPDATE Paciente
            SET IdEstado = (
                SELECT IdEstado
                FROM Estado
                WHERE Descripcion = 'Inactivo'
            )
            WHERE IdPaciente = @IdPaciente
        ";

                comando.Parameters.AddWithValue(
                    "@IdPaciente",
                    idPaciente
                );

                conexion.Open();

                comando.ExecuteNonQuery();
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














    }
}
