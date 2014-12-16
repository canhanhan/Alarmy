using System;
using System.Windows.Forms;

namespace Alarmy.Views
{
    public partial class AlarmForm : Form
    {

        public AlarmForm()
        {
            InitializeComponent();

            this.timeAlarmTime.MinDate = DateTime.Today;
            this.timeAlarmTime.Format = DateTimePickerFormat.Custom;
            this.timeAlarmTime.CustomFormat = "dd.MM.yyyy HH:mm";
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void addTimeAlarm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }   
    }
}
