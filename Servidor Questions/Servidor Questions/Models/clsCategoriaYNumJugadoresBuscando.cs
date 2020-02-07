using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servidor_Questions.Models
{
    class clsCategoriaYNumJugadoresBuscando : ICategoryAndNumOfPlayers
    {
        private String id;
        private String nombre;
        private int numJugadoresBuscando;

        public clsCategoriaYNumJugadoresBuscando(String id, String nombre, int numJugadoresBuscando)
        {
            this.id = id;
            this.nombre = nombre;
            this.numJugadoresBuscando = numJugadoresBuscando;
        }

        public clsCategoriaYNumJugadoresBuscando(clsCategoriaYJugadoresBuscando categoria)
        {
            this.id = categoria.Id;
            this.nombre = categoria.Nombre;
            this.numJugadoresBuscando = categoria.NumJugadoresBuscando;
        }


        public String Id
        {
            get { return id; }
            set { id = value; }
        }

        public String Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public int NumJugadoresBuscando
        {
            get { return numJugadoresBuscando; }
            set { numJugadoresBuscando = value; }
        }
    }
}