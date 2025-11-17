using System;
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

            ProgressBar.IsIndeterminate = isIndeterminate;
            PercentageText.Visibility = isIndeterminate ? Visibility.Collapsed : Visibility.Visible;

            if (isIndeterminate)
            {
                PercentageText.Text = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                MessageText.Text = message;
            }
        }

        public void UpdateProgress(int percent, string? message = null)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateProgress(percent, message));
                return;
            }

            int clamped = Math.Max(0, Math.Min(100, percent));
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = clamped;
            PercentageText.Text = $"{clamped}%";
            PercentageText.Visibility = Visibility.Visible;

            if (!string.IsNullOrWhiteSpace(message))
            {
                MessageText.Text = message;
            }
        }
    }
}
