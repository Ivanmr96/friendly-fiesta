using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Servidor_Questions.Models
{
    public class partidas
    {
        private List<clsCategoriaYJugadoresBuscando> lista;

        private readonly static partidas instance = new partidas();

        private partidas() 
        {
            lista = new List<clsCategoriaYJugadoresBuscando>();

            clsCategoriaYJugadoresBuscando cat;
            foreach(KeyValuePair<String, String> category in Utils.Utils.Categories)
            {
                cat = new clsCategoriaYJugadoresBuscando(category.Key, category.Value, new List<clsJugador>(), new List<clsPartida>());

                lista.Add(cat);
            }
        }

        public static partidas Instance
        {
            get
            {
                return instance;
            }
        }

        public List<clsCategoriaYJugadoresBuscando> Lista
        {
            get { return lista; }
            set { lista = value; }
        }


        /// <summary>
        /// Obtiene la partida perteneciente al connectionID dado
        /// </summary>
        /// <param name="connectionID">El ID de conexión del cliente del que se desea obtener la partida en la que está jugando</param>
        /// <returns>Devuelve la partida perteneciente al connectionID dado. Si el cliente no existe o no está en partida, devuelve null.</returns>
        public clsPartida getMatchByConnectionID(String connectionID)
        {
            bool found = false;

            clsPartida partida = null;
            //Por cada categoria
            for(int i = 0; i < Lista.Count && found == false; i++)
            {
                //Por cada partida dentro de cada categoria
                for(int j = 0; j < Lista[i].Partidas.Count && found == false; j++)
                {
                    partida = Lista[i].Partidas[j];

                    if(partida.Jugador1.ConnectionID.Equals(connectionID) || partida.Jugador2.ConnectionID.Equals(connectionID))
                    {
                        found = true;
                    }
                }
            }

            return partida;
        }
    }
}