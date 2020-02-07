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
using Questions.Connection;
using Windows.UI.Xaml.Controls;

namespace Questions.ViewModels
{
    public class clsPantallaLobbyVM : INotifyPropertyChanged
    {
        #region attributes

        private ObservableCollection<clsCategoriaYNumJugadoresBuscando> categories;
        private clsCategoriaYNumJugadoresBuscando selectedCategory;
        private String nombreUsuario;


        private HubConnection conn;
        private IHubProxy proxy;
        private ContentDialog dialog;

        #endregion

        #region constructor

        public clsPantallaLobbyVM()
        {
            ElementSoundPlayer.State = ElementSoundPlayerState.On;
            ElementSoundPlayer.Volume = 1;

            Message = new clsMensaje();

            SignalR();

            buscarPartida = new Comando(entrarEnCola, () => selectedCategory != null && NombreUsuario != "" && NombreUsuario != null);
            sendMessage = new Comando(executeSendMessage, canExecuteSendMessage);
        }

        #endregion

        #region public properties

        //TODO: Meter esto de la conexion en otra clase independiente
        //public static HubConnection conn;
        //public static IHubProxy proxy;

        public Comando buscarPartida { get; set; }
        public ObservableCollection<IMensaje> Messages { get; set; } = new ObservableCollection<IMensaje>();
        public IMensaje Message { get; set; }
        public Comando sendMessage { get; set; }

        public ObservableCollection<clsCategoriaYNumJugadoresBuscando> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
            }
        }

        public clsCategoriaYNumJugadoresBuscando SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;

                buscarPartida.RaiseCanExecuteChanged();

                NotifyPropertyChanged("SelectedCategory");
                NotifyPropertyChanged("Fondo");
            }
        }

        public String NombreUsuario
        {
            get { return nombreUsuario; }
            set
            {
                nombreUsuario = value;
                Message.nombre = value;
                sendMessage.RaiseCanExecuteChanged();
                buscarPartida.RaiseCanExecuteChanged();
            }
        }

        public String Fondo
        {
            get
            {
                string fondo = "";

                if (selectedCategory != null)
                {
                    fondo = "ms-appx:///Assets/images/" + selectedCategory.Id + ".jpg";
                    NotifyPropertyChanged("Fondo");
                }

                return fondo;
            }
        }

        #endregion

        #region private methods


        /// <summary>
        /// Realiza la configuracion de signalR de inicio para este ViewModel
        /// </summary>
        private async void SignalR()
        {
            //conn = new HubConnection("http://localhost:51833");
            conn = new HubConnection("https://servidorquestions.azurewebsites.net");
            Connection.Connection.conn = conn;

            proxy = conn.CreateHubProxy("QuestionsHub");
            Connection.Connection.proxy = proxy;

            proxy.On<List<clsCategoriaYNumJugadoresBuscando>>("sendCategories", receiveCategories);
            proxy.On<String, String>("newMessage", receiveMessage);

            try 
            {
                await conn.Start();
                proxy.Invoke("pedirCategorias");
            }
            catch(Exception e)
            {
                if (e.Message == "An error occurred while sending the request.")
                {
                    dialog = new ContentDialog
                    {
                        Title = "Connection error",
                        Content = "There is a connection error",
                        PrimaryButtonText = "Exit"
                    };
                }
                else
                {
                    dialog = new ContentDialog
                    {
                        Title = "Unexpected error",
                        Content = "An unexpected error occurred, try restarting the game.",
                        PrimaryButtonText = "Exit"
                    };
                }

                MostrarMensajeYRealizarAccion(dialog, () => Application.Current.Exit());
            }
        }

        private async void MostrarMensajeYRealizarAccion(ContentDialog mensaje, Action accionPrimary)
        {
            ContentDialogResult resultado = await mensaje.ShowAsync();

            if (resultado == ContentDialogResult.Primary)
                accionPrimary.Invoke();
        }

        /// <summary>
        /// Entra en la cola de la categoria seleccionada
        /// </summary>
        private void entrarEnCola()
        {
            if (selectedCategory != null)
            {
                String IdCategoria = selectedCategory.Nombre;

                proxy.Invoke("entrarEnCola", IdCategoria, nombreUsuario);

                NotifyPropertyChanged("SelectedCategory");
            }
        }

        /// <summary>
        /// Comprueba si puede ejecutar el comando para mandar mensajes en el chat
        /// </summary>
        /// <returns></returns>
        private bool canExecuteSendMessage()
        {
            return NombreUsuario != "" && NombreUsuario != null;
        }

        /// <summary>
        /// Manda un mensaje en el chat
        /// </summary>
        private void executeSendMessage()
        {
            proxy.Invoke("sendMessage", Message.nombre, Message.mensaje);
            Message.mensaje = "";
            //NotifyPropertyChanged("Message");
        }

        //public void comenzarPartida(String jugador1, String jugador2, List<clsQuestion> preguntas)
        //{

        //}

        /// <summary>
        /// Método que se ejecutará cuando el servidor mande las categorias.
        /// Se encargará de recoger las categorías y guardarlas en la lista
        /// </summary>
        /// <param name="categories"></param>
        private async void receiveCategories(List<clsCategoriaYNumJugadoresBuscando> categories)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                String idCategoriaSeleccionada = "";

                if (selectedCategory != null)
                    idCategoriaSeleccionada = selectedCategory.Id;

                this.categories = new ObservableCollection<clsCategoriaYNumJugadoresBuscando>(categories);
                NotifyPropertyChanged("Categories");

                selectedCategory = categories.Find(c => c.Id == idCategoriaSeleccionada);   //Esto es simplemente para volver a seleccionar la misma categoria que ya tenia seleccionada cuando se actualiza la lista.

                NotifyPropertyChanged("SelectedCategory");
            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor mande un mensaje en el chat.
        /// Se encargará de añadirlo a la lista de mensajes del chat.
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="mensaje"></param>
        private async void receiveMessage(string nombre, string mensaje)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Messages.Add(new DosStringToMensaje(nombre, mensaje));
                NotifyPropertyChanged("Messages");
                //this.mensaje.mensaje = "";
            });
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
