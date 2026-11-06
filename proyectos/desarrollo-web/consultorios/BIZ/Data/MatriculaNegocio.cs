using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace BIZ
{
    public class MatriculaNegocio
    {
        private const string SELECT_BASE = @"
            SELECT
                M.IdMatricula,
                M.Numero,
                M.EntidadEmisora,
                M.FechaVencimiento,
                M.IdProfesional,
                M.IdTipoMatricula,
                M.IdEstado,

                T.Descripcion  AS TipoMatriculaDescripcion,
                ES.Descripcion AS EstadoDescripcion

            FROM Matricula M

            INNER JOIN TipoMatricula T
                ON M.IdTipoMatricula = T.IdTipoMatricula

            INNER JOIN Estado ES
                ON M.IdEstado = ES.IdEstado
        ";


        private Matricula Mapear(SqlDataReader lector)
        {
            Matricula matricula = new Matricula();

            matricula.IdMatricula =
                Convert.ToInt32(lector["IdMatricula"]);

            matricula.Numero =
                lector["Numero"].ToString();

            matricula.EntidadEmisora =
                lector["EntidadEmisora"].ToString();

            if (lector["FechaVencimiento"] != DBNull.Value)
            {
                matricula.FechaVencimiento =
                    Convert.ToDateTime(lector["FechaVencimiento"]);
            }

            matricula.IdProfesional =
                Convert.ToInt32(lector["IdProfesional"]);


            // TIPO DE MATRICULA
            matricula.IdTipoMatricula =
                Convert.ToInt32(lector["IdTipoMatricula"]);

            matricula.TipoMatricula = new TipoMatricula();

            matricula.TipoMatricula.IdTipoMatricula =
                Convert.ToInt32(lector["IdTipoMatricula"]);

            matricula.TipoMatricula.Descripcion =
                lector["TipoMatriculaDescripcion"].ToString();


            // ESTADO
            matricula.IdEstado =
                Convert.ToInt32(lector["IdEstado"]);

            matricula.Estado = new Estado();

            matricula.Estado.IdEstado =
                Convert.ToInt32(lector["IdEstado"]);

            matricula.Estado.Descripcion =
                lector["EstadoDescripcion"].ToString();

            return matricula;
        }


        /* Todas las matriculas de un profesional. Es la consulta
           que usa la pantalla de detalle del profesional. */
        public List<Matricula> ListarPorProfesional(int idProfesional)
        {
            List<Matricula> lista = new List<Matricula>();

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE +
                    " WHERE M.IdProfesional = @IdProfesional " +
                    " ORDER BY M.FechaVencimiento DESC";

                comando.Parameters.AddWithValue(
                    "@IdProfesional",
                    idProfesional
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(Mapear(lector));
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


        public Matricula BuscarPorId(int idMatricula)
        {
            Matricula matricula = null;

            SqlConnection conexion = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString
            );

            SqlCommand comando = new SqlCommand();

            try
            {
                comando.Connection = conexion;

                comando.CommandText =
                    SELECT_BASE + " WHERE M.IdMatricula = @IdMatricula";

                comando.Parameters.AddWithValue(
                    "@IdMatricula",
                    idMatricula
                );

                conexion.Open();

                SqlDataReader lector = comando.ExecuteReader();

                if (lector.Read())
                {
                    matricula = Mapear(lector);
                }

                lector.Close();

                return matricula;
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


        public int Agregar(Matricula matricula, int idUsuarioAdmin, string motivo)
        {
            using (SqlConnection conexion = CrearConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int id = Agregar(matricula, idUsuarioAdmin, motivo, null, conexion, transaccion);
                    transaccion.Commit();
                    return id;
                }
            }
        }

        internal int AgregarInicialAprobada(Matricula matricula, int idUsuarioAdmin,
            int idSolicitud, SqlConnection conexion, SqlTransaction transaccion)
        {
            BloquearProfesional(matricula.IdProfesional, conexion, transaccion);
            using (SqlCommand comando = new SqlCommand(@"
                SELECT COUNT(*) FROM dbo.Matricula WITH (UPDLOCK,HOLDLOCK)
                WHERE IdProfesional=@IdProfesional;", conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdProfesional", matricula.IdProfesional);
                if (Convert.ToInt32(comando.ExecuteScalar()) > 0)
                    throw new InvalidOperationException(
                        "El profesional ya tiene una matricula registrada. La solicitud no se puede aprobar.");
            }

            return Agregar(matricula, idUsuarioAdmin,
                "Alta inicial aprobada mediante solicitud", idSolicitud, conexion, transaccion);
        }

        private int Agregar(Matricula matricula, int idUsuarioAdmin, string motivo,
            int? idSolicitud, SqlConnection conexion, SqlTransaction transaccion)
        {
            ValidarAdministrador(idUsuarioAdmin, conexion, transaccion);
            ValidarDatos(matricula);
            motivo = ValidarMotivo(motivo);
            BloquearProfesional(matricula.IdProfesional, conexion, transaccion);

            using (SqlCommand comando = new SqlCommand(@"
                INSERT dbo.Matricula
                    (Numero,EntidadEmisora,FechaVencimiento,IdProfesional,IdTipoMatricula,IdEstado)
                OUTPUT INSERTED.IdMatricula
                VALUES (@Numero,@Entidad,@Vencimiento,@IdProfesional,@IdTipo,@IdEstado);",
                conexion, transaccion))
            {
                AgregarParametros(comando, matricula);
                comando.Parameters.AddWithValue("@IdProfesional", matricula.IdProfesional);
                matricula.IdMatricula = Convert.ToInt32(comando.ExecuteScalar());
            }
            RegistrarHistorial(null, matricula, idUsuarioAdmin, motivo, "Alta",
                idSolicitud, conexion, transaccion);
            return matricula.IdMatricula;
        }

        public void Modificar(Matricula matricula, int idUsuarioAdmin, string motivo)
        {
            ValidarDatos(matricula);
            motivo = ValidarMotivo(motivo);
            using (SqlConnection conexion = CrearConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    ValidarAdministrador(idUsuarioAdmin, conexion, transaccion);
                    BloquearProfesional(matricula.IdProfesional, conexion, transaccion);
                    Matricula anterior = LeerParaCambio(matricula.IdMatricula, conexion, transaccion);
                    if (anterior.IdProfesional != matricula.IdProfesional)
                        throw new InvalidOperationException("La matricula no corresponde al profesional indicado.");

                    using (SqlCommand comando = new SqlCommand(@"
                        UPDATE dbo.Matricula
                        SET Numero=@Numero,EntidadEmisora=@Entidad,FechaVencimiento=@Vencimiento,
                            IdTipoMatricula=@IdTipo,IdEstado=@IdEstado
                        WHERE IdMatricula=@IdMatricula;", conexion, transaccion))
                    {
                        AgregarParametros(comando, matricula);
                        comando.Parameters.AddWithValue("@IdMatricula", matricula.IdMatricula);
                        if (comando.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("No se pudo actualizar la matricula.");
                    }
                    RegistrarHistorial(anterior, matricula, idUsuarioAdmin, motivo,
                        "Modificacion", null, conexion, transaccion);
                    transaccion.Commit();
                }
            }
        }

        public void DarDeBaja(int idMatricula, int idProfesional,
            int idUsuarioAdmin, string motivo)
        {
            motivo = ValidarMotivo(motivo);
            using (SqlConnection conexion = CrearConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    ValidarAdministrador(idUsuarioAdmin, conexion, transaccion);
                    BloquearProfesional(idProfesional, conexion, transaccion);
                    Matricula anterior = LeerParaCambio(idMatricula, conexion, transaccion);
                    if (anterior.IdProfesional != idProfesional)
                        throw new InvalidOperationException("La matricula no corresponde al profesional indicado.");

                    using (SqlCommand comando = new SqlCommand(@"
                        UPDATE dbo.Matricula
                        SET IdEstado=(SELECT IdEstado FROM dbo.Estado WHERE Descripcion='Inactivo')
                        WHERE IdMatricula=@IdMatricula;", conexion, transaccion))
                    {
                        comando.Parameters.AddWithValue("@IdMatricula", idMatricula);
                        comando.ExecuteNonQuery();
                    }
                    Matricula nueva = LeerParaCambio(idMatricula, conexion, transaccion);
                    RegistrarHistorial(anterior, nueva, idUsuarioAdmin, motivo,
                        "Baja", null, conexion, transaccion);
                    transaccion.Commit();
                }
            }
        }

        public int AgregarProfesionalConMatricula(Profesional profesional,
            Matricula matricula, int idUsuarioAdmin)
        {
            using (SqlConnection conexion = CrearConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    ValidarAdministrador(idUsuarioAdmin, conexion, transaccion);
                    int idProfesional = new ProfesionalNegocio().Agregar(profesional, conexion, transaccion);
                    matricula.IdProfesional = idProfesional;
                    Agregar(matricula, idUsuarioAdmin, "Alta inicial del profesional",
                        null, conexion, transaccion);
                    transaccion.Commit();
                    return idProfesional;
                }
            }
        }

        public System.Data.DataTable ListarHistorialPorProfesional(int idProfesional, int idUsuarioAdmin)
        {
            System.Data.DataTable tabla = new System.Data.DataTable();
            using (SqlConnection conexion = CrearConexion())
            {
                conexion.Open();
                ValidarAdministrador(idUsuarioAdmin, conexion, null);
                using (SqlCommand comando = new SqlCommand(@"
                    SELECT H.FechaCambio,H.Operacion,H.NumeroAnterior,H.NumeroNuevo,
                        H.EntidadAnterior,H.EntidadNueva,H.VencimientoAnterior,H.VencimientoNuevo,
                        H.IdTipoAnterior,H.IdTipoNuevo,H.IdEstadoAnterior,H.IdEstadoNuevo,
                        H.IdUsuarioAdmin,U.Nombre+' '+COALESCE(U.Apellido,'') AS Administrador,
                        H.Motivo,H.IdSolicitud
                    FROM dbo.HistorialMatricula H
                    INNER JOIN dbo.Matricula M ON M.IdMatricula=H.IdMatricula
                    INNER JOIN dbo.Usuarios U ON U.IdUsuario=H.IdUsuarioAdmin
                    WHERE M.IdProfesional=@IdProfesional
                    ORDER BY H.IdHistorial DESC;", conexion))
                {
                    comando.Parameters.AddWithValue("@IdProfesional", idProfesional);
                    using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
                        adaptador.Fill(tabla);
                }
            }
            return tabla;
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString);
        }

        private void ValidarAdministrador(int idUsuario, SqlConnection conexion, SqlTransaction transaccion)
        {
            using (SqlCommand comando = new SqlCommand(@"
                SELECT COUNT(*) FROM dbo.Usuarios U
                INNER JOIN dbo.Nivel N ON N.IdNivel=U.IdNivel
                INNER JOIN dbo.Estado E ON E.IdEstado=U.IdEstado
                WHERE U.IdUsuario=@IdUsuario AND N.Descripcion='ADMIN' AND E.Descripcion='Activo';",
                conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                if (Convert.ToInt32(comando.ExecuteScalar()) != 1)
                    throw new InvalidOperationException("Esta operacion requiere un administrador activo.");
            }
        }

        private void BloquearProfesional(int idProfesional, SqlConnection conexion, SqlTransaction transaccion)
        {
            using (SqlCommand comando = new SqlCommand(@"
                SELECT IdProfesional FROM dbo.Profesional WITH (UPDLOCK,HOLDLOCK)
                WHERE IdProfesional=@IdProfesional;", conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdProfesional", idProfesional);
                if (comando.ExecuteScalar() == null)
                    throw new InvalidOperationException("No existe el profesional.");
            }
        }

        private void ValidarDatos(Matricula matricula)
        {
            if (matricula == null || matricula.IdProfesional <= 0)
                throw new ArgumentException("Faltan los datos de la matricula.");
            if (string.IsNullOrWhiteSpace(matricula.Numero) || matricula.Numero.Trim().Length > 50)
                throw new ArgumentException("El numero de matricula es obligatorio y admite hasta 50 caracteres.");
            matricula.Numero = matricula.Numero.Trim();
            matricula.EntidadEmisora = string.IsNullOrWhiteSpace(matricula.EntidadEmisora)
                ? null : matricula.EntidadEmisora.Trim();
            if (matricula.EntidadEmisora != null && matricula.EntidadEmisora.Length > 100)
                throw new ArgumentException("La entidad emisora admite hasta 100 caracteres.");
            if (matricula.IdTipoMatricula <= 0)
                throw new ArgumentException("Selecciona el tipo de matricula.");
            if (matricula.IdEstado == 0)
                matricula.IdEstado = 1;
        }

        private string ValidarMotivo(string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > 500)
                throw new ArgumentException("El motivo es obligatorio y admite hasta 500 caracteres.");
            return motivo.Trim();
        }

        private void AgregarParametros(SqlCommand comando, Matricula matricula)
        {
            comando.Parameters.Add("@Numero", System.Data.SqlDbType.VarChar, 50).Value = matricula.Numero;
            comando.Parameters.Add("@Entidad", System.Data.SqlDbType.VarChar, 100).Value =
                (object)matricula.EntidadEmisora ?? DBNull.Value;
            comando.Parameters.Add("@Vencimiento", System.Data.SqlDbType.Date).Value =
                (object)matricula.FechaVencimiento ?? DBNull.Value;
            comando.Parameters.AddWithValue("@IdTipo", matricula.IdTipoMatricula);
            comando.Parameters.AddWithValue("@IdEstado", matricula.IdEstado);
        }

        private Matricula LeerParaCambio(int idMatricula, SqlConnection conexion, SqlTransaction transaccion)
        {
            using (SqlCommand comando = new SqlCommand(@"
                SELECT IdMatricula,IdProfesional,Numero,EntidadEmisora,FechaVencimiento,IdTipoMatricula,IdEstado
                FROM dbo.Matricula WITH (UPDLOCK,HOLDLOCK)
                WHERE IdMatricula=@IdMatricula;", conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdMatricula", idMatricula);
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (!lector.Read())
                        throw new InvalidOperationException("No existe la matricula.");
                    return new Matricula
                    {
                        IdMatricula=Convert.ToInt32(lector["IdMatricula"]),
                        IdProfesional=Convert.ToInt32(lector["IdProfesional"]),
                        Numero=Convert.ToString(lector["Numero"]),
                        EntidadEmisora=lector["EntidadEmisora"] == DBNull.Value ? null : Convert.ToString(lector["EntidadEmisora"]),
                        FechaVencimiento=lector["FechaVencimiento"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(lector["FechaVencimiento"]),
                        IdTipoMatricula=Convert.ToInt32(lector["IdTipoMatricula"]),
                        IdEstado=Convert.ToInt32(lector["IdEstado"])
                    };
                }
            }
        }

        private void RegistrarHistorial(Matricula anterior, Matricula nueva, int idUsuarioAdmin,
            string motivo, string operacion, int? idSolicitud, SqlConnection conexion, SqlTransaction transaccion)
        {
            using (SqlCommand comando = new SqlCommand(@"
                INSERT dbo.HistorialMatricula
                    (IdMatricula,IdUsuarioAdmin,IdSolicitud,Operacion,Motivo,
                     NumeroAnterior,NumeroNuevo,IdTipoAnterior,IdTipoNuevo,
                     EntidadAnterior,EntidadNueva,VencimientoAnterior,VencimientoNuevo,IdEstadoAnterior,IdEstadoNuevo)
                VALUES
                    (@IdMatricula,@IdUsuario,@IdSolicitud,@Operacion,@Motivo,
                     @NumeroAnterior,@NumeroNuevo,@TipoAnterior,@TipoNuevo,
                     @EntidadAnterior,@EntidadNueva,@VencimientoAnterior,@VencimientoNuevo,@EstadoAnterior,@EstadoNuevo);",
                conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdMatricula", nueva.IdMatricula);
                comando.Parameters.AddWithValue("@IdUsuario", idUsuarioAdmin);
                comando.Parameters.AddWithValue("@IdSolicitud", (object)idSolicitud ?? DBNull.Value);
                comando.Parameters.AddWithValue("@Operacion", operacion);
                comando.Parameters.Add("@Motivo", System.Data.SqlDbType.NVarChar, 500).Value = motivo;
                comando.Parameters.AddWithValue("@NumeroAnterior", (object)anterior?.Numero ?? DBNull.Value);
                comando.Parameters.AddWithValue("@NumeroNuevo", nueva.Numero);
                comando.Parameters.AddWithValue("@TipoAnterior", (object)anterior?.IdTipoMatricula ?? DBNull.Value);
                comando.Parameters.AddWithValue("@TipoNuevo", nueva.IdTipoMatricula);
                comando.Parameters.AddWithValue("@EntidadAnterior", (object)anterior?.EntidadEmisora ?? DBNull.Value);
                comando.Parameters.AddWithValue("@EntidadNueva", (object)nueva.EntidadEmisora ?? DBNull.Value);
                comando.Parameters.AddWithValue("@VencimientoAnterior", (object)anterior?.FechaVencimiento ?? DBNull.Value);
                comando.Parameters.AddWithValue("@VencimientoNuevo", (object)nueva.FechaVencimiento ?? DBNull.Value);
                comando.Parameters.AddWithValue("@EstadoAnterior", (object)anterior?.IdEstado ?? DBNull.Value);
                comando.Parameters.AddWithValue("@EstadoNuevo", nueva.IdEstado);
                comando.ExecuteNonQuery();
            }
        }
    }
}
