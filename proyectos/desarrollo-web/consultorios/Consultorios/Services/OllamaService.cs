using BIZ;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Consultorios
{
    public class OllamaService
    {
        private static readonly HttpClient httpClient =
            new HttpClient
            {
                Timeout =
                    TimeSpan.FromMinutes(2)
            };


        // =====================================================
        // REPORTE AUTOMÁTICO
        // =====================================================

        public async Task<string> GenerarReporteAsync(
            string datos)
        {
            string instrucciones =
                "Sos un asistente de análisis operativo "
                +
                "de un consultorio médico. "
                +
                "Los datos que recibís fueron calculados "
                +
                "por el sistema y son la fuente de verdad. "
                +
                "Usalos exactamente como están. "
                +
                "El valor 0 es un dato válido y NO significa "
                +
                "que falte información. "
                +
                "No inventes fechas ni cantidades. "
                +
                "No realices diagnósticos médicos. "
                +
                "No sugieras medicamentos ni tratamientos. "
                +
                "Respondé en español de forma clara.";


            return await EnviarAOllamaAsync(
                instrucciones,
                datos);
        }


        // =====================================================
        // REDACTAR RESPUESTA USANDO RESULTADOS REALES
        // =====================================================

        public async Task<string> ResponderPreguntaAsync(
            string contexto,
            string historial,
            string pregunta)
        {
            string instrucciones =
                "Sos un asistente operativo para médicos. "
                +
                "El sistema ya consultó SQL Server y realizó "
                +
                "los cálculos necesarios. "
                +
                "Los datos proporcionados son la fuente "
                +
                "de verdad. "
                +
                "No recalcules fechas. "
                +
                "No contradigas los resultados del sistema. "
                +
                "El número 0 ES INFORMACIÓN VÁLIDA. "
                +
                "Si el sistema indica 0 turnos, respondé "
                +
                "que existen 0 turnos. "
                +
                "Nunca digas que falta información cuando "
                +
                "el sistema proporcionó un valor de 0. "
                +
                "No inventes cantidades. "
                +
                "No realices diagnósticos médicos. "
                +
                "No recomiendes medicamentos ni tratamientos. "
                +
                "Respondé directamente la pregunta. "
                +
                "No hagas preguntas de seguimiento si "
                +
                "los datos ya permiten responder.";


            StringBuilder contenido =
                new StringBuilder();


            contenido.AppendLine(
                "RESULTADOS CALCULADOS POR EL SISTEMA:");

            contenido.AppendLine();

            contenido.AppendLine(
                contexto);

            contenido.AppendLine();


            if (!string.IsNullOrWhiteSpace(
                historial))
            {
                contenido.AppendLine(
                    "CONVERSACIÓN ANTERIOR:");

                contenido.AppendLine();

                contenido.AppendLine(
                    historial);

                contenido.AppendLine();
            }


            contenido.AppendLine(
                "PREGUNTA ACTUAL:");

            contenido.AppendLine(
                pregunta);

            contenido.AppendLine();

            contenido.AppendLine(
                "Respondé usando los resultados calculados "
                +
                "por el sistema.");

            contenido.AppendLine(
                "Si un resultado es 0, informá 0.");

            contenido.AppendLine(
                "No reemplaces un valor 0 por frases como "
                +
                "'no hay información suficiente'.");


            return await EnviarAOllamaAsync(
                instrucciones,
                contenido.ToString());
        }


        // =====================================================
        // INTERPRETAR PREGUNTA
        // =====================================================

        public async Task<ConsultaIA> InterpretarPreguntaAsync(
            string pregunta,
            string historial)
        {
            if (string.IsNullOrWhiteSpace(
                pregunta))
            {
                throw new Exception(
                    "La pregunta está vacía.");
            }


            string instrucciones =
                "Sos un clasificador de intenciones para "
                +
                "un sistema de gestión médica. "
                +
                "NO respondas la pregunta. "
                +
                "NO calcules estadísticas. "
                +
                "NO calcules fechas relativas. "
                +
                "Solamente identificá la acción y el "
                +
                "tipo de período solicitado. "
                +
                "Respondé exclusivamente con JSON válido.";


            StringBuilder contenido =
                new StringBuilder();


            // =================================================
            // ACCIONES
            // =================================================

            contenido.AppendLine(
                "ACCIONES PERMITIDAS:");

            contenido.AppendLine(
                "AGENDA_PERIODO");

            contenido.AppendLine(
                "RESUMEN_PERIODO");

            contenido.AppendLine(
                "COMPARAR_PERIODOS");

            contenido.AppendLine(
                "DIA_MAS_CARGADO");

            contenido.AppendLine(
                "DIA_MAS_LIBRE");

            contenido.AppendLine(
                "HORARIOS");

            contenido.AppendLine(
                "ATENDIDOS");

            contenido.AppendLine(
                "ESTADOS_TURNOS");

            contenido.AppendLine(
                "DETECTAR_ALERTAS");

            contenido.AppendLine(
                "RESUMEN_GENERAL");

            contenido.AppendLine(
                "NO_SOPORTADO");

            contenido.AppendLine();


            // =================================================
            // PERÍODOS
            // =================================================

            contenido.AppendLine(
                "PERIODOS PERMITIDOS:");

            contenido.AppendLine(
                "HOY");

            contenido.AppendLine(
                "MANANA");

            contenido.AppendLine(
                "AYER");

            contenido.AppendLine(
                "ESTA_SEMANA");

            contenido.AppendLine(
                "PROXIMA_SEMANA");

            contenido.AppendLine(
                "SEMANA_PASADA");

            contenido.AppendLine(
                "ESTE_MES");

            contenido.AppendLine(
                "PROXIMO_MES");

            contenido.AppendLine(
                "MES_PASADO");

            contenido.AppendLine(
                "FECHA_EXPLICITA");

            contenido.AppendLine(
                "RANGO_EXPLICITO");

            contenido.AppendLine(
                "SIN_PERIODO");

            contenido.AppendLine();


            // =================================================
            // REGLA FUNDAMENTAL
            // =================================================

            contenido.AppendLine(
                "REGLA MUY IMPORTANTE:");

            contenido.AppendLine(
                "NO conviertas expresiones relativas "
                +
                "a fechas concretas.");

            contenido.AppendLine();

            contenido.AppendLine(
                "Si dice 'mañana', devolvé:");

            contenido.AppendLine(
                "\"Periodo\": \"MANANA\"");

            contenido.AppendLine();

            contenido.AppendLine(
                "Si dice 'esta semana', devolvé:");

            contenido.AppendLine(
                "\"Periodo\": \"ESTA_SEMANA\"");

            contenido.AppendLine();

            contenido.AppendLine(
                "Si dice 'la próxima semana', devolvé:");

            contenido.AppendLine(
                "\"Periodo\": \"PROXIMA_SEMANA\"");

            contenido.AppendLine();

            contenido.AppendLine(
                "Si dice 'este mes', devolvé:");

            contenido.AppendLine(
                "\"Periodo\": \"ESTE_MES\"");

            contenido.AppendLine();

            contenido.AppendLine(
                "C# calculará las fechas reales.");

            contenido.AppendLine();


            // =================================================
            // FECHAS EXPLÍCITAS
            // =================================================

            contenido.AppendLine(
                "Solamente completá Desde/Hasta cuando "
                +
                "el médico escriba una fecha concreta.");

            contenido.AppendLine();

            contenido.AppendLine(
                "Ejemplo:");

            contenido.AppendLine(
                "¿Qué tengo el 3 de noviembre de 2026?");

            contenido.AppendLine();

            contenido.AppendLine(
                "Periodo = FECHA_EXPLICITA");

            contenido.AppendLine(
                "Desde = 2026-11-03");

            contenido.AppendLine(
                "Hasta = 2026-11-03");

            contenido.AppendLine();


            contenido.AppendLine(
                "Ejemplo:");

            contenido.AppendLine(
                "¿Qué tengo del 3 al 10 de noviembre "
                +
                "de 2026?");

            contenido.AppendLine();

            contenido.AppendLine(
                "Periodo = RANGO_EXPLICITO");

            contenido.AppendLine(
                "Desde = 2026-11-03");

            contenido.AppendLine(
                "Hasta = 2026-11-10");

            contenido.AppendLine();


            // =================================================
            // EJEMPLOS DE ACCIÓN
            // =================================================

            contenido.AppendLine(
                "EJEMPLOS:");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Qué tengo mañana?");

            contenido.AppendLine(
                "Accion = AGENDA_PERIODO");

            contenido.AppendLine(
                "Periodo = MANANA");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Cuántos turnos tengo "
                +
                "esta semana?");

            contenido.AppendLine(
                "Accion = RESUMEN_PERIODO");

            contenido.AppendLine(
                "Periodo = ESTA_SEMANA");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Cuántos pacientes atendí "
                +
                "este mes?");

            contenido.AppendLine(
                "Accion = ATENDIDOS");

            contenido.AppendLine(
                "Periodo = ESTE_MES");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Cómo están distribuidos "
                +
                "los estados de mis turnos este mes?");

            contenido.AppendLine(
                "Accion = ESTADOS_TURNOS");

            contenido.AppendLine(
                "Periodo = ESTE_MES");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Detectás algo llamativo "
                +
                "en mi agenda la próxima semana?");

            contenido.AppendLine(
                "Accion = DETECTAR_ALERTAS");

            contenido.AppendLine(
                "Periodo = PROXIMA_SEMANA");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: Comparame esta semana "
                +
                "con la próxima.");

            contenido.AppendLine(
                "Accion = COMPARAR_PERIODOS");

            contenido.AppendLine(
                "Periodo = ESTA_SEMANA");

            contenido.AppendLine(
                "Periodo2 = PROXIMA_SEMANA");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Qué días y horarios trabajo?");

            contenido.AppendLine(
                "Accion = HORARIOS");

            contenido.AppendLine(
                "Periodo = SIN_PERIODO");


            contenido.AppendLine();

            contenido.AppendLine(
                "Pregunta: ¿Qué medicamento debería "
                +
                "recetar?");

            contenido.AppendLine(
                "Accion = NO_SOPORTADO");

            contenido.AppendLine(
                "Periodo = SIN_PERIODO");


            // =================================================
            // HISTORIAL
            // =================================================

            if (!string.IsNullOrWhiteSpace(
                historial))
            {
                contenido.AppendLine();

                contenido.AppendLine(
                    "CONTEXTO DE CONVERSACIÓN:");

                contenido.AppendLine(
                    historial);
            }


            contenido.AppendLine();

            contenido.AppendLine(
                "PREGUNTA ACTUAL:");

            contenido.AppendLine(
                pregunta);

            contenido.AppendLine();


            // =================================================
            // FORMATO JSON
            // =================================================

            contenido.AppendLine(
                "RESPONDÉ EXACTAMENTE CON ESTA ESTRUCTURA:");

            contenido.AppendLine(
                "{");

            contenido.AppendLine(
                "  \"Accion\": \"\",");

            contenido.AppendLine(
                "  \"Periodo\": \"\",");

            contenido.AppendLine(
                "  \"Periodo2\": \"\",");

            contenido.AppendLine(
                "  \"Desde\": \"\",");

            contenido.AppendLine(
                "  \"Hasta\": \"\",");

            contenido.AppendLine(
                "  \"Desde2\": \"\",");

            contenido.AppendLine(
                "  \"Hasta2\": \"\"");

            contenido.AppendLine(
                "}");


            string json =
                await EnviarJsonAOllamaAsync(
                    instrucciones,
                    contenido.ToString());


            JavaScriptSerializer serializer =
                new JavaScriptSerializer();


            ConsultaIA consulta;


            try
            {
                consulta =
                    serializer.Deserialize<ConsultaIA>(
                        LimpiarJson(
                            json));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Ollama no devolvió una "
                    +
                    "consulta estructurada válida. "
                    +
                    ex.Message);
            }


            if (consulta == null)
            {
                throw new Exception(
                    "No se pudo interpretar "
                    +
                    "la consulta.");
            }


            consulta.PreguntaOriginal =
                pregunta;


            consulta.Accion =
                Normalizar(
                    consulta.Accion);


            consulta.Periodo =
                Normalizar(
                    consulta.Periodo);


            consulta.Periodo2 =
                Normalizar(
                    consulta.Periodo2);


            if (!EsAccionPermitida(
                consulta.Accion))
            {
                consulta.Accion =
                    "NO_SOPORTADO";
            }


            if (!EsPeriodoPermitido(
                consulta.Periodo))
            {
                consulta.Periodo =
                    "SIN_PERIODO";
            }


            if (!string.IsNullOrWhiteSpace(
                    consulta.Periodo2)
                &&
                !EsPeriodoPermitido(
                    consulta.Periodo2))
            {
                consulta.Periodo2 =
                    "SIN_PERIODO";
            }


            return consulta;
        }


        // =====================================================
        // NORMALIZAR
        // =====================================================

        private string Normalizar(
            string valor)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                return "";
            }


            return valor
                .Trim()
                .ToUpperInvariant();
        }


        // =====================================================
        // VALIDAR ACCIÓN
        // =====================================================

        private bool EsAccionPermitida(
            string accion)
        {
            switch (accion)
            {
                case "AGENDA_PERIODO":
                case "RESUMEN_PERIODO":
                case "COMPARAR_PERIODOS":
                case "DIA_MAS_CARGADO":
                case "DIA_MAS_LIBRE":
                case "HORARIOS":
                case "ATENDIDOS":
                case "ESTADOS_TURNOS":
                case "DETECTAR_ALERTAS":
                case "RESUMEN_GENERAL":
                case "NO_SOPORTADO":
                    return true;

                default:
                    return false;
            }
        }


        // =====================================================
        // VALIDAR PERÍODO
        // =====================================================

        private bool EsPeriodoPermitido(
            string periodo)
        {
            switch (periodo)
            {
                case "HOY":
                case "MANANA":
                case "AYER":
                case "ESTA_SEMANA":
                case "PROXIMA_SEMANA":
                case "SEMANA_PASADA":
                case "ESTE_MES":
                case "PROXIMO_MES":
                case "MES_PASADO":
                case "FECHA_EXPLICITA":
                case "RANGO_EXPLICITO":
                case "SIN_PERIODO":
                    return true;

                default:
                    return false;
            }
        }


        // =====================================================
        // SOLICITUD NORMAL
        // =====================================================

        private async Task<string> EnviarAOllamaAsync(
            string instrucciones,
            string mensaje)
        {
            string url =
                ConfigurationManager
                    .AppSettings["OllamaUrl"];


            string modelo =
                ConfigurationManager
                    .AppSettings["OllamaModel"];


            ValidarConfiguracion(
                url,
                modelo);


            var solicitud =
                new
                {
                    model = modelo,

                    stream = false,

                    options =
                        new
                        {
                            temperature = 0
                        },

                    messages =
                        new[]
                        {
                            new
                            {
                                role = "system",

                                content =
                                    instrucciones
                            },

                            new
                            {
                                role = "user",

                                content =
                                    mensaje
                            }
                        }
                };


            return await EjecutarSolicitudAsync(
                url,
                solicitud);
        }


        // =====================================================
        // SOLICITUD JSON
        // =====================================================

        private async Task<string> EnviarJsonAOllamaAsync(
            string instrucciones,
            string mensaje)
        {
            string url =
                ConfigurationManager
                    .AppSettings["OllamaUrl"];


            string modelo =
                ConfigurationManager
                    .AppSettings["OllamaModel"];


            ValidarConfiguracion(
                url,
                modelo);


            var solicitud =
                new
                {
                    model = modelo,

                    stream = false,

                    format = "json",

                    options =
                        new
                        {
                            temperature = 0
                        },

                    messages =
                        new[]
                        {
                            new
                            {
                                role = "system",

                                content =
                                    instrucciones
                            },

                            new
                            {
                                role = "user",

                                content =
                                    mensaje
                            }
                        }
                };


            return await EjecutarSolicitudAsync(
                url,
                solicitud);
        }


        // =====================================================
        // EJECUTAR HTTP
        // =====================================================

        private async Task<string> EjecutarSolicitudAsync(
            string url,
            object solicitud)
        {
            JavaScriptSerializer serializer =
                new JavaScriptSerializer();


            string json =
                serializer.Serialize(
                    solicitud);


            using (StringContent contenido =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"))
            {
                HttpResponseMessage respuesta =
                    await httpClient.PostAsync(
                        url,
                        contenido);


                string respuestaJson =
                    await respuesta.Content
                        .ReadAsStringAsync();


                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new Exception(
                        "Ollama devolvió HTTP "
                        +
                        (int)respuesta.StatusCode
                        +
                        ". "
                        +
                        respuestaJson);
                }


                OllamaRespuesta resultado =
                    serializer
                        .Deserialize<OllamaRespuesta>(
                            respuestaJson);


                if (resultado == null ||
                    resultado.message == null ||
                    string.IsNullOrWhiteSpace(
                        resultado.message.content))
                {
                    throw new Exception(
                        "Ollama no devolvió "
                        +
                        "una respuesta válida.");
                }


                return
                    resultado.message.content;
            }
        }


        // =====================================================
        // CONFIGURACIÓN
        // =====================================================

        private void ValidarConfiguracion(
            string url,
            string modelo)
        {
            if (string.IsNullOrWhiteSpace(
                url))
            {
                throw new Exception(
                    "No está configurada OllamaUrl.");
            }


            if (string.IsNullOrWhiteSpace(
                modelo))
            {
                throw new Exception(
                    "No está configurado OllamaModel.");
            }
        }


        // =====================================================
        // LIMPIAR JSON
        // =====================================================

        private string LimpiarJson(
            string json)
        {
            if (string.IsNullOrWhiteSpace(
                json))
            {
                return "";
            }


            string resultado =
                json.Trim();


            if (resultado.StartsWith(
                "```json",
                StringComparison.OrdinalIgnoreCase))
            {
                resultado =
                    resultado.Substring(7);
            }
            else if (resultado.StartsWith(
                "```",
                StringComparison.OrdinalIgnoreCase))
            {
                resultado =
                    resultado.Substring(3);
            }


            if (resultado.EndsWith(
                "```",
                StringComparison.OrdinalIgnoreCase))
            {
                resultado =
                    resultado.Substring(
                        0,
                        resultado.Length - 3);
            }


            return resultado.Trim();
        }


        // =====================================================
        // RESPUESTA OLLAMA
        // =====================================================

        private class OllamaRespuesta
        {
            public OllamaMensaje message
            {
                get;
                set;
            }
        }


        private class OllamaMensaje
        {
            public string role
            {
                get;
                set;
            }


            public string content
            {
                get;
                set;
            }
        }
    }
}
