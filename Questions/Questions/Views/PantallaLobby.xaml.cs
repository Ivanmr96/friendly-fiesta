using Microsoft.AspNet.SignalR.Client;
using Questions.Models;
using Questions.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace Questions.Views
{
    public sealed partial class PantallaLobby : Page
    {
        public clsPantallaLobbyVM ViewModel { get; }
        private ContentDialog dialog;

        public PantallaLobby()
        {
            this.InitializeComponent();

            ViewModel = ((clsPantallaLobbyVM)(this.DataContext));

            //Connection.Connection.conn.Error += connectionError;
            //Connection.Connection.conn.Closed += connectionClosed;
            Connection.Connection.conn.Reconnecting += reconectando;
            Connection.Connection.conn.Reconnected += reconectado;

        }

        private async void reconectando()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

                dialog = new ContentDialog
                {
                    Title = "Conection lost",
                    Content = "There is a connection problem, trying to reconnect",
                    PrimaryButtonText = "Salir del juego",
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    Application.Current.Exit();
                }
            });
        }

        private async void reconectado()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                dialog.Hide();
            });
        }

        //private void connectionClosed()
        //{
        //    mostrarErrorDeConexion();
        //}

        //private async void mostrarErrorDeConexion()
        //{
        //    await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //    {
        //        ContentDialog dialog = new ContentDialog
        //        {
        //            Title = "Connection error",
        //            Content = "There is a connection error",
        //            PrimaryButtonText = "Exit"
        //        };

        //        MostrarMensajeYRealizarAccion(dialog, () => Application.Current.Exit());
        //    });
        //}

        //private void connectionError(Exception obj)
        //{
        //    mostrarErrorDeConexion();
        //}

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

        /// <summary>
        /// Evento asociado al click del boton "Play".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Mandar el nick del usuario tambien
            clsNombreUsuarioYNombreCategoria nombreYCategoria = new clsNombreUsuarioYNombreCategoria(ViewModel.NombreUsuario, ViewModel.SelectedCategory.Nombre);

            Frame.Navigate(typeof(PantallaPartida), nombreYCategoria);
        }
    }
}
