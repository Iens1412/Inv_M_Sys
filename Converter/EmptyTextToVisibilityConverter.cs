using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Inv_M_Sys.Converter
{
    /// <summary>
    /// Converts an empty or whitespace string into a Visibility value.
    /// If the string is empty or null, returns Visible; otherwise, Collapsed.
    /// Commonly used to show placeholder text in TextBox controls.
    /// </summary>
    public class EmptyTextToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string value to Visibility.
        /// </summary>
        /// <param name="value">The bound value (typically a string).</param>
        /// <param name="targetType">The target property type (Visibility).</param>
        /// <param name="parameter">Optional converter parameter (not used).</param>
        /// <param name="culture">The culture info.</param>
        /// <returns>Visible if string is null or whitespace; otherwise, Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// This method is not implemented because the converter is intended for one-way binding only,
        /// from string to Visibility. Attempting to use ConvertBack will throw an exception.
        /// </summary>
        /// <param name="value">The value from the target (not used).</param>
        /// <param name="targetType">The expected source type (not used).</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture info (not used).</param>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Always thrown since this is a one-way converter.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented in EmptyTextToVisibilityConverter because it's intended for one-way use.");
        }
    }
}