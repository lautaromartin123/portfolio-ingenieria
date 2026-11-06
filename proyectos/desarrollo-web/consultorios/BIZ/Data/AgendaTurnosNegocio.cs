using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BIZ
{
    // Operaciones de las agendas personales. Los IDs propios se obtienen del usuario,
    // nunca de un campo editable de la pagina.
    public class AgendaTurnosNegocio
    {
        public static DateTime Ahora()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        }

        private SqlConnection Conexion()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString);
        }

        private SqlCommand Comando(string sql, SqlConnection conexion, SqlTransaction transaccion,
            params SqlParameter[] parametros)
        {
            SqlCommand comando = new SqlCommand(sql, conexion, transaccion);
            comando.Parameters.AddRange(parametros);
            return comando;
        }

        private SqlParameter P(string nombre, object valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }

        private int Usuario(int idUsuario, string nivel, SqlConnection conexion, SqlTransaction transaccion = null)
        {
            using (SqlCommand comando = Comando(@"
                SELECT U.IdUsuario FROM dbo.Usuarios U
                JOIN dbo.Nivel N ON N.IdNivel=U.IdNivel
                JOIN dbo.Estado E ON E.IdEstado=U.IdEstado
                WHERE U.IdUsuario=@Usuario AND N.Descripcion=@Nivel AND E.Descripcion='Activo';",
                conexion, transaccion, P("@Usuario",idUsuario), P("@Nivel",nivel)))
            {
                if (comando.ExecuteScalar() == null)
                    throw new InvalidOperationException("La cuenta no tiene permiso para esta operacion.");
            }
            if (nivel == "ADMIN") return idUsuario;
            string tabla = nivel == "MEDICO" ? "Profesional" : "Paciente";
            string campo = nivel == "MEDICO" ? "IdProfesional" : "IdPaciente";
            using (SqlCommand comando = Comando("SELECT X." + campo + " FROM dbo." + tabla + @" X
                JOIN dbo.Estado E ON E.IdEstado=X.IdEstado
                WHERE X.IdUsuario=@Usuario AND E.Descripcion='Activo';",
                conexion, transaccion, P("@Usuario",idUsuario)))
            {
                object id = comando.ExecuteScalar();
                if (id == null) throw new InvalidOperationException("La cuenta no tiene un " + tabla.ToLower() + " activo vinculado.");
                return Convert.ToInt32(id);
            }
        }

        private DataTable Tabla(string sql, SqlConnection conexion, SqlTransaction transaccion = null,
            params SqlParameter[] parametros)
        {
            using (SqlCommand comando = Comando(sql,conexion,transaccion,parametros))
            using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
            {
                DataTable tabla = new DataTable();
                adaptador.Fill(tabla);
                return tabla;
            }
        }

        private void BloquearProfesional(int id, SqlConnection conexion, SqlTransaction transaccion)
        {
            using (SqlCommand comando = Comando(@"SELECT IdProfesional FROM dbo.Profesional WITH (UPDLOCK,HOLDLOCK)
                WHERE IdProfesional=@Id;",conexion,transaccion,P("@Id",id)))
                if (comando.ExecuteScalar()==null) throw new InvalidOperationException("No existe el profesional.");
        }

        public DataTable ProfesionalesDisponibles(int idUsuario)
        {
            using (SqlConnection conexion=Conexion())
            {
                conexion.Open(); Usuario(idUsuario,"PACIENTE",conexion);
                return Tabla(@"SELECT P.IdProfesional,P.Apellido+', '+P.Nombre AS Nombre,
                    COALESCE((SELECT STRING_AGG(E.Nombre, ', ') FROM dbo.ProfesionalEspecialidad PE
                        JOIN dbo.Especialidad E ON E.IdEspecialidad=PE.IdEspecialidad
                        WHERE PE.IdProfesional=P.IdProfesional),ES.Nombre) AS Especialidades
                    FROM dbo.Profesional P JOIN dbo.Estado E ON E.IdEstado=P.IdEstado
                    JOIN dbo.Especialidad ES ON ES.IdEspecialidad=P.IdEspecialidad
                    WHERE E.Descripcion='Activo' AND EXISTS (SELECT 1 FROM dbo.Agenda A
                        JOIN dbo.Estado EA ON EA.IdEstado=A.IdEstado
                        WHERE A.IdProfesional=P.IdProfesional AND EA.Descripcion='Activo' AND A.AtencionConTurno=1)
                    ORDER BY P.Apellido,P.Nombre;",conexion);
            }
        }

        public DataTable PacientesParaTurno(int idUsuario)
        {
            using (SqlConnection conexion=Conexion())
            {
                conexion.Open(); Usuario(idUsuario,"MEDICO",conexion);
                return Tabla(@"SELECT P.IdPaciente,P.Apellido+', '+P.Nombre+' - '+COALESCE(O.Nombre,'Sin obra social') AS Nombre
                    FROM dbo.Paciente P JOIN dbo.Estado E ON E.IdEstado=P.IdEstado
                    LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=P.IdObraSocial
                    WHERE E.Descripcion='Activo' ORDER BY P.Apellido,P.Nombre;",conexion);
            }
        }

        public DataTable CoberturaPaciente(int idUsuario)
        {
            using (SqlConnection conexion=Conexion())
            {
                conexion.Open(); int id=Usuario(idUsuario,"PACIENTE",conexion);
                return Tabla(@"SELECT COALESCE(O.Nombre,'Sin obra social') AS ObraSocial,
                    COALESCE(P.NumeroObraSocial,'NULL') AS Afiliado FROM dbo.Paciente P
                    LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=P.IdObraSocial WHERE P.IdPaciente=@Id;",conexion,null,P("@Id",id));
            }
        }

        public List<Agenda> MiDisponibilidad(int idUsuario)
        {
            using (SqlConnection conexion=Conexion())
            {
                conexion.Open(); int id=Usuario(idUsuario,"MEDICO",conexion);
                return new AgendaNegocio().ListarPorProfesional(id);
            }
        }

        private const string CONSULTA_TURNOS = @"
            SELECT T.IdTurno,T.FechaHora,T.DuracionMinutos,T.Motivo,
                CASE WHEN T.FechaHora<=@Ahora AND (ET.Descripcion='Atendido'
                    OR (@EsMedico=1 AND ET.Descripcion='Ausente')) THEN T.Observaciones ELSE NULL END AS Observaciones,
                ET.Descripcion AS Estado,PA.Apellido+', '+PA.Nombre AS Paciente,
                PR.Apellido+', '+PR.Nombre AS Profesional,
                CASE WHEN T.CoberturaRegistrada=1 THEN COALESCE(T.ObraSocialTurno,'Sin obra social')
                    ELSE COALESCE(O.Nombre,'Sin obra social') END AS ObraSocial,
                CASE WHEN T.CoberturaRegistrada=1 THEN T.NumeroAfiliadoTurno ELSE PA.NumeroObraSocial END AS Afiliado,
                CASE WHEN T.CoberturaRegistrada=1 THEN 'Al reservar' ELSE 'Cobertura actual' END AS TipoCobertura
            FROM dbo.Turno T JOIN dbo.Paciente PA ON PA.IdPaciente=T.IdPaciente
            JOIN dbo.Profesional PR ON PR.IdProfesional=T.IdProfesional
            JOIN dbo.EstadoTurno ET ON ET.IdEstadoTurno=T.IdEstadoTurno
            LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=PA.IdObraSocial ";

        public DataTable MisTurnos(int idUsuario, bool esMedico, bool historial, DateTime? fecha = null)
        {
            using (SqlConnection conexion=Conexion())
            {
                conexion.Open(); int id=Usuario(idUsuario,esMedico?"MEDICO":"PACIENTE",conexion);
                string filtro=esMedico ? " WHERE T.IdProfesional=@Id" : " WHERE T.IdPaciente=@Id";
                if (historial) filtro+=(esMedico?" AND ET.Descripcion IN ('Atendido','Ausente')":" AND ET.Descripcion='Atendido'")+" AND T.FechaHora<=@Ahora";
                else if (esMedico) filtro+=" AND ET.Descripcion NOT IN ('Atendido','Ausente')";
                else if (!esMedico) filtro+=" AND ET.Descripcion<>'Atendido' AND T.FechaHora>=CAST(@Ahora AS DATE)";
                if (fecha.HasValue) filtro+=" AND T.FechaHora>=@Fecha AND T.FechaHora<DATEADD(DAY,1,@Fecha)";
                return Tabla(CONSULTA_TURNOS+filtro+" ORDER BY T.FechaHora,T.IdTurno;",conexion,null,
                    P("@Id",id),P("@Ahora",Ahora()),P("@EsMedico",esMedico),P("@Fecha",(object)fecha?.Date??DBNull.Value));
            }
        }

        private DataTable Franjas(int profesional, DateTime fecha, SqlConnection conexion, SqlTransaction transaccion)
        {
            int dia=(int)fecha.DayOfWeek; if(dia==0) dia=7;
            return Tabla(@"SELECT A.HoraInicio,A.HoraFin,A.DuracionMinutos FROM dbo.Agenda A
                JOIN dbo.Estado E ON E.IdEstado=A.IdEstado JOIN dbo.Profesional P ON P.IdProfesional=A.IdProfesional
                JOIN dbo.Estado EP ON EP.IdEstado=P.IdEstado
                WHERE A.IdProfesional=@Id AND A.DiaSemana=@Dia AND E.Descripcion='Activo'
                  AND EP.Descripcion='Activo' AND A.AtencionConTurno=1 AND A.DuracionMinutos BETWEEN 5 AND 240
                ORDER BY A.HoraInicio;",conexion,transaccion,P("@Id",profesional),P("@Dia",dia));
        }

        public DataTable HorariosLibres(int usuario, bool medico, int profesional, DateTime fecha)
        {
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open(); int propio=Usuario(usuario,medico?"MEDICO":"PACIENTE",conexion);
                if(medico) profesional=propio;
                if(fecha.Date<Ahora().Date || fecha.Date>Ahora().Date.AddDays(180))
                    throw new ArgumentException("Elegi una fecha de hoy a los proximos 180 dias.");
                DataTable franjas=Franjas(profesional,fecha,conexion,null);
                DataTable ocupados=Tabla(@"SELECT T.FechaHora,T.DuracionMinutos FROM dbo.Turno T
                    JOIN dbo.EstadoTurno E ON E.IdEstadoTurno=T.IdEstadoTurno
                    WHERE (T.IdProfesional=@Id OR T.IdPaciente=@Paciente) AND E.Descripcion<>'Cancelado'
                        AND DATEADD(MINUTE,T.DuracionMinutos,T.FechaHora)>@Fecha
                        AND T.FechaHora<DATEADD(DAY,1,@Fecha);",conexion,null,
                    P("@Id",profesional),P("@Paciente",medico?0:propio),P("@Fecha",fecha.Date));
                DataTable libres=new DataTable(); libres.Columns.Add("FechaHora",typeof(DateTime)); libres.Columns.Add("Horario");
                foreach(DataRow franja in franjas.Rows)
                {
                    int duracion=Convert.ToInt32(franja["DuracionMinutos"]);
                    DateTime fin=fecha.Date.Add((TimeSpan)franja["HoraFin"]);
                    for(DateTime inicio=fecha.Date.Add((TimeSpan)franja["HoraInicio"]);inicio.AddMinutes(duracion)<=fin;inicio=inicio.AddMinutes(duracion))
                    {
                        if(inicio<=Ahora() || libres.AsEnumerable().Any(r=>r.Field<DateTime>("FechaHora")==inicio)) continue;
                        if(ocupados.AsEnumerable().Any(r=>inicio<r.Field<DateTime>("FechaHora").AddMinutes(r.Field<int>("DuracionMinutos")) && r.Field<DateTime>("FechaHora")<inicio.AddMinutes(duracion))) continue;
                        libres.Rows.Add(inicio,inicio.ToString("HH:mm")+" ("+duracion+" min)");
                    }
                }
                return libres;
            }
        }

        public int Reservar(int usuario, bool medico, int profesional, int paciente, DateTime fecha, string motivo)
        {
            motivo=Texto(motivo,300,false);
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open();
                using(SqlTransaction transaccion=conexion.BeginTransaction())
                {
                    int propio=Usuario(usuario,medico?"MEDICO":"PACIENTE",conexion,transaccion);
                    if(medico) profesional=propio; else paciente=propio;
                    BloquearProfesional(profesional,conexion,transaccion);
                    // Bloquea tambien al paciente para impedir reservas simultaneas con dos medicos.
                    using(SqlCommand comando=Comando(@"SELECT P.IdPaciente FROM dbo.Paciente P WITH (UPDLOCK,HOLDLOCK)
                        JOIN dbo.Estado E ON E.IdEstado=P.IdEstado WHERE P.IdPaciente=@Id AND E.Descripcion='Activo';",
                        conexion,transaccion,P("@Id",paciente)))
                        if(comando.ExecuteScalar()==null) throw new InvalidOperationException("No existe un paciente activo seleccionado.");
                    if(fecha<=Ahora() || fecha>Ahora().Date.AddDays(181)) throw new ArgumentException("El turno debe ser futuro, dentro de los proximos 180 dias.");
                    int duracion=0;
                    foreach(DataRow franja in Franjas(profesional,fecha,conexion,transaccion).Rows)
                    {
                        int minutos=Convert.ToInt32(franja["DuracionMinutos"]);
                        double diferencia=(fecha.TimeOfDay-(TimeSpan)franja["HoraInicio"]).TotalMinutes;
                        if(diferencia>=0 && diferencia%minutos==0 && fecha.TimeOfDay.Add(TimeSpan.FromMinutes(minutos))<=(TimeSpan)franja["HoraFin"] && fecha.Second==0 && fecha.Millisecond==0)
                        { duracion=minutos; break; }
                    }
                    if(duracion==0) throw new InvalidOperationException("Ese horario no pertenece a la disponibilidad aprobada.");
                    VerificarCruce(profesional,paciente,fecha,duracion,0,conexion,transaccion);
                    using(SqlCommand comando=Comando(@"INSERT dbo.Turno(FechaHora,DuracionMinutos,Motivo,IdPaciente,IdProfesional,IdEstadoTurno,
                            CoberturaRegistrada,ObraSocialTurno,NumeroAfiliadoTurno,IdUsuarioCreador)
                        OUTPUT INSERTED.IdTurno
                        SELECT @Fecha,@Duracion,@Motivo,P.IdPaciente,@Profesional,
                            (SELECT IdEstadoTurno FROM dbo.EstadoTurno WHERE Descripcion='Pendiente'),1,O.Nombre,P.NumeroObraSocial,@Usuario
                        FROM dbo.Paciente P LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=P.IdObraSocial WHERE P.IdPaciente=@Paciente;",
                        conexion,transaccion,P("@Fecha",fecha),P("@Duracion",duracion),P("@Motivo",motivo),P("@Profesional",profesional),P("@Usuario",usuario),P("@Paciente",paciente)))
                    {
                        int id=Convert.ToInt32(comando.ExecuteScalar()); transaccion.Commit(); return id;
                    }
                }
            }
        }

        // Los formularios administrativos existentes usan el mismo bloqueo y control de cruces.
        internal int GuardarAdministrativo(Turno turno,bool modificar)
        {
            if(turno==null || turno.IdPaciente<=0 || turno.IdProfesional<=0 || turno.DuracionMinutos<0 || turno.DuracionMinutos>240)
                throw new ArgumentException("Revisa los datos del turno.");
            int duracion=turno.DuracionMinutos==0?30:turno.DuracionMinutos;
            string motivo=Texto(turno.Motivo,300,false),obs=Texto(turno.Observaciones,500,false);
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open();
                using(SqlTransaction transaccion=conexion.BeginTransaction())
                {
                    BloquearProfesional(turno.IdProfesional,conexion,transaccion);
                    using(SqlCommand comando=Comando("SELECT IdPaciente FROM dbo.Paciente WITH (UPDLOCK,HOLDLOCK) WHERE IdPaciente=@Id;",
                        conexion,transaccion,P("@Id",turno.IdPaciente)))
                        if(comando.ExecuteScalar()==null) throw new InvalidOperationException("No existe el paciente.");
                    int estado=turno.IdEstadoTurno==0?1:turno.IdEstadoTurno;
                    using(SqlCommand comando=Comando("SELECT Descripcion FROM dbo.EstadoTurno WHERE IdEstadoTurno=@Id;",conexion,transaccion,P("@Id",estado)))
                        if(Convert.ToString(comando.ExecuteScalar())!="Cancelado")
                            VerificarCruce(turno.IdProfesional,turno.IdPaciente,turno.FechaHora,duracion,modificar?turno.IdTurno:0,conexion,transaccion);
                    string sql=modificar ? @"UPDATE T SET FechaHora=@Fecha,DuracionMinutos=@Duracion,Motivo=@Motivo,
                        Observaciones=@Obs,IdPaciente=@Paciente,IdProfesional=@Profesional,IdEstadoTurno=@Estado,
                        CoberturaRegistrada=CASE WHEN T.IdPaciente<>@Paciente THEN 1 ELSE T.CoberturaRegistrada END,
                        ObraSocialTurno=CASE WHEN T.IdPaciente<>@Paciente THEN O.Nombre ELSE T.ObraSocialTurno END,
                        NumeroAfiliadoTurno=CASE WHEN T.IdPaciente<>@Paciente THEN P.NumeroObraSocial ELSE T.NumeroAfiliadoTurno END
                        FROM dbo.Turno T JOIN dbo.Paciente P ON P.IdPaciente=@Paciente
                        LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=P.IdObraSocial WHERE T.IdTurno=@Id; SELECT @Id;"
                        : @"INSERT dbo.Turno(FechaHora,DuracionMinutos,Motivo,Observaciones,IdPaciente,IdProfesional,IdEstadoTurno,
                                CoberturaRegistrada,ObraSocialTurno,NumeroAfiliadoTurno)
                            OUTPUT INSERTED.IdTurno SELECT @Fecha,@Duracion,@Motivo,@Obs,@Paciente,@Profesional,@Estado,1,O.Nombre,P.NumeroObraSocial
                            FROM dbo.Paciente P LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=P.IdObraSocial WHERE P.IdPaciente=@Paciente;";
                    using(SqlCommand comando=Comando(sql,conexion,transaccion,P("@Fecha",turno.FechaHora),P("@Duracion",duracion),P("@Motivo",motivo),P("@Obs",obs),
                        P("@Paciente",turno.IdPaciente),P("@Profesional",turno.IdProfesional),P("@Estado",estado),P("@Id",turno.IdTurno)))
                    {
                        int id=Convert.ToInt32(comando.ExecuteScalar()); transaccion.Commit(); return id;
                    }
                }
            }
        }

        internal void VerificarCruce(int profesional,int paciente,DateTime fecha,int duracion,int excluir,
            SqlConnection conexion,SqlTransaction transaccion)
        {
            using(SqlCommand comando=Comando(@"SELECT COUNT(*) FROM dbo.Turno T WITH (UPDLOCK,HOLDLOCK)
                JOIN dbo.EstadoTurno E ON E.IdEstadoTurno=T.IdEstadoTurno
                WHERE (T.IdProfesional=@Profesional OR T.IdPaciente=@Paciente) AND T.IdTurno<>@Excluir
                    AND E.Descripcion<>'Cancelado' AND T.FechaHora<@Fin
                    AND @Fecha<DATEADD(MINUTE,T.DuracionMinutos,T.FechaHora);",conexion,transaccion,
                P("@Profesional",profesional),P("@Paciente",paciente),P("@Excluir",excluir),P("@Fecha",fecha),P("@Fin",fecha.AddMinutes(duracion))))
                if(Convert.ToInt32(comando.ExecuteScalar())>0) throw new InvalidOperationException("El profesional o el paciente ya tienen un turno en ese horario. Actualiza los horarios disponibles.");
        }

        public DataTable BuscarTurnoMedico(int usuario,int turno)
        {
            if(turno<=0) throw new ArgumentException("Selecciona un turno valido desde tu agenda.");
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open(); int profesional=Usuario(usuario,"MEDICO",conexion);
                return Tabla(@"SELECT T.IdTurno,T.FechaHora,T.DuracionMinutos,T.Motivo,T.Observaciones,
                    E.Descripcion AS Estado,P.IdPaciente,P.Nombre,P.Apellido,P.Telefono,
                    COALESCE(O.Nombre,'Sin obra social') AS CoberturaActual,
                    CASE WHEN T.CoberturaRegistrada=1 THEN COALESCE(T.ObraSocialTurno,'Sin obra social')
                        ELSE COALESCE(O.Nombre,'Sin obra social') END AS ObraSocial,
                    CASE WHEN T.CoberturaRegistrada=1 THEN T.NumeroAfiliadoTurno ELSE P.NumeroObraSocial END AS Afiliado,
                    CASE WHEN T.CoberturaRegistrada=1 THEN 'Al reservar' ELSE 'Cobertura actual' END AS TipoCobertura
                    FROM dbo.Turno T JOIN dbo.Paciente P ON P.IdPaciente=T.IdPaciente
                    JOIN dbo.EstadoTurno E ON E.IdEstadoTurno=T.IdEstadoTurno
                    LEFT JOIN dbo.ObraSocial O ON O.IdObraSocial=P.IdObraSocial
                    WHERE T.IdTurno=@Turno AND T.IdProfesional=@Profesional;",conexion,null,
                    P("@Turno",turno),P("@Profesional",profesional));
            }
        }

        public void RegistrarAtencion(int usuario,int turno,string observacion)
        {
            RegistrarResultadoAtencion(usuario,turno,observacion,true);
        }

        public void RegistrarResultadoAtencion(int usuario,int turno,string observacion,bool realizado)
        {
            observacion=Texto(observacion,500,true);
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open();
                using(SqlTransaction transaccion=conexion.BeginTransaction())
                {
                    int profesional=Usuario(usuario,"MEDICO",conexion,transaccion);
                    object anterior;
                    int estadoAnterior;
                    string resultado=realizado?"Atendido":"Ausente";
                    using(SqlCommand comando=Comando(@"UPDATE T SET Observaciones=@Obs,
                            IdEstadoTurno=(SELECT IdEstadoTurno FROM dbo.EstadoTurno WHERE Descripcion=@Resultado),
                            IdUsuarioAtencion=@Usuario,FechaAtencionUTC=SYSUTCDATETIME()
                        OUTPUT DELETED.Observaciones,DELETED.IdEstadoTurno
                        FROM dbo.Turno T JOIN dbo.EstadoTurno E ON E.IdEstadoTurno=T.IdEstadoTurno
                        WHERE T.IdTurno=@Turno AND T.IdProfesional=@Profesional AND T.FechaHora<=@Ahora
                            AND E.Descripcion IN ('Pendiente','Confirmado');",conexion,transaccion,
                        P("@Obs",observacion),P("@Usuario",usuario),P("@Turno",turno),P("@Profesional",profesional),P("@Ahora",Ahora()),P("@Resultado",resultado)))
                    {
                        using(SqlDataReader lector=comando.ExecuteReader())
                        {
                            if(!lector.Read()) throw new InvalidOperationException("El turno debe ser propio, haber comenzado y estar pendiente o confirmado. Si ya fue cerrado, el resultado y las observaciones son de solo lectura en el historial.");
                            anterior=lector.GetValue(0);
                            estadoAnterior=lector.GetInt32(1);
                        }
                    }
                    using(SqlCommand comando=Comando(@"INSERT dbo.HistorialAtencion
                        (IdTurno,IdUsuarioMedico,ObservacionAnterior,ObservacionNueva,IdEstadoAnterior,IdEstadoNuevo)
                        SELECT @Turno,@Usuario,@Anterior,@Nueva,@EstadoAnterior,IdEstadoTurno
                        FROM dbo.EstadoTurno WHERE Descripcion=@Resultado;",conexion,transaccion,
                        P("@Turno",turno),P("@Usuario",usuario),P("@Anterior",anterior),P("@Nueva",observacion),
                        P("@EstadoAnterior",estadoAnterior),P("@Resultado",resultado)))
                        comando.ExecuteNonQuery();
                    transaccion.Commit();
                }
            }
        }

        private string Texto(string texto,int limite,bool obligatorio)
        {
            texto=string.IsNullOrWhiteSpace(texto)?null:texto.Trim();
            if((obligatorio && texto==null) || (texto!=null && texto.Length>limite))
                throw new ArgumentException("Completa el texto requerido, con hasta "+limite+" caracteres.");
            return texto;
        }

        public int SolicitarHorario(int usuario,Agenda propuesta,bool baja)
        {
            if(propuesta==null || propuesta.DiaSemana<1 || propuesta.DiaSemana>7 || propuesta.HoraInicio<TimeSpan.Zero || propuesta.HoraFin>=TimeSpan.FromDays(1)
                || propuesta.HoraFin<=propuesta.HoraInicio || propuesta.DuracionMinutos<5 || propuesta.DuracionMinutos>240
                || (propuesta.HoraFin-propuesta.HoraInicio).TotalMinutes<propuesta.DuracionMinutos || (baja && propuesta.IdAgenda<=0))
                throw new ArgumentException("Revisa el dia, horario y duracion (5 a 240 minutos).");
            try
            {
                using(SqlConnection conexion=Conexion())
                {
                    conexion.Open();
                    using(SqlTransaction transaccion=conexion.BeginTransaction())
                    {
                        int profesional=Usuario(usuario,"MEDICO",conexion,transaccion);
                        BloquearProfesional(profesional,conexion,transaccion);
                        object version=DBNull.Value;
                        if(propuesta.IdAgenda>0)
                        {
                            using(SqlCommand comando=Comando(@"SELECT A.VersionAgenda FROM dbo.Agenda A
                                JOIN dbo.Estado E ON E.IdEstado=A.IdEstado
                                WHERE A.IdAgenda=@Agenda AND A.IdProfesional=@Profesional AND E.Descripcion='Activo';",conexion,transaccion,
                                P("@Agenda",propuesta.IdAgenda),P("@Profesional",profesional)))
                                version=comando.ExecuteScalar();
                            if(version==null) throw new InvalidOperationException("Ese horario no pertenece a tu agenda activa.");
                        }
                        int solicitud;
                        using(SqlCommand comando=Comando(@"INSERT dbo.SolicitudModificacion(IdUsuarioSolicitante,IdProfesional)
                            OUTPUT INSERTED.IdSolicitud VALUES (@Usuario,@Profesional);",conexion,transaccion,P("@Usuario",usuario),P("@Profesional",profesional)))
                            solicitud=Convert.ToInt32(comando.ExecuteScalar());
                        using(SqlCommand comando=Comando(@"INSERT dbo.SolicitudModificacionAgenda
                            (IdSolicitud,IdAgenda,VersionAgenda,DiaSemana,HoraInicio,HoraFin,DuracionMinutos,DarDeBaja)
                            VALUES(@Solicitud,@Agenda,@Version,@Dia,@Inicio,@Fin,@Duracion,@Baja);",conexion,transaccion,
                            P("@Solicitud",solicitud),P("@Agenda",propuesta.IdAgenda==0?(object)DBNull.Value:propuesta.IdAgenda),
                            new SqlParameter("@Version",SqlDbType.VarBinary,8){Value=version},P("@Dia",propuesta.DiaSemana),P("@Inicio",propuesta.HoraInicio),P("@Fin",propuesta.HoraFin),
                            P("@Duracion",propuesta.DuracionMinutos),P("@Baja",baja))) comando.ExecuteNonQuery();
                        transaccion.Commit(); return solicitud;
                    }
                }
            }
            catch(SqlException ex)
            {
                if(ex.Number==2601 || ex.Number==2627) throw new InvalidOperationException("Ya tenes una solicitud pendiente de datos u horario. Espera su resolucion.",ex);
                throw;
            }
        }

        public DataTable SolicitudesHorarios(int usuario,bool admin)
        {
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open(); Usuario(usuario,admin?"ADMIN":"MEDICO",conexion);
                return Tabla(@"SELECT S.IdSolicitud,S.IdProfesional,P.Apellido+', '+P.Nombre AS Profesional,
                    S.FechaSolicitud,E.Descripcion AS Estado,S.ObservacionRevision,D.IdAgenda,
                    D.DiaSemana,CONVERT(VARCHAR(5),D.HoraInicio,108)+' a '+CONVERT(VARCHAR(5),D.HoraFin,108) AS Horario,
                    D.DuracionMinutos,CASE WHEN D.DarDeBaja=1 THEN 'Baja' WHEN D.IdAgenda IS NULL THEN 'Alta' ELSE 'Modificacion' END AS Operacion,
                    CONVERT(VARCHAR(5),A.HoraInicio,108)+' a '+CONVERT(VARCHAR(5),A.HoraFin,108) AS HorarioActual,
                    A.DiaSemana AS DiaActual,A.DuracionMinutos AS DuracionActual
                    FROM dbo.SolicitudModificacion S JOIN dbo.SolicitudModificacionAgenda D ON D.IdSolicitud=S.IdSolicitud
                    JOIN dbo.EstadoSolicitud E ON E.IdEstadoSolicitud=S.IdEstadoSolicitud
                    JOIN dbo.Profesional P ON P.IdProfesional=S.IdProfesional LEFT JOIN dbo.Agenda A ON A.IdAgenda=D.IdAgenda
                    WHERE (@Admin=1 AND S.IdEstadoSolicitud=1) OR (@Admin=0 AND S.IdUsuarioSolicitante=@Usuario AND P.IdUsuario=@Usuario)
                    ORDER BY S.IdSolicitud DESC;",conexion,null,P("@Admin",admin),P("@Usuario",usuario));
            }
        }

        public void ResolverHorario(int usuario,int solicitud,bool aprobar,string motivo)
        {
            motivo=Texto(motivo,500,!aprobar);
            using(SqlConnection conexion=Conexion())
            {
                conexion.Open();
                using(SqlTransaction transaccion=conexion.BeginTransaction())
                {
                    Usuario(usuario,"ADMIN",conexion,transaccion);
                    DataTable datos=Tabla(@"SELECT S.IdProfesional,S.IdUsuarioSolicitante,D.* FROM dbo.SolicitudModificacion S WITH (UPDLOCK,HOLDLOCK)
                        JOIN dbo.SolicitudModificacionAgenda D ON D.IdSolicitud=S.IdSolicitud
                        WHERE S.IdSolicitud=@Solicitud AND S.IdEstadoSolicitud=1 AND S.IdUsuarioSolicitante<>@Usuario;",
                        conexion,transaccion,P("@Solicitud",solicitud),P("@Usuario",usuario));
                    if(datos.Rows.Count!=1) throw new InvalidOperationException("La solicitud no existe, ya fue resuelta o es propia.");
                    DataRow dato=datos.Rows[0]; int profesional=Convert.ToInt32(dato["IdProfesional"]);
                    if(aprobar)
                    {
                        if(Usuario(Convert.ToInt32(dato["IdUsuarioSolicitante"]),"MEDICO",conexion,transaccion)!=profesional)
                            throw new InvalidOperationException("El profesional vinculado cambio.");
                        BloquearProfesional(profesional,conexion,transaccion);
                        int agenda=dato["IdAgenda"]==DBNull.Value?0:Convert.ToInt32(dato["IdAgenda"]);
                        bool baja=Convert.ToBoolean(dato["DarDeBaja"]);
                        DataTable anterior=Tabla("SELECT * FROM dbo.Agenda WHERE IdAgenda=@Agenda AND IdProfesional=@Profesional;",
                            conexion,transaccion,P("@Agenda",agenda),P("@Profesional",profesional));
                        if(!baja)
                        {
                            DataTable cruces=Tabla(@"SELECT A.IdAgenda FROM dbo.Agenda A JOIN dbo.Estado E ON E.IdEstado=A.IdEstado
                                WHERE A.IdProfesional=@Profesional AND A.IdAgenda<>@Agenda AND E.Descripcion='Activo'
                                    AND A.DiaSemana=@Dia AND A.HoraInicio<@Fin AND @Inicio<A.HoraFin;",conexion,transaccion,
                                P("@Profesional",profesional),P("@Agenda",agenda),P("@Dia",dato["DiaSemana"]),P("@Inicio",dato["HoraInicio"]),P("@Fin",dato["HoraFin"]));
                            if(cruces.Rows.Count>0) throw new InvalidOperationException("El horario propuesto se superpone con otra franja activa.");
                        }
                        string sql=agenda==0 ? @"INSERT dbo.Agenda(IdProfesional,DiaSemana,HoraInicio,HoraFin,DuracionMinutos,AtencionConTurno,IdEstado)
                            VALUES(@Profesional,@Dia,@Inicio,@Fin,@Duracion,1,@Estado);" : @"UPDATE dbo.Agenda
                            SET DiaSemana=@Dia,HoraInicio=@Inicio,HoraFin=@Fin,DuracionMinutos=@Duracion,AtencionConTurno=1,IdEstado=@Estado
                            WHERE IdAgenda=@Agenda AND IdProfesional=@Profesional AND VersionAgenda=@Version;";
                        using(SqlCommand comando=Comando(sql,conexion,transaccion,P("@Profesional",profesional),P("@Agenda",agenda),P("@Dia",dato["DiaSemana"]),
                            P("@Inicio",dato["HoraInicio"]),P("@Fin",dato["HoraFin"]),P("@Duracion",dato["DuracionMinutos"]),
                            P("@Estado",baja?2:1),new SqlParameter("@Version",SqlDbType.VarBinary,8){Value=dato["VersionAgenda"]}))
                            if(comando.ExecuteNonQuery()!=1) throw new InvalidOperationException("La agenda cambio desde el envio. Rechaza la solicitud y pedi una nueva.");
                        if(anterior.Rows.Count==1) VerificarTurnosVigentes(profesional,anterior.Rows[0],conexion,transaccion);
                    }
                    using(SqlCommand comando=Comando(@"UPDATE dbo.SolicitudModificacion SET IdEstadoSolicitud=@Estado,
                        IdUsuarioRevisor=@Usuario,FechaResolucion=SYSUTCDATETIME(),ObservacionRevision=@Motivo
                        WHERE IdSolicitud=@Solicitud;",conexion,transaccion,P("@Estado",aprobar?2:3),P("@Usuario",usuario),P("@Motivo",motivo),P("@Solicitud",solicitud)))
                        comando.ExecuteNonQuery();
                    transaccion.Commit();
                }
            }
        }

        private void VerificarTurnosVigentes(int profesional,DataRow anterior,SqlConnection conexion,SqlTransaction transaccion)
        {
            // Solo considera los turnos cubiertos por la franja que se modifica.
            using(SqlCommand comando=Comando(@"SELECT COUNT(*) FROM dbo.Turno T JOIN dbo.EstadoTurno ET ON ET.IdEstadoTurno=T.IdEstadoTurno
                WHERE T.IdProfesional=@Profesional AND T.FechaHora>@Ahora AND ET.Descripcion IN ('Pendiente','Confirmado')
                    AND (DATEDIFF(DAY,'19000101',CAST(T.FechaHora AS DATE))%7)+1=@Dia
                    AND CAST(T.FechaHora AS TIME)>=@Inicio AND CAST(DATEADD(MINUTE,T.DuracionMinutos,T.FechaHora) AS TIME)<=@Fin
                    AND NOT EXISTS(SELECT 1 FROM dbo.Agenda A JOIN dbo.Estado E ON E.IdEstado=A.IdEstado
                        WHERE A.IdProfesional=T.IdProfesional AND E.Descripcion='Activo' AND A.AtencionConTurno=1
                            AND A.DiaSemana=(DATEDIFF(DAY,'19000101',CAST(T.FechaHora AS DATE))%7)+1
                            AND A.HoraInicio<=CAST(T.FechaHora AS TIME) AND A.HoraFin>=CAST(DATEADD(MINUTE,T.DuracionMinutos,T.FechaHora) AS TIME));",
                conexion,transaccion,P("@Profesional",profesional),P("@Ahora",Ahora()),P("@Dia",anterior["DiaSemana"]),P("@Inicio",anterior["HoraInicio"]),P("@Fin",anterior["HoraFin"])))
                if(Convert.ToInt32(comando.ExecuteScalar())>0)
                    throw new InvalidOperationException("El cambio dejaria turnos futuros fuera de la agenda. Reprogramalos antes de aprobar.");
        }
    }
}
