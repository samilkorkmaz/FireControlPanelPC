namespace WinFormsSerial
{
    partial class FormUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUser));
            buttonSwitchToDev = new Button();
            textBoxAlarm = new TextBox();
            textBox2 = new TextBox();
            textBoxFireControlPanelConnection = new TextBox();
            button1 = new Button();
            button2 = new Button();
            pictureBox1 = new PictureBox();
            textBoxLog = new TextBox();
            groupBox1 = new GroupBox();
            listBoxFireAlarms = new ListBox();
            groupBox2 = new GroupBox();
            listBoxZoneFaults = new ListBox();
            groupBox3 = new GroupBox();
            listBoxControlPanelFaults = new ListBox();
            listBoxZoneNames = new ListBox();
            listBox9 = new ListBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // buttonSwitchToDev
            // 
            buttonSwitchToDev.Location = new Point(453, 10);
            buttonSwitchToDev.Name = "buttonSwitchToDev";
            buttonSwitchToDev.Size = new Size(175, 23);
            buttonSwitchToDev.TabIndex = 0;
            buttonSwitchToDev.Text = "Switch to developer mode";
            buttonSwitchToDev.UseVisualStyleBackColor = true;
            buttonSwitchToDev.Click += buttonSwitchToDev_Click;
            // 
            // textBoxAlarm
            // 
            textBoxAlarm.BackColor = Color.Red;
            textBoxAlarm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBoxAlarm.ForeColor = Color.White;
            textBoxAlarm.Location = new Point(12, 41);
            textBoxAlarm.Name = "textBoxAlarm";
            textBoxAlarm.ReadOnly = true;
            textBoxAlarm.Size = new Size(206, 23);
            textBoxAlarm.TabIndex = 1;
            textBoxAlarm.Text = "ALARM";
            textBoxAlarm.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox2
            // 
            textBox2.BackColor = Color.Yellow;
            textBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBox2.ForeColor = Color.Red;
            textBox2.Location = new Point(227, 41);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(401, 23);
            textBox2.TabIndex = 2;
            textBox2.Text = "HATA";
            textBox2.TextAlign = HorizontalAlignment.Center;
            // 
            // textBoxFireControlPanelConnection
            // 
            textBoxFireControlPanelConnection.BackColor = Color.Black;
            textBoxFireControlPanelConnection.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBoxFireControlPanelConnection.ForeColor = Color.White;
            textBoxFireControlPanelConnection.Location = new Point(12, 10);
            textBoxFireControlPanelConnection.Name = "textBoxFireControlPanelConnection";
            textBoxFireControlPanelConnection.ReadOnly = true;
            textBoxFireControlPanelConnection.Size = new Size(206, 23);
            textBoxFireControlPanelConnection.TabIndex = 8;
            textBoxFireControlPanelConnection.Text = "BAĞLANTI KONTROL...";
            textBoxFireControlPanelConnection.TextAlign = HorizontalAlignment.Center;
            // 
            // button1
            // 
            button1.Location = new Point(19, 241);
            button1.Name = "button1";
            button1.Size = new Size(119, 23);
            button1.TabIndex = 9;
            button1.Text = "Bölge İsimlerini Al";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(144, 241);
            button2.Name = "button2";
            button2.Size = new Size(74, 23);
            button2.TabIndex = 10;
            button2.Text = "Güncelle";
            button2.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(227, 10);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(194, 25);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.TabIndex = 11;
            pictureBox1.TabStop = false;
            // 
            // textBoxLog
            // 
            textBoxLog.Font = new Font("Courier New", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxLog.Location = new Point(227, 241);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Both;
            textBoxLog.Size = new Size(401, 168);
            textBoxLog.TabIndex = 14;
            textBoxLog.Text = "Log";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(listBoxFireAlarms);
            groupBox1.Location = new Point(12, 70);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(206, 165);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Yangın Alarmları";
            // 
            // listBoxFireAlarms
            // 
            listBoxFireAlarms.Font = new Font("Courier New", 9F);
            listBoxFireAlarms.FormattingEnabled = true;
            listBoxFireAlarms.Location = new Point(7, 20);
            listBoxFireAlarms.Name = "listBoxFireAlarms";
            listBoxFireAlarms.SelectionMode = SelectionMode.None;
            listBoxFireAlarms.Size = new Size(192, 139);
            listBoxFireAlarms.TabIndex = 7;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(listBoxZoneFaults);
            groupBox2.Location = new Point(227, 70);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(206, 165);
            groupBox2.TabIndex = 17;
            groupBox2.TabStop = false;
            groupBox2.Text = "Bölge Hataları";
            // 
            // listBoxZoneFaults
            // 
            listBoxZoneFaults.Font = new Font("Courier New", 9F);
            listBoxZoneFaults.FormattingEnabled = true;
            listBoxZoneFaults.Location = new Point(6, 20);
            listBoxZoneFaults.Name = "listBoxZoneFaults";
            listBoxZoneFaults.SelectionMode = SelectionMode.None;
            listBoxZoneFaults.Size = new Size(193, 139);
            listBoxZoneFaults.TabIndex = 7;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(listBoxControlPanelFaults);
            groupBox3.Location = new Point(439, 70);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(189, 165);
            groupBox3.TabIndex = 18;
            groupBox3.TabStop = false;
            groupBox3.Text = "Kontrol Paneli Hataları";
            // 
            // listBoxControlPanelFaults
            // 
            listBoxControlPanelFaults.Font = new Font("Courier New", 9F);
            listBoxControlPanelFaults.FormattingEnabled = true;
            listBoxControlPanelFaults.Location = new Point(6, 20);
            listBoxControlPanelFaults.Name = "listBoxControlPanelFaults";
            listBoxControlPanelFaults.SelectionMode = SelectionMode.None;
            listBoxControlPanelFaults.Size = new Size(174, 139);
            listBoxControlPanelFaults.TabIndex = 7;
            // 
            // listBoxZoneNames
            // 
            listBoxZoneNames.Font = new Font("Courier New", 9F);
            listBoxZoneNames.FormattingEnabled = true;
            listBoxZoneNames.Location = new Point(37, 270);
            listBoxZoneNames.Name = "listBoxZoneNames";
            listBoxZoneNames.Size = new Size(174, 139);
            listBoxZoneNames.TabIndex = 20;
            // 
            // listBox9
            // 
            listBox9.Font = new Font("Courier New", 9F);
            listBox9.FormattingEnabled = true;
            listBox9.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8" });
            listBox9.Location = new Point(19, 270);
            listBox9.Name = "listBox9";
            listBox9.SelectionMode = SelectionMode.None;
            listBox9.Size = new Size(20, 139);
            listBox9.TabIndex = 19;
            // 
            // FormUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(642, 423);
            Controls.Add(listBoxZoneNames);
            Controls.Add(listBox9);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(textBoxLog);
            Controls.Add(pictureBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBoxFireControlPanelConnection);
            Controls.Add(textBox2);
            Controls.Add(textBoxAlarm);
            Controls.Add(buttonSwitchToDev);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FormUser";
            Text = "NVS-Pointer Yangın Algılama ve İhbar Paneli - v1.0";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonSwitchToDev;
        private TextBox textBoxAlarm;
        private TextBox textBox2;
        private ListBox listBox3;
        private TextBox textBoxFireControlPanelConnection;
        private Button button1;
        private Button button2;
        private PictureBox pictureBox1;
        private ListBox listBox5;
        private TextBox textBoxLog;
        private GroupBox groupBox1;
        private ListBox listBoxFireAlarms;
        private GroupBox groupBox2;
        private ListBox listBoxZoneFaults;
        private GroupBox groupBox3;
        private ListBox listBoxControlPanelFaults;
        private ListBox listBoxZoneNames;
        private ListBox listBox9;
    }
}