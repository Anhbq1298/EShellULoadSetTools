// -------------------------------------------------------------
// File    : UniformLoadSetsWindow.xaml.cs
// Author  : Anh Bui
// Purpose : Simple WPF Window acting as View for Shell Uniform Load Sets.
// -------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using EShellULoadSetTools.ViewModels;

namespace EShellULoadSetTools.Views
{
    public partial class UniformLoadSetsWindow : Window
    {
        public UniformLoadSetsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window; plugin will call Finish() in cPlugin via Closed event.
            this.Close();
        }

        /// <summary>
        /// When the user selects a node in the TreeView, update the ViewModel.SelectedNode
        /// so that the DataGrid on the right can display its records.
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is UniformLoadSetsViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue as UniformLoadSetNodeViewModel;
            }
        }
    }
}
