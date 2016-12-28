using System;
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
        }

        private void Options_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.MinimizePref)
                minimize_to_sys_tray.Checked = true;
            else
                minimize_to_sys_tray.Checked = false;
        }

        public void save_options_button_Click(object sender, EventArgs e)
        { 
            if (minimize_to_sys_tray.Checked)
                Properties.Settings.Default.MinimizePref = true;
            else
                Properties.Settings.Default.MinimizePref = false;

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
    }
}
