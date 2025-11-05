using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRQCC.Models
{
    public class EstadoPago
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int IdEstadoPago { get; set; }

        public string NombreMes
        {
            get
            {
                return new DateTime(Anio, Mes, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));
            }
        }
    }
}
