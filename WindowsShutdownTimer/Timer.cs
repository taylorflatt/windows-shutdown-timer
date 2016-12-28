using System;
using System.Diagnostics;
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
            description_label.Text = "Please enter the number minutes from now that you would like to shut down Windows (e.g. 5 for 5 minutes from now).";

            _currentTime = new TimeSpan();
            _shutdownTime = new TimeSpan();

            time_remaining_timer.Enabled = false;
            time_remaining_timer.Interval = 1000;   // Check time remaining every 1 seconds.
            time_remaining_desc_label.Text = "Time Remaining: ";
            time_remaining_label.Text = _shutdownTime.ToString();

            stopTimerToolStripMenuItem.Enabled = false;
            addTimerToolStripMenuItem.Enabled = false;
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
            SetCurrentTime();   // Important to update to the current time.

            return _shutdownTime.Subtract(_currentTime);
        }

        private void StartShutdownTimer(int numMinutes)
        {
            // Enable timer, stop timer button, and add time button.
            time_remaining_timer.Enabled = true;
            stopTimerToolStripMenuItem.Enabled = true;
            addTimerToolStripMenuItem.Enabled = true;

            SetCurrentTime();
            SetShutdownTime(numMinutes);

            int timeInSeconds = numMinutes * 60;
            //string cmd = "/C timeout " + timeInSeconds + " && shutdown -s";
            string cmd = "/C shutdown -s -t " + timeInSeconds;

            //string cmd = "/C mkdir " + numMinutes;
            var process = new Process();

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = cmd;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new StartTimerException("Could not execute the start shutdown command through the command prompt. ");
            else
                time_remaining_timer_Tick(null, null);
        }

        private void StopShutdownTimer()
        {
            string cmd = "/C shutdown /a";
            var process = Process.Start("CMD.exe", cmd);
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new StopTimerException("Could not execute the stop shutdown command through the command prompt. ");

            else
            {
                // Reset values.
                _currentTime = new TimeSpan(0, 0, 0);
                _shutdownTime = new TimeSpan(0, 0, 0);

                // Reset the remaining time label.
                time_remaining_timer_Tick(null, null);

                // Disable the timer.
                time_remaining_timer.Enabled = false;
                addTimerToolStripMenuItem.Enabled = false;

                // Disable the stop timer button again.
                stopTimerToolStripMenuItem.Enabled = false;
            }
        }

        /// TODO: Add a check to make sure the number is reasonable.
        /// TODO: Show the estimated shut off time based on the number entered in the textbox.
        /// TODO: Add the ability to maintain the current timer (maybe a countdown or something untile executed).
        /// TODO: Force the cmd prompt to be silent.
        /// TODO: Force the blue banner to not display on windows 10.
        /// TODO: Find better comparison than creating a new TimeSpan instance.
        private void addTimer_Button(object sender, EventArgs e)
        {
            if(int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                // Stop a previous timer if one exists.
                if (_shutdownTime != new TimeSpan(0, 0, 0))
                {
                    try
                    {
                        StopShutdownTimer();
                    }
                    catch(StopTimerException)
                    {
                        MessageBox.Show("The timer couldn't be added. Please first stop the current timer by selecting 'Stop Timer' in the menu and then trying to add a new " +
                            "timer.", "WARNING", MessageBoxButtons.OK);
                    }
                }
                    
                // Provided the timer was successfully stopped, add the new timer.
                if(_shutdownTime == new TimeSpan(0, 0, 0))
                {
                    DialogResult confirm = MessageBox.Show("Are you sure you want to shutdown Windows in " + numMinutes + " minutes?", "Confirm Timer", MessageBoxButtons.YesNo);

                    if (confirm == DialogResult.Yes)
                    {
                        StartShutdownTimer(numMinutes);
                    }
                }

                else
                {
                    MessageBox.Show("The timer couldn't be added. Please first stop the current timer by selecting 'Stop Timer' in the menu and then trying to add a new " +
                        "timer.", "WARNING", MessageBoxButtons.OK);
                }
            }

            // Definitely don't need to throw an except here.
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

        // Maybe I don't need to actually throw an exception here.
        private void time_remaining_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                time_remaining_label.Text = TimeRemaining().ToString("hh' hr 'mm' min 'ss' sec'");
            }
            catch
            {
                time_remaining_label.Text = "Formatting Error. Please Restart the app.";
                //throw new FormatException("Had trouble formatting the time remaining label. Please restart the application.");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void add_2_hr_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(2, 0, 0));
        }

        private void add_1_hr_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(1, 0, 0));
        }

        private void add_30_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 30, 0));
        }

        private void add_5_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 5, 0));
        }

        private void stopTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Are you sure you want to stop the current shutdown?", "Confirm Timer", MessageBoxButtons.YesNo);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    StopShutdownTimer();
                }

                catch(StopTimerException)
                {
                    MessageBox.Show("Could not stop the shutdown timer for some reason. Adding an additional 5 minutes to the timer. Please run 'shutdown /a' in " +
                        "the command prompt to stop the timer.", "ERROR", MessageBoxButtons.OK);

                    // Add an additional 5 minutes to the timer.
                    _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 5, 0));
                }
            }
        }
    }
}
