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

        private void TransferAssignmentButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TransferULoadSetAssignmentViewModel viewModel)
            {
                return;
            }

            try
            {
                viewModel.TransferAssignmentsToSafe();
                MessageBox.Show(
                    "Uniform load set assignments transferred to SAFE.",
                    "Transfer Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
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
