using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Models
{ 
    class DosStringToMensaje : IMensaje
    {
        public DosStringToMensaje(string nombre, string mensaje)
        {
            this.nombre = nombre;
            this.mensaje = mensaje;
        }

        public string nombre 
        {
            get; 
            set; 
        }

        public string mensaje 
        { 
            get; 
            set; 
        }
    }
}
