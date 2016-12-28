using System;
using System.Linq;
using System.Windows.Forms;

namespace WindowsShutdownTimer
{
    public partial class TimerForm : Form
    {
        public TimeSpan _currentTime;
        public TimeSpan _shutdownTime;

        public TimerForm()
        {
            InitializeComponent();

            submit_Button.Enabled = false;
            description_label.AutoSize = true;
            description_label.MaximumSize = new System.Drawing.Size(325, 0);
            description_label.Text = "Please enter the number minutes from now that you would like to shut down Windows.";

            time_remaining_timer.Enabled = false;
            time_remaining_timer.Interval = 5000;   // Check time remaining every 5 seconds.
            time_remaining_desc_label.Text = "Time Remaining: ";
            time_remaining_label.Text = "N/A";

            _currentTime = new TimeSpan();
            _shutdownTime = new TimeSpan();

            stopTimerToolStripMenuItem.Enabled = false;
        }

        private void SetCurrentTime()
        {
            _currentTime = DateTime.Now.TimeOfDay;
        }

        private void SetShutdownTime(int numMinutes)
        {
            _shutdownTime = _currentTime.Add(new TimeSpan(0, numMinutes, 0));
        }

        private TimeSpan TimeRemaining()
        {
            SetCurrentTime();
            return _shutdownTime.Subtract(_currentTime);
        }

        /// TODO: Add a check to make sure the number is reasonable.
        /// TODO: Show the estimated shut off time based on the number entered in the textbox.
        /// TODO: Add the ability to maintain the current timer (maybe a countdown or something untile executed).
        /// TODO: Force the cmd prompt to be silent.
        /// TODO: Force the blue banner to not display on windows 10.
        private void addTimer_Button(object sender, EventArgs e)
        {
            string cmd;

            if(int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                DialogResult confirm = MessageBox.Show("Are you sure you want to shutdown Windows in " + numMinutes + " minutes?", "Confirm Timer", MessageBoxButtons.YesNo);

                if(confirm == DialogResult.Yes)
                {
                    // Enable the timer so we begin checking the time remaining.
                    time_remaining_timer.Enabled = true;

                    // Enable the stop timer button.
                    stopTimerToolStripMenuItem.Enabled = true;

                    // Initially set the current time.
                    SetCurrentTime();

                    // Initialize the shutdown time.
                    SetShutdownTime(numMinutes);

                    //int timeInSeconds = numMinutes * 60;
                    //cmd = "/C timeout " + timeInSeconds + " && shutdown -s";

                    cmd = "/C mkdir " + numMinutes;
                    System.Diagnostics.Process.Start("CMD.exe", cmd);

                    time_remaining_timer_Tick(null, null);
                }
            }

            else
                throw new Exception("Must enter a valid time in the minutes textbox!");
        }

        private void MinutesTextBox_TextChanged(object sender, EventArgs e)
        {
            if(MinutesTextBox.Text.Any() && MinutesTextBox.Text.All(char.IsDigit))
                submit_Button.Enabled = true;
            else
                submit_Button.Enabled = false;
        }

        private void time_remaining_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                time_remaining_label.Text = TimeRemaining().ToString("hh' hr 'mm' min 'ss' sec'");
            }
            catch
            {
                throw new Exception("Had trouble formatting the time remaining label. Please restart the application.");
            }
        }

        private void stopTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd;

            DialogResult confirm = MessageBox.Show("Are you sure you want to stop the current shutdown?", "Confirm Timer", MessageBoxButtons.YesNo);

            if (confirm == DialogResult.Yes)
            {
                cmd = "/C mkdir stop";
                var process = System.Diagnostics.Process.Start("CMD.exe", cmd);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    MessageBox.Show("Could not stop the shutdown timer for some reason. Adding an additional 5 minutes to the timer. Please run 'shutdown /a' in " +
                        "the command prompt to stop the timer.", "ERROR!", MessageBoxButtons.OK);

                    // Add an additional 5 minutes to the timer.
                    SetShutdownTime(5);
                }
                else
                {
                    // Disable the timer.
                    time_remaining_timer.Enabled = false;

                    // Disable the stop timer button again.
                    stopTimerToolStripMenuItem.Enabled = false;

                    // Reset values.
                    _currentTime = new TimeSpan(0, 0, 0);
                    _shutdownTime = new TimeSpan(0, 0, 0);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
