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
            buttonGetZoneNames = new Button();
            buttonUpdateZoneNames = new Button();
            pictureBox1 = new PictureBox();
            textBoxLog = new TextBox();
            groupBox1 = new GroupBox();
            listBoxFireAlarms = new ListBox();
            groupBox2 = new GroupBox();
            listBoxZoneFaults = new ListBox();
            groupBox3 = new GroupBox();
            listBoxControlPanelFaults = new ListBox();
            listBoxZoneNames = new ListBox();
            labelAlarm = new Label();
            labelFault = new Label();
            labelFireControlPanelConnection = new Label();
            checkBoxEmulator = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // buttonSwitchToDev
            // 
            buttonSwitchToDev.Enabled = false;
            buttonSwitchToDev.Location = new Point(453, 10);
            buttonSwitchToDev.Name = "buttonSwitchToDev";
            buttonSwitchToDev.Size = new Size(175, 23);
            buttonSwitchToDev.TabIndex = 0;
            buttonSwitchToDev.Text = "Switch to developer mode";
            buttonSwitchToDev.UseVisualStyleBackColor = true;
            buttonSwitchToDev.Click += buttonSwitchToDev_Click;
            // 
            // buttonGetZoneNames
            // 
            buttonGetZoneNames.Enabled = false;
            buttonGetZoneNames.Location = new Point(19, 241);
            buttonGetZoneNames.Name = "buttonGetZoneNames";
            buttonGetZoneNames.Size = new Size(119, 23);
            buttonGetZoneNames.TabIndex = 9;
            buttonGetZoneNames.Text = "Bölge İsimlerini Al";
            buttonGetZoneNames.UseVisualStyleBackColor = true;
            buttonGetZoneNames.Click += buttonGetZoneNames_Click;
            // 
            // buttonUpdateZoneNames
            // 
            buttonUpdateZoneNames.Enabled = false;
            buttonUpdateZoneNames.Location = new Point(144, 241);
            buttonUpdateZoneNames.Name = "buttonUpdateZoneNames";
            buttonUpdateZoneNames.Size = new Size(74, 23);
            buttonUpdateZoneNames.TabIndex = 10;
            buttonUpdateZoneNames.Text = "Güncelle";
            buttonUpdateZoneNames.UseVisualStyleBackColor = true;
            buttonUpdateZoneNames.Click += buttonUpdateZoneNames_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(233, 10);
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
            listBoxZoneNames.Location = new Point(19, 270);
            listBoxZoneNames.Name = "listBoxZoneNames";
            listBoxZoneNames.Size = new Size(192, 139);
            listBoxZoneNames.TabIndex = 20;
            // 
            // labelAlarm
            // 
            labelAlarm.BackColor = Color.White;
            labelAlarm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelAlarm.ForeColor = Color.White;
            labelAlarm.Location = new Point(112, 41);
            labelAlarm.Name = "labelAlarm";
            labelAlarm.Size = new Size(99, 23);
            labelAlarm.TabIndex = 21;
            labelAlarm.Text = "ALARM";
            labelAlarm.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelFault
            // 
            labelFault.BackColor = Color.White;
            labelFault.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelFault.ForeColor = Color.White;
            labelFault.Location = new Point(233, 41);
            labelFault.Name = "labelFault";
            labelFault.Size = new Size(386, 23);
            labelFault.TabIndex = 22;
            labelFault.Text = "HATA";
            labelFault.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelFireControlPanelConnection
            // 
            labelFireControlPanelConnection.BackColor = Color.Black;
            labelFireControlPanelConnection.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelFireControlPanelConnection.ForeColor = Color.White;
            labelFireControlPanelConnection.Location = new Point(19, 10);
            labelFireControlPanelConnection.Name = "labelFireControlPanelConnection";
            labelFireControlPanelConnection.Size = new Size(192, 23);
            labelFireControlPanelConnection.TabIndex = 23;
            labelFireControlPanelConnection.Text = "BAĞLANTI KONTROL...";
            labelFireControlPanelConnection.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // checkBoxEmulator
            // 
            checkBoxEmulator.AutoSize = true;
            checkBoxEmulator.Location = new Point(19, 41);
            checkBoxEmulator.Name = "checkBoxEmulator";
            checkBoxEmulator.Size = new Size(74, 19);
            checkBoxEmulator.TabIndex = 24;
            checkBoxEmulator.Text = "Emulator";
            checkBoxEmulator.UseVisualStyleBackColor = true;
            checkBoxEmulator.CheckedChanged += checkBoxEmulator_CheckedChanged;
            // 
            // FormUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(642, 416);
            Controls.Add(checkBoxEmulator);
            Controls.Add(labelFireControlPanelConnection);
            Controls.Add(labelFault);
            Controls.Add(labelAlarm);
            Controls.Add(listBoxZoneNames);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(textBoxLog);
            Controls.Add(pictureBox1);
            Controls.Add(buttonUpdateZoneNames);
            Controls.Add(buttonGetZoneNames);
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
        private Button buttonGetZoneNames;
        private Button buttonUpdateZoneNames;
        private PictureBox pictureBox1;
        private TextBox textBoxLog;
        private GroupBox groupBox1;
        private ListBox listBoxFireAlarms;
        private GroupBox groupBox2;
        private ListBox listBoxZoneFaults;
        private GroupBox groupBox3;
        private ListBox listBoxControlPanelFaults;
        private ListBox listBoxZoneNames;
        private Label labelAlarm;
        private Label labelFault;
        private Label labelFireControlPanelConnection;
        private CheckBox checkBoxEmulator;
    }
}