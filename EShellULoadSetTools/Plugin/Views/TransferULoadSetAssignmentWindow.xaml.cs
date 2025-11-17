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

            MessageBox.Show(
                "ETABS will appear. Select the floor (shell/area) objects you want to transfer and then return to this window.",
                "Select Floors in ETABS",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            viewModel.RefreshSelectionFromEtabs();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
