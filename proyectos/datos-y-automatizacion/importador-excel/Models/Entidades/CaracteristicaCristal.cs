namespace StockCristales.Models.Entidades
{
    public class CaracteristicaCristal
    {

        public int IdCaracteristicaCristal { get; set; }

        public string Descripcion { get; set; } = string.Empty;


        public ICollection<RelacionCristalCaracteristica> RelacionesCristales { get; set; }
            = new List<RelacionCristalCaracteristica>();
    }



}
