using ClosedXML.Excel;
using StockCristales.Models.Importacion;

namespace StockCristales.Services
{
    public class ImportacionExcelService
    {
        public List<FilaImportacionCristal> LeerExcel(Stream archivo)
        {
            var filas = new List<FilaImportacionCristal>();

            using var workbook = new XLWorkbook(archivo);

            var hoja = workbook.Worksheet(1);

            var filasUsadas = hoja.RowsUsed().ToList();

            if (filasUsadas.Count == 0)
                return filas;


            // Buscar automáticamente la fila real de encabezados
            var encabezados = filasUsadas.FirstOrDefault(fila =>
            {
                var valores = fila.CellsUsed()
                    .Select(celda => celda.GetString().Trim())
                    .ToList();

                bool tieneMarca = valores.Any(v =>
                    v.Equals("Marca", StringComparison.OrdinalIgnoreCase));

                bool tieneCodigo = valores.Any(v =>
                    v.Equals("Código", StringComparison.OrdinalIgnoreCase) ||
                    v.Equals("Codigo", StringComparison.OrdinalIgnoreCase));

                bool tieneModelo = valores.Any(v =>
                    v.Equals("Modelo", StringComparison.OrdinalIgnoreCase));

                return tieneMarca && tieneCodigo && tieneModelo;
            });


            if (encabezados == null)
            {
                throw new Exception(
                    "No se encontró la fila de encabezados del Excel."
                );
            }


            var columnas = encabezados.CellsUsed()
                .ToDictionary(
                    celda => celda.GetString().Trim(),
                    celda => celda.Address.ColumnNumber,
                    StringComparer.OrdinalIgnoreCase
                );


            // Tomamos solamente las filas posteriores a los encabezados
            var filasDatos = filasUsadas
                .Where(fila =>
                    fila.RowNumber() > encabezados.RowNumber());


            foreach (var fila in filasDatos)
            {
                var codigo = ObtenerValor(
                    fila,
                    columnas,
                    "Código",
                    "Codigo"
                );


                // Sin código no consideramos que sea un producto válido
                if (string.IsNullOrWhiteSpace(codigo))
                    continue;


                var modeloOriginal = ObtenerValor(
                    fila,
                    columnas,
                    "Modelo"
                );

                var descripcion = ObtenerValor(
                    fila,
                    columnas,
                    "Descripción",
                    "Descripcion"
                );

                var anioDesde = ObtenerAnioDesde(
                    codigo,
                    modeloOriginal,
                    descripcion
                );

                var cristal = ObtenerValor(
                    fila,
                    columnas,
                    "Cristal"
                );

                var posicion = ObtenerValor(
                    fila,
                    columnas,
                    "Posicion",
                    "Posición"
                );

                var lado = ObtenerValor(
                    fila,
                    columnas,
                    "Lado"
                );


                filas.Add(new FilaImportacionCristal
                {
                    MarcaVehiculo = ObtenerValor(
                        fila,
                        columnas,
                        "Marca"
                    ),

                    Codigo = codigo,

                    ModeloOriginal = modeloOriginal,

                    ModeloNormalizado = NormalizarModelo(
                        modeloOriginal
                    ),

                    Descripcion = descripcion,

                    AnioDesde = anioDesde,

                    Cristal = cristal,

                    Posicion = posicion,

                    Lado = lado,

                    TiposCristalNormalizados = NormalizarTiposCristal(
                            cristal,
                            posicion,
                            lado
                        ),

                    Color = ObtenerValor(
                        fila,
                        columnas,
                        "Color"
                    )
                });
            }


            return filas;
        }


        private string ObtenerValor(
            IXLRow fila,
            Dictionary<string, int> columnas,
            params string[] nombresPosibles)
        {
            foreach (var nombre in nombresPosibles)
            {
                if (columnas.TryGetValue(
                    nombre,
                    out int numeroColumna))
                {
                    return fila.Cell(numeroColumna)
                        .GetString()
                        .Trim();
                }
            }

            return string.Empty;
        }

        private int? ObtenerAnioDesde(
            string codigo,
            string modeloOriginal,
            string descripcion)
        {
            // Primero intentamos obtenerlo del código:
            // posiciones 3 y 4 del código comercial.
            if (codigo.Length >= 4)
            {
                string textoAnio = codigo.Substring(2, 2);

                if (int.TryParse(textoAnio, out int anioCorto))
                {
                    int anioActual = DateTime.Now.Year;
                    int anioActualCorto = anioActual % 100;

                    // Ej.: 96 -> 1996
                    //      08 -> 2008
                    //      13 -> 2013
                    if (anioCorto <= anioActualCorto)
                        return 2000 + anioCorto;

                    return 1900 + anioCorto;
                }
            }

            return null;
        }


        private string NormalizarModelo(string modeloOriginal)
        {
            if (string.IsNullOrWhiteSpace(modeloOriginal))
                return string.Empty;

            var modelo = modeloOriginal.Trim();

            int posicionApostrofe = modelo.IndexOf('\'');

            if (posicionApostrofe >= 0)
            {
                modelo = modelo
                    .Substring(0, posicionApostrofe)
                    .Trim();
            }

            return modelo;
        }


        private List<string> NormalizarTiposCristal(
                string cristal,
                string posicion,
                string lado)
        {
            var tipos = new List<string>();

            cristal = cristal.Trim();
            posicion = posicion.Trim();
            lado = lado.Trim();




            // SINÓNIMOS / NOMBRES ALTERNATIVOS DEL PROVEEDOR

            if (cristal.Equals("Aleta", StringComparison.OrdinalIgnoreCase) ||
                cristal.Equals("Ventilete", StringComparison.OrdinalIgnoreCase) ||
                cristal.Equals("Aleta/Ventilete", StringComparison.OrdinalIgnoreCase))
            {
                cristal = "Ventilete";
            }

            if (cristal.Equals("Coleta", StringComparison.OrdinalIgnoreCase) ||
                cristal.Equals("Custodia", StringComparison.OrdinalIgnoreCase) ||
                cristal.Equals("Custodia/Coleta", StringComparison.OrdinalIgnoreCase))
            {
                cristal = "Custodia";
            }


            // ¿EL MISMO CRISTAL SIRVE PARA AMBOS LADOS?

            bool ambosLados =
                lado.Equals("Der/Izq", StringComparison.OrdinalIgnoreCase) ||
                lado.Equals("Derecho/Izquierdo", StringComparison.OrdinalIgnoreCase) ||
                lado.Equals("Derecha/Izquierda", StringComparison.OrdinalIgnoreCase);


            // PUERTA
            if (cristal.Equals(
                "Puerta",
                StringComparison.OrdinalIgnoreCase))
            {
                // Si no informa posición, significa que el vehículo
                // no tiene puerta trasera: corresponde a delantera.
                string posicionPuerta;

                if (string.IsNullOrWhiteSpace(posicion))
                {
                    posicionPuerta = "delantera";
                }
                else
                {
                    posicionPuerta = posicion.ToLowerInvariant();
                }


                // Si no informa lado, o dice Der/Izq,
                // el mismo cristal sirve para ambos lados.
                bool puertaAmbosLados =
                    ambosLados ||
                    string.IsNullOrWhiteSpace(lado);


                if (puertaAmbosLados)
                {
                    tipos.Add($"puerta {posicionPuerta} derecha");
                    tipos.Add($"puerta {posicionPuerta} izquierda");

                    return tipos;
                }


                string ladoPuerta = NormalizarLado(lado);

                tipos.Add(
                    $"puerta {posicionPuerta} {ladoPuerta}"
                        .Trim()
                        .ToLowerInvariant()
                );

                return tipos;
            }


            // CUSTODIA / COLETA
            if (cristal.Equals(
                "Custodia",
                StringComparison.OrdinalIgnoreCase))
            {
                if (ambosLados)
                {
                    tipos.Add("custodia derecha");
                    tipos.Add("custodia izquierda");

                    return tipos;
                }

                string ladoCustodia = NormalizarLado(lado);

                tipos.Add(
                    $"custodia {ladoCustodia}"
                        .Trim()
                        .ToLowerInvariant()
                );

                return tipos;
            }


            // VENTILETE / ALETA
            if (cristal.Equals(
                "Ventilete",
                StringComparison.OrdinalIgnoreCase))
            {
                string posicionVentilete;

                if (posicion.Equals(
                    "Delantera",
                    StringComparison.OrdinalIgnoreCase))
                {
                    posicionVentilete = "delantero";
                }
                else if (posicion.Equals(
                    "Trasera",
                    StringComparison.OrdinalIgnoreCase))
                {
                    posicionVentilete = "trasero";
                }
                else
                {
                    posicionVentilete = posicion.ToLowerInvariant();
                }


                if (ambosLados)
                {
                    tipos.Add(
                        $"ventilete {posicionVentilete} derecho"
                    );

                    tipos.Add(
                        $"ventilete {posicionVentilete} izquierdo"
                    );

                    return tipos;
                }


                string ladoVentilete;

                if (lado.Equals(
                    "Derecho",
                    StringComparison.OrdinalIgnoreCase))
                {
                    ladoVentilete = "derecho";
                }
                else if (lado.Equals(
                    "Izquierdo",
                    StringComparison.OrdinalIgnoreCase))
                {
                    ladoVentilete = "izquierdo";
                }
                else
                {
                    ladoVentilete = lado.ToLowerInvariant();
                }


                tipos.Add(
                    $"ventilete {posicionVentilete} {ladoVentilete}"
                        .Trim()
                        .ToLowerInvariant()
                );

                return tipos;
            }


            // PARABRISAS, LUNETA, TECHO, ETC.
            tipos.Add(
                cristal.ToLowerInvariant()
            );

            return tipos;
        }


        private string NormalizarLado(string lado)
        {
            if (lado.Equals(
                "Derecho",
                StringComparison.OrdinalIgnoreCase))
            {
                return "derecha";
            }


            if (lado.Equals(
                "Izquierdo",
                StringComparison.OrdinalIgnoreCase))
            {
                return "izquierda";
            }


            return lado
                .Trim()
                .ToLowerInvariant();
        }
    }
}
