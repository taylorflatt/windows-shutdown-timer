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

        /// TODO: Add a check to make sure the number is reasonable.
        /// TODO: Show the estimated shut off time based on the number entered in the textbox.
        private void addTimer_Button(object sender, EventArgs e)
        {
            string cmd;

            if(int.TryParse(MinutesTextBox.Text, out int numMinutes))
            {
                DialogResult confirm = MessageBox.Show("Are you sure you want to shutdown Windows in " + numMinutes + " minutes?", "Confirm Timer", MessageBoxButtons.YesNo);

                if(confirm == DialogResult.Yes)
                {
                    cmd = "/C mkdir " + numMinutes;

                    System.Diagnostics.Process.Start("CMD.exe", cmd);

                    Application.Exit();
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
    }
}
