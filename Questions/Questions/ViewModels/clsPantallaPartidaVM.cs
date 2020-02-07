using Boton.Models.Utils;
using Microsoft.AspNet.SignalR.Client;
using Questions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Questions.ViewModels
{
    public class clsPantallaPartidaVM : INotifyPropertyChanged
    {
        #region attributes

        private HubConnection conn;
        private IHubProxy proxy;

        private int questionIndex { get; set; }     //Usado para mantener el indice de la pregunta que se está jugando 
        //private int turn;
        private String nombreUsuario;
        private String nombreRival;
        private int numberOfQuestions = 10;
        private string category;
        private string difficulty;
        private string type = "multiple";
        private IQuestion selectedQuestion;
        private string colorPregunta;               //Usado para establecer qué color tendrá el cuadrado que representa la pregunta que se ha contestado
        private string colorPreguntaRival;
        private bool haRecibidoRespuestaRival;      //Se usan para saber si ha recibido la respuesta para la pregunta que se está jugando, tanto el usuario como su rival. (en ambos casos hace que salte a la siguiente pregunta)
        private bool haRecibidoRespuesta;
        private int nextQuestionIndex;              //Usado para conocer cual es el índice para la siguiente pregunta.

        #endregion

        #region constructor

        public clsPantallaPartidaVM()
        {
            SignalR();

            questionIndex = 1;
            PreguntasAcertadas = 0;
            PreguntasFalladas = 0;

            cargarPreguntas = new Comando(pedirPreguntas);
            siguientePregunta = new Comando(executeSiguientePregunta);
        }

        #endregion

        #region public properties

        public Comando cargarPreguntas { get; set; }
        public Comando siguientePregunta { get; set; }

        public String Notificacion { get; set; }
        public int PreguntasAcertadas { get; set; }
        public int PreguntasFalladas { get; set; }
        public int PreguntasAcertadasRival { get; set; }
        public int PreguntasFalladasRival { get; set; }
        public String TextoHasGanadoOPerdido { get; set; }

        public ObservableCollection<IQuestion> questions { get; set; }

        public IQuestion SelectedQuestion
        {
            get { return selectedQuestion; }
            set
            {
                selectedQuestion = value;
                NotifyPropertyChanged("SelectedQuestion");
            }
        }

        public String Category
        {
            get { return category; }
            set
            {
                category = value;
                NotifyPropertyChanged("Category");
            }
        }

        public int NumeroPregunta       //Usada para representar en la interfaz qué número de pregunta se está jugando
        {
            get
            {
                return questionIndex;
            }
        }

        //public int Turn
        //{
        //    get { return turn; }
        //    set
        //    {
        //        turn = value;
        //    }
        //}

        public String ColorPregunta
        {
            get
            {
                return colorPregunta;
            }
            set
            {
                colorPregunta = value;
                NotifyPropertyChanged("ColorPregunta");
            }
        }


        public String ColorPreguntaRival
        {
            get
            {
                return colorPreguntaRival;
            }
            set
            {
                colorPreguntaRival = value;
                NotifyPropertyChanged("ColorPreguntaRival");
            }
        }

        /// <summary>
        /// Representa el valor que tiene la propiedad name del control Border al cual se le realizará una animación cuando se conteste la pregunta.
        /// Cada caja representa una de las preguntas de la partida, en esta propiedad se guardará el name de esa caja (para cada pregunta) para saber a cual se le realiza la animación.
        /// </summary>
        public String NombreCajaPregunta
        {
            get;
            set;
        }

        public String NombreCajaPreguntaRival
        {
            get;
            set;
        }

        public String NombreUsuario
        {
            get { return nombreUsuario; }
            set
            {
                nombreUsuario = value;
            }
        }

        public String NombreRival
        {
            get { return nombreRival; }
            set
            {
                nombreRival = value;
            }
        }

        #endregion

        #region callbacks

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique que la partida ha finalizado y mande las estadísticas finales de la partida.
        /// Se encargará de indicar el número de preguntas acertdas y falladas del rival, asi como de establecer que mensaje pondrá en la venta de las estadisticas finales,
        /// en función de si el usuario gano, perdio o empató.
        /// </summary>
        /// <param name="preguntasAcertadas">Número de preguntas acertadas</param>
        /// <param name="preguntasFalladas">Número de preguntas falladas</param>
        /// <param name="preguntasAcertadasRival">Número de preguntas acertadas del rival</param>
        /// <param name="preguntasFalladasRival">Número de preguntas falladas del rival</param>
        public async void receiveMatchStats(int preguntasAcertadas, int preguntasFalladas, int preguntasAcertadasRival, int preguntasFalladasRival)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.PreguntasAcertadasRival = preguntasAcertadasRival;
                this.PreguntasFalladasRival = preguntasFalladasRival;

                if (preguntasAcertadas > preguntasAcertadasRival)
                    TextoHasGanadoOPerdido = "Congratulations, you won!";
                else if (preguntasAcertadas < preguntasAcertadasRival)
                    TextoHasGanadoOPerdido = "Oh no, you lost!";
                else
                    TextoHasGanadoOPerdido = "The game ended in a draw.";

                NotifyPropertyChanged("PreguntasAcertadasRival");
                NotifyPropertyChanged("PreguntasFalladasRival");
                NotifyPropertyChanged("TextoHasGanadoOPerdido");
            });
        }

        //TODO Creo que puedo quitar de la lista de parametros las preguntas
        /// <summary>
        /// Método que se ejecutará cuando el servidor indique que la partida ha comenzado.
        /// Se encargará de establecer el nombre del rival.
        /// </summary>
        /// <param name="jugador1">Nombre del jugador1</param>
        /// <param name="jugador2">Nombre del jugador2</param>
        /// <param name="preguntas">Listado de preguntas de la partida</param>
        public async void comenzarPartida(String jugador1, String jugador2, List<clsQuestion> preguntas)
        {
            //Frame.Navigate(typeof(MainPage), preguntas);
            //Frame.Navigate(typeof(MainPage));
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (jugador1 == nombreUsuario)
                    nombreRival = jugador2;
                else
                    nombreRival = jugador1;

                NotifyPropertyChanged("NombreRival");
            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor envíe la lista de preguntas.
        /// Se encargará de guardar el listado de preguntas.
        /// </summary>
        /// <param name="questions">El listado de preguntas de la partida</param>
        private async void receiveQuestions(List<clsQuestion> questions)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (this.questions == null)
                {
                    this.questions = new ObservableCollection<IQuestion>(questions);
                    NotifyPropertyChanged("questions");

                    selectedQuestion = questions.ElementAt<clsQuestion>(0);
                    NotifyPropertyChanged("SelectedQuestion");
                }
            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique si la pregunta contestada es correcta o no.
        /// Se encarga de mostrar una notificación indicando si la pregunta contestada es correcta o no, así como de sumar el número
        /// de preguntas acertadas o falladas.
        /// También se encargará de mostrar la siguiente pregunta si se ha recibido la información sobre si la respuesta del rival es correcta también.
        /// Si ha contestado la última pregunta, indicará al servidor que la partida ha terminado.
        /// </summary>
        /// <param name="isCorrect">Indica si la respuesta dada es la correcta o no</param>
        /// <param name="questionNumber">número de la pregunta, será útil para saber cuál es la siguiente pregunta</param>
        private async void receiveIsCorrect(bool isCorrect, int questionNumber)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (isCorrect)
                {
                    establecerNotificacion("Correct!");
                    PreguntasAcertadas++;
                    NotifyPropertyChanged("PreguntasAcertadas");
                }

                else
                {
                    establecerNotificacion("That's not the answer ;(");
                    PreguntasFalladas++;
                    NotifyPropertyChanged("PreguntasFalladas");
                }


                haRecibidoRespuesta = true;

                if (!haRecibidoRespuestaRival)
                {
                    nextQuestionIndex = questionNumber + 1;
                    executeSiguientePregunta();
                    haRecibidoRespuestaRival = false;
                    haRecibidoRespuesta = false;

                    //NotifyPropertyChanged("NumeroPregunta");
                    //NotifyPropertyChanged("NombreCajaPregunta");
                }
                if (questionIndex > 10)
                {
                    proxy.Invoke("terminarPartida");
                }

            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique si la pregunta contestada por el rival es correcta o no.
        /// Se encargará de mostrar la siguiente pregunta si se ha recibido la información sobre si la respuesta es correcta del usuario también.
        /// </summary>
        /// <param name="isCorrect">Indica si la respuesta dada es la correcta o no</param>
        /// <param name="questionNumber">número de la pregunta, será útil para saber cuál es la siguiente pregunta</param>
        private async void receiveIsCorrectRival(bool isCorrect, int questionNumber)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                haRecibidoRespuestaRival = true;

                if (!haRecibidoRespuesta)
                {
                    nextQuestionIndex = questionNumber + 1;
                    executeSiguientePregunta();
                    haRecibidoRespuesta = false;
                    haRecibidoRespuestaRival = false;
                }
            });
        }

        #endregion

        #region private methods


        /// <summary>
        /// Realiza la configuración inicial de signalR para este viewmodel.
        /// </summary>
        private async void SignalR()
        {
            proxy = Connection.Connection.proxy;
            conn = Connection.Connection.conn;

            proxy.On<List<clsQuestion>>("sendQuestions", receiveQuestions);
            proxy.On<bool, int>("sendIsCorrect", receiveIsCorrect);
            proxy.On<bool, int>("sendRivalIsCorrect", receiveIsCorrectRival);
            proxy.On<String, String, List<clsQuestion>>("comenzarPartida", comenzarPartida);
            proxy.On<int, int, int, int>("mandarEstadisticasFinalPartida", receiveMatchStats);

            proxy.On<int>("questionBeingPlayed", receiveQuestionBeingPlayed);
        }

        private void receiveQuestionBeingPlayed(int newQuestionIndex)
        {
            questionIndex = newQuestionIndex+1;
            selectedQuestion = questions.ElementAt(questionIndex - 1);

            NotifyPropertyChanged("SelectedQuestion");
            NotifyPropertyChanged("NumeroPregunta");
        }

        /// <summary>
        /// Muestra la siguiente pregunta, si no quedan más preguntas mostrará una notificación indicándolo.
        /// </summary>
        private void executeSiguientePregunta()
        {

            if (nextQuestionIndex - 1 < questions.Count())
            {
                questionIndex = nextQuestionIndex;
                selectedQuestion = questions.ElementAt(questionIndex - 1);
            }
            else
            {
                questionIndex = nextQuestionIndex;
                establecerNotificacion("No quedan más preguntas");
                selectedQuestion = null;

                //NotifyPropertyChanged("NombreCajaPregunta");
            }
            NotifyPropertyChanged("SelectedQuestion");
            NotifyPropertyChanged("NumeroPregunta");
        }

        /// <summary>
        /// Muestra una notificación con el mensaje dado.
        /// </summary>
        /// <param name="mensaje"></param>
        private void establecerNotificacion(String mensaje)
        {
            Notificacion = mensaje;
            NotifyPropertyChanged("Notificacion");
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pide las preguntas de la partida al servidor
        /// </summary>
        public async void pedirPreguntas()
        {
            //proxy.Invoke("Send", mensaje.nombre, mensaje.mensaje);
            if (conn.State == ConnectionState.Connected)
            {
                //category = "Science & Nature";
                await proxy.Invoke("pedirPreguntas", numberOfQuestions, category, difficulty, type);

                if (questions != null)
                {
                    selectedQuestion = questions.ElementAt(questionIndex - 1);
                    NotifyPropertyChanged("SelectedQuestion");
                    NotifyPropertyChanged("NombreCajaPregunta");
                }
            }
            else
            {
                pedirPreguntas();
            }
        }

        /// <summary>
        /// Pide al servidor que compruebe la respuesta para la pregunta que se está actualmente jugando.
        /// </summary>
        /// <param name="answer"></param>
        public void comprobarPregunta(String answer)
        {
            if (selectedQuestion != null)
            {
                if (conn.State == ConnectionState.Connected)
                {
                    proxy.Invoke("comprobarPregunta", selectedQuestion, answer, questionIndex);
                }
                else
                {
                    try
                    {
                        conn.Start();
                    }
                    catch (Exception e)
                    {
                        Application.Current.Exit();
                    }
                }
            }
        }

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
