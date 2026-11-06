namespace StockCristales.Models.Entidades
{
    public class RelacionCristalCaracteristica
    {

        public int IdRelacionCristalCaracteristica { get; set; }

        public int IdCristal { get; set; }

        public Cristal Cristal { get; set; } = null!;


        public int IdCaracteristicaCristal { get; set; }

        public CaracteristicaCristal CaracteristicaCristal { get; set; } = null!;












    }
}
