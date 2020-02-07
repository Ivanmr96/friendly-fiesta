using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Models
{
    public interface IMensaje
    {
        string nombre { get; set; }
        string mensaje { get; set; }
    }
}
