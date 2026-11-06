using BIZ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Consultorios
{
    public class OllamaTurnosPacienteService
    {
        private static readonly HttpClient Cliente =
            new HttpClient
            {
                Timeout =
                    TimeSpan.FromSeconds(12)
            };


        private readonly JavaScriptSerializer serializer =
            new JavaScriptSerializer();


        public async Task<InterpretacionTurnoPacienteIA>
            InterpretarAsync(
                string mensaje,
                string pasoActual)
        {
            if (string.IsNullOrWhiteSpace(
                mensaje))
            {
                return null;
            }


            string modelo =
                ConfigurationManager
                    .AppSettings["OllamaModel"];


            if (string.IsNullOrWhiteSpace(
                modelo))
            {
                modelo =
                    "llama3.2:3b";
            }


            string endpoint =
                ConfigurationManager
                    .AppSettings["OllamaEndpoint"];


            if (string.IsNullOrWhiteSpace(
                endpoint))
            {
                endpoint =
                    "http://localhost:11434/api/chat";
            }


            string systemPrompt =
                string.Join(
                    Environment.NewLine,
                    new string[]
                    {
                        "Sos un clasificador de intenciones para un asistente administrativo de turnos médicos.",

                        "No sos médico y no debés diagnosticar, recomendar medicamentos, dosis ni tratamientos.",

                        "No tenés acceso a SQL ni podés crear, modificar o cancelar turnos por tu cuenta.",

                        "Tu única tarea es interpretar el mensaje y devolver JSON.",

                        "",

                        "IMPORTANTE:",

                        "- No calcules fechas relativas. Para hoy, mañana, próxima semana, etc. devolvé una etiqueta en periodo.",

                        "- Si el usuario escribe una fecha explícita, copiá la fecha textual en fecha.",

                        "- Si menciona una hora o rango horario, copiá esos valores.",

                        "- Si describe un síntoma como motivo para pedir un turno, eso NO es una consulta clínica.",

                        "- Ejemplo: quiero turno por dolor de rodilla => NUEVO_TURNO.",

                        "- Si pide diagnóstico, medicación, dosis o tratamiento => CONSULTA_CLINICA.",

                        "- Si pide datos de otros pacientes => NO_SOPORTADO.",

                        "- Nunca inventes especialidades, profesionales, fechas ni horarios.",

                        "",

                        "ACCIONES PERMITIDAS:",

                        "NUEVO_TURNO",

                        "MIS_TURNOS",

                        "CANCELAR_TURNO",

                        "CONFIRMAR",

                        "CAMBIAR_HORARIO",

                        "REINICIAR",

                        "AYUDA",

                        "CONSULTA_CLINICA",

                        "NO_SOPORTADO",

                        "",

                        "PERIODOS PERMITIDOS:",

                        "HOY",

                        "MANANA",

                        "PASADO_MANANA",

                        "ESTA_SEMANA",

                        "PROXIMA_SEMANA",

                        "ESTE_MES",

                        "FECHA_EXPLICITA",

                        "SIN_PERIODO",

                        "",

                        "MOMENTOS PERMITIDOS:",

                        "MANANA",

                        "TARDE",

                        "CUALQUIERA",

                        "",

                        "Devolvé SOLAMENTE este JSON, sin markdown ni explicación:",

                        "{",

                        "  \"accion\": \"...\",",

                        "  \"especialidad\": \"\",",

                        "  \"profesional\": \"\",",

                        "  \"periodo\": \"SIN_PERIODO\",",

                        "  \"fecha\": \"\",",

                        "  \"momento\": \"CUALQUIERA\",",

                        "  \"horaDesde\": \"\",",

                        "  \"horaHasta\": \"\",",

                        "  \"motivo\": \"\"",

                        "}"
                    });


            string contexto =
                "Paso actual del flujo: "
                +
                (
                    string.IsNullOrWhiteSpace(
                        pasoActual)
                        ? "INICIO"
                        : pasoActual
                )
                +
                "."
                +
                Environment.NewLine
                +
                "Mensaje del paciente: "
                +
                mensaje;


            OllamaRequest solicitud =
                new OllamaRequest
                {
                    model =
                        modelo,

                    stream =
                        false,

                    options =
                        new OllamaOptions
                        {
                            temperature =
                                0
                        },

                    messages =
                        new List<OllamaMessage>
                        {
                            new OllamaMessage
                            {
                                role =
                                    "system",

                                content =
                                    systemPrompt
                            },

                            new OllamaMessage
                            {
                                role =
                                    "user",

                                content =
                                    contexto
                            }
                        }
                };


            try
            {
                string jsonSolicitud =
                    serializer.Serialize(
                        solicitud);


                using (
                    StringContent contenido =
                        new StringContent(
                            jsonSolicitud,
                            Encoding.UTF8,
                            "application/json")
                )
                using (
                    HttpResponseMessage respuesta =
                        await Cliente.PostAsync(
                            endpoint,
                            contenido)
                )
                {
                    if (!respuesta
                        .IsSuccessStatusCode)
                    {
                        return null;
                    }


                    string jsonRespuesta =
                        await respuesta
                            .Content
                            .ReadAsStringAsync();


                    OllamaResponse respuestaOllama =
                        serializer
                            .Deserialize<OllamaResponse>(
                                jsonRespuesta);


                    if (
                        respuestaOllama == null
                        ||
                        respuestaOllama.message == null
                        ||
                        string.IsNullOrWhiteSpace(
                            respuestaOllama
                                .message
                                .content)
                    )
                    {
                        return null;
                    }


                    string jsonInterpretacion =
                        ExtraerJson(
                            respuestaOllama
                                .message
                                .content);


                    if (string.IsNullOrWhiteSpace(
                        jsonInterpretacion))
                    {
                        return null;
                    }


                    InterpretacionTurnoPacienteIA
                        interpretacion =
                            serializer
                                .Deserialize
                                <
                                    InterpretacionTurnoPacienteIA
                                >(
                                    jsonInterpretacion);


                    NormalizarInterpretacion(
                        interpretacion);


                    return interpretacion;
                }
            }
            catch
            {
                /*
                 * Si Ollama está apagado o responde
                 * algo inválido, el sistema sigue
                 * funcionando mediante las burbujas.
                 */

                return null;
            }
        }


        private static void
            NormalizarInterpretacion(
                InterpretacionTurnoPacienteIA
                    interpretacion)
        {
            if (interpretacion == null)
            {
                return;
            }


            interpretacion.Accion =
                (
                    interpretacion.Accion
                    ??
                    "NO_SOPORTADO"
                )
                .Trim()
                .ToUpperInvariant();


            interpretacion.Periodo =
                (
                    interpretacion.Periodo
                    ??
                    "SIN_PERIODO"
                )
                .Trim()
                .ToUpperInvariant();


            interpretacion.Momento =
                (
                    interpretacion.Momento
                    ??
                    "CUALQUIERA"
                )
                .Trim()
                .ToUpperInvariant();


            interpretacion.Especialidad =
                (
                    interpretacion.Especialidad
                    ??
                    ""
                )
                .Trim();


            interpretacion.Profesional =
                (
                    interpretacion.Profesional
                    ??
                    ""
                )
                .Trim();


            interpretacion.Fecha =
                (
                    interpretacion.Fecha
                    ??
                    ""
                )
                .Trim();


            interpretacion.HoraDesde =
                (
                    interpretacion.HoraDesde
                    ??
                    ""
                )
                .Trim();


            interpretacion.HoraHasta =
                (
                    interpretacion.HoraHasta
                    ??
                    ""
                )
                .Trim();


            interpretacion.Motivo =
                (
                    interpretacion.Motivo
                    ??
                    ""
                )
                .Trim();
        }


        private static string ExtraerJson(
            string texto)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return null;
            }


            int inicio =
                texto.IndexOf('{');


            int fin =
                texto.LastIndexOf('}');


            if (
                inicio < 0
                ||
                fin <= inicio
            )
            {
                return null;
            }


            return texto.Substring(
                inicio,
                fin - inicio + 1);
        }


        private class OllamaRequest
        {
            public string model
            {
                get;
                set;
            }


            public bool stream
            {
                get;
                set;
            }


            public OllamaOptions options
            {
                get;
                set;
            }


            public List<OllamaMessage> messages
            {
                get;
                set;
            }
        }


        private class OllamaOptions
        {
            public double temperature
            {
                get;
                set;
            }
        }


        private class OllamaMessage
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


        private class OllamaResponse
        {
            public OllamaMessage message
            {
                get;
                set;
            }
        }
    }
}
