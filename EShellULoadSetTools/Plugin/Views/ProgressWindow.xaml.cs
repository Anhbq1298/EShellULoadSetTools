using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace EShellULoadSetTools.Views
{
    public partial class ProgressWindow : Window
    {
        private Storyboard? _spinnerAnimation;

        public ProgressWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            StartSpinner();
        }

        private void StartSpinner()
        {
            _spinnerAnimation ??= (Storyboard)FindResource("SpinnerAnimation");
            _spinnerAnimation.Begin(this, true);
        }

        private void StopSpinner()
        {
            _spinnerAnimation ??= (Storyboard)FindResource("SpinnerAnimation");
            _spinnerAnimation.Stop(this);
        }

        public void SetIndeterminate(bool isIndeterminate, string? message = null)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetIndeterminate(isIndeterminate, message));
                return;
            }

            SpinnerContainer.Visibility = isIndeterminate ? Visibility.Visible : Visibility.Collapsed;
            if (isIndeterminate)
            {
                StartSpinner();
            }
            else
            {
                StopSpinner();
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
            SpinnerContainer.Visibility = Visibility.Visible;
            StartSpinner();

            if (!string.IsNullOrWhiteSpace(message))
            {
                MessageText.Text = message;
            }
            else
            {
                MessageText.Text = $"Processing... {clamped}%";
            }
        }
    }
}
