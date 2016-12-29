namespace WindowsShutdownTimer
{
    partial class TimerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimerForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.description_label = new System.Windows.Forms.Label();
            this.MinutesTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.submit_Button = new System.Windows.Forms.Button();
            this.time_remaining_desc_label = new System.Windows.Forms.Label();
            this.time_remaining_label = new System.Windows.Forms.Label();
            this.time_remaining_timer = new System.Windows.Forms.Timer(this.components);
            this.main_menu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.add_5_min_menu_item = new System.Windows.Forms.ToolStripMenuItem();
            this.add_30_min_menu_item = new System.Windows.Forms.ToolStripMenuItem();
            this.add_1_hr_menu_item = new System.Windows.Forms.ToolStripMenuItem();
            this.add_2_hr_menu_item = new System.Windows.Forms.ToolStripMenuItem();
            this.stopTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.add10MinutesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopTimerContextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.main_menu.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.description_label);
            this.groupBox1.Controls.Add(this.MinutesTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(331, 97);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Set Timer";
            // 
            // description_label
            // 
            this.description_label.AutoSize = true;
            this.description_label.Location = new System.Drawing.Point(7, 20);
            this.description_label.Name = "description_label";
            this.description_label.Size = new System.Drawing.Size(35, 13);
            this.description_label.TabIndex = 2;
            this.description_label.Text = "label2";
            // 
            // MinutesTextBox
            // 
            this.MinutesTextBox.Location = new System.Drawing.Point(58, 61);
            this.MinutesTextBox.Name = "MinutesTextBox";
            this.MinutesTextBox.Size = new System.Drawing.Size(145, 20);
            this.MinutesTextBox.TabIndex = 1;
            this.MinutesTextBox.TextChanged += new System.EventHandler(this.MinutesTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Minutes:";
            // 
            // submit_Button
            // 
            this.submit_Button.Location = new System.Drawing.Point(268, 130);
            this.submit_Button.Name = "submit_Button";
            this.submit_Button.Size = new System.Drawing.Size(75, 23);
            this.submit_Button.TabIndex = 1;
            this.submit_Button.Text = "Set Timer";
            this.submit_Button.UseVisualStyleBackColor = true;
            this.submit_Button.Click += new System.EventHandler(this.addTimer_Button);
            // 
            // time_remaining_desc_label
            // 
            this.time_remaining_desc_label.AutoSize = true;
            this.time_remaining_desc_label.Location = new System.Drawing.Point(19, 132);
            this.time_remaining_desc_label.Name = "time_remaining_desc_label";
            this.time_remaining_desc_label.Size = new System.Drawing.Size(86, 13);
            this.time_remaining_desc_label.TabIndex = 2;
            this.time_remaining_desc_label.Text = "Time Remaining:";
            // 
            // time_remaining_label
            // 
            this.time_remaining_label.AutoSize = true;
            this.time_remaining_label.Location = new System.Drawing.Point(104, 132);
            this.time_remaining_label.Name = "time_remaining_label";
            this.time_remaining_label.Size = new System.Drawing.Size(35, 13);
            this.time_remaining_label.TabIndex = 3;
            this.time_remaining_label.Text = "label3";
            // 
            // time_remaining_timer
            // 
            this.time_remaining_timer.Tick += new System.EventHandler(this.time_remaining_timer_Tick);
            // 
            // main_menu
            // 
            this.main_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.timerToolStripMenuItem});
            this.main_menu.Location = new System.Drawing.Point(0, 0);
            this.main_menu.Name = "main_menu";
            this.main_menu.Size = new System.Drawing.Size(355, 24);
            this.main_menu.TabIndex = 4;
            this.main_menu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // timerToolStripMenuItem
            // 
            this.timerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTimerToolStripMenuItem,
            this.stopTimerToolStripMenuItem});
            this.timerToolStripMenuItem.Name = "timerToolStripMenuItem";
            this.timerToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.timerToolStripMenuItem.Text = "Timer";
            // 
            // addTimerToolStripMenuItem
            // 
            this.addTimerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.add_5_min_menu_item,
            this.add_30_min_menu_item,
            this.add_1_hr_menu_item,
            this.add_2_hr_menu_item});
            this.addTimerToolStripMenuItem.Name = "addTimerToolStripMenuItem";
            this.addTimerToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.addTimerToolStripMenuItem.Text = "Add Time";
            // 
            // add_5_min_menu_item
            // 
            this.add_5_min_menu_item.Name = "add_5_min_menu_item";
            this.add_5_min_menu_item.Size = new System.Drawing.Size(132, 22);
            this.add_5_min_menu_item.Text = "5 Minutes";
            this.add_5_min_menu_item.Click += new System.EventHandler(this.add_5_min_menu_item_Click);
            // 
            // add_30_min_menu_item
            // 
            this.add_30_min_menu_item.Name = "add_30_min_menu_item";
            this.add_30_min_menu_item.Size = new System.Drawing.Size(132, 22);
            this.add_30_min_menu_item.Text = "30 Minutes";
            this.add_30_min_menu_item.Click += new System.EventHandler(this.add_30_min_menu_item_Click);
            // 
            // add_1_hr_menu_item
            // 
            this.add_1_hr_menu_item.Name = "add_1_hr_menu_item";
            this.add_1_hr_menu_item.Size = new System.Drawing.Size(132, 22);
            this.add_1_hr_menu_item.Text = "1 Hour";
            this.add_1_hr_menu_item.Click += new System.EventHandler(this.add_1_hr_menu_item_Click);
            // 
            // add_2_hr_menu_item
            // 
            this.add_2_hr_menu_item.Name = "add_2_hr_menu_item";
            this.add_2_hr_menu_item.Size = new System.Drawing.Size(132, 22);
            this.add_2_hr_menu_item.Text = "2 Hours";
            this.add_2_hr_menu_item.Click += new System.EventHandler(this.add_2_hr_menu_item_Click);
            // 
            // stopTimerToolStripMenuItem
            // 
            this.stopTimerToolStripMenuItem.Name = "stopTimerToolStripMenuItem";
            this.stopTimerToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.stopTimerToolStripMenuItem.Text = "Stop Timer";
            this.stopTimerToolStripMenuItem.Click += new System.EventHandler(this.stopTimerToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon1";
            this.notifyIcon.Visible = true;
            this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTimerToolStripMenuItem,
            this.createTimerToolStripMenuItem,
            this.add10MinutesToolStripMenuItem,
            this.stopTimerContextToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(158, 92);
            // 
            // showTimerToolStripMenuItem
            // 
            this.showTimerToolStripMenuItem.Name = "showTimerToolStripMenuItem";
            this.showTimerToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.showTimerToolStripMenuItem.Text = "Show Timer";
            this.showTimerToolStripMenuItem.Click += new System.EventHandler(this.showTimerToolStripMenuItem_Click);
            // 
            // createTimerToolStripMenuItem
            // 
            this.createTimerToolStripMenuItem.Name = "createTimerToolStripMenuItem";
            this.createTimerToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.createTimerToolStripMenuItem.Text = "Create Timer";
            this.createTimerToolStripMenuItem.Click += new System.EventHandler(this.createTimerToolStripMenuItem_Click);
            // 
            // add10MinutesToolStripMenuItem
            // 
            this.add10MinutesToolStripMenuItem.Name = "add10MinutesToolStripMenuItem";
            this.add10MinutesToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.add10MinutesToolStripMenuItem.Text = "Add 10 Minutes";
            this.add10MinutesToolStripMenuItem.Click += new System.EventHandler(this.add_10_min_menu_item_Click);
            // 
            // stopTimerContextToolStripMenuItem
            // 
            this.stopTimerContextToolStripMenuItem.Name = "stopTimerContextToolStripMenuItem";
            this.stopTimerContextToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.stopTimerContextToolStripMenuItem.Text = "Stop Timer";
            this.stopTimerContextToolStripMenuItem.Click += new System.EventHandler(this.stopTimerToolStripMenuItem_Click);
            // 
            // TimerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 162);
            this.Controls.Add(this.time_remaining_label);
            this.Controls.Add(this.time_remaining_desc_label);
            this.Controls.Add(this.submit_Button);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.main_menu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.main_menu;
            this.MaximizeBox = false;
            this.Name = "TimerForm";
            this.Text = "Shutdown Windows";
            this.Resize += new System.EventHandler(this.TimerForm_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.main_menu.ResumeLayout(false);
            this.main_menu.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox MinutesTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button submit_Button;
        private System.Windows.Forms.Label description_label;
        private System.Windows.Forms.Label time_remaining_desc_label;
        private System.Windows.Forms.Label time_remaining_label;
        private System.Windows.Forms.Timer time_remaining_timer;
        private System.Windows.Forms.MenuStrip main_menu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTimerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem add_5_min_menu_item;
        private System.Windows.Forms.ToolStripMenuItem add_30_min_menu_item;
        private System.Windows.Forms.ToolStripMenuItem add_1_hr_menu_item;
        private System.Windows.Forms.ToolStripMenuItem add_2_hr_menu_item;
        private System.Windows.Forms.ToolStripMenuItem stopTimerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem stopTimerContextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem add10MinutesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTimerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTimerToolStripMenuItem;
    }
}

