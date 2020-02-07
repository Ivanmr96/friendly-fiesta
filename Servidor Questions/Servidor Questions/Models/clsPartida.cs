using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servidor_Questions.Models
{
    /// <summary>
    /// Esta clase recoge la información de una partida
    /// </summary>
    public class clsPartida
    {
        private string id;
        private clsJugador jugador1;
        private clsJugador jugador2;
        private List<clsQuestion> preguntas;
        private static int idIndex = 0;

        public clsPartida(string id, clsJugador jugador1, clsJugador jugador2, List<clsQuestion> preguntas)
        {
            this.id = id;
            this.jugador1 = jugador1;
            this.jugador2 = jugador2;
            this.preguntas = preguntas;
        }

        public static int IdIndex
        {
            get
            {
                return idIndex++;
            }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public clsJugador Jugador1
        {
            get { return jugador1; }
            set { jugador1 = value; }
        }

        public clsJugador Jugador2
        {
            get { return jugador2; }
            set { jugador2 = value; }
        }

        public List<clsQuestion> Preguntas
        {
            get { return preguntas; }
            set 
            { 
                if(preguntas.Count == 0 || preguntas == null)
                {
                    preguntas = value;
                }
            }
        }

        public bool jugador1HaContestado { get; set; }
        public bool jugador1EsRespuestaCorrecta { get; set; }

        public bool jugador2HaContestado { get; set; }
        public bool jugador2EsRespuestaCorrecta { get; set; }
    }
}