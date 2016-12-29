using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

/// TODO: Need a cleaner option to handle all the switching for button toggling. Not sure if there really is a nicer option.
/// 
/// TODO: Add scheduler ability to shutdown computer everyday at a certain time. Basically add a scheduled event rather than 
/// command line.
/// 
/// TODO: Add the ability to turn off options saving so no files will be stored on the local computer. Not sure the best approach 
/// to this other than not doing any sort of setting set prior to an initial change. That will add some additional overhead inline 
/// since I will need to store local versions of the default variables and check if settings have been enabled and switch over at that 
/// point. Not sure if it is really worth it to be honest.
/// 
/// TODO: Add a check for updates function (https://msdn.microsoft.com/en-us/library/1zyc39fb.aspx.
/// 

namespace WindowsShutdownTimer
{
    public partial class TimerForm : Form
    {
        /// <summary>
        /// Tracks the current time and the projected shutdown time. Used for calculating remaining time to display to the user.
        /// </summary>
        public DateTime _currentTime;
        public DateTime _shutdownTime;

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

            _currentTime = new DateTime();
            _shutdownTime = new DateTime();

            time_remaining_timer.Enabled = false;
            time_remaining_timer.Interval = 1000;   // Check time remaining every 1 seconds.
            time_remaining_desc_label.Text = "Time Remaining: ";
            time_remaining_label.Text = "0 hr 0 min 0 sec";

            EnableStopTimerButtons();
            DisableModifyTimerButtons();

            // If the last shutdown time was scheduled after the current time then go ahead and redisplay it
            // so the user can see it. I do a hard reset on the timer so this could result in a timer created 
            // by SOMETHING ELSE being removed and this one superseding it.
            /// REMARK: Maybe give a dialog box that asks the user if they want to keep the existing timer or not.
            /// TODO: Could mute the sound for the notification (possibly) to help mitigate some of the annoyance.
            if (Properties.Settings.Default.ShutdownTimer > _currentTime)
            {
                SetCurrentTime();
                var timeRemaining = Properties.Settings.Default.ShutdownTimer.Subtract(_currentTime);

                // Make sure the timer wasn't stopped manually.
                if (ShutdownTimerExists())
                {
                    StopShutdownTimer(false);
                    StartShutdownTimer(timeRemaining.Minutes);
                    createTimerToolStripMenuItem.Enabled = false;
                }
            }
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

        private void ResetTimers()
        {
            _currentTime = default(DateTime);
            _shutdownTime = default(DateTime);
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
            _currentTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the shutdown time.
        /// </summary>
        /// <param name="numMinutes">The number of minutes from the current time that the shutdown will be set.</param>
        private void SetShutdownTime(int numMinutes)
        {
            _shutdownTime = _currentTime.AddMinutes(numMinutes);
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
            string cmd = "/C shutdown -s -t " + timeInSeconds;

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
                throw new StartTimerException("Could not execute the start shutdown command through the command prompt. Windows Shutdown Error Code: " + process.ExitCode);

            else
            {
                createTimerToolStripMenuItem.Enabled = false;
                time_remaining_timer_Tick(null, null);

                // Save the latest shutdown time.
                Properties.Settings.Default.ShutdownTimer = _shutdownTime;
                Properties.Settings.Default.Save();
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

            // Exit Code 1190: A shutdown event is currently in progress. We ignore this event and force the stoppage.
            // Exit Code 1116: No shutdown event currently exists. Great, don't care.
            if (process.ExitCode != 0 && process.ExitCode != 1190 && process.ExitCode != 1116)
                throw new StopTimerException("Could not execute the stop shutdown command through the command prompt. ");

            else
            {
                if(reset)
                {
                    createTimerToolStripMenuItem.Enabled = true;

                    // Reset values.
                    ResetTimers();

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
        /// This should always be called prior to making a call to StopShutdownTimer since that method assumes a complete stoppage (whether 
        /// </summary>
        /// <returns>Returns whether or not a shutdown timer has been initiated by any program.</returns>
        private bool ShutdownTimerExists(int numMinutes = 500000, bool stopOtherTimer = false)
        {
            // If the program hasn't created a timer OR the last saved timer is default, check for an existing timer.
            if(Properties.Settings.Default.ShutdownTimer == DateTime.MinValue || _shutdownTime == DateTime.MinValue)
            {
                // Create a temporary shutdown command to see if it succeedes and then immediately stop it if it was created.
                string cmd = "/C shutdown -s -t " + numMinutes + " && shutdown /a";
                var process = Process.Start("CMD.exe", cmd);
                process.WaitForExit();

                if (process.ExitCode == 1190)
                    return true;
            }

            return false;
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
                SetCurrentTime();
                SetShutdownTime(numMinutes);

                bool timerExists = ShutdownTimerExists();

                // Shutdown exists and the user has entered a new shutdown time.
                if (timerExists && numMinutes != 0)
                {
                    /// TODO: Add my own custom dialog box to customize the options so they aren't confusing.
                    DialogResult result = MessageBox.Show("A timer to shutdown Windows already exists. Would you like to stop that timer and add yours for "
                        + numMinutes + " minutes from now? Or choose no to simply stop the other timer without adding the new one. Otherwise choose cancel to take " +
                        "no action.", "Stop Existing Timer", MessageBoxButtons.YesNoCancel);

                    // Remove old timer and add the new one.
                    if (result == DialogResult.Yes)
                    {
                        StopShutdownTimer(false);
                        StartShutdownTimer(numMinutes);
                    }

                    // Remove old timer only.
                    else if (result == DialogResult.No)
                    {
                        StopShutdownTimer(false);
                        ResetTimers();
                    }

                    // Take no action.
                    else
                        return;
                }

                else
                {
                    DialogResult confirm = MessageBox.Show("Are you sure you want to shutdown Windows in " + numMinutes + " minutes?", "Confirm Timer", MessageBoxButtons.YesNo);

                    if (confirm == DialogResult.Yes)
                        StartShutdownTimer(numMinutes);
                    else
                        return;
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
            DialogResult confirm = MessageBox.Show("Are you sure you want to stop the current shutdown if one exists?", "Confirm Timer", MessageBoxButtons.YesNo);

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

                    // Add an additional 5 minutes to the timer to diagnose the issue.
                    _shutdownTime.AddMinutes(5);
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
            _shutdownTime = _shutdownTime.AddHours(2);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Adds 1 hour to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_1_hr_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.AddHours(1);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Adds 30 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_30_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.AddMinutes(30);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Adds 5 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_5_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.AddMinutes(5);
            Properties.Settings.Default.Save();
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
            if (_shutdownTime == default(DateTime))
                StopShutdownTimer(false);
            else
                StopShutdownTimer(true);
        }

        private void add_10_min_menu_item_Click(object sender, EventArgs e)
        {
            _shutdownTime = _shutdownTime.AddMinutes(10);
            Properties.Settings.Default.Save();
        }

        private void showTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            // Only re-show the app if the user has selected it.
            if (Properties.Settings.Default.LClickOpenSysTray)
                WindowState = FormWindowState.Normal;
        }
    }
}
