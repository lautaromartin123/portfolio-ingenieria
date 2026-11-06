namespace StockCristales.Models.Entidades
{
    public class Vehiculo
    {


        public int IdVehiculo { get; set; }

        public string Modelo { get; set; } = string.Empty;

        public int IdMarcaVehiculo { get; set; }

        public MarcaVehiculo MarcaVehiculo { get; set; } = null!;


        public ICollection<RelacionCristalVehiculo> RelacionesCristales { get; set; }
           = new List<RelacionCristalVehiculo>();








    }
}
