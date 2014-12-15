using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Alarmy
{
    public partial class MainForm : Form
    {
        private readonly IAlarmService alarmService;
        private readonly ITimerService timerService;
        private readonly IAlarmRepository alarmRepository;

        public MainForm()
        {
            InitializeComponent();

            this.timerService = new TimerService();
            this.alarmRepository = new AlarmRepository();
            this.alarmService = new AlarmService(alarmRepository, timerService);
        }
    }
}
