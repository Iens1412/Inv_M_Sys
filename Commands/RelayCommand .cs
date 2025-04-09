using System;
using System.Windows.Input;

namespace Inv_M_Sys.Commands
{
    /// <summary>
    /// A generic implementation of ICommand for executing parameterized actions from the UI.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to the command.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The condition that determines whether the command can execute.</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute with the provided parameter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>true if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            if (parameter is T castParam || parameter == null)
                return _canExecute == null || _canExecute((T)parameter);

            return false;
        }

        /// <summary>
        /// Executes the command action with the provided parameter.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public void Execute(object parameter)
        {
            if (parameter is T || parameter == null)
                _execute((T)parameter);
        }

        /// <summary>
        /// Occurs when changes affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}