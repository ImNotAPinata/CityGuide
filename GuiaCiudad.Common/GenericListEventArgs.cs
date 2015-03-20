using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaCiudad.Common
{
    public class GenericListEventArgs<T> : EventArgs
    {
        public List<T> ListResults { get; private set; }
        
        public GenericListEventArgs(List<T> results)
        {
            this.ListResults = results;
        }

    }
}
