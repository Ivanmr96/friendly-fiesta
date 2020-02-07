using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servidor_Questions.Models
{
    /// <summary>
    /// Contiene la información de una categoría, su id y los jugadores que están buscando partida para la categoría
    /// </summary>
    public class clsCategoriaYJugadoresBuscando : ICategoryAndNumOfPlayers
    {
        private string id;
        private String nombre;
        private List<clsJugador> jugadoresBuscando;
        private List<clsPartida> partidas;

        public clsCategoriaYJugadoresBuscando(string id, string nombre, List<clsJugador> jugadoresBuscando, List<clsPartida> partidas)
        { 
            this.id = id;
            this.nombre = nombre;
            this.jugadoresBuscando = jugadoresBuscando;
            this.partidas = partidas;
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public List<clsJugador> JugadoresBuscando
        {
            get { return jugadoresBuscando; }
            set { jugadoresBuscando = value; }
        }

        public List<clsPartida> Partidas
        {
            get { return partidas; }
            set { partidas = value; }
        }

        public int NumJugadoresBuscando 
        { 
            get { return jugadoresBuscando.Count; }
        }
    }
}