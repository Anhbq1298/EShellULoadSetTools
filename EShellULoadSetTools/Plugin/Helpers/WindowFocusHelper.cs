using System.Windows;

namespace EShellULoadSetTools.Helpers
{
    /// <summary>
    /// Utility methods for focusing and foregrounding WPF windows.
    /// </summary>
    public static class WindowFocusHelper
    {
        /// <summary>
        /// Brings the specified window to the foreground without permanently
        /// changing its Topmost state. Avoids unnecessary toggles when the
        /// window is already active to reduce flicker.
        /// </summary>
        /// <param name="window">The window to activate.</param>
        public static void BringToFront(Window? window)
        {
            if (window == null)
            {
                return;
            }

            if (window.IsActive)
            {
                return;
            }

            bool originalTopMost = window.Topmost;

            window.Topmost = true;
            window.Activate();
            window.Topmost = originalTopMost;
        }
    }
}
