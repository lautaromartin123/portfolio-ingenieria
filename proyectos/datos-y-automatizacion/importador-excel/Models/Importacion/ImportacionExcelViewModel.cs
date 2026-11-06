namespace StockCristales.Models.Importacion
{
    public class ImportacionExcelViewModel
    {
        public List<FilaImportacionCristal> Filas { get; set; }
            = new List<FilaImportacionCristal>();

        public int TotalFilas { get; set; }

        public string? MensajeError { get; set; }


        public List<string> TiposCristalNoReconocidos { get; set; }
           = new List<string>();

        public List<string> ColoresNoReconocidos { get; set; }
            = new List<string>();


        public int CantidadSinAnioDesde { get; set; }

        public List<string> CodigosSinAnioDesde { get; set; }
            = new List<string>();







    }


}
