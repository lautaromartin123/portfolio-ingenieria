namespace StockCristales.Models.Entidades
{
    public class RelacionCristalVehiculo
    {

        public int IdRelacionCristalVehiculo { get; set; }

        public int IdCristal { get; set; }
        public Cristal Cristal { get; set; } = null!;

        public int IdVehiculo { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        public int AnioDesde { get; set; }

        public int? AnioHasta { get; set; }







    }
}
