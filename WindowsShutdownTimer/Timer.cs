using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32.TaskScheduler;

namespace WindowsShutdownTimer
{
    public partial class TimerForm : Form
    {
        #region Main
        /// <summary>
        /// Tracks the current time and the projected shutdown time. Used for calculating remaining time to display to the user.
        /// </summary>
        public DateTime _currentTime;
        public DateTime _shutdownTime;

        public const string DEFAULT_TIMER_DISPLAY = "0 days 0 hr 0 min 0 sec";
        public const string DEFAULT_TASK_NAME = "ScheduledShutdownTimer";
        private const string SHUTDOWN_COMMAND = "shutdown";
        private const string SHUTDOWN_COMMAND_ARGS = "/s /c \"Scheduled Computer shutdown via the Windows Shutdown Timer App\" /t "; // Note the space at the end.

        /// <summary>
        /// Sets the initial program parameters.
        /// </summary>
        public TimerForm()
        {
            bool programRunning = (Application.OpenForms.OfType<TimerForm>().Count() >= 1) ? true : false;

            if (!programRunning)
            {
                InitializeComponent();

                this.CenterToScreen();

                this.Name = "Windows Shutdown Timer";
                this.Text = "Windows Shutdown Timer";
                this.notifyIcon.Text = "Windows Shutdown Timer";

                // Only set once so a reload of the application is required.
                if (Properties.Settings.Default.MinimizeToSysTray)
                    this.ShowInTaskbar = false;
                else
                    this.ShowInTaskbar = true;

                this.AcceptButton = this.submit_Button;

                submit_Button.Enabled = false;
                description_label.AutoSize = true;
                description_label.MaximumSize = new System.Drawing.Size(325, 0);
                description_label.Text = "Enter the number minutes from now that you would like to shut down Windows (e.g. Enter the number 5 for 5 minutes from now).";

                _shutdownTime = new DateTime();

                time_remaining_timer.Enabled = false;   // Disable the tracking timer.
                time_remaining_timer.Interval = 1000;   // Check time remaining every 1 seconds.

                verify_time_remaining_timer.Enabled = false;    // Disable the verify timer.
                verify_time_remaining_timer.Interval = 15000;   // Check with scheduler's time every 15 seconds.
                time_remaining_desc_label.Text = "Time Remaining: ";

                StopLocalTimer();       // Default everything to 'off'.
                UpdateCurrentTime();    // This must be called AFTER the StopLocalTimer() so it updates the _currentTime object rather than resetting it.

                // Only run on version update or first time run.
                if (Properties.Settings.Default.FirstRun)
                    FirstRun();

                // Check if there is currently a timer running.
                if (Properties.Settings.Default.ShutdownTimer > _currentTime && !programRunning)
                {
                    try
                    {
                        _shutdownTime = GetRunningTimer(DEFAULT_TASK_NAME, true);
                        StartLocalTimer();
                    }
                    catch (NoTimerExists)
                    {
                        StopLocalTimer();
                    }
                    catch (Exception)
                    {
                        DisplayGenericError();
                    }
                }
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Only run on the first time executing the program. Welcomes the user and gives a brief how-to.
        /// </summary>
        private void FirstRun()
        {
            // Gets the user's previous settings (if any).
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Save();

            // If the user's FirstRun is STILL true, then this truly is the first time they have run the program.
            // Note: If the user puts the version in a NEW location, this will still display as a first run and not pull settings.
            if (Properties.Settings.Default.FirstRun)
            {
                MessageBox.Show("This appears to be the first time running the Windows Shutdown Timer! \nCreating a Timer: " +
                    "Enter a time in the textbox and press \"Set Timer\". \n\nStopping a Timer: In the main menu under \"Timer\" > \"Stop Timer\".",
                    " Welcome to Windows Shutdown Timer!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Important to set the date (year specifically) to 0002 so it will actually save. Otherwise it won't save the date.
                Properties.Settings.Default.ShutdownTimer = SetDefaultDateTime(Properties.Settings.Default.ShutdownTimer);
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("Thank you for downloading the most recent version of the Windows Shutdown Timer! You may now safely delete the old " +
                    "version of the timer if you haven't already.", "Thank you for upgrading!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Displays a generic error message and provides a link in order to report the bug.
        /// </summary>
        private void DisplayGenericError()
        {
            DialogResult report = MessageBox.Show("Could not check if there is an existing shutdown timer or not. Please note that the 'Time Remaining' timer may be inaccurate so it " +
                "might be safest to simply stop the timer through the menu before attempting to add a timer. If you would like to report this issue (I would really appreciate it), " +
                "select YES.", "Submit Bug Report - Cannot Check for Existing Timer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

            if (report == DialogResult.Yes)
                Process.Start("https://github.com/taylorflatt/windows-shutdown-timer/issues");
            else
                return;
        }

        /// <summary>
        /// Enables the add time buttons in the menu and system tray menu.
        /// </summary>
        private void EnableModifyTimerButtons()
        {
            addTimerToolStripMenuItem.Enabled = true;
            add10MinutesToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// Disables the add time buttons in the menu and system tray menu.
        /// </summary>
        private void DisableModifyTimerButtons()
        {
            addTimerToolStripMenuItem.Enabled = false;
            add10MinutesToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Enables the stop timer buttons in the menu and system tray menu.
        /// </summary>
        private void EnableStopTimerButtons()
        {
            stopTimerToolStripMenuItem.Enabled = true;
            stopTimerContextToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// Disables the stop timer buttons in the menu and system tray menu.
        /// </summary>
        private void DisableStopTimerButtons()
        {
            stopTimerToolStripMenuItem.Enabled = false;
            stopTimerContextToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Resets the timers for the program.
        /// </summary>
        private void ResetTimers()
        {
            _currentTime = SetDefaultDateTime(_currentTime);
            _shutdownTime = SetDefaultDateTime(_shutdownTime);
        }

        /// <summary>
        /// Restores the form after a minimization.
        /// </summary>
        public void RestoreForm()
        {
            // If Windows 7.
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                this.Visible = true;

            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Pulls the calling form into the foreground.
        /// </summary>
        public void BringFormForward()
        {
            this.TopMost = true;
            this.Focus();
            this.BringToFront();
            this.TopMost = false;
        }

        /// <summary>
        /// Fix the default date so that it will save in the user settings.
        /// </summary>
        /// <param name="obj"></param>
        /// <remarks>For some reason, C# won't save 1/1/0001 00:00:00 but it WILL save 1/1/0002 00:00:00. 
        /// So the DateTime needs a single year added to it for it to work.</remarks>
        public DateTime SetDefaultDateTime(DateTime obj)
        {
            obj = default(DateTime);
            return obj.AddYears(1);
        }

        /// <summary>
        /// Gets the current time relative to the user's computer.
        /// </summary>
        private void UpdateCurrentTime()
        {
            _currentTime = DateTime.Now;
        }

        /// <summary>
        /// Sets the shutdown time.
        /// </summary>
        /// <param name="numSeconds">The number of seconds from the current time that then shutdown will be set.</param>
        private void SetShutdownTime(int numSeconds)
        {
            _shutdownTime = _currentTime.AddSeconds(numSeconds);
        }

        /// <summary>
        /// Updates the shutdown timer both locally and in the save file while also modifying the scheduled task.
        /// </summary>
        /// <param name="numSeconds">The number of seconds from the method call that the computer will be set to shutdown.</param>
        private void AddTime(int numSeconds)
        {
            _shutdownTime = _shutdownTime.AddSeconds(numSeconds);
            Properties.Settings.Default.ShutdownTimer = _shutdownTime;
            Properties.Settings.Default.Save();

            ModifyShutdownTimer(numSeconds);
        }

        /// <summary>
        /// Checks to see if a timer currently exists in the Task Scheduler.
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <returns>Returns true if a task with the parameter name is found.</returns>
        public bool TimerExists(string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                try
                {
                    Task task = ts.GetTask(DEFAULT_TASK_NAME);

                    if (task == null)
                        return false;
                    else
                        return true;
                }
                catch (AggregateException e)
                {
                    e.Handle((x) =>
                    {
                        if (x is UnauthorizedAccessException) // This we know how to handle.
                            MessageBox.Show("You do not have permission to create a task in the Task Scheduler.");

                        return false;
                    });
                }

                return false;
            }
        }

        /// <summary>
        /// Checks to see if the specified timer is disabled.
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <returns>Returns true if a task with the parameter name is disabled.</returns>
        public bool TimerDisabled(string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                try
                {
                    Task task = ts.GetTask(taskName);

                    if (task == null || task.Name != taskName)
                        throw new NoTimerExists();
                    else if (task.State == TaskState.Disabled ||
                        task.State == TaskState.Unknown)
                        return true;
                    else
                        return false;
                }
                catch (AggregateException e)
                {
                    e.Handle((x) =>
                    {
                        if (x is UnauthorizedAccessException) // This we know how to handle.
                            MessageBox.Show("You do not have permission to create a task in the Task Scheduler.");

                        return false;
                    });
                }

                return false;
            }
        }

        /// <summary>
        /// Checks to see if the specified timer is running.
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <param name="currentTime">The current local time.</param>
        /// <returns>Returns true if a task with the parameter name is found and running.</returns>
        public bool TimerRunning(string taskName, DateTime currentTime)
        {
            if (Properties.Settings.Default.ShowShutdownNotification)
            {
                // Create a temporary shutdown command to see if it succeedes and then immediately stop it if it was created.
                string cmd = "/C shutdown -s  -c \"Attempting to check if a timer already exists via the Windows Shutdown Timer. \" -t 50000 && shutdown /a";
                var process = Process.Start("CMD.exe", cmd);
                process.WaitForExit();

                if (process.ExitCode == 1190)
                    return true;
                else
                    return false;
            }

            else
            {
                using (TaskService ts = new TaskService())
                {
                    try
                    {
                        Task task = ts.GetTask(taskName);
                        if (TimerExists(taskName) && !TimerDisabled(taskName) && task.NextRunTime != null && task.NextRunTime > currentTime)
                            return true;
                        else
                            return false;
                    }
                    catch (AggregateException e)
                    {
                        e.Handle((x) =>
                        {
                            if (x is UnauthorizedAccessException) // This we know how to handle.
                                MessageBox.Show("You do not have permission to create a task in the Task Scheduler.");

                            return false;
                        });
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Computes the remaining time before the computer is set to shutdown.
        /// </summary>
        /// <returns></returns>
        private TimeSpan TimeRemaining()
        {
            UpdateCurrentTime();   // Important to update to the current time.

            if (_shutdownTime <= _currentTime)
                throw new TimerEnded("The timer has ended successfully. However, you shouldn't be seeing this message. ");

            else
                return _shutdownTime.Subtract(_currentTime);
        }

        /// <summary>
        /// Gets the timer for any running timer if it exists.
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <param name="onlyCheckRunning">If true, this will only return a timer's DateTime if it is currently running. Otherwise a 'NoTimerExists' exception is raised.</param>
        /// <returns>Returns the DateTime of when the scheduled event is set to fire.</returns>
        public DateTime GetRunningTimer(string taskName, bool onlyCheckRunning)
        {
            if (TimerExists(taskName))
            {
                // This will likely be inaccurate but the user WAS warned about the possibility.
                if (Properties.Settings.Default.ShowShutdownNotification)
                    return Properties.Settings.Default.ShutdownTimer;

                else
                {
                    using (TaskService ts = new TaskService())
                    {
                        if (onlyCheckRunning)
                        {
                            if (!TimerRunning(taskName, _currentTime))
                                throw new NoTimerExists("The timer doesn't exist in the task schduler");
                        }

                        Task task = ts.GetTask(taskName);
                        return task.Definition.Triggers.FirstOrDefault().StartBoundary;
                    }
                }
            }
            else
                throw new NoTimerExists("The timer doesn't exist in the task scheduler.");

        }

        /// <summary>
        /// Creates a scheduled task to shutdown the computer.
        /// </summary>
        /// <param name="numSeconds">The number of seconds from the start time that the (shutdown) task will execute.</param>
        /// <remarks>This should only be called when creating a NEW scheduled event. It will throw a TimerExists exception if you attempt 
        /// to recreate an existing timer.</remarks>
        /// <exception cref="TimerExists()">Timer already exists. Use ModifyShutdownTimer instead of this method to make changes to 
        /// that timer. Otherwise, remove it and call this method again.</exception>
        private void CreateShutdownTimer(int numSeconds)
        {
            using (TaskService ts = new TaskService())
            {
                // If the task doesn't exist, create it.
                if (TimerExists(DEFAULT_TASK_NAME))
                    throw new TimerExists("The timer already exists in the task scheduler. You must modify it instead of attempting to create it!");
                else
                {
                    try
                    {
                        TaskDefinition td = ts.NewTask();
                        td.RegistrationInfo.Date = _currentTime;
                        td.RegistrationInfo.Source = "Windows Shutdown Timer";
                        td.RegistrationInfo.Description = "Shutdown Timer initiated by Windows Shutdown Timer";

                        td.Settings.Enabled = true;

                        // Set the timer to the shutdown command so windows will display the log off notification event.
                        if (Properties.Settings.Default.ShowShutdownNotification)
                            td.Triggers.Add(new TimeTrigger(DateTime.Now));
                        else
                        {
                            numSeconds = 1;     // Set it to 1 second so it executes immediately (no notification).
                            td.Triggers.Add(new TimeTrigger(_shutdownTime));
                        }

                        td.Actions.Add(new ExecAction(SHUTDOWN_COMMAND, SHUTDOWN_COMMAND_ARGS + numSeconds, null));

                        TaskService.Instance.RootFolder.RegisterTaskDefinition(DEFAULT_TASK_NAME, td);

                        Properties.Settings.Default.ShutdownTimer = _shutdownTime;
                        Properties.Settings.Default.Save();

                        StartLocalTimer();
                    }
                    catch (Exception)
                    {
                        DialogResult alert = MessageBox.Show("The timer couldn't be set. ", "Error - Couldn't Set Timer!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                        if (alert == DialogResult.Retry)
                            CreateShutdownTimer(numSeconds);
                    }
                }
            }
        }

        /// <summary>
        /// Modifies an existing scheduled task. This should be used if only a single scheduled task will be used (Recommended).
        /// </summary>
        /// <param name="numSeconds">The number of seconds from the start time that the (shutdown) task will execute.</param>
        /// <remarks>This should be used instead of CreateShutdownTimer when there already exists a shutdown timer.</remarks>
        private void ModifyShutdownTimer(int numSeconds)
        {
            using (TaskService ts = new TaskService())
            {
                // If the task exists, update the trigger.
                if (TimerExists(DEFAULT_TASK_NAME))
                {
                    Task task = ts.GetTask(DEFAULT_TASK_NAME);

                    if (task.Definition.Triggers.Count == 1)
                        task.Definition.Triggers.RemoveAt(0);

                    else if (task.Definition.Triggers.Count > 1)
                    {
                        for (int index = 0; index < task.Definition.Triggers.Count - 1; index++)
                        {
                            task.Definition.Triggers.RemoveAt(index);
                        }
                    }

                    if (task.Definition.Actions.Count == 1)
                        task.Definition.Actions.RemoveAt(0);

                    else if (task.Definition.Actions.Count > 1)
                    {
                        for (int index = 0; index < task.Definition.Actions.Count - 1; index++)
                        {
                            task.Definition.Actions.RemoveAt(index);
                        }
                    }

                    // Set the timer to the shutdown command so windows will display the log off notification event.
                    if (Properties.Settings.Default.ShowShutdownNotification)
                        task.Definition.Triggers.Add(new TimeTrigger(DateTime.Now.AddSeconds(2)));
                    else
                    {
                        numSeconds = 1;     // Set it to 1 second so it executes immediately (no notification).
                        task.Definition.Triggers.Add(new TimeTrigger(_shutdownTime));
                    }

                    task.Definition.Actions.Add(new ExecAction(SHUTDOWN_COMMAND, SHUTDOWN_COMMAND_ARGS + numSeconds, null));

                    // Reset the status in case it was set as anything but "Ready"
                    task.Definition.Settings.Enabled = true;
                    task.RegisterChanges();

                    Properties.Settings.Default.ShutdownTimer = _shutdownTime;
                    Properties.Settings.Default.Save();

                    StartLocalTimer();
                }

                else
                    throw new NoTimerExists("The timer doesn't exist in the task scheduler. You must create it instead of attempting to modify it!");
            }
        }

        /// <summary>
        /// Stops the scheduled (shutdown) task by removing the trigger or stopping the shutdown through the command prompt.
        /// </summary>
        /// <remarks>If the user opted to display the shutdown notification, it stops the timer by executing a command prompt command to do so.</remarks>
        private void StopShutdownTimer()
        {
            // Need to stop the timer from the command line since there isn't a task to stop.
            if (Properties.Settings.Default.ShowShutdownNotification)
            {
                string cmd = "/C shutdown /a";
                var process = Process.Start("CMD.exe", cmd);
                process.WaitForExit();

                if (process.ExitCode == 1116)
                    throw new NoTimerExists("The timer doesn't exist from running the command line shutdown kill.");
                else
                    StopLocalTimer();
            }
            else
            {
                using (TaskService ts = new TaskService())
                {
                    // If the task exists, remove the trigger. Note: the Stop() method doesn't work for some reason.
                    if (TimerExists(DEFAULT_TASK_NAME))
                    {
                        Task task = ts.GetTask(DEFAULT_TASK_NAME);
                        task.Definition.Triggers.RemoveAt(0);
                        task.RegisterChanges();
                        StopLocalTimer();
                    }
                    else
                        throw new NoTimerExists("The timer doesn't exist in the task scheduler. You must create it instead of attempting to modify it!");
                }
            }
        }

        /// <summary>
        /// Starts the local timers and sets all the necessary options.
        /// </summary>
        /// <remarks>It will enable the stop and modify timer buttons as well as enable the timers. It also disables the start timer button in the menu.</remarks>
        private void StartLocalTimer()
        {
            EnableStopTimerButtons();
            EnableModifyTimerButtons();
            createTimerToolStripMenuItem.Enabled = false;

            time_remaining_timer.Enabled = true;
            verify_time_remaining_timer.Enabled = true;
        }

        /// <summary>
        /// Stops the local timer and resets all the necessary options.
        /// </summary>
        /// <remarks>It will reset the local shutdown time and current time. It will also disable the modify and stop timer buttons as well as disable the local timers.</remarks>
        private void StopLocalTimer(bool disabled = false)
        {
            ResetTimers();

            DisableModifyTimerButtons();
            DisableStopTimerButtons();

            createTimerToolStripMenuItem.Enabled = true;

            time_remaining_timer.Enabled = false;
            if (disabled)
                time_remaining_label.Text = "Timer was disabled. Create a \nnew timer to enable it!";
            else
                time_remaining_label.Text = DEFAULT_TIMER_DISPLAY;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Adds the shutdown timer for a particular number of minutes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addTimer_Button(object sender, EventArgs e)
        {
            if (int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                int timeInSeconds = numMinutes * 60;
                bool timerExists = TimerExists(DEFAULT_TASK_NAME);

                UpdateCurrentTime();
                SetShutdownTime(timeInSeconds);

                // The number was able to be parsed but turned out too large.
                if (numMinutes > 5255999)
                {
                    MessageBox.Show("You can't set a timer for longer than 10 years (5255999 minutes). Anything longer is currently not " +
                        "supported.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Make sure they want to actually shut down right now.
                else if (numMinutes == 0)
                {
                    DialogResult confirm = MessageBox.Show("You entered 0 minutes, this means the computer will shutdown IMMEDIATELY. " +
                        "Are you sure?", "Warning - Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    if (confirm == DialogResult.Yes)
                    {
                        UpdateCurrentTime();
                        SetShutdownTime(1);
                        Properties.Settings.Default.ShutdownTimer = DateTime.UtcNow;
                        try
                        {
                            CreateShutdownTimer(timeInSeconds);
                            return;
                        }
                        catch (TimerExists)
                        {
                            ModifyShutdownTimer(timeInSeconds);
                            return;
                        }
                    }
                    else
                        return;
                }

                try
                {
                    // There is an existing Shutdown and it is running and the user has entered a new shutdown time.
                    if (timerExists && TimerRunning(DEFAULT_TASK_NAME, _currentTime) && numMinutes != 0)
                    {
                        /// TODO: Add my own custom dialog box to customize the options so they aren't confusing.
                        DialogResult result = MessageBox.Show("A timer to shutdown Windows already exists. Would you like to stop that timer and add yours for "
                            + numMinutes + " minutes from now? Or choose 'No' to simply stop the other timer without adding the new one. Otherwise choose 'Cancel' to take " +
                            "no action.", "Stop Existing Timer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                        // Remove old timer and add the new one.
                        if (result == DialogResult.Yes)
                            ModifyShutdownTimer(timeInSeconds);

                        // Remove old timer only.
                        else if (result == DialogResult.No)
                            StopShutdownTimer();

                        // Take no action.
                        else
                            return;
                    }

                    else
                    {
                        DialogResult confirm = MessageBox.Show("Are you sure you want to shutdown Windows in " + numMinutes + " minutes?", "Confirm Timer", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);

                        if (confirm == DialogResult.Yes)
                        {
                            if (timerExists)
                                ModifyShutdownTimer(timeInSeconds);
                            else
                                CreateShutdownTimer(timeInSeconds);
                        }
                        else
                            return;
                    }
                }

                catch (NoTimerExists)
                {
                    MessageBox.Show("The old timer doesn't exist anymore. It may have been deleted by other means. This " +
                        "shouldn't be a problem though.", "Old timer removed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                catch (Exception)
                {
                    DisplayGenericError();
                }
            }

            else
            {
                MessageBox.Show("There was difficulty parsing the number that you entered. Perhaps you made the number too large " +
                    "(>5255999 minutes)? The number I see: " + MinutesTextBox.Text + ". Please try a different number.", "Trouble Creating Timer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            BringFormForward();
        }

        /// <summary>
        /// Enables and disables the add timer button when valid digits are entered into the minutes textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinutesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (MinutesTextBox.Text.Any() && MinutesTextBox.Text.All(char.IsDigit))
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
                if (Properties.Settings.Default.ShowShutdownNotification || (TimerExists(DEFAULT_TASK_NAME) && TimerRunning(DEFAULT_TASK_NAME, _currentTime)))
                    time_remaining_label.Text = TimeRemaining().ToString("dd' days 'hh' hr 'mm' min 'ss' sec'");
                else
                    throw new TimerEnded("The timer has either been stopped and is no longer running in the scheduler or it has been removed from the scheduler completely.");
            }
            catch (NoTimerExists)
            {
                if (TimerExists(DEFAULT_TASK_NAME) && TimerDisabled(DEFAULT_TASK_NAME))
                    StopLocalTimer(true);
                else
                    StopLocalTimer();
            }
            catch (TimerEnded)
            {
                if (TimerExists(DEFAULT_TASK_NAME) && TimerDisabled(DEFAULT_TASK_NAME))
                    StopLocalTimer(true);
                else
                    StopLocalTimer();
            }
            catch (FormatException)
            {
                time_remaining_label.Text = "Formatting Error. Please Restart the app.";
            }
        }

        /// <summary>
        /// Polls the task scheduler and updates the local time shutdown timer in the event the task has been changed manually. If the 
        /// scheduled event has been stopped, it will reset all local parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void verify_time_remaining_timer_Tick(object sender, EventArgs e)
        {
            // If there isn't a task to check against, simply exit from the method.
            if (Properties.Settings.Default.ShowShutdownNotification)
                return;
            else if (TimerExists(DEFAULT_TASK_NAME) && !TimerDisabled(DEFAULT_TASK_NAME))
            {
                using (TaskService ts = new TaskService())
                {
                    Task task = ts.GetTask(DEFAULT_TASK_NAME);
                    if (task.NextRunTime != _shutdownTime)
                        _shutdownTime = task.NextRunTime;
                }
            }
            else
                StopLocalTimer(true);
        }

        /// <summary>
        /// Stops the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Are you sure you want to stop the current shutdown if one exists?", "Confirm Timer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    StopShutdownTimer();
                }

                catch (NoTimerExists)
                {
                    MessageBox.Show("The old timer doesn't exist anymore. It may have been deleted by other means. This " +
                        "shouldn't be a problem though.", "Old timer removed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Adds two hours to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_2_hr_menu_item_Click(object sender, EventArgs e)
        {
            AddTime(7200);
        }

        /// <summary>
        /// Adds 1 hour to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_1_hr_menu_item_Click(object sender, EventArgs e)
        {
            AddTime(3600);
        }

        /// <summary>
        /// Adds 30 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_30_min_menu_item_Click(object sender, EventArgs e)
        {
            AddTime(1800);
        }

        /// <summary>
        /// Adds 10 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_10_min_menu_item_Click(object sender, EventArgs e)
        {
            AddTime(600);
        }

        /// <summary>
        /// Adds 5 minutes to the shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_5_min_menu_item_Click(object sender, EventArgs e)
        {
            AddTime(300);
        }

        /// <summary>
        /// The stop timer button in the menu that when clicked will stop any shutdown timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopTimerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_shutdownTime == SetDefaultDateTime(_shutdownTime))
                    StopShutdownTimer();
                else
                    StopShutdownTimer();
            }
            catch (NoTimerExists)
            {
                StopLocalTimer();
            }
        }

        /// <summary>
        /// Shuts the computer off after 10 seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shutdownNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MinutesTextBox.Text = "0";      // Required for the addTimer_Button method to handle the zero.
            addTimer_Button(null, null);
            MinutesTextBox.Text = "";       // Reset the textbox in the event they selected to not shutdown.
        }

        /// <summary>
        /// Occurs when the user attempts to manipulate the window in any way (such as minimizing).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForm_Resize(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.MinimizeToSysTray)
            {
                notifyIcon.Visible = true;

                // If Windows 7, set the form to invisible so it won't show above the taskbar after a minimization.
                if (this.WindowState == FormWindowState.Minimized && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                    this.Visible = false;
            }

            else
                notifyIcon.Visible = false;
        }

        /// <summary>
        /// When the icon in the system tray is double clicked, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreForm();
            TimerForm_Resize(null, null);
        }

        /// <summary>
        /// When the create button is clicked in the menu, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreForm();
            TimerForm_Resize(null, null);
        }

        /// <summary>
        /// When the show timer button is clicked in the menu, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreForm();
            TimerForm_Resize(null, null);
        }

        /// <summary>
        /// When the icon in the system tray is clicked, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>This depends completely on user settings and will not fire unless the user has selected the option in the options form.</remarks>
        private void notifyIcon_Click(object sender, MouseEventArgs e)
        {
            // Only re-show the app if the user has selected it and they are left clicking (not right clicking).
            if (Properties.Settings.Default.LClickOpenSysTray && e.Button == MouseButtons.Left)
            {
                RestoreForm();
                TimerForm_Resize(null, null);
            }
        }

        /// <summary>
        /// Initiates an update by calling another action. Used from the main menu rather than the options form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options tempOptions = new Options(new TimerForm());
            tempOptions.check_update_button_Click(null, null);
        }

        /// <summary>
        /// The options button in the menu that when clicked will create an options instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options optionsForm = new Options(this);

            optionsForm.Visible = true;
            optionsForm.Height = this.Height;
            optionsForm.Width = this.Width;
            optionsForm.Text = this.Text + " Options";
            optionsForm.Name = this.Name + " Options";
        }

        /// <summary>
        /// Opens the issues page on the github for the user to submit a report.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bugReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/taylorflatt/windows-shutdown-timer/issues");
        }

        /// <summary>
        /// Displays the About information for the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// TODO: Create clickable link.
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Current Version: " + typeof(Options).Assembly.GetName().Version.ToString() + ". Created by Taylor Flatt. More information can be found " +
                "at https://github.com/taylorflatt/windows-shutdown-timer.", "About Windows Shutdown Timer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Called after any form closing event. Save the user's settings in the event they have been deleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // In case the settings are deleted before the program exits, save the user's settings prior to exiting.
            // Adds a bit of overhead but I think it is worth it.
            Properties.Settings.Default.Save();
            notifyIcon.Dispose();
        }

        /// <summary>
        /// Exits the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
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

        #endregion
    }
}
