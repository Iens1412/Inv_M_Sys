using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inv_M_Sys.ViewModels
{
    /// <summary>
    /// Base class for ViewModels that implements INotifyPropertyChanged and
    /// provides a helper method for property setters.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the field to the specified value and raises OnPropertyChanged only if the value changed.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies the UI that a property has changed.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
