using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuiaCiudad.Common
{
    public class GenericEventArgs<T> : EventArgs
    {
        public List<T> ListResults { get; private set; }
        public T Results { get; private set; }

        public GenericEventArgs (T results) 
        {
            this.Results = results;
        }

        public GenericEventArgs(List<T> results)
        {
            this.ListResults = results;
        }

    }
}
