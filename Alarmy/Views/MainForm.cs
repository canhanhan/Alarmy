using Alarmy.Common;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Alarmy.Views
{
    internal partial class MainForm : AppBar
    {
        private readonly IAlarmService alarmService;
        private delegate void ShowDelegate();

        private Point clickedPosition;
        private bool WasRegistered;

        public MainForm(IAlarmService alarmService)
        {
            this.alarmService = alarmService;
            this.alarmService.Interval = 1000;
            this.alarmService.AlarmStatusChanged += alarmService_AlarmStatusChanged;

            InitializeComponent();
            listView1.DoubleBuffered(true);
            notifyIcon1.Icon = Icon;
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
            Invoke(new ShowDelegate(AlarmStatusChanged));
        }

        private void AlarmStatusChanged()
        {
            if (!Visible)
            {
                Show();
            }
            RefreshList();
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
                                    .GroupBy(x => x.Time.RoundUp(TimeSpan.FromMinutes(15)))
                                    .ToList().ForEach(x =>
            {
                var group = new ListViewGroup(x.Key.ToString(), String.Format("{0}-{1}", x.Key.AddMinutes(-15).ToShortTimeString(), x.Key.ToShortTimeString()));
                listView1.Groups.Add(group);
                listView1.Items.AddRange(x.Select(item => new ListViewItem(new [] { item.Title, item.Time.ToShortTimeString(), item.Status.ToString() }, group) { Tag = item, BackColor = GetColor(item), ToolTipText = item.CancelReason }).ToArray());
            });

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.EndUpdate();
        }


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

                itemContext.Show(Cursor.Position);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void completeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var alarm = listView1.SelectedItems[0].Tag as Alarm;
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
                alarm.Cancel(cancelForm.CancelReason.Text);
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

                alarmService.Add(alarm);
                RefreshList();
            }
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
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
    }
}
