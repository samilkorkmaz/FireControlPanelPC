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
            this.btnConnect = new System.Windows.Forms.Button();
            this.comboBoxCOMPorts = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCommunicate = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonGetZoneNames = new System.Windows.Forms.Button();
            this.listBoxZoneNames = new System.Windows.Forms.ListBox();
            this.buttonSetZoneNames = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(232, 17);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // comboBoxCOMPorts
            // 
            this.comboBoxCOMPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCOMPorts.FormattingEnabled = true;
            this.comboBoxCOMPorts.Location = new System.Drawing.Point(12, 17);
            this.comboBoxCOMPorts.Name = "comboBoxCOMPorts";
            this.comboBoxCOMPorts.Size = new System.Drawing.Size(121, 23);
            this.comboBoxCOMPorts.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(139, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Baud rate: 9600";
            // 
            // buttonCommunicate
            // 
            this.buttonCommunicate.Enabled = false;
            this.buttonCommunicate.Location = new System.Drawing.Point(322, 17);
            this.buttonCommunicate.Name = "buttonCommunicate";
            this.buttonCommunicate.Size = new System.Drawing.Size(105, 23);
            this.buttonCommunicate.TabIndex = 4;
            this.buttonCommunicate.Text = "Communicate";
            this.buttonCommunicate.UseVisualStyleBackColor = true;
            this.buttonCommunicate.Click += new System.EventHandler(this.buttonCommunicate_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBoxLog.Location = new System.Drawing.Point(12, 56);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(776, 382);
            this.textBoxLog.TabIndex = 5;
            // 
            // buttonGetZoneNames
            // 
            this.buttonGetZoneNames.Enabled = false;
            this.buttonGetZoneNames.Location = new System.Drawing.Point(12, 457);
            this.buttonGetZoneNames.Name = "buttonGetZoneNames";
            this.buttonGetZoneNames.Size = new System.Drawing.Size(214, 23);
            this.buttonGetZoneNames.TabIndex = 6;
            this.buttonGetZoneNames.Text = "Get Zone Names";
            this.buttonGetZoneNames.UseVisualStyleBackColor = true;
            this.buttonGetZoneNames.Click += new System.EventHandler(this.buttonGetZoneNames_Click);
            // 
            // listBoxZoneNames
            // 
            this.listBoxZoneNames.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listBoxZoneNames.FormattingEnabled = true;
            this.listBoxZoneNames.ItemHeight = 15;
            this.listBoxZoneNames.Location = new System.Drawing.Point(12, 486);
            this.listBoxZoneNames.Name = "listBoxZoneNames";
            this.listBoxZoneNames.ScrollAlwaysVisible = true;
            this.listBoxZoneNames.Size = new System.Drawing.Size(776, 139);
            this.listBoxZoneNames.TabIndex = 7;
            // 
            // buttonSetZoneNames
            // 
            this.buttonSetZoneNames.Enabled = false;
            this.buttonSetZoneNames.Location = new System.Drawing.Point(12, 631);
            this.buttonSetZoneNames.Name = "buttonSetZoneNames";
            this.buttonSetZoneNames.Size = new System.Drawing.Size(214, 23);
            this.buttonSetZoneNames.TabIndex = 8;
            this.buttonSetZoneNames.Text = "Set Zone Names";
            this.buttonSetZoneNames.UseVisualStyleBackColor = true;
            this.buttonSetZoneNames.Click += new System.EventHandler(this.buttonSetZoneNames_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 732);
            this.Controls.Add(this.buttonSetZoneNames);
            this.Controls.Add(this.listBoxZoneNames);
            this.Controls.Add(this.buttonGetZoneNames);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.buttonCommunicate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxCOMPorts);
            this.Controls.Add(this.btnConnect);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Serial Connection Demo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnConnect;
        private ComboBox comboBoxCOMPorts;
        private Label label2;
        private Button buttonCommunicate;
        private TextBox textBoxLog;
        private Button buttonGetZoneNames;
        private ListBox listBoxZoneNames;
        private Button buttonSetZoneNames;
    }
}