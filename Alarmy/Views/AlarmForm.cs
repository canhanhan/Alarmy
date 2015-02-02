using System;
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
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
