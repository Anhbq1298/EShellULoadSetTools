using System;
using System.Threading.Tasks;
using System.Windows;
using EShellULoadSetTools.ViewModels;

namespace EShellULoadSetTools.Views
{
    public partial class TransferULoadSetAssignmentView : Window
    {
        public TransferULoadSetAssignmentView()
        {
            InitializeComponent();
        }

        private async void SelectFloorButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TransferULoadSetAssignmentViewModel viewModel)
            {
                return;
            }

            var progressWindow = new ProgressView
            {
                Owner = this
            };

            progressWindow.Show();
            progressWindow.SetIndeterminate(true, "Retrieving selected slabs from ETABS...");

            try
            {
                await viewModel.RefreshSelectionFromEtabsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to retrieve selected slabs from ETABS:" + Environment.NewLine + ex.Message,
                    "ETABS Selection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                progressWindow.Close();
                RestorePluginAfterSelection();
            }
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

            var progressWindow = new ProgressView
            {
                Owner = this
            };

            progressWindow.Show();
            progressWindow.SetIndeterminate(true, "Transferring assignments to SAFE...");

            try
            {
                await Task.Run(() => viewModel.TransferAssignmentsToSafe());
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
