using System.Windows;

namespace EShellULoadSetTools.Views
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void SetIndeterminate(bool isIndeterminate, string? message = null)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetIndeterminate(isIndeterminate, message));
                return;
            }

            _ = isIndeterminate;
            MessageText.Text = string.IsNullOrWhiteSpace(message) ? "Please Wait" : message;
        }
    }
}
