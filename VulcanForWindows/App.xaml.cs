using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
            m_window.ExtendsContentIntoTitleBar = true;

            if (!IsProcessRunning("VulcanForWindowsBgWorker"))
                try
                {
                    // Relative path to the executable within your application's directory
                    string relativePathToExecutable = @"VulcanForWindowsBgWorker.exe";

                    // Get the directory where your application is running from
                    string appDirectory = AppContext.BaseDirectory + "Assets\\BgWorker";

                    // Combine the application directory with the relative path to get the full path to the executable
                    string fullPathToExecutable = Path.Combine(appDirectory, relativePathToExecutable);
                    Debug.WriteLine("PATH:" + fullPathToExecutable);
                    // Check if the executable file exists
                    if (File.Exists(fullPathToExecutable))
                    {
                        // Launch the executable
                        Process.Start(fullPathToExecutable);
                    }
                    else
                    {
                        // File not found, display an error message
                        throw new FileNotFoundException("Executable file not found.", fullPathToExecutable);
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during launching the executable
                    // For example, display an error message
                    Console.WriteLine("Error: " + ex.Message);
                }

        }

        static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        private Window m_window;
    }
}
