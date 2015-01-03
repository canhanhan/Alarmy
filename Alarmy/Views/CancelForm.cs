using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.Views
{
    internal partial class CancelForm : Form
    {
        public CancelForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
