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
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The function that determines whether the command can execute.</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            if (parameter is T castParam || parameter == null)
                return _canExecute == null || _canExecute((T)parameter);
            return false;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            if (parameter is T castParam || parameter == null)
                _execute((T)parameter);
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    /// <summary>
    /// A non-generic implementation of ICommand for parameterless actions.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The function that determines whether the command can execute.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        /// <inheritdoc />
        public void Execute(object parameter) => _execute();

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
