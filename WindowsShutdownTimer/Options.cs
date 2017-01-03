using Microsoft.Win32.TaskScheduler;
using System;
using System.Windows.Forms;

namespace WindowsShutdownTimer
{
    public partial class Options : Form
    {
        #region Main

        /// <summary>
        /// The parent form object.
        /// </summary>
        TimerForm timerWindow;

        /// <summary>
        /// Sets the initial options form parameters as well as set the parent attributes.
        /// </summary>
        /// <param name="parent">This form's parent.</param>
        public Options(TimerForm parent)
        {
            InitializeComponent();

            timerWindow = parent;
            timerWindow.Enabled = false;

            // Setting this in the design menu doesn't work. Must be set here.
            this.CenterToParent();

            last_shutdown_label_desc.Text = "Last Attempted Shutdown: ";

            if (timerWindow.TimerExists(TimerForm.DEFAULT_TASK_NAME))
            {
                if (timerWindow.TimerRunning(TimerForm.DEFAULT_TASK_NAME, DateTime.Now))
                    last_shutdown_label.Text = "Pending " + "(" + Convert.ToString(Properties.Settings.Default.ShutdownTimer) + ")";
                else
                {
                    try
                    {
                        // If the timer was never run, display the time accordingly.
                        if (GetLastRunTime(TimerForm.DEFAULT_TASK_NAME) == timerWindow.SetDefaultDateTime(new DateTime()))
                            last_shutdown_label.Text = "N/A";
                        else
                        {
                            // Windows 7 doesn't have the last run time so I need to do something else.
                            if (GetLastRunTime(TimerForm.DEFAULT_TASK_NAME) == default(DateTime))
                                last_shutdown_label.Text = Convert.ToString(Properties.Settings.Default.ShutdownTimer);
                            else
                                last_shutdown_label.Text = Convert.ToString(GetLastRunTime(TimerForm.DEFAULT_TASK_NAME));
                        }
                    }
                    catch(NoTimerExists)
                    {
                        last_shutdown_label.Text = "N/A";
                    }
                }
            }
            else
                last_shutdown_label.Text = "N/A";

            timerWindow.BringFormForward();
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Gets the last time a scheduled task was run (successfully or not).
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <returns>Returns the DateTime of when the scheduled even last fired.</returns>
        public DateTime GetLastRunTime(string taskName)
        {
            if(Properties.Settings.Default.ShowShutdownNotification)
                return Properties.Settings.Default.ShutdownTimer;

            else
            {
                using (TaskService ts = new TaskService())
                {
                    // If the task exists, return the last run time.
                    if (timerWindow.TimerExists(taskName))
                    {
                        Task task = ts.GetTask(taskName);
                        return task.LastRunTime;
                    }

                    else
                        throw new NoTimerExists("The timer doesn't exist in the task scheduler.");
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// When the form loads, set the check boxes according to the user's prior settings (if any).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Options_Load(object sender, EventArgs e)
        {
            // First check box
            if (Properties.Settings.Default.MinimizeToSysTray)
                minimize_to_sys_tray.Checked = true;
            else
                minimize_to_sys_tray.Checked = false;

            // Second check box
            if (Properties.Settings.Default.LClickOpenSysTray)
                left_click_open_sys_tray.Checked = true;
            else
                left_click_open_sys_tray.Checked = false;

            // Third check box
            if (Properties.Settings.Default.ShowShutdownNotification)
                show_shutdown_notification.Checked = true;
            else
                show_shutdown_notification.Checked = false;
        }

        /// <summary>
        /// Save and close the options form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void save_options_button_Click(object sender, EventArgs e)
        {
            // Minimize program to system tray rather than to the taskbar.
            if (minimize_to_sys_tray.Checked)
                Properties.Settings.Default.MinimizeToSysTray = true;
            else
                Properties.Settings.Default.MinimizeToSysTray = false;

            // Single left click to re-show program when clicking icon in system tray.
            if (left_click_open_sys_tray.Checked)
                Properties.Settings.Default.LClickOpenSysTray = true;
            else
                Properties.Settings.Default.LClickOpenSysTray = false;

            // Display the shutdown notification from windows alerting the user of an impending shutdown.
            if (show_shutdown_notification.Checked && !Properties.Settings.Default.ShowShutdownNotification)
            {
                DialogResult confirm = MessageBox.Show("Are you sure you want to show the shutdown notifications? This may result in the " +
                    "timer being inaccurate and you not being shown the correct time remaining. In addition, it also has to create/remove a " +
                    "shutdown timer each time you load the program in order to check if one exists. So that might result in a message indicating " +
                    "that a shutdown was stopped but it was only a check. I honestly don't recommend enabling this option. Proceed with caution!"
                    , "WARNING - Are you sure?" , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                    Properties.Settings.Default.ShowShutdownNotification = true;
                else
                    return;
            }
            else
                Properties.Settings.Default.ShowShutdownNotification = false;

            Properties.Settings.Default.Save();

            // Close the options form and redisplay the parent.
            ActiveForm.Close();
            timerWindow.Enabled = true;
            timerWindow.Visible = true;

            timerWindow.BringFormForward();
        }

        /// <summary>
        /// Definitely make sure the parent is redisplayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerWindow.Enabled = true;
            timerWindow.BringFormForward();
        }

        /// <summary>
        /// Update function that will compare the current program assembly version against a web document containing
        /// the most updated version number. If the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void check_update_button_Click(object sender, EventArgs e)
        {
            var currentVersion = typeof(Options).Assembly.GetName().Version.ToString();

            System.Net.WebClient wc = new System.Net.WebClient();
            wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            string webVersion = wc.DownloadString(@"https://raw.githubusercontent.com/taylorflatt/windows-shutdown-timer/master/VERSION").TrimEnd('\n');

            Array webV = webVersion.Split('.');
            Array curV = currentVersion.Split('.');

            // Note: The user's version number should never be higher than the release version. So I don't even consider that case.
            for (int i = 0; i < 4; i++)
            {
                if (Convert.ToInt32(webV.GetValue(i)) > Convert.ToInt32(curV.GetValue(i)))
                {
                    DialogResult result = MessageBox.Show("The current version is: " + currentVersion + " and the newest version is " + webVersion + ". Would you " +
                        "like to download the newest version?", "New Version Found!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        string exeName = typeof(Options).Assembly.GetName().ToString();
                        string newFilePath = AppDomain.CurrentDomain.BaseDirectory + "WindowsShutdownTimer_v" + webVersion + ".exe";     // This should be distinct from old by version number.
                        string updatedAppLocation = "https://github.com/taylorflatt/windows-shutdown-timer/raw/master/WindowsShutdownTimer.exe";

                        try
                        {
                            wc.DownloadFile(new Uri(updatedAppLocation), newFilePath);

                            DialogResult close = MessageBox.Show("You have successfully updated to version " + webVersion + "! The new version was downloaded to the same directory as this " +
                                "program. Would you like to close this program now?", "Update Completed!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                            if (close == DialogResult.Yes)
                                Application.Exit();
                            else
                                return;
                        }

                        catch
                        {
                            DialogResult error = MessageBox.Show("There was an error attempting to grab the latest update. Would you like to " +
                                "retry or cancel?", "Error Downloading Update!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                            if (error == DialogResult.Retry)
                                check_update_button_Click(null, null);
                            else
                                return;
                        }
                    }

                    else
                        return;
                }
            }

            MessageBox.Show("The current version is: " + currentVersion + " and it is up to date!", "No New Update!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}
