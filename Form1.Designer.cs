namespace WinFormsSerial
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnConnect = new Button();
            comboBoxCOMPorts = new ComboBox();
            label2 = new Label();
            buttonCommunicate1Hz = new Button();
            textBoxLog = new TextBox();
            buttonGetZoneNames = new Button();
            listBoxZoneNames = new ListBox();
            buttonUpdateZoneNames = new Button();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(232, 17);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(93, 23);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += buttonConnect_Click;
            // 
            // comboBoxCOMPorts
            // 
            comboBoxCOMPorts.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxCOMPorts.FormattingEnabled = true;
            comboBoxCOMPorts.Location = new Point(12, 17);
            comboBoxCOMPorts.Name = "comboBoxCOMPorts";
            comboBoxCOMPorts.Size = new Size(121, 23);
            comboBoxCOMPorts.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(139, 21);
            label2.Name = "label2";
            label2.Size = new Size(87, 15);
            label2.TabIndex = 3;
            label2.Text = "Baud rate: 9600";
            // 
            // buttonCommunicate1Hz
            // 
            buttonCommunicate1Hz.Enabled = false;
            buttonCommunicate1Hz.Location = new Point(336, 17);
            buttonCommunicate1Hz.Name = "buttonCommunicate1Hz";
            buttonCommunicate1Hz.Size = new Size(137, 23);
            buttonCommunicate1Hz.TabIndex = 4;
            buttonCommunicate1Hz.Text = "Communicate 1Hz";
            buttonCommunicate1Hz.UseVisualStyleBackColor = true;
            buttonCommunicate1Hz.Click += buttonCommunicate1Hz_Click;
            // 
            // textBoxLog
            // 
            textBoxLog.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxLog.Location = new Point(12, 56);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Both;
            textBoxLog.Size = new Size(776, 382);
            textBoxLog.TabIndex = 5;
            // 
            // buttonGetZoneNames
            // 
            buttonGetZoneNames.Enabled = false;
            buttonGetZoneNames.Location = new Point(12, 457);
            buttonGetZoneNames.Name = "buttonGetZoneNames";
            buttonGetZoneNames.Size = new Size(214, 23);
            buttonGetZoneNames.TabIndex = 6;
            buttonGetZoneNames.Text = "Get Zone Names";
            buttonGetZoneNames.UseVisualStyleBackColor = true;
            buttonGetZoneNames.Click += buttonGetZoneNames_Click;
            // 
            // listBoxZoneNames
            // 
            listBoxZoneNames.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point);
            listBoxZoneNames.FormattingEnabled = true;
            listBoxZoneNames.ItemHeight = 15;
            listBoxZoneNames.Location = new Point(12, 486);
            listBoxZoneNames.Name = "listBoxZoneNames";
            listBoxZoneNames.ScrollAlwaysVisible = true;
            listBoxZoneNames.Size = new Size(776, 139);
            listBoxZoneNames.TabIndex = 7;
            // 
            // buttonUpdateZoneNames
            // 
            buttonUpdateZoneNames.Enabled = false;
            buttonUpdateZoneNames.Location = new Point(12, 631);
            buttonUpdateZoneNames.Name = "buttonUpdateZoneNames";
            buttonUpdateZoneNames.Size = new Size(214, 23);
            buttonUpdateZoneNames.TabIndex = 8;
            buttonUpdateZoneNames.Text = "Update Zone Names";
            buttonUpdateZoneNames.UseVisualStyleBackColor = true;
            buttonUpdateZoneNames.Click += buttonUpdateZoneNames_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 732);
            Controls.Add(buttonUpdateZoneNames);
            Controls.Add(listBoxZoneNames);
            Controls.Add(buttonGetZoneNames);
            Controls.Add(textBoxLog);
            Controls.Add(buttonCommunicate1Hz);
            Controls.Add(label2);
            Controls.Add(comboBoxCOMPorts);
            Controls.Add(btnConnect);
            MaximizeBox = false;
            Name = "Form1";
            Text = "Serial Connection Demo";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnConnect;
        private ComboBox comboBoxCOMPorts;
        private Label label2;
        private Button buttonCommunicate1Hz;
        private TextBox textBoxLog;
        private Button buttonGetZoneNames;
        private ListBox listBoxZoneNames;
        private Button buttonUpdateZoneNames;
    }
}