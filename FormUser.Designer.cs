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
            buttonSwitchToDev = new Button();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            listBox1 = new ListBox();
            listBox2 = new ListBox();
            listBox3 = new ListBox();
            label1 = new Label();
            listBox4 = new ListBox();
            textBox3 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // buttonSwitchToDev
            // 
            buttonSwitchToDev.Location = new Point(227, 13);
            buttonSwitchToDev.Name = "buttonSwitchToDev";
            buttonSwitchToDev.Size = new Size(189, 23);
            buttonSwitchToDev.TabIndex = 0;
            buttonSwitchToDev.Text = "Switch to developer mode";
            buttonSwitchToDev.UseVisualStyleBackColor = true;
            buttonSwitchToDev.Click += buttonSwitchToDev_Click;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Red;
            textBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBox1.ForeColor = Color.White;
            textBox1.Location = new Point(12, 41);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(189, 23);
            textBox1.TabIndex = 1;
            textBox1.Text = "ALARM";
            textBox1.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox2
            // 
            textBox2.BackColor = Color.Yellow;
            textBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBox2.ForeColor = Color.Red;
            textBox2.Location = new Point(227, 41);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(384, 23);
            textBox2.TabIndex = 2;
            textBox2.Text = "HATA";
            textBox2.TextAlign = HorizontalAlignment.Center;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Items.AddRange(new object[] { "Yangın Alarmları:", "1- ", "2- ", "3- ", "4- ", "5- ", "6- ", "7- ", "8- " });
            listBox1.Location = new Point(12, 70);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(189, 154);
            listBox1.TabIndex = 3;
            // 
            // listBox2
            // 
            listBox2.FormattingEnabled = true;
            listBox2.Items.AddRange(new object[] { "Bölge Hataları", "1- ", "2- ", "3- ", "4- ", "5- ", "6- ", "7- ", "8- " });
            listBox2.Location = new Point(227, 70);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(189, 154);
            listBox2.TabIndex = 4;
            // 
            // listBox3
            // 
            listBox3.FormattingEnabled = true;
            listBox3.Items.AddRange(new object[] { "Kontrol Paneli Hataları:", "1- ", "2- ", "3- ", "4- ", "5- ", "6- ", "7- ", "8- " });
            listBox3.Location = new Point(422, 70);
            listBox3.Name = "listBox3";
            listBox3.Size = new Size(189, 154);
            listBox3.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 243);
            label1.Name = "label1";
            label1.Size = new Size(78, 15);
            label1.TabIndex = 6;
            label1.Text = "Bölge İsimleri";
            // 
            // listBox4
            // 
            listBox4.FormattingEnabled = true;
            listBox4.Items.AddRange(new object[] { "1- ", "2- ", "3- ", "4- ", "5- ", "6- ", "7- ", "8- " });
            listBox4.Location = new Point(12, 268);
            listBox4.Name = "listBox4";
            listBox4.Size = new Size(206, 139);
            listBox4.TabIndex = 7;
            // 
            // textBox3
            // 
            textBox3.BackColor = Color.FromArgb(0, 192, 0);
            textBox3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBox3.ForeColor = Color.White;
            textBox3.Location = new Point(12, 13);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(189, 23);
            textBox3.TabIndex = 8;
            textBox3.Text = "BAĞLANTI VAR";
            textBox3.TextAlign = HorizontalAlignment.Center;
            // 
            // button1
            // 
            button1.Location = new Point(96, 239);
            button1.Name = "button1";
            button1.Size = new Size(42, 23);
            button1.TabIndex = 9;
            button1.Text = "Al";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(144, 239);
            button2.Name = "button2";
            button2.Size = new Size(74, 23);
            button2.TabIndex = 10;
            button2.Text = "Güncelle";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // FormUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 414);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox3);
            Controls.Add(listBox4);
            Controls.Add(label1);
            Controls.Add(listBox3);
            Controls.Add(listBox2);
            Controls.Add(listBox1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(buttonSwitchToDev);
            MaximizeBox = false;
            Name = "FormUser";
            Text = "NVS-Pointer Yangın Algılama ve İhbar Paneli - v1.0";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonSwitchToDev;
        private TextBox textBox1;
        private TextBox textBox2;
        private ListBox listBox1;
        private ListBox listBox2;
        private ListBox listBox3;
        private Label label1;
        private ListBox listBox4;
        private TextBox textBox3;
        private Button button1;
        private Button button2;
    }
}