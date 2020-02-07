using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Models
{
    public class clsMensaje : INotifyPropertyChanged, IMensaje
    {
        private string _nombre;

        private string _mensaje;

        public clsMensaje() { }

        //public clsMensaje(string nombre, string mensaje)
        //{
        //    this._nombre = nombre;
        //    this._mensaje = mensaje;
        //}

        public string nombre
        {
            get { return _nombre; }
            set
            {
                _nombre = value;
                NotifyPropertyChanged("nombre");
            }
        }

        public string mensaje
        {
            get { return _mensaje; }
            set
            {
                _mensaje = value;
                NotifyPropertyChanged("mensaje");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
