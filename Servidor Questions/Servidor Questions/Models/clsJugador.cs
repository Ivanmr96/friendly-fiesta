using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servidor_Questions.Models
{
    /// <summary>
    /// Contiene la información de un jugador en una partida
    /// </summary>
    public class clsJugador
    {
        private string nombre;
        private string connectionID;
        private int preguntasAcertadas;
        private int preguntasFalladas;

        public clsJugador(string nombre, string connectionID, int preguntasAcertadas, int preguntasFalladas)
        {
            this.nombre = nombre;
            this.connectionID = connectionID;
            this.preguntasAcertadas = preguntasAcertadas;
            this.preguntasFalladas = preguntasFalladas;
        }

        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public string ConnectionID
        {
            get { return connectionID; }
            set { connectionID = value; }
        }

        public int PreguntasAcertadas
        {
            get { return preguntasAcertadas; }
            set { preguntasAcertadas = value; }
        }

        public int PreguntasFalladas
        {
            get { return preguntasFalladas; }
            set { preguntasFalladas = value; }
        }
    }
}