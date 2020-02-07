    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Boton.Models.Utils
{
    public class Comando : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Se "levanta" cuando RaiseCanExecuteChanged es llamado.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Crea un comando que puede ser ejecutado siempre
        /// </summary>
        /// <param name="execute"></param>
        public Comando(Action execute)
        {
            this.execute = execute;

            //CanExecuteChanged += (ICommand, EventArgs) => Console.WriteLine("yo");
        }


        /// <summary>
        /// Crea un nuevo comando.
        /// </summary>
        /// <param name="execute">La acción a ejecutar.</param>
        /// <param name="canExecute">La lógica del estado de la ejecución (si puede ejecutar o no)</param>
        public Comando(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Determina si la acción puede ejecutar o no
        /// </summary>
        /// <param name="parameter">Los datos usados por el comando, si no necesita datos, puede ser null.</param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(); //Si es null, devuelve true, si no, consulta la función definida en "canExecute".
        }

        /// <summary>
        /// Ejecuta la acción
        /// </summary>
        /// <param name="parameter">Los datos usados por el comando, si no necesita datos, puede ser null.</param>
        public void Execute(object parameter)
        {
            execute();
        }

        //Método usado para "levantar" el evento CanExecuteChanged para indicar que el valor de retorno de CanExecute ha cambiado. <- Esto no lo entiendo muy bien
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;

            if (handler != null)
                handler(this, EventArgs.Empty);

            //Otra forma de realizar lo mismo:
            //CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        }
    }
}
