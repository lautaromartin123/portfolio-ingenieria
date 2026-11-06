namespace StockCristales.Models.Entidades
{
    public class TipoCristal
    {

        public int IdTipoCristal { get; set; }

        public string Descripcion { get; set; } = string.Empty;


        public ICollection<RelacionCristalTipoCristal> RelacionesCristales { get; set; }
    = new List<RelacionCristalTipoCristal>();



    }
}
