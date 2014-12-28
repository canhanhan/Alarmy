using Alarmy.Common;
using Alarmy.Infrastructure;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.Views
{
    internal partial class MainForm : AppBar, IMainView
    {
        private readonly double alarmListGroupInterval;

        public new event EventHandler OnLoad;
        public new event EventHandler OnClosing;
        public event EventHandler OnHideRequest;
        public event EventHandler OnExitRequest;
        public event EventHandler<AlarmEventArgs> OnHushRequest;
        public event EventHandler<AlarmEventArgs> OnCompleteRequest;
        public event EventHandler<AlarmEventArgs> OnCancelRequest;
        public event EventHandler<AlarmEventArgs> OnChangeRequest;
        public event EventHandler OnNewRequest;
        public event EventHandler OnEnableSoundChange;
        public event EventHandler OnPopupOnAlarmChange;
        public event EventHandler OnSmartAlarmChange;

        public MainForm(Settings settings)
        {
            this.alarmListGroupInterval = settings.AlarmListGroupInterval;

            this.InitializeComponent();
            this.listView1.DoubleBuffered(true);
            this.notifyIcon1.Text = Text;
            this.notifyIcon1.Icon = Icon;
        }

        public bool EnableSound
        {
            get
            {
                return this.soundToolStripMenuItem.Checked;
            }
            set
            {
                this.soundToolStripMenuItem.Checked = value;
            }
        }

        public bool PopupOnAlarm
        {
            get
            {
                return this.popupOnAlarmMenuItem.Checked;
            }
            set
            {
                this.popupOnAlarmMenuItem.Checked = value;
            }
        }

        public bool SmartAlarm
        {
            get
            {
                return this.smartAlarmStripMenuItem.Checked;
            }
            set
            {
                this.smartAlarmStripMenuItem.Checked = value;
            }
        }

        public string AskCancelReason(IAlarm alarm)
        {
            var cancelForm = new CancelForm();
            cancelForm.alarmLabel.Text = string.Format("{0} ({1})", alarm.Title, alarm.Time.ToShortTimeString());
            if (cancelForm.ShowDialog() == DialogResult.OK)
            {
                return cancelForm.CancelReason.Text;
            }

            return null;
        }

        public AlarmMetadata AskAlarmMetadata(IAlarm alarm = null)
        {
            var alarmForm = new AlarmForm();
            if (alarm != null)
            {
                alarmForm.timeAlarmTime.Value = alarm.Time;
                alarmForm.timeAlarmTitle.Text = alarm.Title;
            }
            else
            {
                alarmForm.timeAlarmTitle.Clear();
                alarmForm.timeAlarmTime.Value = DateTime.Now;
            }

            if (alarmForm.ShowDialog() == DialogResult.OK)
            {
                return new AlarmMetadata
                {
                    Title = alarmForm.timeAlarmTitle.Text,
                    Time = alarmForm.timeAlarmTime.Value
                };
            }

            return null;
        }

        public void UpdateAlarm(IAlarm alarm)
        {
            this.InvokeIfNecessary(() =>
            {
                var alarmItem = this.GetAlarmItem(alarm);
                UpdateAlarmItem(alarm, alarmItem);
                this.RefreshAlarmGroups();
            });
        }

        public void RemoveAlarm(IAlarm alarm)
        {
            InvokeIfNecessary(() =>
            {
                var alarmItem = this.GetAlarmItem(alarm);
                this.listView1.Items.Remove(alarmItem);
                this.RefreshAlarmGroups();
            });
        }

        public void AddAlarm(IAlarm alarm)
        {
            this.InvokeIfNecessary(() =>
            {
                var alarmItem = new ListViewItem();
                alarmItem.SubItems.Add(string.Empty);
                alarmItem.SubItems.Add(string.Empty);

                UpdateAlarmItem(alarm, alarmItem);
                this.listView1.Items.Add(alarmItem);
                this.RefreshAlarmGroups();
            });
        }

        public new void Show()
        {
            this.InvokeIfNecessary(base.Show);
        }

        public new void Hide()
        {
            this.InvokeIfNecessary(base.Hide);
        }

        private ListViewItem GetAlarmItem(IAlarm alarm)
        {
            return this.listView1.Items.Cast<ListViewItem>().Single(x => ((IAlarm)x.Tag).Id == alarm.Id);
        }

        private void RefreshAlarmGroups()
        {
            this.listView1.BeginUpdate();

            var grouppedItems = this.listView1.Items.Cast<ListViewItem>()
                .OrderBy(x => ((IAlarm)x.Tag).Time)
                .GroupBy(x => ((IAlarm)x.Tag).Time.RoundUp(TimeSpan.FromMinutes(alarmListGroupInterval))).ToArray();

            for (var key = 0; key < grouppedItems.Length; key++)
            {
                var group = grouppedItems[key];
                var begining = group.Key.AddMinutes(-alarmListGroupInterval);
                string title;
                if (begining.Date == DateTime.Today) {
                    title = begining.ToShortTimeString();
                } else {
                    title = string.Format("{0} {1}", begining.ToShortDateString(), begining.ToShortTimeString());
                }
                title += " - " + group.Key.ToShortTimeString();

                ListViewGroup listViewGroup;
                if (listView1.Groups.Count >= key)
                {
                    listViewGroup = new ListViewGroup(title);
                    this.listView1.Groups.Add(listViewGroup);
                }
                else
                {
                    listViewGroup = this.listView1.Groups[key];
                    listViewGroup.Header = title;
                }
                listViewGroup.Items.Cast<ListViewItem>().ToList().ForEach(x => x.Group = null);
                group.ToList().ForEach(item => item.Group = listViewGroup);
            }

            this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView1.EndUpdate();
        }

        private void InvokeIfNecessary(Action action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(action));
            }
            else
            {
                action.Invoke();
            }
        }

        private static void UpdateAlarmItem(IAlarm alarm, ListViewItem alarmItem)
        {
            alarmItem.Text = alarm.Title;
            alarmItem.SubItems[1].Text = alarm.Time.ToShortTimeString();
            alarmItem.SubItems[2].Text = AlarmStatusText(alarm);
            alarmItem.Tag = alarm;
            alarmItem.BackColor = GetColor(alarm);
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

        #region Form Events
        private Point clickedPosition;
        private bool WasRegistered;

        private void MainForm_Load(object sender, EventArgs e)
        {
            base.RegisterBar();

            if (this.OnLoad != null)
            {
                this.OnLoad.Invoke(sender, e);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.UnRegisterBar();

            if (this.OnClosing != null)
            {
                this.OnClosing.Invoke(sender, e);
            }
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.clickedPosition != Point.Empty && e.Button.HasFlag(MouseButtons.Left))
            {
                this.Left += e.Location.X - this.clickedPosition.X;
                this.Top += e.Location.Y - this.clickedPosition.Y;
            }
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.clickedPosition == Point.Empty && e.Button.HasFlag(MouseButtons.Left))
            {
                this.clickedPosition = e.Location;
            }
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            this.clickedPosition = Point.Empty;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.notifyIcon1.Visible = false;
            }
        }

        private void MainForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.notifyIcon1.Visible = false;
                this.hideToolStripMenuItem.Text = "&Hide";
                if (this.WasRegistered)
                {
                    this.WasRegistered = false;
                    base.RegisterBar();
                }
            }
            else
            {
                this.notifyIcon1.Visible = true;
                this.hideToolStripMenuItem.Text = "&Show";
                this.WasRegistered = IsRegistered;
                base.UnRegisterBar();
            }
        }
        #endregion

        #region Menu Events
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.OnExitRequest != null)
                this.OnExitRequest.Invoke(this, null);
        }

        private void hushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            var alarm = this.listView1.SelectedItems[0].Tag as IAlarm;
            if (this.OnHushRequest != null)
                this.OnHushRequest.Invoke(this, new AlarmEventArgs(alarm));

            this.hushToolStripMenuItem.Checked = alarm.IsHushed;
        }

        private void completeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            var alarm = this.listView1.SelectedItems[0].Tag as IAlarm;

            if (this.OnCompleteRequest != null)
                this.OnCompleteRequest.Invoke(this, new AlarmEventArgs(alarm));
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            var alarm = this.listView1.SelectedItems[0].Tag as IAlarm;

            if (this.OnCancelRequest != null)
                this.OnCancelRequest.Invoke(this, new AlarmEventArgs(alarm));
        }

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                return;

            var alarm = this.listView1.SelectedItems[0].Tag as Alarm;

            if (this.OnChangeRequest != null)
                this.OnChangeRequest.Invoke(this, new AlarmEventArgs(alarm));
        }

        private void newAlarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.OnNewRequest != null)
                this.OnNewRequest.Invoke(this, null);
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.OnHideRequest != null)
                this.OnHideRequest.Invoke(this, null);
        }

        private void soundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.soundToolStripMenuItem.Checked 
                    && !this.popupOnAlarmMenuItem.Checked 
                    && MessageBox.Show("\"Popup on Alarm\" feature is turned off. If you mute the sound, you will not be notified for ringing alarms. Are you sure?", "Notification Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
            {
                this.soundToolStripMenuItem.Checked = true;
                return;
            }

            if (this.OnEnableSoundChange != null)
                this.OnEnableSoundChange.Invoke(this, null);

            if (soundToolStripMenuItem.Checked)
            {
                this.label1.Text = "Alarmy";
            }
            else
            {
                this.label1.Text = "Alarmy (Muted)";
            }
        }

        private void popupOnAlarmMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.soundToolStripMenuItem.Checked
                    && !this.popupOnAlarmMenuItem.Checked
                    && MessageBox.Show("Alarmy is muted. If you disable \"Popup on Alarm\" feature, you will not be notified for ringing alarms. Are you sure?", "Notification Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
            {
                this.popupOnAlarmMenuItem.Checked = true;
                return;
            }

            if (this.OnPopupOnAlarmChange != null)
                this.OnPopupOnAlarmChange.Invoke(this, null);
        }

        private void smartAlarmStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (this.OnSmartAlarmChange != null)
                this.OnSmartAlarmChange.Invoke(this, null);
        }
        #endregion

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            var item = this.listView1.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                this.listView1.SelectedItems.Clear();
                item.Selected = true;

                var alarm = item.Tag as Alarm;
                this.completeToolStripMenuItem.Enabled = alarm.CanBeCompleted;
                this.cancelStripMenuItem.Enabled = alarm.CanBeCancelled;
                this.changeToolStripMenuItem.Enabled = alarm.CanBeSet;
                this.hushToolStripMenuItem.Checked = alarm.IsHushed;
                this.hushToolStripMenuItem.Enabled = alarm.Status == AlarmStatus.Ringing;

                this.itemContext.Show(Cursor.Position);
            }
            else
            {
                if (e.Button.HasFlag(MouseButtons.Right))
                {
                    this.listViewContext.Show(Cursor.Position);
                }
            }
        }
    }
}
