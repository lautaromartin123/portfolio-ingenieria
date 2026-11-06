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

    public class SolicitudModificacionNegocio
    {
        public int EnviarPaciente(
            int idUsuario,
            SolicitudModificacionPaciente datos)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es valido.");

            if (datos == null)
                throw new ArgumentException("Faltan los datos propuestos.");

            string nombre =
                ValidarTexto(datos.Nombre, "Nombre", 100, true);

            string apellido =
                ValidarTexto(datos.Apellido, "Apellido", 100, true);

            string telefono =
                ValidarTexto(datos.Telefono, "Telefono", 30, false);

            string numeroObraSocial =
                ValidarTexto(
                    datos.NumeroObraSocial,
                    "Numero de afiliado",
                    100,
                    false
                );

            if (datos.IdObraSocial.HasValue &&
                datos.IdObraSocial.Value <= 0)
            {
                throw new ArgumentException(
                    "La obra social seleccionada no es valida.");
            }

            if (!datos.IdObraSocial.HasValue &&
                numeroObraSocial != null)
            {
                throw new ArgumentException(
                    "Selecciona una obra social para indicar el numero de afiliado.");
            }

            string cadenaConexion =
                ConfigurationManager
                    .ConnectionStrings["Conexion"]
                    .ConnectionString;

            try
            {
                using (SqlConnection conexion =
                    new SqlConnection(cadenaConexion))
                {
                    conexion.Open();

                    using (SqlTransaction transaccion =
                        conexion.BeginTransaction())
                    {
                        int idSolicitud;

                        // Crear la solicitud para el paciente de esta cuenta.
                        using (SqlCommand comando = new SqlCommand())
                        {
                            comando.Connection = conexion;
                            comando.Transaction = transaccion;

                            comando.CommandText = @"
                                INSERT INTO dbo.SolicitudModificacion
                                (
                                    IdUsuarioSolicitante,
                                    IdPaciente
                                )
                                OUTPUT INSERTED.IdSolicitud
                                SELECT
                                    U.IdUsuario,
                                    P.IdPaciente
                                FROM dbo.Usuarios U
                                INNER JOIN dbo.Paciente P
                                    ON P.IdUsuario = U.IdUsuario
                                INNER JOIN dbo.Nivel N
                                    ON N.IdNivel = U.IdNivel
                                INNER JOIN dbo.Estado EU
                                    ON EU.IdEstado = U.IdEstado
                                INNER JOIN dbo.Estado EP
                                    ON EP.IdEstado = P.IdEstado
                                WHERE U.IdUsuario = @IdUsuario
                                  AND N.Descripcion = 'PACIENTE'
                                  AND EU.Descripcion = 'Activo'
                                  AND EP.Descripcion = 'Activo';
                            ";

                            comando.Parameters.Add(
                                "@IdUsuario",
                                SqlDbType.Int
                            ).Value = idUsuario;

                            object resultado = comando.ExecuteScalar();

                            if (resultado == null || resultado == DBNull.Value)
                            {
                                throw new InvalidOperationException(
                                    "La cuenta debe ser un paciente activo " +
                                    "y tener un paciente activo vinculado.");
                            }

                            idSolicitud = Convert.ToInt32(resultado);
                        }

                        // Guardar los datos propuestos.
                        using (SqlCommand comando = new SqlCommand())
                        {
                            comando.Connection = conexion;
                            comando.Transaction = transaccion;

                            comando.CommandText = @"
                                INSERT INTO dbo.SolicitudModificacionPaciente
                                (
                                    IdSolicitud,
                                    Nombre,
                                    Apellido,
                                    Telefono,
                                    IdObraSocial,
                                    NumeroObraSocial
                                )
                                VALUES
                                (
                                    @IdSolicitud,
                                    @Nombre,
                                    @Apellido,
                                    @Telefono,
                                    @IdObraSocial,
                                    @NumeroObraSocial
                                );
                            ";

                            comando.Parameters.Add(
                                "@IdSolicitud", SqlDbType.Int
                            ).Value = idSolicitud;

                            comando.Parameters.Add(
                                "@Nombre", SqlDbType.VarChar, 100
                            ).Value = nombre;

                            comando.Parameters.Add(
                                "@Apellido", SqlDbType.VarChar, 100
                            ).Value = apellido;

                            comando.Parameters.Add(
                                "@Telefono", SqlDbType.VarChar, 30
                            ).Value = (object)telefono ?? DBNull.Value;

                            comando.Parameters.Add(
                                "@IdObraSocial", SqlDbType.Int
                            ).Value = (object)datos.IdObraSocial ?? DBNull.Value;

                            comando.Parameters.Add(
                                "@NumeroObraSocial", SqlDbType.VarChar, 100
                            ).Value = (object)numeroObraSocial ?? DBNull.Value;

                            comando.ExecuteNonQuery();
                        }

                        transaccion.Commit();

                        return idSolicitud;
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    throw new InvalidOperationException(
                        "Ya tenes una solicitud pendiente de revision.",
                        ex
                    );
                }

                throw;
            }
        }

        public int EnviarMedico(int idUsuario, SolicitudModificacionMedico datos)
        {
            if (idUsuario <= 0 || datos == null)
                throw new ArgumentException("Faltan el usuario o los datos propuestos.");

            string nombre = ValidarTexto(datos.Nombre, "Nombre", 100, true);

            string apellido = ValidarTexto(datos.Apellido, "Apellido", 100, true);


            List<int> especialidades = datos.IdsEspecialidades == null
                ? new List<int>() : datos.IdsEspecialidades.Distinct().ToList();

            if (especialidades.Count == 0 && datos.IdEspecialidad > 0)
                especialidades.Add(datos.IdEspecialidad);

            if (especialidades.Count == 0 || especialidades.Any(id => id <= 0))
                throw new ArgumentException("Selecciona al menos una especialidad valida.");

            string numero = ValidarTexto(datos.NumeroMatricula, "Numero de matricula", 50, false);

            string entidad = ValidarTexto(datos.EntidadEmisora, "Entidad emisora", 100, false);

            if (numero != null && (!datos.IdTipoMatricula.HasValue || datos.IdTipoMatricula.Value <= 0))
                throw new ArgumentException("Selecciona el tipo de matricula.");

            if (numero == null && (datos.IdTipoMatricula.HasValue || entidad != null || datos.FechaVencimiento.HasValue))
                throw new ArgumentException("Indica el numero de matricula para cargar sus datos.");

            string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        int idSolicitud;
                        int principal = especialidades[0];
                        using (SqlCommand comando = new SqlCommand())
                        {
                            comando.Connection=conexion;
                            comando.Transaction=transaccion;
                            comando.CommandText=@"
                                INSERT dbo.SolicitudModificacion(IdUsuarioSolicitante,IdProfesional)
                                OUTPUT INSERTED.IdSolicitud
                                SELECT U.IdUsuario,P.IdProfesional FROM dbo.Usuarios U
                                INNER JOIN dbo.Profesional P ON P.IdUsuario=U.IdUsuario
                                INNER JOIN dbo.Nivel N ON N.IdNivel=U.IdNivel
                                INNER JOIN dbo.Estado EU ON EU.IdEstado=U.IdEstado
                                INNER JOIN dbo.Estado EP ON EP.IdEstado=P.IdEstado
                                WHERE U.IdUsuario=@IdUsuario AND N.Descripcion='MEDICO'
                                  AND EU.Descripcion='Activo' AND EP.Descripcion='Activo';";
                            comando.Parameters.AddWithValue("@IdUsuario",idUsuario);
                            object resultado=comando.ExecuteScalar();
                            if (resultado == null || resultado == DBNull.Value)
                                throw new InvalidOperationException("La cuenta debe ser un medico activo con un profesional activo vinculado.");
                            idSolicitud=Convert.ToInt32(resultado);
                            comando.CommandText=@"
                                SELECT IdProfesional,IdEspecialidad FROM dbo.Profesional WITH (UPDLOCK,HOLDLOCK)
                                WHERE IdUsuario=@IdUsuario;";
                            int idProfesional;
                            using (SqlDataReader lector=comando.ExecuteReader())
                            {
                                if (!lector.Read())
                                    throw new InvalidOperationException("La cuenta ya no tiene un profesional vinculado.");
                                idProfesional=Convert.ToInt32(lector["IdProfesional"]);
                                int actual=Convert.ToInt32(lector["IdEspecialidad"]);
                                if (especialidades.Contains(actual)) principal=actual;
                            }
                            if (numero != null)
                            {
                                comando.CommandText=@"SELECT COUNT(*) FROM dbo.Matricula WITH (UPDLOCK,HOLDLOCK)
                                    WHERE IdProfesional=@IdProfesional;";
                                comando.Parameters.AddWithValue("@IdProfesional",idProfesional);
                                if (Convert.ToInt32(comando.ExecuteScalar()) > 0)
                                    throw new InvalidOperationException("Ya tenes una matricula registrada. Solo un administrador puede cambiarla indicando un motivo.");
                            }
                        }
                        using (SqlCommand comando=new SqlCommand())
                        {
                            comando.Connection=conexion;
                            comando.Transaction=transaccion;
                            comando.CommandText=@"
                                INSERT dbo.SolicitudModificacionMedico
                                    (IdSolicitud,Nombre,Apellido,IdEspecialidad,NumeroMatricula,IdTipoMatricula,EntidadEmisora,FechaVencimiento)
                                SELECT @IdSolicitud,@Nombre,@Apellido,E.IdEspecialidad,@Numero,@IdTipo,@Entidad,@Vencimiento
                                FROM dbo.Especialidad E WHERE E.IdEspecialidad=@IdEspecialidad
                                AND (@Numero IS NULL OR EXISTS (SELECT 1 FROM dbo.TipoMatricula WHERE IdTipoMatricula=@IdTipo));";
                            comando.Parameters.AddWithValue("@IdSolicitud",idSolicitud);
                            comando.Parameters.Add("@Nombre",SqlDbType.VarChar,100).Value=nombre;
                            comando.Parameters.Add("@Apellido",SqlDbType.VarChar,100).Value=apellido;
                            comando.Parameters.AddWithValue("@IdEspecialidad",principal);
                            comando.Parameters.Add("@Numero",SqlDbType.VarChar,50).Value=(object)numero ?? DBNull.Value;
                            comando.Parameters.Add("@IdTipo",SqlDbType.Int).Value=(object)datos.IdTipoMatricula ?? DBNull.Value;
                            comando.Parameters.Add("@Entidad",SqlDbType.VarChar,100).Value=(object)entidad ?? DBNull.Value;
                            comando.Parameters.Add("@Vencimiento",SqlDbType.Date).Value=(object)datos.FechaVencimiento ?? DBNull.Value;
                            if (comando.ExecuteNonQuery()!=1)
                                throw new ArgumentException("La especialidad o el tipo de matricula no existe.");
                            comando.CommandText=@"
                                INSERT dbo.SolicitudModificacionMedicoEspecialidad(IdSolicitud,IdEspecialidad)
                                SELECT @IdSolicitud,IdEspecialidad FROM dbo.Especialidad WHERE IdEspecialidad=@IdEspecialidad;";
                            foreach(int id in especialidades)
                            {
                                comando.Parameters["@IdEspecialidad"].Value=id;
                                if (comando.ExecuteNonQuery()!=1)
                                    throw new ArgumentException("Una de las especialidades seleccionadas no existe.");
                            }
                        }
                        transaccion.Commit();
                        return idSolicitud;
                    }
                }
            }
            catch(SqlException ex)
            {
                if(ex.Number==2601 || ex.Number==2627)
                    throw new InvalidOperationException("Ya tenes una solicitud pendiente de revision.",ex);
                throw;
            }
        }

        private string ValidarTexto(
            string valor,
            string campo,
            int longitudMaxima,
            bool obligatorio)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                if (obligatorio)
                    throw new ArgumentException(campo + " es obligatorio.");

                return null;
            }

            string texto = valor.Trim();

            if (texto.Length > longitudMaxima)
            {
                throw new ArgumentException(
                    campo + " admite hasta " + longitudMaxima + " caracteres.");
            }

            return texto;
        }


        public int? BuscarIdPendientePaciente(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es valido.");

            string cadenaConexion =
                ConfigurationManager
                    .ConnectionStrings["Conexion"]
                    .ConnectionString;

            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                using (SqlCommand comando = new SqlCommand())
                {
                    comando.Connection = conexion;

                    comando.CommandText = @"
                SELECT S.IdSolicitud
                FROM dbo.SolicitudModificacion S
                INNER JOIN dbo.Paciente P
                    ON P.IdPaciente = S.IdPaciente
                WHERE S.IdUsuarioSolicitante = @IdUsuario
                  AND P.IdUsuario = @IdUsuario
                  AND S.IdEstadoSolicitud = 1;
            ";

                    comando.Parameters.Add(
                        "@IdUsuario",
                        SqlDbType.Int
                    ).Value = idUsuario;

                    conexion.Open();

                    object resultado = comando.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                        return null;

                    return Convert.ToInt32(resultado);
                }
            }
        }

        public int? BuscarIdPendienteMedico(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es valido.");

            string cadenaConexion =
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = new SqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    SELECT S.IdSolicitud
                    FROM dbo.SolicitudModificacion S
                    INNER JOIN dbo.Profesional P
                        ON P.IdProfesional = S.IdProfesional
                    WHERE S.IdUsuarioSolicitante = @IdUsuario
                      AND P.IdUsuario = @IdUsuario
                      AND S.IdEstadoSolicitud = 1;
                ";

                comando.Parameters.Add(
                    "@IdUsuario", SqlDbType.Int).Value = idUsuario;

                conexion.Open();
                object resultado = comando.ExecuteScalar();

                if (resultado == null || resultado == DBNull.Value)
                    return null;

                return Convert.ToInt32(resultado);
            }
        }

        private void ValidarAdministrador(
            int idUsuario,
            SqlConnection conexion,
            SqlTransaction transaccion = null)
        {
            using (SqlCommand comando = new SqlCommand())
            {
                comando.Connection = conexion;
                comando.Transaction = transaccion;

                comando.CommandText = @"
            SELECT COUNT(*)
            FROM dbo.Usuarios U
            INNER JOIN dbo.Nivel N
                ON N.IdNivel = U.IdNivel
            INNER JOIN dbo.Estado E
                ON E.IdEstado = U.IdEstado
            WHERE U.IdUsuario = @IdUsuario
              AND N.Descripcion = 'ADMIN'
              AND E.Descripcion = 'Activo';
        ";

                comando.Parameters.Add(
                    "@IdUsuario",
                    SqlDbType.Int
                ).Value = idUsuario;

                int cantidad =
                    Convert.ToInt32(comando.ExecuteScalar());

                if (cantidad == 0)
                {
                    throw new InvalidOperationException(
                        "Esta operacion requiere un administrador activo.");
                }
            }
        }

        public DataTable ListarPendientesPaciente(int idUsuarioAdmin)
        {
            DataTable tabla = new DataTable();

            string cadenaConexion =
                ConfigurationManager
                    .ConnectionStrings["Conexion"]
                    .ConnectionString;

            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                ValidarAdministrador(idUsuarioAdmin, conexion);

                using (SqlCommand comando = new SqlCommand())
                {
                    comando.Connection = conexion;

                    comando.CommandText = @"
                SELECT
                    S.IdSolicitud,
                    S.IdUsuarioSolicitante,
                    S.IdPaciente,
                    S.FechaSolicitud,

                    P.Nombre AS NombreActual,
                    D.Nombre AS NombrePropuesto,

                    P.Apellido AS ApellidoActual,
                    D.Apellido AS ApellidoPropuesto,

                    P.Telefono AS TelefonoActual,
                    D.Telefono AS TelefonoPropuesto,

                    OA.Nombre AS ObraSocialActual,
                    OP.Nombre AS ObraSocialPropuesta,

                    P.NumeroObraSocial AS NumeroObraSocialActual,
                    D.NumeroObraSocial AS NumeroObraSocialPropuesto

                FROM dbo.SolicitudModificacion S

                INNER JOIN dbo.SolicitudModificacionPaciente D
                    ON D.IdSolicitud = S.IdSolicitud

                INNER JOIN dbo.Paciente P
                    ON P.IdPaciente = S.IdPaciente

                LEFT JOIN dbo.ObraSocial OA
                    ON OA.IdObraSocial = P.IdObraSocial

                LEFT JOIN dbo.ObraSocial OP
                    ON OP.IdObraSocial = D.IdObraSocial

                WHERE S.IdEstadoSolicitud = 1

                ORDER BY S.FechaSolicitud, S.IdSolicitud;
            ";

                    using (SqlDataAdapter adaptador =
                        new SqlDataAdapter(comando))
                    {
                        adaptador.Fill(tabla);
                    }
                }
            }

            return tabla;
        }




        public DataTable ListarPendientesMedico(int idUsuarioAdmin)
        {
            DataTable tabla=new DataTable();
            string cadena=ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;
            using(SqlConnection conexion=new SqlConnection(cadena))
            {
                conexion.Open();
                ValidarAdministrador(idUsuarioAdmin,conexion);
                using(SqlCommand comando=new SqlCommand(@"
                    SELECT S.IdSolicitud,S.IdUsuarioSolicitante,S.IdProfesional,S.FechaSolicitud,
                        P.Nombre AS NombreActual,D.Nombre AS NombrePropuesto,
                        P.Apellido AS ApellidoActual,D.Apellido AS ApellidoPropuesto,
                        COALESCE((SELECT STRING_AGG(E.Nombre, ', ') WITHIN GROUP (ORDER BY E.Nombre)
                            FROM dbo.ProfesionalEspecialidad PE INNER JOIN dbo.Especialidad E ON E.IdEspecialidad=PE.IdEspecialidad
                            WHERE PE.IdProfesional=P.IdProfesional),EA.Nombre) AS EspecialidadActual,
                        COALESCE((SELECT STRING_AGG(E.Nombre, ', ') WITHIN GROUP (ORDER BY E.Nombre)
                            FROM dbo.SolicitudModificacionMedicoEspecialidad DE INNER JOIN dbo.Especialidad E ON E.IdEspecialidad=DE.IdEspecialidad
                            WHERE DE.IdSolicitud=S.IdSolicitud),EP.Nombre) AS EspecialidadPropuesta,
                        COALESCE((SELECT STRING_AGG(M.Numero, ', ') FROM dbo.Matricula M
                            WHERE M.IdProfesional=P.IdProfesional),'NULL') AS MatriculaActual,
                        COALESCE(D.NumeroMatricula,'Sin cambios') AS MatriculaPropuesta,
                        COALESCE(T.Descripcion,'NULL') AS TipoMatriculaPropuesto,
                        COALESCE(D.EntidadEmisora,'NULL') AS EntidadPropuesta,
                        COALESCE(CONVERT(VARCHAR(10),D.FechaVencimiento,23),'NULL') AS VencimientoPropuesto
                    FROM dbo.SolicitudModificacion S
                    INNER JOIN dbo.SolicitudModificacionMedico D ON D.IdSolicitud=S.IdSolicitud
                    INNER JOIN dbo.Profesional P ON P.IdProfesional=S.IdProfesional
                    INNER JOIN dbo.Especialidad EA ON EA.IdEspecialidad=P.IdEspecialidad
                    INNER JOIN dbo.Especialidad EP ON EP.IdEspecialidad=D.IdEspecialidad
                    LEFT JOIN dbo.TipoMatricula T ON T.IdTipoMatricula=D.IdTipoMatricula
                    WHERE S.IdEstadoSolicitud=1 ORDER BY S.FechaSolicitud,S.IdSolicitud;",conexion))
                using(SqlDataAdapter adaptador=new SqlDataAdapter(comando))
                    adaptador.Fill(tabla);
            }
            return tabla;
        }

        public void RechazarPaciente(
            int idSolicitud,
            int idUsuarioAdmin,
            string observacion)
        {
            RechazarSolicitud(idSolicitud, idUsuarioAdmin, observacion, false);
        }

        public void RechazarMedico(
            int idSolicitud,
            int idUsuarioAdmin,
            string observacion)
        {
            RechazarSolicitud(idSolicitud, idUsuarioAdmin, observacion, true);
        }

        private void RechazarSolicitud(
            int idSolicitud,
            int idUsuarioAdmin,
            string observacion,
            bool esMedico)
        {
            if (idSolicitud <= 0)
                throw new ArgumentException("La solicitud no es valida.");

            string motivo =
                ValidarTexto(observacion, "El motivo del rechazo", 500, true);

            string cadenaConexion =
                ConfigurationManager
                    .ConnectionStrings["Conexion"]
                    .ConnectionString;

            using (SqlConnection conexion =
                new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlTransaction transaccion =
                    conexion.BeginTransaction())
                {
                    ValidarAdministrador(
                        idUsuarioAdmin,
                        conexion,
                        transaccion
                    );

                    using (SqlCommand comando = new SqlCommand())
                    {
                        comando.Connection = conexion;
                        comando.Transaction = transaccion;

                        comando.CommandText = @"
                    UPDATE dbo.SolicitudModificacion
                    SET
                        IdEstadoSolicitud = 3,
                        IdUsuarioRevisor = @IdUsuarioRevisor,
                        FechaResolucion = SYSUTCDATETIME(),
                        ObservacionRevision = @Observacion
                    WHERE IdSolicitud = @IdSolicitud
                      AND ((@EsMedico = 1 AND IdProfesional IS NOT NULL
                            AND EXISTS (SELECT 1 FROM dbo.SolicitudModificacionMedico D
                                WHERE D.IdSolicitud=dbo.SolicitudModificacion.IdSolicitud))
                        OR (@EsMedico = 0 AND IdPaciente IS NOT NULL))
                      AND IdEstadoSolicitud = 1;
                ";

                        comando.Parameters.Add(
                            "@IdSolicitud",
                            SqlDbType.Int
                        ).Value = idSolicitud;

                        comando.Parameters.Add(
                            "@IdUsuarioRevisor",
                            SqlDbType.Int
                        ).Value = idUsuarioAdmin;

                        comando.Parameters.Add(
                            "@Observacion",
                            SqlDbType.NVarChar,
                            500
                        ).Value = motivo;

                        comando.Parameters.Add(
                            "@EsMedico", SqlDbType.Bit
                        ).Value = esMedico;

                        int filas = comando.ExecuteNonQuery();

                        if (filas != 1)
                        {
                            throw new InvalidOperationException(
                                "La solicitud no existe, no corresponde al tipo indicado " +
                                "o ya fue resuelta. Actualiza el listado.");
                        }
                    }

                    transaccion.Commit();
                }
            }
        }


        public void AprobarMedico(int idSolicitud, int idUsuarioAdmin)
        {
            if (idSolicitud <= 0)
                throw new ArgumentException("La solicitud no es valida.");

            string cadenaConexion =
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    ValidarAdministrador(idUsuarioAdmin, conexion, transaccion);

                    using (SqlCommand comando = new SqlCommand())
                    {
                        comando.Connection = conexion;
                        comando.Transaction = transaccion;
                        comando.Parameters.Add(
                            "@IdSolicitud", SqlDbType.Int).Value = idSolicitud;
                        comando.Parameters.Add(
                            "@IdUsuarioRevisor", SqlDbType.Int).Value = idUsuarioAdmin;

                        comando.CommandText = @"
                            UPDATE dbo.SolicitudModificacion
                            SET IdEstadoSolicitud = 2,
                                IdUsuarioRevisor = @IdUsuarioRevisor,
                                FechaResolucion = SYSUTCDATETIME(),
                                ObservacionRevision = NULL
                            WHERE IdSolicitud = @IdSolicitud
                              AND IdProfesional IS NOT NULL
                              AND IdEstadoSolicitud = 1
                              AND IdUsuarioSolicitante <> @IdUsuarioRevisor;
                        ";

                        if (comando.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "La solicitud no existe, no corresponde a un medico, " +
                                "ya fue resuelta o estas intentando aprobar tu propia solicitud.");
                        }

                        comando.CommandText = @"
                            UPDATE P
                            SET Nombre = D.Nombre,
                                Apellido = D.Apellido,
                                IdEspecialidad = D.IdEspecialidad
                            FROM dbo.Profesional P
                            INNER JOIN dbo.SolicitudModificacion S
                                ON S.IdProfesional = P.IdProfesional
                            INNER JOIN dbo.SolicitudModificacionMedico D
                                ON D.IdSolicitud = S.IdSolicitud
                            INNER JOIN dbo.Especialidad E
                                ON E.IdEspecialidad = D.IdEspecialidad
                            INNER JOIN dbo.Usuarios U
                                ON U.IdUsuario = S.IdUsuarioSolicitante
                               AND U.IdUsuario = P.IdUsuario
                            INNER JOIN dbo.Nivel N
                                ON N.IdNivel = U.IdNivel
                            INNER JOIN dbo.Estado EU
                                ON EU.IdEstado = U.IdEstado
                            INNER JOIN dbo.Estado EP
                                ON EP.IdEstado = P.IdEstado
                            WHERE S.IdSolicitud = @IdSolicitud
                              AND N.Descripcion = 'MEDICO'
                              AND EU.Descripcion = 'Activo'
                              AND EP.Descripcion = 'Activo';
                        ";

                        if (comando.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "No se pudieron aplicar los datos. Verifica que la cuenta " +
                                "siga siendo un medico activo vinculado al mismo profesional activo.");
                        }

                        comando.CommandText = @"
                            UPDATE U
                            SET Nombre = D.Nombre,
                                Apellido = D.Apellido
                            FROM dbo.Usuarios U
                            INNER JOIN dbo.SolicitudModificacion S
                                ON S.IdUsuarioSolicitante = U.IdUsuario
                            INNER JOIN dbo.SolicitudModificacionMedico D
                                ON D.IdSolicitud = S.IdSolicitud
                            INNER JOIN dbo.Profesional P
                                ON P.IdProfesional = S.IdProfesional
                               AND P.IdUsuario = U.IdUsuario
                            WHERE S.IdSolicitud = @IdSolicitud;
                        ";

                        if (comando.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("No se pudieron sincronizar los datos de la cuenta.");

                        comando.CommandText=@"
                            DELETE FROM dbo.ProfesionalEspecialidad
                            WHERE IdProfesional=(SELECT IdProfesional FROM dbo.SolicitudModificacion WHERE IdSolicitud=@IdSolicitud);";
                        comando.ExecuteNonQuery();
                        comando.CommandText=@"
                            INSERT dbo.ProfesionalEspecialidad(IdProfesional,IdEspecialidad)
                            SELECT S.IdProfesional,DE.IdEspecialidad FROM dbo.SolicitudModificacion S
                            INNER JOIN dbo.SolicitudModificacionMedicoEspecialidad DE ON DE.IdSolicitud=S.IdSolicitud
                            WHERE S.IdSolicitud=@IdSolicitud;";
                        if(comando.ExecuteNonQuery()==0)
                            throw new InvalidOperationException("La solicitud no tiene especialidades propuestas.");

                        Matricula matricula=null;
                        comando.CommandText=@"
                            SELECT S.IdProfesional,D.NumeroMatricula,D.IdTipoMatricula,D.EntidadEmisora,D.FechaVencimiento
                            FROM dbo.SolicitudModificacion S
                            INNER JOIN dbo.SolicitudModificacionMedico D ON D.IdSolicitud=S.IdSolicitud
                            WHERE S.IdSolicitud=@IdSolicitud;";
                        using(SqlDataReader lector=comando.ExecuteReader())
                        {
                            if(lector.Read() && lector["NumeroMatricula"]!=DBNull.Value)
                            {
                                matricula=new Matricula
                                {
                                    IdProfesional=Convert.ToInt32(lector["IdProfesional"]),
                                    Numero=Convert.ToString(lector["NumeroMatricula"]),
                                    IdTipoMatricula=Convert.ToInt32(lector["IdTipoMatricula"]),
                                    EntidadEmisora=lector["EntidadEmisora"]==DBNull.Value ? null : Convert.ToString(lector["EntidadEmisora"]),
                                    FechaVencimiento=lector["FechaVencimiento"]==DBNull.Value ? (DateTime?)null : Convert.ToDateTime(lector["FechaVencimiento"])
                                };
                            }
                        }
                        if(matricula!=null)
                            new MatriculaNegocio().AgregarInicialAprobada(
                                matricula,idUsuarioAdmin,idSolicitud,conexion,transaccion);

                    }

                    transaccion.Commit();
                }
            }
        }

        public void AprobarPaciente(int idSolicitud, int idUsuarioAdmin)
        {
            if (idSolicitud <= 0)
                throw new ArgumentException("La solicitud no es valida.");

            string cadenaConexion =
                ConfigurationManager
                    .ConnectionStrings["Conexion"]
                    .ConnectionString;

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    ValidarAdministrador(
                        idUsuarioAdmin,
                        conexion,
                        transaccion
                    );

                    using (SqlCommand comando = new SqlCommand())
                    {
                        comando.Connection = conexion;
                        comando.Transaction = transaccion;

                        comando.Parameters.Add(
                            "@IdSolicitud", SqlDbType.Int
                        ).Value = idSolicitud;

                        comando.Parameters.Add(
                            "@IdUsuarioRevisor", SqlDbType.Int
                        ).Value = idUsuarioAdmin;

                        // 1. Resolver unicamente una solicitud pendiente.
                        comando.CommandText = @"
                    UPDATE dbo.SolicitudModificacion
                    SET
                        IdEstadoSolicitud = 2,
                        IdUsuarioRevisor = @IdUsuarioRevisor,
                        FechaResolucion = SYSUTCDATETIME(),
                        ObservacionRevision = NULL
                    WHERE IdSolicitud = @IdSolicitud
                      AND IdPaciente IS NOT NULL
                      AND IdEstadoSolicitud = 1
                      AND IdUsuarioSolicitante <> @IdUsuarioRevisor;
                ";

                        if (comando.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "La solicitud no existe, no corresponde a un paciente, " +
                                "ya fue resuelta o estas intentando aprobar " +
                                "tu propia solicitud.");
                        }

                        // 2. Aplicar los datos guardados en la solicitud.
                        comando.CommandText = @"
                    UPDATE P
                    SET
                        Nombre = D.Nombre,
                        Apellido = D.Apellido,
                        Telefono = D.Telefono,
                        IdObraSocial = D.IdObraSocial,
                        NumeroObraSocial = D.NumeroObraSocial
                    FROM dbo.Paciente P
                    INNER JOIN dbo.SolicitudModificacion S
                        ON S.IdPaciente = P.IdPaciente
                    INNER JOIN dbo.SolicitudModificacionPaciente D
                        ON D.IdSolicitud = S.IdSolicitud
                    INNER JOIN dbo.Usuarios U
                        ON U.IdUsuario = S.IdUsuarioSolicitante
                       AND U.IdUsuario = P.IdUsuario
                    INNER JOIN dbo.Nivel N
                        ON N.IdNivel = U.IdNivel
                    INNER JOIN dbo.Estado EU
                        ON EU.IdEstado = U.IdEstado
                    INNER JOIN dbo.Estado EP
                        ON EP.IdEstado = P.IdEstado
                    WHERE S.IdSolicitud = @IdSolicitud
                      AND N.Descripcion = 'PACIENTE'
                      AND EU.Descripcion = 'Activo'
                      AND EP.Descripcion = 'Activo';
                ";

                        if (comando.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "No se pudieron aplicar los datos. Verifica que " +
                                "la solicitud tenga su detalle y que la cuenta " +
                                "siga siendo un paciente activo vinculado " +
                                "al mismo paciente activo.");
                        }

                        // 3. Mantener nombre y apellido sincronizados en la cuenta.
                        comando.CommandText = @"
                    UPDATE U
                    SET
                        Nombre = D.Nombre,
                        Apellido = D.Apellido
                    FROM dbo.Usuarios U
                    INNER JOIN dbo.SolicitudModificacion S
                        ON S.IdUsuarioSolicitante = U.IdUsuario
                    INNER JOIN dbo.SolicitudModificacionPaciente D
                        ON D.IdSolicitud = S.IdSolicitud
                    INNER JOIN dbo.Paciente P
                        ON P.IdPaciente = S.IdPaciente
                       AND P.IdUsuario = U.IdUsuario
                    WHERE S.IdSolicitud = @IdSolicitud;
                ";

                        if (comando.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "No se pudieron sincronizar los datos de la cuenta.");
                        }
                    }

                    transaccion.Commit();
                }
            }
        }

















    }
}
