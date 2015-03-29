using System;
using System.Windows.Forms;

namespace Alarmy.Views
{
    public partial class ImportForm : Form
    {
        public string Path
        {
            get { return this.pathTextBox.Text; }
            set { this.pathTextBox.Text = value;  }
        }

        public bool DeleteExistingAlarms
        {
            get { return this.deleteExistingCheckBox.Checked; }
            set { this.deleteExistingCheckBox.Checked = value; }
        }

        public ImportForm()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
