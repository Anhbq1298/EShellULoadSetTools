// -------------------------------------------------------------
// File    : BaseViewModel.cs
// Author  : Anh Bui
// Purpose : Base class for MVVM ViewModels (INotifyPropertyChanged).
// -------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EShellULoadSetTools.ViewModels
{
    /// <summary>
    /// Simple base class implementing INotifyPropertyChanged for MVVM pattern.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notify the UI that a property has changed.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}
