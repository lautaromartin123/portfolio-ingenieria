using Microsoft.AspNetCore.Mvc;
using StockCristales.Data;
using StockCristales.Models.Importacion;
using StockCristales.Services;

namespace StockCristales.Controllers
{
    public class ImportacionController : Controller
    {
        private readonly ImportacionExcelService _importacionExcelService;
        private readonly StockCristalesContext _context;

        public ImportacionController(
            ImportacionExcelService importacionExcelService,
            StockCristalesContext context)
        {
            _importacionExcelService = importacionExcelService;
            _context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImportacionExcelViewModel());
        }


        [HttpPost]
        public IActionResult Index(IFormFile archivo)
        {
            var modelo = new ImportacionExcelViewModel();

            if (archivo == null || archivo.Length == 0)
            {
                modelo.MensajeError = "Debe seleccionar un archivo.";
                return View(modelo);
            }

            var extension = Path.GetExtension(archivo.FileName);

            if (!extension.Equals(".xlsx",
                StringComparison.OrdinalIgnoreCase))
            {
                modelo.MensajeError =
                    "El archivo debe tener formato .xlsx.";

                return View(modelo);
            }

            try
            {
                using var stream = archivo.OpenReadStream();

                var filas = _importacionExcelService.LeerExcel(stream);

                var tiposValidos = _context.TiposCristal
                    .Select(x => x.Descripcion)
                    .ToList();

                var coloresValidos = _context.ColoresCristal
                    .Select(x => x.Descripcion)
                    .ToList();

                modelo.TiposCristalNoReconocidos = filas
                    .SelectMany(x => x.TiposCristalNormalizados)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(tipo => !tiposValidos.Any(t =>
                        t.Equals(
                            tipo,
                            StringComparison.OrdinalIgnoreCase
                        )))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();


                modelo.ColoresNoReconocidos = filas
                    .Select(x => x.Color)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(color => !coloresValidos.Any(c =>
                        c.Equals(color, StringComparison.OrdinalIgnoreCase)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();


                // VALIDACIÓN DE AÑOS

                modelo.CantidadSinAnioDesde = filas
                    .Count(x => x.AnioDesde == null);

                modelo.CodigosSinAnioDesde = filas
                    .Where(x => x.AnioDesde == null)
                    .Select(x => x.Codigo)
                    .Take(20)
                    .ToList();


                // DATOS DE VISTA PREVIA



                modelo.TotalFilas = filas.Count;

                modelo.Filas = filas
                    .Take(20)
                    .ToList();
            }
            catch (Exception ex)
            {
                modelo.MensajeError =
                    $"No se pudo leer el archivo: {ex.Message}";
            }

            return View(modelo);
        }
    }
}
