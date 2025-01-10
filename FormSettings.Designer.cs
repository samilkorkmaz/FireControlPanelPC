namespace FireControlPanelPC
{
    partial class FormSettings
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
            label1 = new Label();
            numericUpDownWriteReadDelay_ms = new NumericUpDown();
            numericUpDownPollingPeriod_ms = new NumericUpDown();
            label2 = new Label();
            buttonOk = new Button();
            buttonCancel = new Button();
            checkBoxShowLog = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDownWriteReadDelay_ms).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPollingPeriod_ms).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 45);
            label1.Name = "label1";
            label1.Size = new Size(216, 15);
            label1.TabIndex = 0;
            label1.Text = "Serial yazma/okuma arası bekleme [ms]";
            label1.TextAlign = ContentAlignment.TopRight;
            // 
            // numericUpDownWriteReadDelay_ms
            // 
            numericUpDownWriteReadDelay_ms.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numericUpDownWriteReadDelay_ms.Location = new Point(236, 41);
            numericUpDownWriteReadDelay_ms.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDownWriteReadDelay_ms.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numericUpDownWriteReadDelay_ms.Name = "numericUpDownWriteReadDelay_ms";
            numericUpDownWriteReadDelay_ms.Size = new Size(84, 23);
            numericUpDownWriteReadDelay_ms.TabIndex = 1;
            numericUpDownWriteReadDelay_ms.Value = new decimal(new int[] { 300, 0, 0, 0 });
            // 
            // numericUpDownPollingPeriod_ms
            // 
            numericUpDownPollingPeriod_ms.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numericUpDownPollingPeriod_ms.Location = new Point(236, 12);
            numericUpDownPollingPeriod_ms.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDownPollingPeriod_ms.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numericUpDownPollingPeriod_ms.Name = "numericUpDownPollingPeriod_ms";
            numericUpDownPollingPeriod_ms.Size = new Size(84, 23);
            numericUpDownPollingPeriod_ms.TabIndex = 3;
            numericUpDownPollingPeriod_ms.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(53, 16);
            label2.Name = "label2";
            label2.Size = new Size(174, 15);
            label2.TabIndex = 2;
            label2.Text = "Alarm/hata sorgu peryodu [ms]";
            label2.TextAlign = ContentAlignment.TopRight;
            // 
            // buttonOk
            // 
            buttonOk.Location = new Point(80, 108);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(75, 23);
            buttonOk.TabIndex = 4;
            buttonOk.Text = "Tamam";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(176, 108);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "İptal";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // checkBoxShowLog
            // 
            checkBoxShowLog.AutoSize = true;
            checkBoxShowLog.Location = new Point(12, 79);
            checkBoxShowLog.Name = "checkBoxShowLog";
            checkBoxShowLog.Size = new Size(82, 19);
            checkBoxShowLog.TabIndex = 6;
            checkBoxShowLog.Text = "Log göster";
            checkBoxShowLog.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(351, 141);
            Controls.Add(checkBoxShowLog);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(numericUpDownPollingPeriod_ms);
            Controls.Add(label2);
            Controls.Add(numericUpDownWriteReadDelay_ms);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            Name = "FormSettings";
            Text = "Ayarlar";
            ((System.ComponentModel.ISupportInitialize)numericUpDownWriteReadDelay_ms).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPollingPeriod_ms).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private NumericUpDown numericUpDownWriteReadDelay_ms;
        private NumericUpDown numericUpDownPollingPeriod_ms;
        private Label label2;
        private Button buttonOk;
        private Button buttonCancel;
        private CheckBox checkBoxShowLog;
    }
}