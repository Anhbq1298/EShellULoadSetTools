// -------------------------------------------------------------
// File    : UniformLoadSetsWindowView.xaml.cs
// Author  : Anh Bui
// Purpose : Simple WPF Window acting as View for Shell Uniform Load Sets.
// -------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using EShellULoadSetTools.ViewModels;

namespace EShellULoadSetTools.Views
{
    public partial class UniformLoadSetsWindowView : Window
    {
        private WindowState _previousWindowState = WindowState.Normal;

        public UniformLoadSetsWindowView()
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

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UniformLoadSetsViewModel viewModel)
            {
                viewModel.SelectAllLeafNodes();
            }
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UniformLoadSetsViewModel viewModel)
            {
                viewModel.DeselectAllLeafNodes();
            }
        }

        private void AttachToSafeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UniformLoadSetsViewModel viewModel)
            {
                viewModel.AttachToSafe();
            }
        }

        private void ApplyToSafeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UniformLoadSetsViewModel viewModel)
            {
                try
                {
                    viewModel.ApplyToSafe();
                    MessageBox.Show(
                        "Shell Uniform Load Sets were transferred to the attached SAFE model.",
                        "SAFE Import",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Unable to apply Shell Uniform Load Sets to SAFE:" + Environment.NewLine + ex.Message,
                        "SAFE Import Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void TransferULoadSetAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not UniformLoadSetsViewModel viewModel)
            {
                return;
            }

            var assignmentViewModel = viewModel.CreateTransferAssignmentViewModel();
            var window = new TransferULoadSetAssignmentWindow
            {
                DataContext = assignmentViewModel,
                Owner = this
            };

            window.Closed += TransferAssignmentWindowOnClosed;
            window.Show();

            _previousWindowState = WindowState;
            WindowState = WindowState.Minimized;
        }

        private void TransferAssignmentWindowOnClosed(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = _previousWindowState;
            }

            Activate();

            if (sender is Window window)
            {
                window.Closed -= TransferAssignmentWindowOnClosed;
            }
        }
    }
}
