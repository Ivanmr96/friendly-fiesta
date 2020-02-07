using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Servidor_Questions.Models;
using Servidor_Questions.Models.Utils;

namespace Servidor_Questions
{
    public class QuestionsHub : Hub
    {
        #region attributes

        //List<clsQuestion> questions;
        partidas partidas;  //Clase singleton que guarda en memoria toda la información necesaria para el juego (categorias, partidas, etc)

        #endregion

        #region constructor

        public QuestionsHub(partidas partidas)
        {
            this.partidas = partidas;
        }

        #endregion

        #region lobby methods

        /// <summary>
        /// Pide el listado de categorias disponibles
        /// </summary>
        public void pedirCategorias()
        {
            //Clients.Caller.sendCategories(Utils.Categories);

            List<clsCategoriaYNumJugadoresBuscando> categories = new List<clsCategoriaYNumJugadoresBuscando>();

            foreach (clsCategoriaYJugadoresBuscando cat in partidas.Lista)
            {
                categories.Add(new clsCategoriaYNumJugadoresBuscando(cat));
            }

            Clients.Caller.sendCategories(categories);
        }

        //TODO: Esto es un poco ineficiente porque manda el mensaje a todos los clientes aunque estén en partida (los que están en partida no lo necesitan)
        /// <summary>
        /// Manda un mensaje a todos los clientes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        public void sendMessage(String name, String message)
        {
            Clients.All.newMessage(name, message);
        }

        //TODO PROBAR EL ONDISCONNECTED

        public override Task OnDisconnected(bool stopCalled)
        {
            salir();
            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// Entra en la cola para jugar en una categoria
        /// </summary>
        /// <param name="categoryName">Nombre de la categoría a la que se desea entrar en cola</param>
        /// <param name="nombreUsuario">Nombre de usuario que entra en cola</param>
        public void entrarEnCola(String categoryName, String nombreUsuario)
        {
            //Busca la categoria por su nombre según el nombre dado
            clsCategoriaYJugadoresBuscando categoria = partidas.Lista.Find(cat => cat.Nombre == categoryName);
            //clsCategoriaYJugadoresBuscando categoria = partidas.Lista.Find(cat => cat.Id == Utils.Categories[pepe]);

            //Si el nombre de la categoria pertenece a una categoria existente
            if (categoria != null)
            {
                //Saca de cola al cliente, por si estuviera en otra cola ya
                salirDeCola();

                //Añade a la cola de esta categoria al cliente
                categoria.JugadoresBuscando.Add(new clsJugador(nombreUsuario, Context.ConnectionId, 0, 0));

                //Si la cola de la categoria tiene más de un jugador, comienza una nueva partida para esta categoria
                if (categoria.NumJugadoresBuscando > 1)
                {
                    comenzarPartida(categoria);
                }
            }

            //Manda de nuevo las categorias a los clientes
            List<clsCategoriaYNumJugadoresBuscando> categories = new List<clsCategoriaYNumJugadoresBuscando>();

            foreach (clsCategoriaYJugadoresBuscando cat in partidas.Lista)
            {
                categories.Add(new clsCategoriaYNumJugadoresBuscando(cat));
            }

            Clients.All.sendCategories(categories);
        }

        /// <summary>
        /// Sale de la cola para buscar una partida
        /// </summary>
        public void salirDeCola()
        {
            //Por cada categoria
            for (int i = 0; i < partidas.Lista.Count; i++)
            {
                //Por cada jugador en la cola de la categoria
                for (int j = 0; j < partidas.Lista[i].JugadoresBuscando.Count; j++)
                {
                    //Si son el mismo cliente, lo borra de la cola.
                    if (partidas.Lista[i].JugadoresBuscando[j].ConnectionID == Context.ConnectionId)
                        partidas.Lista[i].JugadoresBuscando.RemoveAt(j);
                }
            }

            //Manda de nuevo las categorias a todos los clientes
            List<clsCategoriaYNumJugadoresBuscando> categories = new List<clsCategoriaYNumJugadoresBuscando>();

            foreach (clsCategoriaYJugadoresBuscando cat in partidas.Lista)
            {
                categories.Add(new clsCategoriaYNumJugadoresBuscando(cat));
            }

            Clients.All.sendCategories(categories);
        }

        #endregion

        #region game methods

        //TODO: Quitarle la categoria de la lista de parametros, no la necesita (en realidad no uso ninguno de los parametros xD)
        /// <summary>
        /// Pide las preguntas a la API.
        /// </summary>
        /// <param name="numberOfQuestions">Número de preguntas que se desean obtener</param>
        /// <param name="category">Categoría que tendrán las preguntas</param>
        /// <param name="difficulty">Dificultad de las preguntas a obtener</param>
        /// <param name="type">Tipo de las preguntas a obtener (multichoice o true/false)</param>
        /// <returns></returns>
        public async Task pedirPreguntas(int numberOfQuestions, string category, string difficulty, string type)
        {
            category = Utils.Categories.First(cat => cat.Value == category).Key;

            clsPartida partida = partidas.getMatchByConnectionID(Context.ConnectionId);

            //Si la partida todavía no tiene preguntas (necesario ya que si no se ejecutaría dos veces, ya que cada cliente lo pide una vez)
            if (partida.Preguntas.Count == 0)
            {
                List<clsQuestion> questions = await QuestionsHandler.getQuestions(numberOfQuestions, category, difficulty, type); ///////////////////////////////////////////////////////////////////////////////
                partida.Preguntas = questions;

                Clients.Client(partida.Jugador1.ConnectionID).sendQuestions(partida.Preguntas);
                Clients.Client(partida.Jugador2.ConnectionID).sendQuestions(partida.Preguntas);
            }
        }

        /// <summary>
        /// Comprueba si la respuesta dada para la pregunta dada es correcta
        /// </summary>
        /// <param name="question">La pregunta a comprobar</param>
        /// <param name="answer">La respuesta que se desea comprobar si es correcta o no</param>
        /// <param name="questionNumber">número de la pregunta (necesario en el cliente)</param>
        public void comprobarPregunta(clsQuestion question, String answer, int questionNumber)
        {
            clsQuestion questionInstanceOfTheGame;

            //Obtiene la partida del cliente que llamo a este método
            clsPartida partida = partidas.getMatchByConnectionID(Context.ConnectionId);

            //Si el cliente está en una partida
            if (partida != null)
            {
                //Si es la respuesta correcta
                if (question.correct_answer == answer)
                {
                    //isCorrect = true;

                    //Si el cliente que llamo al método es el jugador 1, le suma la pregunta acertada e indica que ya ha contestado y que la respuesta es correcta
                    if (partida.Jugador1.ConnectionID.Equals(Context.ConnectionId))
                    {
                        partida.Jugador1.PreguntasAcertadas++;
                        partida.jugador1HaContestado = true;
                        partida.jugador1EsRespuestaCorrecta = true;
                    }
                    else //Si no, significa que el cliente que llamó al método es el jugador2
                    {
                        partida.Jugador2.PreguntasAcertadas++;
                        partida.jugador2HaContestado = true;
                        partida.jugador2EsRespuestaCorrecta = true;
                    }
                }
                else //Si no es la respuesta correcta
                {
                    //isCorrect = false;

                    //Si el cliente que llamó al método es el jugador1, le suma la pregunta fallada e indica que el jugador1 ha contestado y que la respuesta no es correcta
                    if (partida.Jugador1.ConnectionID.Equals(Context.ConnectionId))
                    {
                        partida.Jugador1.PreguntasFalladas++;
                        partida.jugador1HaContestado = true;
                        partida.jugador1EsRespuestaCorrecta = false;
                    }
                    else //Si no, significa que el cliente que llamó al método es el jugador2
                    {
                        partida.Jugador2.PreguntasFalladas++;
                        partida.jugador2HaContestado = true;
                        partida.jugador2EsRespuestaCorrecta = false;
                    }
                }

                //Si ambos jugadores han contestado
                if (partida.jugador1HaContestado && partida.jugador2HaContestado)
                {
                    questionInstanceOfTheGame = partida.Preguntas.Find(p => p.question == question.question);

                    if(questionInstanceOfTheGame != null)
                    {
                        questionInstanceOfTheGame.alreadyPlayed = true;
                    }

                    //Indica de nuevo que todavía no han contestado (para la proxima vez que se llame a este método)
                    partida.jugador1HaContestado = false;
                    partida.jugador2HaContestado = false;


                    //Informa a los clientes indicando tanto si su respuesta es correcta o no como la de su rival.
                    Clients.Client(partida.Jugador1.ConnectionID).sendIsCorrect(partida.jugador1EsRespuestaCorrecta, questionNumber);
                    Clients.Client(partida.Jugador2.ConnectionID).sendRivalIsCorrect(partida.jugador1EsRespuestaCorrecta, questionNumber);

                    Clients.Client(partida.Jugador2.ConnectionID).sendIsCorrect(partida.jugador2EsRespuestaCorrecta, questionNumber);
                    Clients.Client(partida.Jugador1.ConnectionID).sendRivalIsCorrect(partida.jugador2EsRespuestaCorrecta, questionNumber);
                }

                //Clients.Caller.sendIsCorrect(isCorrect);
            }
            else //Si el cliente no está en una partida, le indica que la partida no existe (puede darse cuando uno de los dos clientes ha salido de la partida, ya que la partida desaparece)
            {
                Clients.Caller.laPartidaNoExiste();
            }
        }

        /// <summary>
        /// Hace comenzar una nueva partida, cogiendo a los dos jugadores que estén primero en la cola
        /// </summary>
        /// <param name="categoria">Categoria en la que se jugará la partida</param>
        public void comenzarPartida(clsCategoriaYJugadoresBuscando categoria)
        {
            bool clientesDisponibles = false;

            //Coge a los dos jugadores que estén primero en la cola (Aunque esto no es una cola como tal xD, estaría bien cambiarlo)
            //clsJugador jugador1 = categoria.JugadoresBuscando[0];
            //clsJugador jugador2 = categoria.JugadoresBuscando[1];

            Queue<clsJugador> colaJugadoresBuscando = new Queue<clsJugador>(categoria.JugadoresBuscando);
            clsJugador jugador1 = colaJugadoresBuscando.Dequeue();
            clsJugador jugador2 = colaJugadoresBuscando.Dequeue();


            //Crea una lista vacia de preguntas (cuando alguno de los clientes entre en la partida entonces se solicitarán las preguntas, no sabía hacerlo de otra forma, haciendolo aqui tengo problemas con la asincronia)
            List<clsQuestion> questions = new List<clsQuestion>();

            //Crea el objeto con la partida
            clsPartida partida = new clsPartida(clsPartida.IdIndex + categoria.Nombre, jugador1, jugador2, questions);

            //Añade la partida a la lista de partidas de la categoría dada
            categoria.Partidas.Add(partida);

            //Elimina de la cola a ambos jugadores que van a empezar a jugar
            String connectionID;
            int eliminadosDeCola = 0;
            //Por cada categoria o cuando ambos hayan sido eliminados de la cola
            for (int i = 0; i < partidas.Lista.Count && eliminadosDeCola < 2; i++)
            {
                //Por cada jugador en cola o cuando ambos hayan sido eliminados de la cola
                for (int j = 0; j < partidas.Lista[i].JugadoresBuscando.Count && eliminadosDeCola < 2; j++)
                {
                    connectionID = partidas.Lista[i].JugadoresBuscando[j].ConnectionID;

                    //Si es el jugador1 o jugador2, lo saca de cola
                    if (connectionID == partida.Jugador1.ConnectionID || connectionID == partida.Jugador2.ConnectionID)
                    {
                        partidas.Lista[i].JugadoresBuscando.RemoveAt(j);
                        eliminadosDeCola++;
                    }
                }
            }

            //Indica a ambos jugadores que la partida comienza, mandando la informacion de esta (nombre de ambos jugadores y lista de preguntas)
            Clients.Client(jugador1.ConnectionID).comenzarPartida(partida.Jugador1.Nombre, partida.Jugador2.Nombre, partida.Preguntas);
            Clients.Client(jugador2.ConnectionID).comenzarPartida(partida.Jugador1.Nombre, partida.Jugador2.Nombre, partida.Preguntas);
        }

        /// <summary>
        /// Finaliza una partida.
        /// </summary>
        public void terminarPartida()
        {
            //Obtiene la partida del cliente que llamó al método
            clsPartida partida = partidas.getMatchByConnectionID(Context.ConnectionId);

            //Si el cliente está en una partida
            if (partida != null)
            {
                //Por cada categoria
                for (int i = 0; i < partidas.Lista.Count; i++)
                {
                    //Por cada partida
                    for (int j = 0; j < partidas.Lista[i].Partidas.Count; j++)
                    {
                        //Si encuentra la partida, la borra de la lista
                        if (partidas.Lista[i].Partidas[j].Id == partida.Id)
                            partidas.Lista[i].Partidas.Remove(partida);
                    }
                }

                clsJugador jugador1 = partida.Jugador1;
                clsJugador jugador2 = partida.Jugador2;

                //Informa a ambos jugadores de la partida de que la partida ha terminado y envía las estadísticas de esta (preguntas acertadas y falladas de cada jugador)
                Clients.Client(jugador1.ConnectionID).mandarEstadisticasFinalPartida(jugador1.PreguntasAcertadas, jugador1.PreguntasFalladas, jugador2.PreguntasAcertadas, jugador2.PreguntasFalladas);
                Clients.Client(jugador2.ConnectionID).mandarEstadisticasFinalPartida(jugador2.PreguntasAcertadas, jugador2.PreguntasFalladas, jugador1.PreguntasAcertadas, jugador1.PreguntasFalladas);
            }
        }

        #endregion

        #region shared methods

        /// <summary>
        /// Sale del juego, haciendo que salga de la cola si está en alguna o de la partida si está en una
        /// </summary>
        public void salir()
        {
            //Sale de la cola si está en alguna
            salirDeCola();

            clsPartida partida = partidas.getMatchByConnectionID(Context.ConnectionId);

            //Si está en partida
            if (partida != null)
            {
                //Borra la partida
                for (int i = 0; i < partidas.Lista.Count; i++)
                {
                    for (int j = 0; j < partidas.Lista[i].Partidas.Count; j++)
                    {
                        if (partidas.Lista[i].Partidas[j].Id == partida.Id)
                            partidas.Lista[i].Partidas.Remove(partida);
                    }
                }

                clsJugador jugador1 = partida.Jugador1;
                clsJugador jugador2 = partida.Jugador2;

                //Indica al otro jugador de la partida que la partida ha acabado porque uno de los jugadores se ha ido
                if (jugador1.ConnectionID == Context.ConnectionId)
                    Clients.Client(jugador2.ConnectionID).jugadorSeFue();
                else
                    Clients.Client(jugador1.ConnectionID).jugadorSeFue();
            }
        }

        #endregion
    }
}