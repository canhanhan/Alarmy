using System;
using System.Drawing;
using System.Windows.Forms;

namespace Alarmy.Views
{
    internal partial class AlarmForm : Form
    {
        public AlarmForm()
        {
            InitializeComponent();

            timeAlarmTime.MinDate = DateTime.Today;
            timeAlarmTime.Format = DateTimePickerFormat.Custom;
            timeAlarmTime.CustomFormat = "dd.MM.yyyy HH:mm";
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void addTimeAlarm_Click(object sender, EventArgs e)
        {            
            if (!this.ValidateForm())
            {
                MessageBox.Show("Please enter the title", "Missing Title", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(this.timeAlarmTitle.Text))
            {
                this.timeAlarmTitle.BackColor = Color.Red;
                return false;
            }
            else
            {
                this.timeAlarmTitle.BackColor = SystemColors.Window;
                return true;
            }
        }

        private void timeAlarmTitle_KeyUp(object sender, KeyEventArgs e)
        {
            this.ValidateForm();
        }
    }
}
