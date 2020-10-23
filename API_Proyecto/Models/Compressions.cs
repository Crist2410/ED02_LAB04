using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Proyecto.Models
{
    public class Compressions
    {
        public string NombreOriginal { get; set; }
        public string NombreComprimido { get; set; }
        public string RutaComprimido { get; set; }
        public double RazonCompresion { get; set; }
        public double FactorCompresion { get; set; }
        public double PorcentajeReduccion { get; set; }
    }
}
