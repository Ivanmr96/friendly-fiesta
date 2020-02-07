using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Models
{
    public class clsNombreUsuarioYNombreCategoria
    {
        public String nombreUsuario { get; set; }
        public String nombreCategoria { get; set; }

        public clsNombreUsuarioYNombreCategoria(String nombreUsuario, String nombreCategoria)
        {
            this.nombreUsuario = nombreUsuario;
            this.nombreCategoria = nombreCategoria;
        }
    }
}
