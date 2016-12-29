using System;
using System.Deployment.Application;
using System.IO;
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
        /// <param name="parent"></param>
        public Options(TimerForm parent)
        {
            InitializeComponent();

            timerWindow = parent;
            timerWindow.Enabled = false;

            last_shutdown_label_desc.Text = "Last Shutdown: ";

            DateTime defaultDate = new DateTime(0002, 1, 1, 0, 0, 0, 0);
                
            // Check on the status of the previous shutdown. If it was ever set or cancelled. 
            // Or if it is still in the future (pending). Or successful then display when.
            if (Properties.Settings.Default.ShutdownTimer == defaultDate)
                last_shutdown_label.Text = "N/A or Unsuccessful";
            else if(Properties.Settings.Default.ShutdownTimer > DateTime.UtcNow.ToLocalTime())
                last_shutdown_label.Text = "Pending " + "(" + Convert.ToString(Properties.Settings.Default.ShutdownTimer.ToLocalTime()) + ")";
            else
                last_shutdown_label.Text = Convert.ToString(Properties.Settings.Default.ShutdownTimer.ToLocalTime());
        }

        /// <summary>
        /// When the form loads, set the check boxes according to the user's prior settings (if any).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Options_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.MinimizePref)
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
                Properties.Settings.Default.MinimizePref = true;
            else
                Properties.Settings.Default.MinimizePref = false;
            
            // Single left click to re-show program when clicking icon in system tray.
            if (left_click_open_sys_tray.Checked)
                Properties.Settings.Default.LClickOpenSysTray = true;
            else
                Properties.Settings.Default.LClickOpenSysTray = false;

            Properties.Settings.Default.Save();
            timerWindow.ApplyUserSettings();

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

            for(int i = 0; i < 4; i++)
            {
                if(Convert.ToInt32(webV.GetValue(i)) > Convert.ToInt32(curV.GetValue(i)))
                {
                    DialogResult result = MessageBox.Show("The current version is: " + currentVersion + " and the newest version is " + webVersion + ". Would you " +
                        "like to download the newest version?", "New Version Found!", MessageBoxButtons.YesNoCancel);

                    if (result == DialogResult.Yes)
                    {
                        string exeName = typeof(Options).Assembly.GetName().ToString();
                        string newFilePath = AppDomain.CurrentDomain.BaseDirectory + "WindowsShutdownTimer_v" + webVersion + ".exe";     // This should be distinct from old by version number.
                        string updatedAppLocation = "https://github.com/taylorflatt/windows-shutdown-timer/raw/master/WindowsShutdownTimer.exe";

                        try
                        {
                            wc.DownloadFile(new Uri(updatedAppLocation), newFilePath);

                            DialogResult close = MessageBox.Show("You have successfully updated to version " + webVersion + "! The new version was downloaded to the same directory as this " +
                                "program. Would you like to close this program now?", "Update Completed!", MessageBoxButtons.YesNo);

                            if (close == DialogResult.Yes)
                                Application.Exit();
                            else
                                return;
                        }

                        catch
                        {
                            DialogResult error = MessageBox.Show("There was an error attempting to grab the newest update. Would you like to " +
                                "retry or cancel?", "Error Downloading Update!", MessageBoxButtons.RetryCancel);

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

            MessageBox.Show("The current version is: " + currentVersion + " and it is up to date!", "No New Update!", MessageBoxButtons.OK);
        }
    }
}
