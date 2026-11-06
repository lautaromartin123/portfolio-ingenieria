using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class CalcularEdad
    {
        public static int Calcular(DateTime fechaNacimiento)
        {
            int edad = DateTime.Now.Year - fechaNacimiento.Year;
            if (DateTime.Today < fechaNacimiento.Date.AddYears(edad)) edad--;
            return edad;
        }
    }
}
