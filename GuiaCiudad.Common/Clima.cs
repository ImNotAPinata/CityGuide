using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaCiudad.Common
{
    public class Clima
    {
        public string clima { get; set; }
        public string precipitaciones { get; set; }
        public string humedad { get; set; }
        public string viento { get; set; }
        public string temperatura { get; set; }
        public string fechaobservacion { get; set; }
        public object icon_url { get; set; }
    }
}
