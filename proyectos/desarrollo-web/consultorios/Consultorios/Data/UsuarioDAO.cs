using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Consultorios
{
    public class UsuarioDAO
    {
        // =====================================================
        // TOKEN PARA RECUPERACIÓN
        // =====================================================

        private static class PasswordResetHelper
        {
            public static string GenerarToken()
            {
                byte[] bytes = new byte[32];

                using (RNGCryptoServiceProvider rng =
                    new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(bytes);
                }

                return Convert
                    .ToBase64String(bytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");
            }


            public static byte[] ObtenerHashToken(
                string token)
            {
                using (SHA256 sha =
                    SHA256.Create())
                {
                    return sha.ComputeHash(
                        Encoding.UTF8.GetBytes(token));
                }
            }
        }


        private readonly string cadenaConexion =
            ConfigurationManager
                .ConnectionStrings["Conexion"]
                .ConnectionString;


        // =====================================================
        // LOGIN MEDIANTE EMAIL
        // =====================================================

        public Usuario Login(
            string email,
            string password)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        U.IdUsuario,
                        U.Nombre,
                        U.Apellido,
                        U.Email,
                        U.PasswordHash,
                        U.PasswordSalt,
                        U.IdNivel,
                        U.IdEstado,
                        N.Descripcion AS Nivel

                    FROM dbo.Usuarios U

                    INNER JOIN dbo.Nivel N
                        ON U.IdNivel = N.IdNivel

                    INNER JOIN dbo.Estado E
                        ON U.IdEstado = E.IdEstado

                    WHERE U.Email = @Email
                      AND E.Descripcion = 'Activo'";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        254
                    ).Value = email;


                    conexion.Open();


                    using (SqlDataReader reader =
                        comando.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }


                        byte[] hashGuardado =
                            (byte[])reader["PasswordHash"];

                        byte[] saltGuardado =
                            (byte[])reader["PasswordSalt"];


                        bool passwordCorrecto =
                            PasswordHelper.VerificarPassword(
                                password,
                                hashGuardado,
                                saltGuardado);


                        if (!passwordCorrecto)
                        {
                            return null;
                        }


                        Usuario usuario =
                            new Usuario();


                        usuario.IdUsuario =
                            Convert.ToInt32(
                                reader["IdUsuario"]);


                        usuario.Nombre =
                            reader["Nombre"].ToString();


                        usuario.Apellido =
                            reader["Apellido"].ToString();


                        usuario.Email =
                            reader["Email"].ToString();


                        usuario.IdNivel =
                            Convert.ToInt32(
                                reader["IdNivel"]);


                        usuario.IdEstado =
                            Convert.ToInt32(
                                reader["IdEstado"]);


                        usuario.Nivel =
                            reader["Nivel"].ToString();


                        return usuario;
                    }
                }
            }
        }


        // =====================================================
        // COMPROBAR EMAIL
        // =====================================================

        public bool ExisteEmail(
            string email)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT COUNT(*)

                    FROM dbo.Usuarios

                    WHERE Email = @Email";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        254
                    ).Value = email;


                    conexion.Open();


                    int cantidad =
                        Convert.ToInt32(
                            comando.ExecuteScalar());


                    return cantidad > 0;
                }
            }
        }


        // =====================================================
        // REGISTRAR PACIENTE
        // =====================================================

        public void RegistrarPaciente(
            string nombre,
            string apellido,
            string email,
            string password)
        {
            byte[] passwordHash;
            byte[] passwordSalt;


            PasswordHelper.CrearPasswordHash(
                password,
                out passwordHash,
                out passwordSalt);


            /*
             * NombreUsuario ya no se usa para login.
             *
             * La columna todavía existe en la base,
             * por lo que guardamos un valor interno
             * único para mantener compatibilidad.
             */

            string nombreUsuarioLegacy =
                "legacy_" +
                Guid.NewGuid()
                    .ToString("N");


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();


                SqlTransaction transaccion =
                    conexion.BeginTransaction();


                try
                {
                    // ============================================
                    // CREAR USUARIO
                    // ============================================

                    string sqlUsuario = @"
                INSERT INTO dbo.Usuarios
                (
                    Nombre,
                    Apellido,
                    Email,
                    NombreUsuario,
                    PasswordHash,
                    PasswordSalt,
                    IdNivel,
                    IdEstado,
                    Rol,
                    Activo,
                    FechaCreacion
                )

                OUTPUT INSERTED.IdUsuario

                SELECT
                    @Nombre,
                    @Apellido,
                    @Email,
                    @NombreUsuarioLegacy,
                    @PasswordHash,
                    @PasswordSalt,
                    N.IdNivel,
                    E.IdEstado,
                    N.Descripcion,
                    1,
                    GETDATE()

                FROM dbo.Nivel N

                CROSS JOIN dbo.Estado E

                WHERE N.Descripcion = 'PACIENTE'
                  AND E.Descripcion = 'Activo';";


                    int idUsuario;


                    using (SqlCommand comando =
                        new SqlCommand(
                            sqlUsuario,
                            conexion,
                            transaccion))
                    {
                        comando.Parameters.Add(
                            "@Nombre",
                            SqlDbType.NVarChar,
                            200
                        ).Value = nombre.Trim();


                        comando.Parameters.Add(
                            "@Apellido",
                            SqlDbType.NVarChar,
                            200
                        ).Value = apellido.Trim();


                        comando.Parameters.Add(
                            "@Email",
                            SqlDbType.NVarChar,
                            508
                        ).Value =
                            email
                                .Trim()
                                .ToLowerInvariant();


                        comando.Parameters.Add(
                            "@NombreUsuarioLegacy",
                            SqlDbType.NVarChar,
                            100
                        ).Value =
                            nombreUsuarioLegacy;


                        comando.Parameters.Add(
                            "@PasswordHash",
                            SqlDbType.VarBinary,
                            64
                        ).Value =
                            passwordHash;


                        comando.Parameters.Add(
                            "@PasswordSalt",
                            SqlDbType.VarBinary,
                            32
                        ).Value =
                            passwordSalt;


                        object resultado =
                            comando.ExecuteScalar();


                        if (
                            resultado == null
                            ||
                            resultado == DBNull.Value
                        )
                        {
                            throw new Exception(
                                "No se pudo crear el usuario paciente.");
                        }


                        idUsuario =
                            Convert.ToInt32(
                                resultado);
                    }


                    // ============================================
                    // CREAR FILA PACIENTE VINCULADA
                    // ============================================

                    string sqlPaciente = @"
                INSERT INTO dbo.Paciente
                (
                    Nombre,
                    Apellido,
                    Telefono,
                    Foto,
                    NumeroObraSocial,
                    IdObraSocial,
                    IdEstado,
                    IdDomicilio,
                    IdUsuario
                )

                SELECT
                    @Nombre,
                    @Apellido,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    E.IdEstado,
                    NULL,
                    @IdUsuario

                FROM dbo.Estado E

                WHERE E.Descripcion = 'Activo';";


                    using (SqlCommand comando =
                        new SqlCommand(
                            sqlPaciente,
                            conexion,
                            transaccion))
                    {
                        comando.Parameters.Add(
                            "@Nombre",
                            SqlDbType.VarChar,
                            100
                        ).Value =
                            nombre.Trim();


                        comando.Parameters.Add(
                            "@Apellido",
                            SqlDbType.VarChar,
                            100
                        ).Value =
                            apellido.Trim();


                        comando.Parameters.Add(
                            "@IdUsuario",
                            SqlDbType.Int
                        ).Value =
                            idUsuario;


                        int filas =
                            comando.ExecuteNonQuery();


                        if (filas != 1)
                        {
                            throw new Exception(
                                "No se pudo crear el paciente asociado.");
                        }
                    }


                    // ============================================
                    // TODO CORRECTO
                    // ============================================

                    transaccion.Commit();
                }
                catch
                {
                    try
                    {
                        transaccion.Rollback();
                    }
                    catch
                    {
                    }


                    throw;
                }
            }
        }



        // =====================================================
        // PERMISOS
        // =====================================================

        public bool TieneAcceso(
            int idNivel,
            string pagina)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT COUNT(*)

                    FROM dbo.Nivel_URL NU

                    INNER JOIN dbo.URL U
                        ON NU.IdUrl = U.IdUrl

                    INNER JOIN dbo.Estado E
                        ON NU.IdEstado = E.IdEstado

                    WHERE NU.IdNivel = @IdNivel
                      AND U.Descripcion = @Pagina
                      AND E.Descripcion = 'Activo'";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdNivel",
                        SqlDbType.Int
                    ).Value = idNivel;


                    comando.Parameters.Add(
                        "@Pagina",
                        SqlDbType.VarChar,
                        150
                    ).Value = pagina;


                    conexion.Open();


                    int cantidad =
                        Convert.ToInt32(
                            comando.ExecuteScalar());


                    return cantidad > 0;
                }
            }
        }


        // =====================================================
        // REGISTRO AUTOMÁTICO DE URL
        // =====================================================

        public void RegistrarUrlSiNoExiste(
            string pagina)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    IF NOT EXISTS
                    (
                        SELECT 1

                        FROM dbo.URL

                        WHERE Descripcion = @Pagina
                    )
                    BEGIN

                        INSERT INTO dbo.URL
                        (
                            Descripcion
                        )
                        VALUES
                        (
                            @Pagina
                        )

                    END";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@Pagina",
                        SqlDbType.VarChar,
                        150
                    ).Value = pagina;


                    conexion.Open();

                    comando.ExecuteNonQuery();
                }
            }
        }


        // =====================================================
        // NIVELES
        // =====================================================

        public DataTable ObtenerNiveles()
        {
            DataTable tabla =
                new DataTable();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        IdNivel,
                        Descripcion

                    FROM dbo.Nivel

                    ORDER BY Descripcion";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }


            return tabla;
        }


        // =====================================================
        // ESTADOS
        // =====================================================

        public DataTable ObtenerEstados()
        {
            DataTable tabla =
                new DataTable();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        IdEstado,
                        Descripcion

                    FROM dbo.Estado

                    ORDER BY IdEstado";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }


            return tabla;
        }


        // =====================================================
        // OBTENER USUARIOS
        // =====================================================

        public DataTable ObtenerUsuarios()
        {
            DataTable tabla =
                new DataTable();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        U.IdUsuario,
                        U.Nombre,
                        U.Apellido,
                        U.Email,
                        U.IdNivel,
                        N.Descripcion AS Nivel,
                        U.IdEstado,
                        E.Descripcion AS Estado

                    FROM dbo.Usuarios U

                    INNER JOIN dbo.Nivel N
                        ON U.IdNivel = N.IdNivel

                    INNER JOIN dbo.Estado E
                        ON U.IdEstado = E.IdEstado

                    ORDER BY
                        U.Apellido,
                        U.Nombre";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }


            return tabla;
        }


        // =====================================================
        // OBTENER USUARIO POR ID
        // =====================================================

        public DataRow ObtenerUsuarioPorId(
            int idUsuario)
        {
            DataTable tabla =
                new DataTable();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        IdUsuario,
                        Nombre,
                        Apellido,
                        Email,
                        IdNivel,
                        IdEstado

                    FROM dbo.Usuarios

                    WHERE IdUsuario = @IdUsuario";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int
                    ).Value = idUsuario;


                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }


            if (tabla.Rows.Count == 0)
            {
                return null;
            }


            return tabla.Rows[0];
        }


        // =====================================================
        // ACTUALIZAR NIVEL Y ESTADO
        // =====================================================

        public void ActualizarNivelEstado(
            int idUsuario,
            int idNivel,
            int idEstado)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    UPDATE U

                    SET
                        U.IdNivel = @IdNivel,
                        U.IdEstado = @IdEstado,

                        U.Rol =
                            N.Descripcion,

                        U.Activo =
                            CASE
                                WHEN E.Descripcion = 'Activo'
                                THEN 1
                                ELSE 0
                            END

                    FROM dbo.Usuarios U

                    INNER JOIN dbo.Nivel N
                        ON N.IdNivel = @IdNivel

                    INNER JOIN dbo.Estado E
                        ON E.IdEstado = @IdEstado

                    WHERE U.IdUsuario = @IdUsuario";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int
                    ).Value = idUsuario;


                    comando.Parameters.Add(
                        "@IdNivel",
                        SqlDbType.Int
                    ).Value = idNivel;


                    comando.Parameters.Add(
                        "@IdEstado",
                        SqlDbType.Int
                    ).Value = idEstado;


                    conexion.Open();


                    int filas =
                        comando.ExecuteNonQuery();


                    if (filas == 0)
                    {
                        throw new Exception(
                            "No se pudo actualizar el usuario.");
                    }
                }
            }
        }


        // =====================================================
        // URLS
        // =====================================================

        public DataTable ObtenerUrls()
        {
            DataTable tabla =
                new DataTable();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        IdUrl,
                        Descripcion

                    FROM dbo.URL

                    ORDER BY Descripcion";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }


            return tabla;
        }


        // =====================================================
        // PERMISOS POR URL
        // =====================================================

        public DataTable ObtenerPermisosUrl(
            int idUrl)
        {
            DataTable tabla =
                new DataTable();


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        N.IdNivel,
                        N.Descripcion AS Nivel,

                        CASE
                            WHEN NU.IdEstado IS NOT NULL
                             AND E.Descripcion = 'Activo'
                            THEN 1
                            ELSE 0
                        END AS TieneAcceso

                    FROM dbo.Nivel N

                    LEFT JOIN dbo.Nivel_URL NU
                        ON N.IdNivel = NU.IdNivel
                        AND NU.IdUrl = @IdUrl

                    LEFT JOIN dbo.Estado E
                        ON NU.IdEstado = E.IdEstado

                    ORDER BY N.IdNivel";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdUrl",
                        SqlDbType.Int
                    ).Value = idUrl;


                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }


            return tabla;
        }


        // =====================================================
        // GUARDAR PERMISO
        // =====================================================

        public void GuardarPermiso(
            int idNivel,
            int idUrl,
            bool permitir)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();


                if (permitir)
                {
                    string sql = @"
                        IF EXISTS
                        (
                            SELECT 1

                            FROM dbo.Nivel_URL

                            WHERE IdNivel = @IdNivel
                              AND IdUrl = @IdUrl
                        )
                        BEGIN

                            UPDATE dbo.Nivel_URL

                            SET IdEstado =
                            (
                                SELECT IdEstado

                                FROM dbo.Estado

                                WHERE Descripcion = 'Activo'
                            )

                            WHERE IdNivel = @IdNivel
                              AND IdUrl = @IdUrl

                        END

                        ELSE

                        BEGIN

                            INSERT INTO dbo.Nivel_URL
                            (
                                IdNivel,
                                IdUrl,
                                IdEstado
                            )

                            SELECT
                                @IdNivel,
                                @IdUrl,
                                IdEstado

                            FROM dbo.Estado

                            WHERE Descripcion = 'Activo'

                        END";


                    using (SqlCommand comando =
                        new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.Add(
                            "@IdNivel",
                            SqlDbType.Int
                        ).Value = idNivel;


                        comando.Parameters.Add(
                            "@IdUrl",
                            SqlDbType.Int
                        ).Value = idUrl;


                        comando.ExecuteNonQuery();
                    }
                }
                else
                {
                    string sql = @"
                        UPDATE dbo.Nivel_URL

                        SET IdEstado =
                        (
                            SELECT IdEstado

                            FROM dbo.Estado

                            WHERE Descripcion = 'Inactivo'
                        )

                        WHERE IdNivel = @IdNivel
                          AND IdUrl = @IdUrl";


                    using (SqlCommand comando =
                        new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.Add(
                            "@IdNivel",
                            SqlDbType.Int
                        ).Value = idNivel;


                        comando.Parameters.Add(
                            "@IdUrl",
                            SqlDbType.Int
                        ).Value = idUrl;


                        comando.ExecuteNonQuery();
                    }
                }
            }
        }


        // =====================================================
        // CREAR RECUPERACIÓN POR EMAIL
        // =====================================================

        public string CrearRecuperacionPassword(
            string email)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();


                string sqlUsuario = @"
                    SELECT
                        U.IdUsuario

                    FROM dbo.Usuarios U

                    INNER JOIN dbo.Estado E
                        ON U.IdEstado = E.IdEstado

                    WHERE U.Email = @Email
                      AND E.Descripcion = 'Activo'";


                int idUsuario;


                using (SqlCommand comando =
                    new SqlCommand(
                        sqlUsuario,
                        conexion))
                {
                    comando.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        254
                    ).Value = email;


                    object resultado =
                        comando.ExecuteScalar();


                    if (resultado == null ||
                        resultado == DBNull.Value)
                    {
                        return null;
                    }


                    idUsuario =
                        Convert.ToInt32(resultado);
                }


                string token =
                    PasswordResetHelper
                        .GenerarToken();


                byte[] tokenHash =
                    PasswordResetHelper
                        .ObtenerHashToken(token);


                // Invalidamos tokens anteriores.

                string invalidar = @"
                    UPDATE dbo.PasswordReset

                    SET Usado = 1

                    WHERE IdUsuario = @IdUsuario
                      AND Usado = 0";


                using (SqlCommand comando =
                    new SqlCommand(
                        invalidar,
                        conexion))
                {
                    comando.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int
                    ).Value = idUsuario;


                    comando.ExecuteNonQuery();
                }


                // Nuevo token por 30 minutos.

                string insertar = @"
                    INSERT INTO dbo.PasswordReset
                    (
                        IdUsuario,
                        TokenHash,
                        FechaExpiracion,
                        Usado
                    )

                    VALUES
                    (
                        @IdUsuario,
                        @TokenHash,
                        DATEADD(
                            MINUTE,
                            30,
                            GETUTCDATE()
                        ),
                        0
                    )";


                using (SqlCommand comando =
                    new SqlCommand(
                        insertar,
                        conexion))
                {
                    comando.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int
                    ).Value = idUsuario;


                    comando.Parameters.Add(
                        "@TokenHash",
                        SqlDbType.VarBinary,
                        32
                    ).Value = tokenHash;


                    comando.ExecuteNonQuery();
                }


                return token;
            }
        }


        // =====================================================
        // OBTENER USUARIO MEDIANTE TOKEN
        // =====================================================

        public int ObtenerUsuarioPorToken(
            string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }


            byte[] tokenHash =
                PasswordResetHelper
                    .ObtenerHashToken(token);


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        IdUsuario

                    FROM dbo.PasswordReset

                    WHERE TokenHash = @TokenHash
                      AND Usado = 0
                      AND FechaExpiracion > GETUTCDATE()";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@TokenHash",
                        SqlDbType.VarBinary,
                        32
                    ).Value = tokenHash;


                    conexion.Open();


                    object resultado =
                        comando.ExecuteScalar();


                    if (resultado == null ||
                        resultado == DBNull.Value)
                    {
                        return 0;
                    }


                    return Convert.ToInt32(
                        resultado);
                }
            }
        }


        // =====================================================
        // COMPROBAR CONTRASEÑA ACTUAL
        // =====================================================

        public bool EsPasswordActual(
            int idUsuario,
            string password)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT
                        PasswordHash,
                        PasswordSalt

                    FROM dbo.Usuarios

                    WHERE IdUsuario = @IdUsuario";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int
                    ).Value = idUsuario;


                    conexion.Open();


                    using (SqlDataReader reader =
                        comando.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }


                        byte[] passwordHash =
                            (byte[])reader["PasswordHash"];


                        byte[] passwordSalt =
                            (byte[])reader["PasswordSalt"];


                        return PasswordHelper
                            .VerificarPassword(
                                password,
                                passwordHash,
                                passwordSalt);
                    }
                }
            }
        }


        // =====================================================
        // RESTABLECER CONTRASEÑA
        // =====================================================

        public void RestablecerPassword(
            int idUsuario,
            string token,
            string nuevaPassword)
        {
            byte[] nuevoHash;
            byte[] nuevoSalt;


            PasswordHelper.CrearPasswordHash(
                nuevaPassword,
                out nuevoHash,
                out nuevoSalt);


            byte[] tokenHash =
                PasswordResetHelper
                    .ObtenerHashToken(token);


            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();


                SqlTransaction transaccion =
                    conexion.BeginTransaction();


                try
                {
                    string sqlToken = @"
                        UPDATE dbo.PasswordReset

                        SET Usado = 1

                        WHERE TokenHash = @TokenHash
                          AND IdUsuario = @IdUsuario
                          AND Usado = 0
                          AND FechaExpiracion > GETUTCDATE()";


                    int tokenActualizado;


                    using (SqlCommand comando =
                        new SqlCommand(
                            sqlToken,
                            conexion,
                            transaccion))
                    {
                        comando.Parameters.Add(
                            "@TokenHash",
                            SqlDbType.VarBinary,
                            32
                        ).Value = tokenHash;


                        comando.Parameters.Add(
                            "@IdUsuario",
                            SqlDbType.Int
                        ).Value = idUsuario;


                        tokenActualizado =
                            comando.ExecuteNonQuery();
                    }


                    if (tokenActualizado == 0)
                    {
                        throw new Exception(
                            "El enlace venció o ya fue utilizado.");
                    }


                    string sqlPassword = @"
                        UPDATE dbo.Usuarios

                        SET
                            PasswordHash = @PasswordHash,
                            PasswordSalt = @PasswordSalt

                        WHERE IdUsuario = @IdUsuario";


                    int usuarioActualizado;


                    using (SqlCommand comando =
                        new SqlCommand(
                            sqlPassword,
                            conexion,
                            transaccion))
                    {
                        comando.Parameters.Add(
                            "@PasswordHash",
                            SqlDbType.VarBinary,
                            64
                        ).Value = nuevoHash;


                        comando.Parameters.Add(
                            "@PasswordSalt",
                            SqlDbType.VarBinary,
                            32
                        ).Value = nuevoSalt;


                        comando.Parameters.Add(
                            "@IdUsuario",
                            SqlDbType.Int
                        ).Value = idUsuario;


                        usuarioActualizado =
                            comando.ExecuteNonQuery();
                    }


                    if (usuarioActualizado == 0)
                    {
                        throw new Exception(
                            "No se pudo actualizar la contraseña.");
                    }


                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();

                    throw;
                }
            }
        }


        // =====================================================
        // OBTENER EMAIL DE CUENTA ACTIVA
        // =====================================================

        public string ObtenerEmailUsuarioActivo(
            string email)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
                    SELECT U.Email

                    FROM dbo.Usuarios U

                    INNER JOIN dbo.Estado E
                        ON U.IdEstado = E.IdEstado

                    WHERE U.Email = @Email
                      AND E.Descripcion = 'Activo'";


                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        254
                    ).Value = email;


                    conexion.Open();


                    object resultado =
                        comando.ExecuteScalar();


                    if (resultado == null ||
                        resultado == DBNull.Value)
                    {
                        return null;
                    }


                    return resultado.ToString();
                }
            }
        }

        // =====================================================
        // OBTENER ACCESOS A PÁGINAS SEGÚN NIVEL
        // =====================================================

        public DataTable ObtenerAccesosNivel(
            int idNivel)
        {
            DataTable tabla =
                new DataTable();

            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
            SELECT
                U.IdUrl,
                U.Descripcion,

                CASE
                    WHEN NU.IdEstado IS NOT NULL
                     AND E.Descripcion = 'Activo'
                    THEN 1
                    ELSE 0
                END AS TieneAcceso

            FROM dbo.URL U

            LEFT JOIN dbo.Nivel_URL NU
                ON U.IdUrl = NU.IdUrl
                AND NU.IdNivel = @IdNivel

            LEFT JOIN dbo.Estado E
                ON NU.IdEstado = E.IdEstado

            ORDER BY U.Descripcion";

                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdNivel",
                        SqlDbType.Int
                    ).Value = idNivel;

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }

            return tabla;
        }


        // =====================================================
        // OBTENER PERMISOS SEGÚN NIVEL
        // =====================================================

        public DataTable ObtenerPermisosNivel(
            int idNivel)
        {
            DataTable tabla =
                new DataTable();

            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
            SELECT
                P.IdPermiso,
                P.Codigo,
                P.Grupo,
                P.Descripcion,

                P.Grupo + ' - ' +
                P.Descripcion AS Texto,

                CASE
                    WHEN NP.IdEstado IS NOT NULL
                     AND ENP.Descripcion = 'Activo'
                    THEN 1
                    ELSE 0
                END AS TienePermiso

            FROM dbo.Permiso P

            INNER JOIN dbo.Estado EP
                ON P.IdEstado = EP.IdEstado

            LEFT JOIN dbo.Nivel_Permiso NP
                ON P.IdPermiso = NP.IdPermiso
                AND NP.IdNivel = @IdNivel

            LEFT JOIN dbo.Estado ENP
                ON NP.IdEstado = ENP.IdEstado

            WHERE EP.Descripcion = 'Activo'

            ORDER BY
                P.Grupo,
                P.Descripcion";

                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdNivel",
                        SqlDbType.Int
                    ).Value = idNivel;

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(comando))
                    {
                        adapter.Fill(tabla);
                    }
                }
            }

            return tabla;
        }


        // =====================================================
        // GUARDAR PERMISO DE ACCIÓN
        // =====================================================

        public void GuardarPermisoNivel(
            int idNivel,
            int idPermiso,
            bool permitir)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string estado =
                    permitir
                        ? "Activo"
                        : "Inactivo";

                string sql = @"
            IF EXISTS
            (
                SELECT 1

                FROM dbo.Nivel_Permiso

                WHERE IdNivel = @IdNivel
                  AND IdPermiso = @IdPermiso
            )
            BEGIN

                UPDATE dbo.Nivel_Permiso

                SET IdEstado =
                (
                    SELECT IdEstado
                    FROM dbo.Estado
                    WHERE Descripcion = @Estado
                )

                WHERE IdNivel = @IdNivel
                  AND IdPermiso = @IdPermiso

            END

            ELSE

            BEGIN

                INSERT INTO dbo.Nivel_Permiso
                (
                    IdNivel,
                    IdPermiso,
                    IdEstado
                )

                SELECT
                    @IdNivel,
                    @IdPermiso,
                    IdEstado

                FROM dbo.Estado

                WHERE Descripcion = @Estado

            END";

                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdNivel",
                        SqlDbType.Int
                    ).Value = idNivel;

                    comando.Parameters.Add(
                        "@IdPermiso",
                        SqlDbType.Int
                    ).Value = idPermiso;

                    comando.Parameters.Add(
                        "@Estado",
                        SqlDbType.NVarChar,
                        50
                    ).Value = estado;

                    comando.ExecuteNonQuery();
                }
            }
        }


        // =====================================================
        // COMPROBAR PERMISO DE ACCIÓN
        // =====================================================

        public bool TienePermiso(
            int idNivel,
            string codigoPermiso)
        {
            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                string sql = @"
            SELECT COUNT(*)

            FROM dbo.Nivel_Permiso NP

            INNER JOIN dbo.Permiso P
                ON NP.IdPermiso = P.IdPermiso

            INNER JOIN dbo.Estado ENP
                ON NP.IdEstado = ENP.IdEstado

            INNER JOIN dbo.Estado EP
                ON P.IdEstado = EP.IdEstado

            WHERE NP.IdNivel = @IdNivel
              AND P.Codigo = @Codigo
              AND ENP.Descripcion = 'Activo'
              AND EP.Descripcion = 'Activo'";

                using (SqlCommand comando =
                    new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "@IdNivel",
                        SqlDbType.Int
                    ).Value = idNivel;

                    comando.Parameters.Add(
                        "@Codigo",
                        SqlDbType.VarChar,
                        100
                    ).Value = codigoPermiso;

                    conexion.Open();

                    int cantidad =
                        Convert.ToInt32(
                            comando.ExecuteScalar());

                    return cantidad > 0;
                }
            }
        }

    }
}
