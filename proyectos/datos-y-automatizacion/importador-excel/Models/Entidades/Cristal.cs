namespace StockCristales.Models.Entidades
{
    public class Cristal
    {

        public int IdCristal { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Descripcion {  get; set; } = string.Empty;




        //OBLIGATORIO
        public int IdMarcaCristal { get; set;  }

        public MarcaCristal MarcaCristal { get;set; }

        //POR AHORA OPCIONAL
        public int? IdColorCristal { get; set; }

        public ColorCristal ColorCristal { get; set; }


        //POR AHORA OPCIONAL
        public int? IdOrigenCristal { get; set; }

        public OrigenCristal? OrigenCristal { get; set; }


        public ICollection<RelacionCristalCaracteristica> RelacionesCaracteristicas { get; set; }
           = new List<RelacionCristalCaracteristica>();

        public ICollection<RelacionCristalVehiculo> RelacionesVehiculos { get; set; }
            = new List<RelacionCristalVehiculo>();

        public ICollection<RelacionCristalTipoCristal> RelacionesTiposCristal { get; set; }
    = new List<RelacionCristalTipoCristal>();











    }
}
