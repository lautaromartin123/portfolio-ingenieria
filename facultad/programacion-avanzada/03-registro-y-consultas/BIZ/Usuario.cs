using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Usuario
    {
        public class STUsuario
        {
            public int IdUsuario { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string Telefono { get; set; }
            public string password { get; set; }
            public byte[] foto { get; set; }
            public int nivel { get; set; } = 0;
            public int IdEstado { get; set; }
        }

        private static string Coneccion()
        {
            return ConfigurationManager.ConnectionStrings["CLASE"].ConnectionString;
        }

        public static STUsuario AgregarUsuario(STUsuario usuario)
        {
            STUsuario Datos = new STUsuario();
            try
            {
                string query = @"INSERT INTO Usuario (IdUsuario,nombre, apellido, correo, usuario, password, id_nivel, foto, id_estado) VALUES (@IdUsuario, @Nombre, @Apellido, @Email, @Usuario, @Password, @IdNivel, @Foto, @IdEstado)";
                using (SqlConnection CN = new SqlConnection(Coneccion()))
                {
                    CN.Open();
                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", CalcularIdUsuario());
                        cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                        cmd.Parameters.AddWithValue("@Email", usuario.Email);
                        cmd.Parameters.AddWithValue("@Usuario", usuario.Email); // Assuming 'usuario' is the same as 'email'
                        cmd.Parameters.AddWithValue("@Password", usuario.password);
                        cmd.Parameters.AddWithValue("@Foto", usuario.foto);
                        cmd.Parameters.AddWithValue("@IdNivel", 1); // Assuming a default level
                        cmd.Parameters.AddWithValue("@IdEstado", usuario.IdEstado);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Datos = usuario; // Return the user data if insertion was successful
                        }
                    }
                }
                return Datos;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }
        public static STUsuario ActualizarUsuario(STUsuario usuario)
        {
            STUsuario Datos = new STUsuario();
            try
            {
                string query = @"UPDATE Usuario
                         SET nombre = @Nombre,
                             apellido = @Apellido,
                             correo = @Email,
                             usuario = @Usuario,
                             password = @Password,
                             id_nivel = @IdNivel,
                             foto = @Foto,
                             id_estado = @IdEstado
                         WHERE IdUsuario = @IdUsuario";

                using (SqlConnection CN = new SqlConnection(Coneccion()))
                {
                    CN.Open();
                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                        cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido);
                        cmd.Parameters.AddWithValue("@Email", usuario.Email);
                        cmd.Parameters.AddWithValue("@Usuario", usuario.Email); // si 'usuario' es igual al email
                        cmd.Parameters.AddWithValue("@Password", usuario.password);
                        cmd.Parameters.AddWithValue("@Foto", (object)usuario.foto ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdNivel", usuario.nivel); // aquí puedes usar el nivel real en lugar de fijo
                        cmd.Parameters.AddWithValue("@IdEstado", usuario.IdEstado);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Datos = usuario; // devolver el objeto actualizado si la operación fue exitosa
                        }
                    }
                }
                return Datos;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }
        public string LoginUsuario(string mail, string password)
        {
            string resultado = "Usuario o contraseña incorrectos.";
            try
            {
                string query = @"SELECT COUNT(*) FROM Usuario WHERE correo = @Email AND password = @Password";
                using (SqlConnection CN = new SqlConnection(Coneccion()))
                {
                    CN.Open();
                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        cmd.Parameters.AddWithValue("@Email", mail);
                        cmd.Parameters.AddWithValue("@Password", password);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            resultado = "OK";
                        }
                    }
                }
                return resultado;
            }
            catch (Exception)
            {
                return resultado;
                //throw;
            }
        }

        private static int CalcularIdUsuario()
        {
            int idUsuario = 0;
            try
            {
                string query = "select isnull(max(IdUsuario) + 1,1000) IdUsuario from usuario";
                using (SqlConnection CN = new SqlConnection(Coneccion()))
                {
                    CN.Open();
                    using (SqlCommand cmd = new SqlCommand(query, CN))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            idUsuario = Convert.ToInt32(result); // Use the calculated ID from the query
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return idUsuario;
        }


    }
}
