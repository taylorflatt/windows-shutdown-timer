using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32.TaskScheduler;

///
/// TODO: Find a way to keep user settings across different instances (i.e. open two different .exes which are the same version 
/// but in different locations...currently that will create/store info in two difference files).
/// 
/// TODO: Add scheduler ability to shutdown computer everyday at a certain time. Basically add a scheduled event rather than 
/// command line. This might honestly be outside the scope of this program. But then again, a scheduled shutdown time isn't 
/// really that obtuse.
/// 

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
        private const string SHUTDOWN_COMMAND = "echo \"Hello!\" ";

        /// <summary>
        /// Sets the initial program parameters.
        /// </summary>
        public TimerForm()
        {
            bool programRunning = (Application.OpenForms.OfType<TimerForm>().Count() >= 1) ? true : false;

            if(!programRunning)
            {
                InitializeComponent();

                this.CenterToScreen();

                this.Name = "Windows Shutdown Timer";
                this.Text = "Windows Shutdown Timer";
                this.notifyIcon.Text = "Windows Shutdown Timer";

                this.AcceptButton = this.submit_Button;

                submit_Button.Enabled = false;
                description_label.AutoSize = true;
                description_label.MaximumSize = new System.Drawing.Size(325, 0);
                description_label.Text = "Enter the number minutes from now that you would like to shut down Windows (e.g. Enter the number 5 for 5 minutes from now).";

                UpdateCurrentTime();

                _shutdownTime = new DateTime();

                time_remaining_timer.Enabled = false;   // Enable the tracking timer.
                time_remaining_timer.Interval = 1000;   // Check time remaining every 1 seconds.
                time_remaining_desc_label.Text = "Time Remaining: ";
                time_remaining_label.Text = DEFAULT_TIMER_DISPLAY;


                EnableStopTimerButtons();       // Decided to always have this enabled in case they want to stop another timer not created by this program. No real harm.
                DisableModifyTimerButtons();    // Immediately disable this the add time option and re-enable later when necessary.

                // If first run, then welcome the user and notify that they may remove the old program.
                if (Properties.Settings.Default.FirstRun == true)
                {
                    // Very important! For some reason I couldn't set it to the default in the settings for VS so I have to set it to a random date.
                    // that should be overwritten the moment the program is started for the first time.
                    Properties.Settings.Default.ShutdownTimer = SetDefaultDateTime(Properties.Settings.Default.ShutdownTimer);
                    Properties.Settings.Default.FirstRun = false;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("This appears to be the first time running this program. If you are new, then Welcome! If you have updated from a previous version, then " +
                        "you may now safely remove the previous version of the program.", "Welcome to Windows Shutdown Timer!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                var temp = "Stored Shutdown Time: " + Properties.Settings.Default.ShutdownTimer + " and the current time: " + _currentTime + " and is the programming running: " + programRunning;

                if (Properties.Settings.Default.ShutdownTimer > _currentTime && !programRunning)
                {
                    try
                    {
                        _shutdownTime = GetRunningTimer(DEFAULT_TASK_NAME, true);
                        time_remaining_timer.Enabled = true;
                    }
                    catch (NoTimerExists)
                    {
                        time_remaining_label.Text = DEFAULT_TIMER_DISPLAY;
                    }
                    catch(Exception)
                    {
                        DialogResult report = MessageBox.Show("Could not check if there is an existing shutdown timer or not. Please note that the 'Time Remaining' timer may be inaccurate so it " +
                            "might be safest to simply stop the timer through the menu before attempting to add a timer. If you would like to report this issue (I would really appreciate it), " +
                            "select YES.", "Submit Bug Report - Cannot Check for Existing Timer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

                        if (report == DialogResult.Yes)
                            Process.Start("https://github.com/taylorflatt/windows-shutdown-timer/issues");
                        else
                            return;
                    }
                }
            }
        }

        #endregion

        #region Helper Functions

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
        /// Applies the user's settings.
        /// </summary>
        /// <remarks>Should probably rename this to SetMinimizePref or something more accurate.</remarks>
        public void ApplyUserSettings()
        {
            if (Properties.Settings.Default.MinimizeToSysTray)
                notifyIcon.Visible = true;

            else
                notifyIcon.Visible = false;
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
                Task task = ts.GetTask(DEFAULT_TASK_NAME);

                if (task == null)
                    return false;
                else
                    return true;
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
            using (TaskService ts = new TaskService())
            {
                Task task = ts.GetTask(taskName);

                if (task != null && task.NextRunTime != null && task.Name == taskName && task.NextRunTime > currentTime)
                    return true;
                else
                    return false;
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
        /// Gets the timer for the running scheduled task.
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <param name="onlyCheckRunning">If true, this will only return a timer's DateTime if it is currently running. Otherwise a 'NoTimerExists' exception is raised.</param>
        /// <returns>Returns the DateTime of when the scheduled event is set to fire.</returns>
        public DateTime GetRunningTimer(string taskName, bool onlyCheckRunning)
        {
            using (TaskService ts = new TaskService())
            {
                // If the task exists, return the trigger time.
                if (TimerExists(taskName))
                {
                    if (onlyCheckRunning)
                    {
                        if (!TimerRunning(taskName, _currentTime))
                            throw new NoTimerExists("The timer doesn't exist in the task schduler");
                    }

                    Task task = ts.GetTask(taskName);
                    return task.Definition.Triggers.FirstOrDefault().StartBoundary;
                }

                else
                    throw new NoTimerExists("The timer doesn't exist in the task scheduler.");
            }
        }

        /// <summary>
        /// Gets the last time a scheduled task was run (successfully or not).
        /// </summary>
        /// <param name="taskName">The name of the task that will be searched.</param>
        /// <returns>Returns the DateTime of when the scheduled even last fired.</returns>
        public DateTime GetLastRunTime(string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                // If the task exists, return the last run time.
                if (TimerExists(taskName))
                {
                    Task task = ts.GetTask(taskName);

                    return task.LastRunTime;
                }

                else
                    throw new NoTimerExists("The timer doesn't exist in the task scheduler.");
            }
        }

        /// <summary>
        /// Creates a scheduled task to shutdown the computer.
        /// </summary>
        /// <param name="numSeconds">The number of seconds from the method call that the computer will be set to shutdown.</param>
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

                        td.Triggers.Add(new TimeTrigger(_shutdownTime));
                        td.Actions.Add(new ExecAction("cmd.exe", SHUTDOWN_COMMAND + " seconds entered: " + numSeconds, null));

                        TaskService.Instance.RootFolder.RegisterTaskDefinition(DEFAULT_TASK_NAME, td);

                        Properties.Settings.Default.ShutdownTimer = _shutdownTime;
                        Properties.Settings.Default.Save();

                        EnableStopTimerButtons();
                        EnableModifyTimerButtons();

                        time_remaining_timer.Enabled = true;
                    }
                    catch
                    {
                        DialogResult alert = MessageBox.Show("The timer couldn't be set. ", "Error - Couldn't Set Timer!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                        if (alert == DialogResult.Retry)
                            CreateShutdownTimer(numSeconds);
                    }
                }
            }
        }

        /// <summary>
        /// Modifies an existing scheduled task. This should be used if only a single scheduled task will be used.
        /// </summary>
        /// <param name="numSeconds">The number of seconds from the method call that the computer will be set to shutdown.</param>
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

                    task.Definition.Triggers.Add(new TimeTrigger(_shutdownTime));

                    task.RegisterChanges();

                    Properties.Settings.Default.ShutdownTimer = _shutdownTime;
                    Properties.Settings.Default.Save();

                    EnableStopTimerButtons();
                    EnableModifyTimerButtons();

                    time_remaining_timer.Enabled = true;
                }

                else
                    throw new NoTimerExists("The timer doesn't exist in the task scheduler. You must create it instead of attempting to modify it!");
            }
        }

        /// <summary>
        /// Stops the scheduled task by removing the trigger.
        /// </summary>
        private void StopShutdownTimer()
        {
            using (TaskService ts = new TaskService())
            {
                // If the task exists, remove the trigger. Note: the Stop() method doesn't work for some reason.
                if (TimerExists(DEFAULT_TASK_NAME))
                {
                    Task task = ts.GetTask(DEFAULT_TASK_NAME);
                    task.Definition.Triggers.RemoveAt(0);
                    task.RegisterChanges();

                    ResetTimers();

                    time_remaining_label.Text = DEFAULT_TIMER_DISPLAY;
                    time_remaining_timer.Enabled = false;

                    DisableModifyTimerButtons();
                    DisableStopTimerButtons();
                }

                else
                    throw new NoTimerExists("The timer doesn't exist in the task scheduler. You must create it instead of attempting to modify it!");
            }
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
            if(int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                // The number was able to be parsed but turned out too large.
                if (numMinutes > 5255999)
                {
                    MessageBox.Show("You can't set a timer for longer than 10 years (5255999 minutes). Anything longer is currently not " +
                        "supported.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Make sure they want to actually shut down right now.
                else if(numMinutes == 0)
                {
                    DialogResult confirm = MessageBox.Show("You entered 0 minutes, this means the computer will shutdown IMMEDIATELY. " +
                        "Are you sure?", "Warning - Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    if (confirm == DialogResult.Yes)
                    {
                        Properties.Settings.Default.ShutdownTimer = DateTime.UtcNow;
                        CreateShutdownTimer(0);
                    }
                }

                int timeInSeconds = numMinutes * 60;
                bool timerExists = TimerExists(DEFAULT_TASK_NAME);
                UpdateCurrentTime();
                SetShutdownTime(timeInSeconds);

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

                        if (confirm == DialogResult.Yes && timerExists)
                            ModifyShutdownTimer(timeInSeconds);
                        else if (confirm == DialogResult.Yes && !timerExists)
                            CreateShutdownTimer(timeInSeconds);
                        else
                            return;
                    }
                }

                catch(Exception)
                {
                    DialogResult report = MessageBox.Show("Could not check if there is an existing shutdown timer or not. Please note that the 'Time Remaining' timer may be inaccurate so it " +
                        "might be safest to simply stop the timer through the menu before attempting to add a timer. If you would like to report this issue (I would really appreciate it), " +
                        "select YES.", "Submit Bug Report - Cannot Check for Existing Timer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

                    if (report == DialogResult.Yes)
                        Process.Start("https://github.com/taylorflatt/windows-shutdown-timer/issues");
                    else
                        return;
                }
            }

            else
            {
                MessageBox.Show("There was difficulty parsing the number that you entered. Perhaps you made the number too large " +
                    "(>5255999 minutes)? The number I see: " + numMinutes + ". Please try a different number." , "Trouble Creating Timer", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                time_remaining_label.Text = TimeRemaining().ToString("dd' days 'hh' hr 'mm' min 'ss' sec'");
            }
            catch(TimerEnded)
            {
                time_remaining_label.Text = DEFAULT_TIMER_DISPLAY;
                time_remaining_timer.Enabled = false;
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
            DialogResult confirm = MessageBox.Show("Are you sure you want to stop the current shutdown if one exists?", "Confirm Timer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    StopShutdownTimer();
                }

                catch (StopTimerException ex)
                {
                    MessageBox.Show("Could not stop the shutdown timer for some reason. Adding an additional 5 minutes to the timer. Please run 'shutdown /a' in " +
                        "the command prompt to stop the timer. Error Code: " + ex.ErrorCode, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Add an additional 5 minutes to the timer to diagnose the issue.
                    _shutdownTime.AddMinutes(5);
                    Properties.Settings.Default.ShutdownTimer = _shutdownTime;
                    Properties.Settings.Default.Save();
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
            if (_shutdownTime == SetDefaultDateTime(_shutdownTime))
                StopShutdownTimer();
            else
                StopShutdownTimer();
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

                if (Properties.Settings.Default.MinimizeToSysTray)
                {
                    this.ShowInTaskbar = false;
                    this.Visible = false;
                    // If this is windows 7, there is a small problem of it not fully minimizing the first time.
                    // Need to simply hide the window as well or it will show a small version right above the taskbar.
                    if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                        this.Visible = false;
                }
                else
                    this.ShowInTaskbar = true;
            }

            else
                this.Visible = true;
        }

        /// <summary>
        /// When the icon in the system tray is double clicked, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Visible = true;
        }

        /// <summary>
        /// When the create button is clicked in the menu, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Visible = true;
        }

        /// <summary>
        /// When the show timer button is clicked in the menu, the app will be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showTimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Visible = true;
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
                WindowState = FormWindowState.Normal;
                this.Visible = true;
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
        }

        #endregion
    }
}
