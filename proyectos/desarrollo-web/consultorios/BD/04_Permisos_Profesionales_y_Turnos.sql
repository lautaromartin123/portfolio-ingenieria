/* ============================================================
   TP CONSULTORIO - Grupo 05
   Base: db69590 (hosting compartido)

   PERMISOS DE LAS PANTALLAS DE PROFESIONALES Y TURNOS
   Autor: Jaremczuk

   Contexto:
   El login migro de roles en sesion a un sistema de niveles con
   permisos por URL (tablas Nivel, URL y Nivel_URL). Las pantallas
   de Profesionales y Turnos son anteriores a ese cambio, asi que
   nunca se dieron de alta ahi.

   La aplicacion registra sola una URL la primera vez que alguien
   la visita (UsuarioDAO.RegistrarUrlSiNoExiste), pero AltaProfesional,
   Matriculas y AltaTurno solo se alcanzan desde adentro de los
   listados, que sin permiso rebotan al login. Este script corta ese
   circulo dando de alta las seis de una vez.

   Despues de ejecutarlo, los permisos se pueden ajustar desde
   GestionPermisos.aspx sin volver a tocar SQL.

   Se puede ejecutar varias veces sin duplicar nada.
   ============================================================ */


/* ---------- 1. Dar de alta las paginas en URL ---------- */

DECLARE @Paginas TABLE (Pagina VARCHAR(200));

INSERT INTO @Paginas (Pagina) VALUES
    ('ListadoProfesionales.aspx'),
    ('AltaProfesional.aspx'),
    ('Matriculas.aspx'),
    ('ListadoTurnos.aspx'),
    ('AltaTurno.aspx'),
    ('AgendaProfesional.aspx');

INSERT INTO dbo.URL (Descripcion)
SELECT P.Pagina
FROM @Paginas P
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.URL U
    WHERE U.Descripcion = P.Pagina
);


/* ---------- 2. Dar acceso al nivel ADMIN ----------
   Nivel_URL necesita IdEstado = Activo: TieneAcceso filtra por
   Estado.Descripcion = 'Activo', no le alcanza con que exista la fila.
   ------------------------------------------------------------ */

DECLARE @IdNivelAdmin INT =
    (SELECT IdNivel FROM dbo.Nivel WHERE Descripcion = 'ADMIN');

DECLARE @IdEstadoActivo INT =
    (SELECT IdEstado FROM dbo.Estado WHERE Descripcion = 'Activo');

INSERT INTO dbo.Nivel_URL (IdNivel, IdUrl, IdEstado)
SELECT @IdNivelAdmin, U.IdUrl, @IdEstadoActivo
FROM dbo.URL U
INNER JOIN @Paginas P
    ON P.Pagina = U.Descripcion
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Nivel_URL NU
    WHERE NU.IdNivel = @IdNivelAdmin
      AND NU.IdUrl   = U.IdUrl
);


/* ============================================================
   NOTA: el nivel MEDICO no recibe acceso en este script.
   ListadoTurnos.aspx podria servirle para ver su agenda del dia,
   pero eso es una decision de disenio del grupo y conviene
   tomarla desde GestionPermisos.aspx, no aca.
   ============================================================ */
