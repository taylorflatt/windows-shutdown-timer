using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

/// TODO: Save the last run date, the last timer run, and then compare that information after running a test shutdown timer to 
/// see if the user has a current running timer. Alternatively, I can move to a DateTime rather than TimeSpan since the DateTime
/// would store the additional date information which would eliminate one of the settings needed to be saved. Need to sleep.
/// 
/// This is mainly to allow you to cancel a previous timer in the event the program is closed. 
/// 
/// TODO: Need a cleaner option to handle all the switching for button toggling. Not sure if there really is a nicer option.
/// 

namespace WindowsShutdownTimer
{
    public partial class TimerForm : Form
    {
        /// <summary>
        /// Tracks the current time and the projected shutdown time. Used for calculating remaining time to display to the user.
        /// </summary>
        public TimeSpan _currentTime;
        public TimeSpan _shutdownTime;

        /// <summary>
        /// Initial program parameters.
        /// </summary>
        public TimerForm()
        {
            InitializeComponent();

            this.Name = "Shutdown Windows Timer";
            this.Text = "Shutdown Windows Timer";

            submit_Button.Enabled = false;
            description_label.AutoSize = true;
            description_label.MaximumSize = new System.Drawing.Size(325, 0);
            description_label.Text = "Enter the number minutes from now that you would like to shut down Windows (e.g. 5 for 5 minutes from now).";

            _currentTime = new TimeSpan();
            //Properties.Settings.Default.ShutdownTimer;
            _shutdownTime = new TimeSpan();

            time_remaining_timer.Enabled = false;
            time_remaining_timer.Interval = 1000;   // Check time remaining every 1 seconds.
            time_remaining_desc_label.Text = "Time Remaining: ";
            time_remaining_label.Text = _shutdownTime.ToString();

            DisableStopTimerButtons();
            DisableModifyTimerButtons();
        }

        private void EnableModifyTimerButtons()
        {
            addTimerToolStripMenuItem.Enabled = true;
            add10MinutesToolStripMenuItem.Enabled = true;
        }

        private void DisableModifyTimerButtons()
        {
            addTimerToolStripMenuItem.Enabled = false;
            add10MinutesToolStripMenuItem.Enabled = false;
        }

        private void EnableStopTimerButtons()
        {
            stopTimerToolStripMenuItem.Enabled = true;
            stopTimerContextToolStripMenuItem.Enabled = true;
        }

        private void DisableStopTimerButtons()
        {
            stopTimerToolStripMenuItem.Enabled = false;
            stopTimerContextToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Applies the user's settings.
        /// </summary>
        public void ApplyUserSettings()
        {
            if (Properties.Settings.Default.MinimizePref)
                notifyIcon.Visible = true;

            else
                notifyIcon.Visible = false;
        }

        /// <summary>
        /// Gets the current time relative to the user's computer.
        /// </summary>
        private void SetCurrentTime()
        {
            _currentTime = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Sets the shutdown time.
        /// </summary>
        /// <param name="numMinutes">The number of minutes from the current time that the shutdown will be set.</param>
        private void SetShutdownTime(int numMinutes)
        {
            _shutdownTime = _currentTime.Add(new TimeSpan(0, numMinutes, 0));
        }

        /// <summary>
        /// Computes the remaining time before the computer is set to shutdown.
        /// </summary>
        /// <returns></returns>
        private TimeSpan TimeRemaining()
        {
            SetCurrentTime();   // Important to update to the current time.

            return _shutdownTime.Subtract(_currentTime);
        }

        /// <summary>
        /// Starts the shutdown timer.
        /// </summary>
        /// <param name="numMinutes">The number of minutes from the current time that the shutdown will be set.</param>
        private void StartShutdownTimer(int numMinutes)
        {
            // Enable timer, stop timer button, and add time button.
            time_remaining_timer.Enabled = true;
            EnableStopTimerButtons();
            EnableModifyTimerButtons();

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

            if (process.ExitCode == 1190)
            {
                DialogResult result = MessageBox.Show("A windows shutdown is already pending. This must be stopped before a new one may be " +
                    "added. Would you like to stop the other timer and add the new one?", "ERROR", MessageBoxButtons.YesNo);

                // Don't reset the timer parameters so that the new timer may still be added.
                if (result == DialogResult.Yes)
                {
                    StopShutdownTimer(false);
                    StartShutdownTimer(numMinutes);
                }
                else
                    return;
            }
            else if (process.ExitCode != 0)
                throw new StartTimerException("Could not execute the start shutdown command through the command prompt. ");
            else
            {
                createTimerToolStripMenuItem.Enabled = false;
                time_remaining_timer_Tick(null, null);
            }
        }

        /// <summary>
        /// Stops the shutdown timer by removing the old timer.
        /// </summary>
        /// <param name="reset">Determines if the timer parameters need to be reset.</param>
        private void StopShutdownTimer(bool reset)
        {
            string cmd = "/C shutdown /a";
            var process = Process.Start("CMD.exe", cmd);
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new StopTimerException("Could not execute the stop shutdown command through the command prompt. ");

            else
            {
                if(reset)
                {
                    createTimerToolStripMenuItem.Enabled = true;

                    // Reset values.
                    _currentTime = TimeSpan.Zero;
                    _shutdownTime = TimeSpan.Zero;

                    // Reset the remaining time label.
                    time_remaining_label.Text = Convert.ToString(TimeSpan.Zero);

                    // Disable the timer.
                    time_remaining_timer.Enabled = false;
                    DisableModifyTimerButtons();

                    // Disable the stop timer button again.
                    DisableStopTimerButtons();
                }
            }
        }

        /// <summary>
        /// Adds the shutdown timer for a particular number of minutes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addTimer_Button(object sender, EventArgs e)
        {
            if(int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                // Stop a previous timer if one exists.
                if (_shutdownTime != TimeSpan.Zero)
                {
                    try
                    {
                        StopShutdownTimer(true);
                    }
                    catch(StopTimerException)
                    {
                        MessageBox.Show("The timer couldn't be added. Please first stop the current timer by selecting 'Stop Timer' in the menu and then trying to add a new " +
                            "timer.", "WARNING", MessageBoxButtons.OK);
                    }
                }
                    
                // Provided the timer was successfully stopped, add the new timer.
                if(_shutdownTime == TimeSpan.Zero)
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
        }

        /// <summary>
        /// Enables and disables the add timer button when valid digits are entered into the minutes textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinutesTextBox_TextChanged(object sender, EventArgs e)
        {
            if(MinutesTextBox.Text.Any() && MinutesTextBox.Text.All(char.IsDigit))
                submit_Button.Enabled = true;
            else
                submit_Button.Enabled = false;
        }

        /// <summary>
        /// Timer that updates the time remaining label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void time_remaining_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                time_remaining_label.Text = TimeRemaining().ToString("hh' hr 'mm' min 'ss' sec'");
            }
            catch(FormatException)
            {
                time_remaining_label.Text = "Formatting Error. Please Restart the app.";
            }
        }

        /// <summary>
        /// Stops the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Are you sure you want to stop the current shutdown?", "Confirm Timer", MessageBoxButtons.YesNo);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    StopShutdownTimer(true);
                }

                catch (StopTimerException)
                {
                    MessageBox.Show("Could not stop the shutdown timer for some reason. Adding an additional 5 minutes to the timer. Please run 'shutdown /a' in " +
                        "the command prompt to stop the timer.", "ERROR", MessageBoxButtons.OK);

                    // Add an additional 5 minutes to the timer.
                    _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 5, 0));
                }
            }
        }

        /// <summary>
        /// Displays the About information for the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created and maintained by Taylor Flatt. More information can be found at https://github.com/taylorflatt/windows-shutdown-timer.", "About", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Adds two hours to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_2_hr_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(2, 0, 0));
        }

        /// <summary>
        /// Adds 1 hour to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_1_hr_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(1, 0, 0));
        }

        /// <summary>
        /// Adds 30 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_30_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 30, 0));
        }

        /// <summary>
        /// Adds 5 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_5_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 5, 0));
        }

        /// <summary>
        /// Exits the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Test!");
        }

        /// <summary>
        /// Occurs when the user attempts to manipulate the window in any way (such as minimizing).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ApplyUserSettings();

                if (Properties.Settings.Default.MinimizePref)
                    this.ShowInTaskbar = false;
                else
                    this.ShowInTaskbar = true;
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options optionsForm = new Options(this);

            optionsForm.Visible = true;
            optionsForm.Height = this.Height;
            optionsForm.Width = this.Width;
            optionsForm.Text = this.Text + " Options";
            optionsForm.Name = this.Name + " Options";
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void createTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void stopTimerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_shutdownTime == TimeSpan.Zero)
                StopShutdownTimer(false);
            else
                StopShutdownTimer(true);
        }

        private void add_10_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.Add(new TimeSpan(0, 10, 0));
        }

        private void showTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }
    }
}
