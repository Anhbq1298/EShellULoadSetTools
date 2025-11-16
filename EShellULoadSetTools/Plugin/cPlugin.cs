// -------------------------------------------------------------
// Project : EShellULoadSetTools
// File    : cPlugin.cs
// Author  : Anh Bui
// Target  : .NET 8.0 (Windows) - Class Library
// Purpose : Entry point for ETABS plugin DLL.
// -------------------------------------------------------------

using System;
using System.Windows;
using ETABSv1;
using EShellULoadSetTools.Services;
using EShellULoadSetTools.ViewModels;
using EShellULoadSetTools.Views;

namespace EShellULoadSetTools
{
    /// <summary>
    /// Mandatory ETABS plugin entry class.
    /// Class name must be exactly 'cPlugin' so that ETABS can find it.
    /// </summary>
    public class cPlugin
    {
        /// <summary>
        /// Main entry point called by ETABS when the user runs the plugin.
        /// </summary>
        /// <param name="SapModel">ETABS cSapModel instance passed by reference.</param>
        /// <param name="ISapPlugin">Plugin callback used to notify ETABS when the plugin is finished.</param>
        public void Main(ref cSapModel SapModel, ref cPluginCallback ISapPlugin)
        {
            try
            {
                var etabsConnectionService = new EtabsConnectionService();
                etabsConnectionService.Initialize(SapModel);

                // Create ViewModel and load Shell Uniform Load Sets from ETABS.
                var viewModel = new UniformLoadSetsViewModel(etabsConnectionService);
                viewModel.LoadFromEtabs();

                // Create WPF Window as View.
                var window = new UniformLoadSetsWindow
                {
                    DataContext = viewModel,
                    Title = "Shell Uniform Load Sets",
                    Width = 360,
                    Height = 480,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                // Cache the callback into a local variable so that we can
                // safely use it inside the lambda (C# does not allow capturing
                // ref parameters directly in lambdas).
                var pluginCallback = ISapPlugin;

                // When the window is closed, tell ETABS that the plugin has finished.
                window.Closed += (s, e) =>
                {
                    try
                    {
                        // 0 = success, non-zero = error
                        pluginCallback.Finish(0);
                    }
                    catch
                    {
                        // Ignore any error when calling Finish.
                    }
                };

                // Show window as modal dialog. This call blocks until the user closes it.
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unexpected error in Uniform Load Set plugin:" + Environment.NewLine + ex.Message,
                    "ETABS Plugin Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                try
                {
                    // Use non-zero exit code on error.
                    ISapPlugin.Finish(1);
                }
                catch
                {
                    // Ignore any secondary error.
                }
            }
        }

        /// <summary>
        /// Called by ETABS when the user clicks Info in the Plugin Manager.
        /// </summary>
        /// <param name="Text">Description text shown by ETABS.</param>
        /// <returns>0 if successful.</returns>
        public int Info(ref string Text)
        {
            Text =
                "Shell Uniform Load Sets Viewer" + Environment.NewLine +
                "Author: Anh Bui" + Environment.NewLine + Environment.NewLine +
                "Displays all Shell Uniform Load Sets defined in the current ETABS model " +
                "in a simple tree structure: a root node 'Shell Uniform Load Sets' and " +
                "one child node per defined load set.";
            return 0;
        }
    }
}
