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
        private delegate void ShowDelegate(AlarmStatusChangedEventArgs args);
        private delegate void MethodInvoker<TArg>(TArg arg1);

        private readonly SoundPlayer soundPlayer;
        private readonly IAlarmService alarmService;
        private readonly int alarmListGroupInterval;
        private ILogger logger = NullLogger.Instance;

        public ILogger Logger
        {
            get { return this.logger; }
            set { this.logger = value; }
        }

        public MainForm(IAlarmService alarmService, Settings settings)
        {
            this.alarmListGroupInterval = settings.AlarmListGroupInterval;
            this.soundPlayer = new SoundPlayer(settings.AlarmSoundFile);
            this.alarmService = alarmService;
            this.alarmService.Interval = settings.CheckInterval;
            this.alarmService.AlarmStatusChanged += alarmService_AlarmStatusChanged;
            this.alarmService.AlarmAdded += alarmService_AlarmAdded;              
            this.alarmService.AlarmRemoved += alarmService_AlarmRemoved;
            this.alarmService.AlarmUpdated += alarmService_AlarmUpdated;

            InitializeComponent();
            listView1.DoubleBuffered(true);
            notifyIcon1.Icon = Icon;

            soundToolStripMenuItem.Checked = settings.EnableSound;
            popupOnAlarmMenuItem.Checked = !settings.DontPopup;

            if (settings.StartHidden)
                hideToolStripMenuItem_Click(null, null);
        }

        #region AlarmService Events
        private void alarmService_AlarmUpdated(object sender, AlarmEventArgs e)
        {
            this.Invoke(new MethodInvoker<IAlarm>(UpdateAlarm), e.Alarm);
        }

        private void alarmService_AlarmRemoved(object sender, AlarmEventArgs e)
        {
            this.Invoke(new MethodInvoker<IAlarm>(RemoveAlarm), e.Alarm);
        }

        private void alarmService_AlarmAdded(object sender, AlarmEventArgs e)
        {
            this.Invoke(new MethodInvoker<IAlarm>(AddAlarm), e.Alarm);
        }

        private void alarmService_AlarmStatusChanged(object sender, AlarmStatusChangedEventArgs e)
        {
            if ((e.Alarm.Status == AlarmStatus.Ringing || e.Alarm.Status == AlarmStatus.Missed) && !Visible)
            {
                this.Invoke(new MethodInvoker(this.Show));
            }

            this.alarmService_AlarmUpdated(this, new AlarmEventArgs(e.Alarm));
        }
        #endregion

        #region Form Events
        private void AlarmsForm_Load(object sender, EventArgs e)
        {
            alarmService.Start();
            RegisterBar();
        }

        private void AlarmsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.alarmService.Stop();
            RegisterBar();
        }
        #endregion

        #region Window Resize, Move
        private Point clickedPosition;
        private bool WasRegistered;

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

        #region Menu Events
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
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void soundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (soundToolStripMenuItem.Checked)
            {
                Logger.Info("Sound is enabled.");
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
        #endregion

        #region ListView Events
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                listView1.SelectedItems.Clear();
                item.Selected = true;

                var alarm = item.Tag as Alarm;
                completeToolStripMenuItem.Enabled = alarm.CanBeCompleted;
                cancelStripMenuItem.Enabled = alarm.CanBeCancelled;
                changeToolStripMenuItem.Enabled = alarm.CanBeSet;
                hushToolStripMenuItem.Checked = alarm.IsHushed;
                hushToolStripMenuItem.Enabled = alarm.Status == AlarmStatus.Ringing;

                itemContext.Show(Cursor.Position);            
            } 
            else if (e.Button.HasFlag(MouseButtons.Right))
            {
                listViewContext.Show(Cursor.Position);
            }
        }

        #endregion

        private ListViewItem GetAlarmItem(IAlarm alarm)
        {
            return this.listView1.Items.Cast<ListViewItem>().Single(x => ((IAlarm)x.Tag).Id == alarm.Id);
        }

        private void UpdateAlarm(IAlarm alarm)
        {
            var alarmItem = GetAlarmItem(alarm);
            UpdateAlarmItem(alarm, alarmItem);
            RefreshAlarmGroups();
        }

        private void RemoveAlarm(IAlarm alarm)
        {
            var alarmItem = GetAlarmItem(alarm);
            listView1.Items.Remove(alarmItem);
            RefreshAlarmGroups();
        }

        private void AddAlarm(IAlarm alarm)
        {
            var alarmItem = new ListViewItem();
            alarmItem.SubItems.Add("");
            alarmItem.SubItems.Add("");

            UpdateAlarmItem(alarm, alarmItem);
            listView1.Items.Add(alarmItem);
            RefreshAlarmGroups();
        }

        private void RefreshAlarmGroups()
        {
            listView1.BeginUpdate();

            listView1.Items.Cast<ListViewItem>()
                .OrderBy(x => ((IAlarm)x.Tag).Time)
                .GroupBy(x => ((IAlarm)x.Tag).Time.RoundUp(TimeSpan.FromMinutes(this.alarmListGroupInterval)))
                .ToList().ForEach(groups =>
                {
                    var key = groups.Key.ToString();
                    var title = String.Format("{0}-{1}", groups.Key.AddMinutes(-this.alarmListGroupInterval).ToShortTimeString(), groups.Key.ToShortTimeString());

                    var group = listView1.Groups[key];
                    if (group == null)
                    {
                        group = new ListViewGroup(key, title);
                        listView1.Groups.Add(group);
                    }

                    groups.ToList().ForEach(item => item.Group = group);
                });

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.EndUpdate();

            if (this.soundToolStripMenuItem.Checked)
                CheckForAlarmSound();

        }

        private void CheckForAlarmSound()
        {
            var alarms = this.alarmService.List();
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

        private static void UpdateAlarmItem(IAlarm alarm, ListViewItem alarmItem)
        {
            alarmItem.Text = alarm.Title;
            alarmItem.SubItems[1].Text = alarm.Time.ToShortTimeString();
            alarmItem.SubItems[2].Text = AlarmStatusText(alarm);
            alarmItem.Tag = alarm;
            alarmItem.BackColor = GetColor(alarm);
            alarmItem.ToolTipText = alarm.CancelReason;
        }

        private static Color GetColor(IAlarm alarm)
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

        private static string AlarmStatusText(IAlarm item)
        {
            return item.Status.ToString() + (item.IsHushed ? " (Hushed)" : string.Empty);
        }
    }
}
