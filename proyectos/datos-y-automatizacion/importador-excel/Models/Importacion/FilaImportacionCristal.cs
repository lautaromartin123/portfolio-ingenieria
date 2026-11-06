namespace StockCristales.Models.Importacion
{
    public class FilaImportacionCristal
    {
        public string MarcaVehiculo { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string ModeloOriginal { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Cristal { get; set; } = string.Empty;

        public string Posicion { get; set; } = string.Empty;

        public string Lado { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int? AnioDesde { get; set; }

        public int? AnioHasta { get; set; }

        public string ModeloNormalizado { get; set; } = string.Empty;

        public List<string> TiposCristalNormalizados { get; set; }
         = new List<string>();


    }



}
