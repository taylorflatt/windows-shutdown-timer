using System;
using System.Linq;
using System.Windows.Forms;

namespace WindowsShutdownTimer
{
    public partial class TimerForm : Form
    {
        public TimerForm()
        {
            InitializeComponent();

            submit_Button.Enabled = false;
            description_label.AutoSize = true;
            description_label.MaximumSize = new System.Drawing.Size(250, 0);
            description_label.Text = "Please enter the number minutes from now that you would like to shut down Windows.";
        }

        private void addTimer_Button(object sender, EventArgs e)
        {
            string cmd;

            if(int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                cmd = "/C mkdir " + numMinutes;

                System.Diagnostics.Process.Start("CMD.exe", cmd);
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
    }
}
