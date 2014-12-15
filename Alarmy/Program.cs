using Alarmy.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Alarmy
{
    static class Program
    {
#if !DEBUG
        private static Mutex mutex = new Mutex(true, "Local\\{AD1766D6-1245-4449-AD70-0C83CA39BB3C}");
#endif
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DEBUG
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
#if !DEBUG
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Only you can only run one instance at a time", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }
    }
}
