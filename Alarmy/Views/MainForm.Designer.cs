namespace Alarmy.Views
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.titleHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewContext = new System.Windows.Forms.ContextMenu();
            this.newAlarmToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.MenuItem("-");
            this.soundToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.popupOnAlarmMenuItem = new System.Windows.Forms.MenuItem();
            this.smartAlarmStripMenuItem = new System.Windows.Forms.MenuItem("-");
            this.hideToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.MenuItem("-");
            this.exitToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.itemContext = new System.Windows.Forms.ContextMenu();
            this.changeToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.completeToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.cancelStripMenuItem = new System.Windows.Forms.MenuItem();
            this.hushToolStripMenuItem = new System.Windows.Forms.MenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.components.Add(this.listViewContext);
            this.components.Add(this.itemContext);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.titleHeader,
            this.timeHeader,
            this.statusHeader});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(3, 33);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(211, 1004);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            this.listView1.ContextMenu = listViewContext;
            // 
            // titleHeader
            // 
            this.titleHeader.Text = "Title";
            // 
            // timeHeader
            // 
            this.timeHeader.Text = "Time";
            // 
            // statusHeader
            // 
            this.statusHeader.Text = "Status";
            // 
            // listViewContext
            // 
            this.listViewContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.newAlarmToolStripMenuItem,
            this.toolStripMenuItem1,
            this.soundToolStripMenuItem,
            this.popupOnAlarmMenuItem,
            this.smartAlarmStripMenuItem,
            this.hideToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.listViewContext.Name = "listViewContext";
            // 
            // newAlarmToolStripMenuItem
            // 
            this.newAlarmToolStripMenuItem.Name = "newAlarmToolStripMenuItem";
            this.newAlarmToolStripMenuItem.Text = "&New Alarm";
            this.newAlarmToolStripMenuItem.Click += new System.EventHandler(this.newAlarmToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // soundToolStripMenuItem
            // 
            this.soundToolStripMenuItem.Checked = true;
            this.soundToolStripMenuItem.Name = "soundToolStripMenuItem";
            this.soundToolStripMenuItem.Text = "&Sound";
            this.soundToolStripMenuItem.Click += new System.EventHandler(this.soundToolStripMenuItem_CheckedChanged);
            // 
            // popupOnAlarmMenuItem
            // 
            this.popupOnAlarmMenuItem.Checked = true;
            this.popupOnAlarmMenuItem.Name = "popupOnAlarmMenuItem";
            this.popupOnAlarmMenuItem.Text = "&Popup on Alarm";
            this.popupOnAlarmMenuItem.Click += new System.EventHandler(this.popupOnAlarmMenuItem_CheckedChanged);
            // 
            // smartAlarmStripMenuItem
            // 
            this.smartAlarmStripMenuItem.Checked = true;
            this.smartAlarmStripMenuItem.Name = "smartAlarmStripMenuItem";
            this.smartAlarmStripMenuItem.Text = "Smart Alarm";
            this.smartAlarmStripMenuItem.Click += new System.EventHandler(this.smartAlarmStripMenuItem_CheckedChanged);
            // 
            // hideToolStripMenuItem
            // 
            this.hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            this.hideToolStripMenuItem.Text = "&Hide";
            this.hideToolStripMenuItem.Click += new System.EventHandler(this.hideToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(217, 1040);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 30);
            this.label1.TabIndex = 1;
            this.label1.Text = "Alarms";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label1_MouseDown);
            this.label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label1_MouseMove);
            this.label1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label1_MouseUp);
            // 
            // itemContext
            // 
            this.itemContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.changeToolStripMenuItem,
            this.completeToolStripMenuItem,
            this.cancelStripMenuItem,
            this.hushToolStripMenuItem});
            this.itemContext.Name = "itemContext";
            // 
            // changeToolStripMenuItem
            // 
            this.changeToolStripMenuItem.Name = "changeToolStripMenuItem";
            this.changeToolStripMenuItem.Text = "&Change";
            this.changeToolStripMenuItem.Click += new System.EventHandler(this.changeToolStripMenuItem_Click);
            // 
            // completeToolStripMenuItem
            // 
            this.completeToolStripMenuItem.Name = "completeToolStripMenuItem";
            this.completeToolStripMenuItem.Text = "Co&mplete";
            this.completeToolStripMenuItem.Click += new System.EventHandler(this.completeToolStripMenuItem_Click);
            // 
            // cancelStripMenuItem
            // 
            this.cancelStripMenuItem.Name = "cancelStripMenuItem";
            this.cancelStripMenuItem.Text = "Ca&ncel";
            this.cancelStripMenuItem.Click += new System.EventHandler(this.cancelToolStripMenuItem_Click);
            // 
            // hushToolStripMenuItem
            // 
            this.hushToolStripMenuItem.Name = "hushToolStripMenuItem";
            this.hushToolStripMenuItem.Text = "&Hush";
            this.hushToolStripMenuItem.Click += new System.EventHandler(this.hushToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenu = this.listViewContext;
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += hideToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(217, 1040);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Alarmy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenu listViewContext;
        private System.Windows.Forms.MenuItem newAlarmToolStripMenuItem;
        private System.Windows.Forms.MenuItem toolStripMenuItem1;
        private System.Windows.Forms.MenuItem soundToolStripMenuItem;
        private System.Windows.Forms.MenuItem hideToolStripMenuItem;
        private System.Windows.Forms.MenuItem toolStripMenuItem2;
        private System.Windows.Forms.MenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ContextMenu itemContext;
        private System.Windows.Forms.MenuItem changeToolStripMenuItem;
        private System.Windows.Forms.MenuItem completeToolStripMenuItem;
        private System.Windows.Forms.MenuItem cancelStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.MenuItem popupOnAlarmMenuItem;
        private System.Windows.Forms.MenuItem hushToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader titleHeader;
        private System.Windows.Forms.ColumnHeader timeHeader;
        private System.Windows.Forms.ColumnHeader statusHeader;
        private System.Windows.Forms.MenuItem smartAlarmStripMenuItem;
    }
}