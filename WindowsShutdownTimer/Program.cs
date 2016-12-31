using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WindowsShutdownTimer
{
    static class Program
    {
        /// <summary>
        /// Sets the foreground window.
        /// </summary>
        /// <param name="winHandle">Handle for the window that will be brought to the foreground.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr winHandle);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var mutex = new Mutex(true, "WindowsShutdownTimerMutex", out bool unqiue))
            {
                if(unqiue)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new TimerForm());
                }

                else
                {
                    // Exits program and brings the other process to the foreground.
                    Process current = Process.GetCurrentProcess();
                    foreach(Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if(process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            return;
                        }
                    }
                }
            }
        }
    }
}
