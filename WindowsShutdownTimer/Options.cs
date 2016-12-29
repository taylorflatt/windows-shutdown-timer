using System;
using System.Deployment.Application;
using System.IO;
using System.Windows.Forms;

namespace WindowsShutdownTimer
{
    public partial class Options : Form
    {
        TimerForm timerWindow;

        public Options(TimerForm parent)
        {
            InitializeComponent();

            timerWindow = parent;
            timerWindow.Enabled = false;

            last_shutdown_label_desc.Text = "Last Shutdown: ";

            // Check on the status of the previous shutdown. If it was ever set or cancelled. 
            // Or if it is still in the future (pending). Or successful then display when.
            if (Properties.Settings.Default.ShutdownTimer == default(DateTime))
                last_shutdown_label.Text = "N/A or Unsuccessful";
            else if(Properties.Settings.Default.ShutdownTimer > DateTime.UtcNow.ToLocalTime())
                last_shutdown_label.Text = "Pending " + "(" + Convert.ToString(Properties.Settings.Default.ShutdownTimer.ToLocalTime()) + ")";
            else
                last_shutdown_label.Text = Convert.ToString(Properties.Settings.Default.ShutdownTimer.ToLocalTime());
        }

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

            ActiveForm.Close();
            timerWindow.Enabled = true;
            timerWindow.Visible = true;
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerWindow.Enabled = true;
        }

        private void check_update_button_Click(object sender, EventArgs e)
        {
            var currentVersion = typeof(Options).Assembly.GetName().Version.ToString();

            System.Net.WebClient wc = new System.Net.WebClient();
            string webVersion = wc.DownloadString(@"https://raw.githubusercontent.com/taylorflatt/windows-shutdown-timer/master/VERSION").TrimEnd('\n');

            if (webVersion != currentVersion)
            {
                DialogResult result = MessageBox.Show("The current version is: " + currentVersion + " and the newest version is " + webVersion + ". Would you " +
                    "like to download the newest version?", "New Version Found", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    string exeName = typeof(Options).Assembly.GetName().ToString();
                    string newFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + exeName + "v_" + currentVersion;     // This should be distinct from old by version number.
                    string updatedAppLocation = "https://github.com/taylorflatt/windows-shutdown-timer/blob/master/WindowsShutdownTimer.exe";

                    wc.DownloadFile(updatedAppLocation, newFilePath);
                }
            }
        }
    }
}
