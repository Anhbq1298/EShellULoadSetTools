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

        public void UpdateProgress(int percent, string? message = null)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateProgress(percent, message));
                return;
            }

            int clamped = Math.Max(0, Math.Min(100, percent));
            ProgressBar.Value = clamped;
            PercentageText.Text = $"{clamped}%";

            if (!string.IsNullOrWhiteSpace(message))
            {
                MessageText.Text = message;
            }
        }
    }
}
