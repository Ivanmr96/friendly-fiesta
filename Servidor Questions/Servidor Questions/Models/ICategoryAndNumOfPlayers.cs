using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servidor_Questions.Models
{
    interface ICategoryAndNumOfPlayers
    {
        String Id { get; set; }
        String Nombre { get; set; }
        int NumJugadoresBuscando { get; }
    }
}
