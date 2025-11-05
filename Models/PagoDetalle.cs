using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRQCC.Models
{
    public class PagoDetalle
    {
        // Estos nombres deben coincidir EXACTAMENTE con las columnas de la BD
        public int mes { get; set; }
        public int anio { get; set; }
        public decimal monto { get; set; }
        public int id_estado_pago_detalle { get; set; }

        // Propiedades calculadas (pueden mantener naming convencional)
        public string EstadoDisplay
        {
            get
            {
                if (id_estado_pago_detalle == 2)
                    return "Pagado";

                var fechaActual = DateTime.Now;
                var fechaPago = new DateTime(anio, mes, 15);

                if (fechaPago > fechaActual)
                    return "Pendiente";
                else
                    return "Vencido";
            }
        }

        public string ColorEstado
        {
            get
            {
                if (id_estado_pago_detalle == 2)
                    return "#48BB78";

                var fechaActual = DateTime.Now;
                var fechaPago = new DateTime(anio, mes, 15);

                if (fechaPago > fechaActual)
                    return "#ED8936";
                else
                    return "#F56565";
            }
        }

        public string NombreMes
        {
            get
            {
                return mes switch
                {
                    1 => "Enero",
                    2 => "Febrero",
                    3 => "Marzo",
                    4 => "Abril",
                    5 => "Mayo",
                    6 => "Junio",
                    7 => "Julio",
                    8 => "Agosto",
                    9 => "Septiembre",
                    10 => "Octubre",
                    11 => "Noviembre",
                    12 => "Diciembre",
                    _ => "Mes inválido"
                };
            }
        }

        public string FechaVencimiento => $"15 {NombreMes.Substring(0, 3).ToLower()} {anio}";
    }
}
