namespace BIZ
{
    public class ConsultaIA
    {
        public string Accion
        {
            get;
            set;
        }


        // Período relativo interpretado por Ollama.
        //
        // Ejemplos:
        // HOY
        // MANANA
        // ESTA_SEMANA
        // PROXIMA_SEMANA
        // ESTE_MES
        // RANGO_EXPLICITO

        public string Periodo
        {
            get;
            set;
        }


        public string Periodo2
        {
            get;
            set;
        }


        // Estas fechas solamente se utilizan
        // cuando el usuario escribió fechas concretas.

        public string Desde
        {
            get;
            set;
        }


        public string Hasta
        {
            get;
            set;
        }


        public string Desde2
        {
            get;
            set;
        }


        public string Hasta2
        {
            get;
            set;
        }


        public string PreguntaOriginal
        {
            get;
            set;
        }
    }
}
