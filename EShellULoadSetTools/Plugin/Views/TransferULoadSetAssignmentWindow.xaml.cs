using System.Threading.Tasks;
using System.Windows;
using EShellULoadSetTools.ViewModels;

namespace EShellULoadSetTools.Views
{
    public partial class TransferULoadSetAssignmentWindow : Window
    {
        public TransferULoadSetAssignmentWindow()
        {
            InitializeComponent();
        }

        private void SelectFloorButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TransferULoadSetAssignmentViewModel viewModel)
            {
                return;
            }

            viewModel.RefreshSelectionFromEtabs();
            RestorePluginAfterSelection();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void TransferAssignmentButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TransferULoadSetAssignmentViewModel viewModel)
            {
                return;
            }

            var progressWindow = new ProgressWindow
            {
                Owner = this
            };

            progressWindow.Show();
            progressWindow.UpdateProgress(0, "Transferring assignments to SAFE...");

            var progress = new Progress<int>(p => progressWindow.UpdateProgress(p));

            try
            {
                await Task.Run(() => viewModel.TransferAssignmentsToSafe(progress));

                progressWindow.UpdateProgress(100);
                progressWindow.Close();

                MessageBox.Show(
                    "Uniform load set assignments transferred to SAFE.",
                    "Transfer Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                progressWindow.Close();

                MessageBox.Show(
                    "Unable to transfer assignments to SAFE:" + System.Environment.NewLine + ex.Message,
                    "SAFE Transfer Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RestorePluginAfterSelection()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
        }
    }
}
