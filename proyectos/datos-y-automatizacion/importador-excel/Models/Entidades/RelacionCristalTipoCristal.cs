namespace StockCristales.Models.Entidades
{
    public class RelacionCristalTipoCristal
    {
        public int IdRelacionCristalTipoCristal { get; set; }

        public int IdCristal { get; set; }
        public Cristal Cristal { get; set; } = null!;

        public int IdTipoCristal { get; set; }
        public TipoCristal TipoCristal { get; set; } = null!;
    }
}
