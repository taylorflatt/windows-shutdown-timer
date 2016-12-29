namespace WindowsShutdownTimer
{
    partial class Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));
            this.options_group_box = new System.Windows.Forms.GroupBox();
            this.minimize_to_sys_tray = new System.Windows.Forms.CheckBox();
            this.save_options_button = new System.Windows.Forms.Button();
            this.last_shutdown_label_desc = new System.Windows.Forms.Label();
            this.last_shutdown_label = new System.Windows.Forms.Label();
            this.options_group_box.SuspendLayout();
            this.SuspendLayout();
            // 
            // options_group_box
            // 
            this.options_group_box.Controls.Add(this.last_shutdown_label);
            this.options_group_box.Controls.Add(this.last_shutdown_label_desc);
            this.options_group_box.Controls.Add(this.minimize_to_sys_tray);
            this.options_group_box.Location = new System.Drawing.Point(12, 12);
            this.options_group_box.Name = "options_group_box";
            this.options_group_box.Size = new System.Drawing.Size(331, 108);
            this.options_group_box.TabIndex = 0;
            this.options_group_box.TabStop = false;
            this.options_group_box.Text = "Options";
            // 
            // minimize_to_sys_tray
            // 
            this.minimize_to_sys_tray.AutoSize = true;
            this.minimize_to_sys_tray.Location = new System.Drawing.Point(7, 20);
            this.minimize_to_sys_tray.Name = "minimize_to_sys_tray";
            this.minimize_to_sys_tray.Size = new System.Drawing.Size(133, 17);
            this.minimize_to_sys_tray.TabIndex = 0;
            this.minimize_to_sys_tray.Text = "Minimize to system tray";
            this.minimize_to_sys_tray.UseVisualStyleBackColor = true;
            // 
            // save_options_button
            // 
            this.save_options_button.Location = new System.Drawing.Point(268, 127);
            this.save_options_button.Name = "save_options_button";
            this.save_options_button.Size = new System.Drawing.Size(75, 23);
            this.save_options_button.TabIndex = 1;
            this.save_options_button.Text = "Save";
            this.save_options_button.UseVisualStyleBackColor = true;
            this.save_options_button.Click += new System.EventHandler(this.save_options_button_Click);
            // 
            // last_shutdown_label_desc
            // 
            this.last_shutdown_label_desc.AutoSize = true;
            this.last_shutdown_label_desc.Location = new System.Drawing.Point(6, 92);
            this.last_shutdown_label_desc.Name = "last_shutdown_label_desc";
            this.last_shutdown_label_desc.Size = new System.Drawing.Size(84, 13);
            this.last_shutdown_label_desc.TabIndex = 1;
            this.last_shutdown_label_desc.Text = "Last Shutdown: ";
            // 
            // last_shutdown_label
            // 
            this.last_shutdown_label.AutoSize = true;
            this.last_shutdown_label.Location = new System.Drawing.Point(96, 92);
            this.last_shutdown_label.Name = "last_shutdown_label";
            this.last_shutdown_label.Size = new System.Drawing.Size(35, 13);
            this.last_shutdown_label.TabIndex = 2;
            this.last_shutdown_label.Text = "label1";
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 162);
            this.Controls.Add(this.save_options_button);
            this.Controls.Add(this.options_group_box);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Options";
            this.Text = "Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Options_FormClosing);
            this.Load += new System.EventHandler(this.Options_Load);
            this.options_group_box.ResumeLayout(false);
            this.options_group_box.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox options_group_box;
        private System.Windows.Forms.Button save_options_button;
        private System.Windows.Forms.CheckBox minimize_to_sys_tray;
        private System.Windows.Forms.Label last_shutdown_label_desc;
        private System.Windows.Forms.Label last_shutdown_label;
    }
}