using System.Windows;
using EShellULoadSetTools.ViewModels;

namespace EShellULoadSetTools.Views
{
    public partial class TransferULoadSetAssignmentWindow : Window
    {
        private WindowState _previousWindowState = WindowState.Normal;

        public TransferULoadSetAssignmentWindow()
        {
            InitializeComponent();
        }

        private void SelectFloorButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TransferULoadSetAssignmentViewModel)
            {
                return;
            }

            HidePluginForEtabsSelection();
        }

        private void DoneSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TransferULoadSetAssignmentViewModel viewModel)
            {
                return;
            }

            RestorePluginAfterSelection();
            viewModel.RefreshSelectionFromEtabs();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HidePluginForEtabsSelection()
        {
            _previousWindowState = WindowState;
            Topmost = false;
            WindowState = WindowState.Minimized;
        }

        private void RestorePluginAfterSelection()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = _previousWindowState;
            }

            Activate();
        }
    }
}
