using System;
using System.Windows.Forms;

namespace WindowsShutdownTimer
{
    public partial class Options : Form
    {
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

            if(timerWindow.TimerExists(TimerForm.DEFAULT_TASK_NAME))
            {
                if(timerWindow.TimerRunning(TimerForm.DEFAULT_TASK_NAME, DateTime.Now))
                    last_shutdown_label.Text = "Pending " + "(" + Convert.ToString(Properties.Settings.Default.ShutdownTimer) + ")";
                else
                    last_shutdown_label.Text = Convert.ToString(timerWindow.GetLastRunTime(TimerForm.DEFAULT_TASK_NAME));
            }
            else
                last_shutdown_label.Text = "N/A";
        }

        /// <summary>
        /// When the form loads, set the check boxes according to the user's prior settings (if any).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Options_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.MinimizeToSysTray)
                minimize_to_sys_tray.Checked = true;
            else
                minimize_to_sys_tray.Checked = false;

            if (Properties.Settings.Default.LClickOpenSysTray)
                left_click_open_sys_tray.Checked = true;
            else
                left_click_open_sys_tray.Checked = false;
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

            Properties.Settings.Default.Save();
            //timerWindow.ApplyUserSettings();

            // Close the options form and redisplay the parent.
            ActiveForm.Close();
            timerWindow.Enabled = true;
            timerWindow.Visible = true;
        }

        /// <summary>
        /// Definitely make sure the parent is redisplayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerWindow.Enabled = true;
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
            for(int i = 0; i < 4; i++)
            {
                if(Convert.ToInt32(webV.GetValue(i)) > Convert.ToInt32(curV.GetValue(i)))
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
    }
}
