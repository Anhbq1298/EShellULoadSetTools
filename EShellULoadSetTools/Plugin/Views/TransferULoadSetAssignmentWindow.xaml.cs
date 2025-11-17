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

            MessageBox.Show(
                "ETABS will appear. Select the floor (shell/area) objects you want to transfer and then click 'Done Selection' in this window to import them.",
                "Select Floors in ETABS",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

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
