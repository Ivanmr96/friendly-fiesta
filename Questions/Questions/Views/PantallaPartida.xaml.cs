using Microsoft.AspNet.SignalR.Client;
using Questions.Models;
using Questions.ViewModels;
using Questions.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace Questions
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class PantallaPartida : Page
    {
        #region attributes

        clsPantallaPartidaVM vm;
        MediaPlayer mp;
        ContentDialog dialog;
        private bool musicaEstaReproduciendose;

        #endregion

        #region constructor

        public PantallaPartida()
        {
            this.InitializeComponent();

            this.vm = ((clsPantallaPartidaVM)(this.DataContext));

            #region configuracion de la interfaz al inicio

            musicaEstaReproduciendose = false;

            respuesta1.IsTapEnabled = false;
            respuesta2.IsTapEnabled = false;
            respuesta3.IsTapEnabled = false;
            respuesta4.IsTapEnabled = false;

            framePartida.Visibility = Visibility.Collapsed;
            esperandoRival.Visibility = Visibility.Visible;
            preguntasPropias.Visibility = Visibility.Collapsed;
            preguntasRival.Visibility = Visibility.Collapsed;
            panelMatchStats.Visibility = Visibility.Collapsed;
            esperandoQueContesteElRival.Visibility = Visibility.Collapsed;
            progressRing.IsActive = true;
            progressRing.Visibility = Visibility.Visible;
            progressRingRespuesta.IsActive = true;
            progressRingRespuesta.Visibility = Visibility.Visible;

            #endregion

            #region callbacks

            Connection.Connection.proxy.On<String, String, List<clsQuestion>>("comenzarPartida", comenzarPartida);
            Connection.Connection.proxy.On<bool, int>("sendIsCorrect", receiveIsCorrect);
            Connection.Connection.proxy.On<bool, int>("sendRivalIsCorrect", receiveRivalIsCorrect);
            Connection.Connection.proxy.On<int, int, int, int>("mandarEstadisticasFinalPartida", receiveMatchStats);
            Connection.Connection.proxy.On("jugadorSeFue", jugadorSeFue);
            Connection.Connection.proxy.On("laPartidaNoExiste", conexionCerrada);

            #endregion

            #region eventos

            Connection.Connection.conn.Closed += conexionCerrada;
            //Connection.Connection.conn.Reconnecting += reconectando;
            //Connection.Connection.conn.Reconnected += reconectado;
            myStoryboard.Completed += storyBoardCompleted;
            rivalStoryboard.Completed += rivalStoryBoardCompleted;
            customStoryBoard.Completed += customStoryBoardCompleted;
            storyBoardFondoVerde.Completed += storyBoardFondoCompleted;
            storyBoardFondoRojo.Completed += storyBoardFondoCompleted;

            #endregion
        }

        #endregion

        #region public properties

        public string nombreCaja { get; private set; }

        #endregion

        #region callbacks

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique que la partida ha acabado y mande las estadísticas del final de la partida.
        /// Se encargará de mostrar en pantalla las estadísticas y de reproducir un audio en función de si el usuario gano, perdio, o empató.
        /// </summary>
        /// <param name="preguntasAcertadas">Preguntas acertadas propias</param>
        /// <param name="preguntasFalladas">Preguntas falladas propias</param>
        /// <param name="preguntasAcertadasRival">Preguntas acerdatas del rival</param>
        /// <param name="preguntasFalladasRival">Preguntas falladas del rival</param>
        public async void receiveMatchStats(int preguntasAcertadas, int preguntasFalladas, int preguntasAcertadasRival, int preguntasFalladasRival)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                panelMatchStats.Visibility = Visibility.Visible;

                pregunta.Visibility = Visibility.Collapsed;
                respuesta1.Visibility = Visibility.Collapsed;
                respuesta2.Visibility = Visibility.Collapsed;
                respuesta3.Visibility = Visibility.Collapsed;
                respuesta4.Visibility = Visibility.Collapsed;

                if (preguntasAcertadas > preguntasAcertadasRival)
                    mp.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/applause.mp3"));
                else
                    mp.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/boo.mp3"));

                mp.Play();
                musicaEstaReproduciendose = false;
            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique el rival ha abandonado la partida.
        /// Se encargará de mostrar un diálogo indicando que el oponente se ha ido con un botón para salir de la partida y volver a la pantalla principal.
        /// </summary>
        public async void jugadorSeFue()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "The match is over",
                    Content = "The opponent left the game.",
                    PrimaryButtonText = "Salir",
                };

                MostrarMensajeYRealizarAccion(dialog, salir);

                mp.Pause();
            });
        }

        

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique que se va a comenzar la partida.
        /// Se encargará de mostrar el frame con la interfaz de la partida y de habilitarlo para su uso, así como reproducir un audio con música de fondo.
        /// </summary>
        /// <param name="jugador1">Nombre del jugador1</param>
        /// <param name="jugador2">Nombre del jugador2</param>
        /// <param name="preguntas">Listado con las preguntas</param>
        public async void comenzarPartida(String jugador1, String jugador2, List<clsQuestion> preguntas)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //Reproduce un audio con música de fondo
                mp = new MediaPlayer();

                mp.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/bensound-enigmatic.mp3"));

                mp.CurrentStateChanged += mediaPlayerStateChanged;

                mp.Play();
                musicaEstaReproduciendose = true;


                //Habilita la interfaz de la partida para su uso
                esperandoRival.Visibility = Visibility.Collapsed;
                framePartida.Visibility = Visibility.Visible;
                preguntasPropias.Visibility = Visibility.Visible;
                preguntasRival.Visibility = Visibility.Visible;

                //Realiza animaciones para la lista visual de las preguntas de cada jugador
                preguntasPropiasStoryBoard.Begin();
                preguntasRivalStoryBoard.Begin();

                //Pide las preguntas al servidor si no las tiene todavía
                if (vm.questions == null)
                    vm.pedirPreguntas();

                //Actualiza la interfaz
                Frame.UpdateLayout();

                //Habilita los botones de las respuesta para que se puedan pulsar
                respuesta1.IsTapEnabled = true;
                respuesta2.IsTapEnabled = true;
                respuesta3.IsTapEnabled = true;
                respuesta4.IsTapEnabled = true;
            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor mande información sobre la respuesta a una pregunta respondida.
        /// Se encarga de quitar la ventana de "Esperando que el rival conteste".
        /// También se encargará de realizar distintas animaciones (cambiar el color del fondo durante un breve periodo de tiempo, marcar la caja de la pregunta en la interfaz, etc)
        /// con colores rojo si la respuesta es erronea o verde si el usuario ha acertado.
        /// </summary>
        /// <param name="isCorrect"></param>
        /// <param name="questionIndex"></param>
        private async void receiveIsCorrect(bool isCorrect, int questionIndex)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //Quita la ventana "esperando que conteste el rival"
                esperandoQueContesteElRival.Visibility = Visibility.Collapsed;
                framePartida.Visibility = Visibility.Visible;

                //Reinicia los bordes (cuando el usuario elige una respuesta, esta se marca con un borde acentuado, aquí se reinicia esto para la siguiente ronda)
                respuesta1.BorderThickness = new Thickness(0);
                respuesta2.BorderThickness = new Thickness(0);
                respuesta3.BorderThickness = new Thickness(0);
                respuesta4.BorderThickness = new Thickness(0);


                //myStoryboard.Stop();

                //Cambia el nombre de la caja de la pregunta, será necesario para saber a que caja de pregunta debe hacerse la animación (cambiar su color a verde/rojo)
                vm.NombreCajaPregunta = "pregunta" + questionIndex;
                vm.NotifyPropertyChanged("NombreCajaPregunta");

                //Si es correcta indica que la animación para la caja de la pregunta será verde y realizará una animación de cambo de color del fondo durante un breve periodo de tiempo)
                if (isCorrect)
                {
                    txtNotificacion.Foreground = new SolidColorBrush(Color.FromArgb(255, 144, 238, 144));
                    vm.ColorPregunta = "#FF90EE90";
                    storyBoardFondoVerde.Begin();
                }
                else //Si no, en lugar de verde será rojo.
                {
                    txtNotificacion.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 128, 128));
                    //vm.ColorPregunta = "Red";
                    vm.ColorPregunta = "#FFF08080";
                    storyBoardFondoRojo.Begin();
                }

                //Deshabilita la interfaz para que no se pueda usar mientras duren las animaciones, esto previene excepciones por cambios en atributos de los cuales las animaciones dependen.
                Frame.IsEnabled = false;

                //Realiza la animacion en la caja de la pregunta cambiandole el color
                myStoryboard.Begin();

                //Realiza una animacion en la pregunta y las respuestas.
                customStoryBoard2.Begin();

                //customStoryBoard2.Begin();

            });
        }

        /// <summary>
        /// Método que se ejecutará cuando el servidor indique si la respuesta elegida por el rival es correcta o no.
        /// Se encargará de quitar la venta "esperando que conteste el rival", así como de realizar una animación en la caja de la pregunta del rival
        /// </summary>
        /// <param name="isCorrect"></param>
        /// <param name="questionIndex"></param>
        private async void receiveRivalIsCorrect(bool isCorrect, int questionIndex)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                esperandoQueContesteElRival.Visibility = Visibility.Collapsed;
                framePartida.Visibility = Visibility.Visible;

                vm.NombreCajaPreguntaRival = "preguntaRival" + questionIndex;
                vm.NotifyPropertyChanged("NombreCajaPreguntaRival");


                if (isCorrect)
                    vm.ColorPreguntaRival = "#FF90EE90";
                else
                    //vm.ColorPreguntaRival = "Red";
                    vm.ColorPreguntaRival = "#FFF08080";

                //nombreCaja = vm.NombreCajaPregunta;

                Frame.IsEnabled = false;
                rivalStoryboard.Begin();

            });
        }

        #endregion

        #region events

        private void mediaPlayerStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Paused && musicaEstaReproduciendose)
            {
                sender.Play();
            }
        }

        //private async void reconectando()
        //{
        //    await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //    {

        //        dialog = new ContentDialog
        //        {
        //            Title = "Conection lost",
        //            Content = "There is a connection problem, trying to reconnect",
        //            PrimaryButtonText = "Salir del juego",
        //        };

        //        ContentDialogResult result = await dialog.ShowAsync();

        //        if (result == ContentDialogResult.Primary)
        //        {
        //            Application.Current.Exit();
        //            mp.Pause();
        //        }
        //    });
        //}

        //private async void reconectado()
        //{
        //    await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //    {
        //        dialog.Hide();
        //    });
        //}

        private async void conexionCerrada()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Connection lost",
                    Content = "There was a connection problema, the game will shut down.",
                    PrimaryButtonText = "Salir",
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                    Application.Current.Exit();

                //MostrarMensajeYRealizarAccion(dialog, Application.Current.Exit());

                mp.Pause();
            });
        }

        private void storyBoardFondoCompleted(object sender, Object e)
        {
            //vm.Notificacion = "";
            //vm.NotifyPropertyChanged("Notificacion");
        }

        private void customStoryBoardCompleted(object sender, Object e)
        {
            customStoryBoard.Stop();
        }

        private void storyBoardCompleted(object sender, Object e)
        {
            myStoryboard.Stop();
            Frame.IsEnabled = true;

            Border caja = (Border)this.FindName(vm.NombreCajaPregunta);

            if (vm.ColorPregunta == "#FFF08080")
                //caja.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                caja.Background = new SolidColorBrush(Color.FromArgb(255, 240, 128, 128));
            else
                caja.Background = new SolidColorBrush(Color.FromArgb(255, 144, 238, 144));

            vm.Notificacion = "";
            vm.NotifyPropertyChanged("Notificacion");
        }

        private void rivalStoryBoardCompleted(object sender, Object e)
        {
            rivalStoryboard.Stop();
            Frame.IsEnabled = true;

            Border caja = (Border)this.FindName(vm.NombreCajaPreguntaRival);

            if (vm.ColorPreguntaRival == "#FFF08080")
                //caja.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                caja.Background = new SolidColorBrush(Color.FromArgb(255, 240, 128, 128));
            else
                caja.Background = new SolidColorBrush(Color.FromArgb(255, 144, 238, 144));
        }

        private void respuesta_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //ElementSoundPlayer.State = ElementSoundPlayerState.On;
            //ElementSoundPlayer.Volume = 1;
            ElementSoundPlayer.Play(ElementSoundKind.Invoke);
            Border border = (Border)sender;
            TextBlock answer = (TextBlock)border.Child;

            if (answer.Text != "")
            {
                border.BorderThickness = new Thickness(6);
                //answer.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));


                vm.comprobarPregunta(answer.Text);

                esperandoQueContesteElRival.Visibility = Visibility.Visible;
                //framePartida.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            clsNombreUsuarioYNombreCategoria nombreYCategoria = (clsNombreUsuarioYNombreCategoria)e.Parameter;

            vm.Category = nombreYCategoria.nombreCategoria;
            vm.NombreUsuario = nombreYCategoria.nombreUsuario;

            //vm.Category = (String)e.Parameter;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            salir();
        }

        #endregion

        /// <summary>
        /// Sale de la cola         //TODO: Pa qué uso yo esto?, no sería mejor usar el método "salir" del servidor en lugar de "salirDeCola"?
        /// </summary>
        private void salir()
        {
            Connection.Connection.proxy.Invoke("salirDeCola");
            musicaEstaReproduciendose = false;

            if(mp != null)
            {
                mp.Pause();
            }
            
            Frame.Navigate(typeof(PantallaLobby));
        }

        /// <summary>
        /// Muestra un dialogo y realiza la acción dada si se pulsa en el boton indicado como Primary
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="accionPrimary"></param>
        private async void MostrarMensajeYRealizarAccion(ContentDialog mensaje, Action accionPrimary)
        {
            ContentDialogResult resultado = await mensaje.ShowAsync();

            if (resultado == ContentDialogResult.Primary)
                accionPrimary.Invoke();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Leave",
                Content = "Do you really want to leave the game? You won't be able to come back.",
                PrimaryButtonText = "Leave",
                CloseButtonText = "Cancel"
            };

            MostrarMensajeYRealizarAccion(dialog, () =>
                {
                    Connection.Connection.proxy.Invoke("salir");
                    salir();
                });
        }
    }
}
