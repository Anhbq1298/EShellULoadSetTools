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
