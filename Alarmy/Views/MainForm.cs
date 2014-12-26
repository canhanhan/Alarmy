using Alarmy.Common;
using Alarmy.Infrastructure;
using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.Views
{
    internal partial class MainForm : AppBar
    {
        private readonly SoundPlayer soundPlayer;
        private readonly IAlarmService alarmService;
        private readonly int alarmListGroupInterval;

        private delegate void ShowDelegate(AlarmStatusChangedEventArgs args);

        private ILogger logger = NullLogger.Instance;
        private Point clickedPosition;
        private bool WasRegistered;

        public ILogger Logger
        {
            get { return this.logger; }
            set { this.logger = value; }
        }

        public MainForm(IAlarmService alarmService, Settings settings)
        {
            this.alarmListGroupInterval = settings.AlarmListGroupInterval;
            var alarmSoundPath = settings.AlarmSoundFile;
            this.soundPlayer = new SoundPlayer(alarmSoundPath);
            this.alarmService = alarmService;
            this.alarmService.Interval = settings.CheckInterval;
            this.alarmService.AlarmStatusChanged += alarmService_AlarmStatusChanged;

            InitializeComponent();
            listView1.DoubleBuffered(true);
            notifyIcon1.Icon = Icon;

            soundToolStripMenuItem.Checked = settings.EnableSound;
            popupOnAlarmMenuItem.Checked = !settings.DontPopup;
            timer1.Interval = settings.RefreshInterval;
            timer1.Enabled = true;

            if (settings.StartHidden)
                hideToolStripMenuItem_Click(null, null);
        }

        private void AlarmsForm_Load(object sender, EventArgs e)
        {
            alarmService.Start();
            RefreshList();
            RegisterBar();
        }

        private void AlarmsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegisterBar();
        }

        private void alarmService_AlarmStatusChanged(object sender, AlarmStatusChangedEventArgs e)
        {
            Invoke(new ShowDelegate(AlarmStatusChanged), e);
        }

        private void AlarmStatusChanged(AlarmStatusChangedEventArgs args)
        {            
            if ((args.Alarm.Status == AlarmStatus.Ringing || args.Alarm.Status == AlarmStatus.Missed) && !Visible)
            {
                Show();
            }

            RefreshList();
        }

        private void CheckForAlarmSound(IEnumerable<IAlarm> alarms)
        {
            if (alarms.Any(x => x.Status == AlarmStatus.Ringing && !x.IsHushed))
            {
                if (!this.soundPlayer.IsPlaying)
                    this.soundPlayer.Play();
            }
            else
            {
                if (this.soundPlayer.IsPlaying)
                    this.soundPlayer.Stop();
            }
        }

        private void RefreshList()
        {
            if (listView1.SelectedItems.Count > 0 && itemContext.Visible)
            {
                return;
            }
            listView1.BeginUpdate();
            listView1.Clear();
            listView1.View = View.Details;
            listView1.ShowGroups = true;
            listView1.Columns.Add("Title");
            listView1.Columns.Add("Time");
            listView1.Columns.Add("Status");

            var alarms = alarmService.List().ToArray();
            alarms.OfType<Alarm>()
                                    .Where(x => x.IsWorthShowing)
                                    .OrderBy(x => x.Time)
                                    .GroupBy(x => x.Time.RoundUp(TimeSpan.FromMinutes(this.alarmListGroupInterval)))
                                    .ToList().ForEach(x =>
            {
                var group = new ListViewGroup(x.Key.ToString(), String.Format("{0}-{1}", x.Key.AddMinutes(-this.alarmListGroupInterval).ToShortTimeString(), x.Key.ToShortTimeString()));
                listView1.Groups.Add(group);
                listView1.Items.AddRange(x.Select(item => new ListViewItem(new[] { item.Title, item.Time.ToShortTimeString(), AlarmStatus(item) }, group) { Tag = item, BackColor = GetColor(item), ToolTipText = item.CancelReason }).ToArray());
            });

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.EndUpdate();

            if (this.soundToolStripMenuItem.Checked)
                CheckForAlarmSound(alarms);
        }

        private static string AlarmStatus(Alarm item)
        {
            return item.Status.ToString() + (item.IsHushed ? " (Hushed)" : string.Empty);
        }

        #region Window Resize, Move
        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (clickedPosition != Point.Empty && e.Button.HasFlag(MouseButtons.Left))
            {
                Left += e.Location.X - clickedPosition.X;
                Top += e.Location.Y - clickedPosition.Y;
            }
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (clickedPosition == Point.Empty && e.Button.HasFlag(MouseButtons.Left))
            {
                clickedPosition = e.Location;
            }
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            clickedPosition = Point.Empty;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
            }

            else
            {
                if (FormWindowState.Normal == WindowState)
                {
                    notifyIcon1.Visible = false;
                }
            }
        }

        private void MainForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                notifyIcon1.Visible = false;
                hideToolStripMenuItem.Text = "&Hide";
                if (WasRegistered && !IsRegistered)
                {
                    WasRegistered = false;
                    RegisterBar();
                }
            }
            else
            {
                notifyIcon1.Visible = true;
                hideToolStripMenuItem.Text = "&Show";
                if (IsRegistered)
                {
                    WasRegistered = true;
                    RegisterBar();
                }
            }
        }
        #endregion

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Right))
            {
                return;
            }
            var item = listView1.GetItemAt(e.X, e.Y);
            if (item == null)
            {
                listViewContext.Show(Cursor.Position);
            }
            else
            {
                var alarm = item.Tag as Alarm;
                item.Selected = true;
                completeToolStripMenuItem.Enabled = alarm.CanBeCompleted;
                toolStripMenuItem4.Enabled = alarm.CanBeCancelled;
                changeToolStripMenuItem.Enabled = alarm.CanBeSet;
                hushToolStripMenuItem.Checked = alarm.IsHushed;
                hushToolStripMenuItem.Enabled = alarm.Status == AlarmStatus.Ringing;

                itemContext.Show(Cursor.Position);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var alarm = listView1.SelectedItems[0].Tag as Alarm;

            if (alarm.IsHushed)
            {
                this.Logger.Info(alarm + " is un-hushed.");
                alarm.IsHushed = false;
            }
            else
            {
                this.Logger.Info(alarm + " is hushed.");
                alarm.IsHushed = true;
            }

            hushToolStripMenuItem.Checked = alarm.IsHushed;
            alarmService.Update(alarm);
            RefreshList();
        }

        private void completeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var alarm = listView1.SelectedItems[0].Tag as Alarm;
            this.Logger.Info(alarm + " is completed.");
            alarm.Complete();
            alarmService.Update(alarm);
            RefreshList();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var alarm = listView1.SelectedItems[0].Tag as Alarm;
            var cancelForm = new CancelForm();
            cancelForm.alarmLabel.Text = string.Format("{0} ({1})", alarm.Title, alarm.Time.ToShortTimeString());
            if (cancelForm.ShowDialog() == DialogResult.OK)
            {
                var reason = cancelForm.CancelReason.Text;
                this.Logger.InfoFormat("{0} is cancelled. Reason: {1}", alarm.ToString(), reason);                
                alarm.Cancel(reason);
                alarmService.Update(alarm);
                RefreshList();
            }
        }

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var alarm = listView1.SelectedItems[0].Tag as Alarm;
            var alarmForm = new AlarmForm();
            alarmForm.timeAlarmTime.Value = alarm.Time;
            alarmForm.timeAlarmTitle.Text = alarm.Title;

            if (alarmForm.ShowDialog() == DialogResult.OK)
            {
                this.Logger.InfoFormat("{0} is changed. New time: {1}, New title: {2}", alarm.ToString(), alarmForm.timeAlarmTitle.Text, alarmForm.timeAlarmTime.Value);
                alarm.Title = alarmForm.timeAlarmTitle.Text;
                alarm.Set(alarmForm.timeAlarmTime.Value);
                alarmService.Update(alarm);
                RefreshList();
            }
        }

        private void newAlarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var alarmForm = new AlarmForm();
            alarmForm.timeAlarmTitle.Clear();
            alarmForm.timeAlarmTime.Value = DateTime.Now;

            if (alarmForm.ShowDialog() == DialogResult.OK)
            {
                var alarm = new Alarm() { Title = alarmForm.timeAlarmTitle.Text };
                alarm.Set(alarmForm.timeAlarmTime.Value);
                this.Logger.Info(alarm.ToString() + " is created");
                alarmService.Add(alarm);
                RefreshList();
            }
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Visible)
            {
                Logger.Info("List is hid.");
                Hide();
            }
            else
            {
                Logger.Info("List is visible.");
                Show();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshList();
        }

        private static Color GetColor(Alarm alarm)
        {
            switch (alarm.Status)
            {
                case AlarmStatus.Completed:
                    return Color.Green;
                case AlarmStatus.Cancelled:
                    return Color.Gray;
                case AlarmStatus.Ringing:
                    return Color.Yellow;
                case AlarmStatus.Missed:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }

        private void soundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (this.soundToolStripMenuItem.Checked)
            {
                Logger.Info("Sound is enabled.");
                this.RefreshList();
            }
            else
            {
                Logger.Info("Sound is disabled.");
                if (this.soundPlayer.IsPlaying)
                    this.soundPlayer.Stop();
            }
        }

        private void popupOnAlarmMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (popupOnAlarmMenuItem.Checked)
            {
                Logger.Info("List is set to popup on alarm");
            }
            else
            {
                Logger.Info("List is set not to popup on alarm");
            }

        }
    }
}
