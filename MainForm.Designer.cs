namespace TamaSmartApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label deviceCountLabel;
        private System.Windows.Forms.ComboBox deviceComboBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button readIdButton;
        private System.Windows.Forms.Label flashIdLabel;
        private System.Windows.Forms.Label icNameLabel;
        private System.Windows.Forms.Label chipSizeLabel;
        private System.Windows.Forms.Label icLabel;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.Label themeLabel;
        private System.Windows.Forms.Label chipThemeLabel;
        private System.Windows.Forms.Button readFlashButton;
        private System.Windows.Forms.Button writeFlashButton;
        private System.Windows.Forms.Button eraseButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.GroupBox deviceGroupBox;
        private System.Windows.Forms.GroupBox flashGroupBox;
        private System.Windows.Forms.GroupBox logGroupBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.deviceGroupBox = new System.Windows.Forms.GroupBox();
            this.deviceCountLabel = new System.Windows.Forms.Label();
            this.deviceComboBox = new System.Windows.Forms.ComboBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.flashGroupBox = new System.Windows.Forms.GroupBox();
            this.readIdButton = new System.Windows.Forms.Button();
            this.flashIdLabel = new System.Windows.Forms.Label();
            this.icLabel = new System.Windows.Forms.Label();
            this.icNameLabel = new System.Windows.Forms.Label();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.chipSizeLabel = new System.Windows.Forms.Label();
            this.themeLabel = new System.Windows.Forms.Label();
            this.chipThemeLabel = new System.Windows.Forms.Label();
            this.readFlashButton = new System.Windows.Forms.Button();
            this.writeFlashButton = new System.Windows.Forms.Button();
            this.eraseButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            this.logGroupBox = new System.Windows.Forms.GroupBox();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.deviceGroupBox.SuspendLayout();
            this.flashGroupBox.SuspendLayout();
            this.logGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // deviceGroupBox
            // 
            this.deviceGroupBox.Controls.Add(this.deviceCountLabel);
            this.deviceGroupBox.Controls.Add(this.deviceComboBox);
            this.deviceGroupBox.Controls.Add(this.connectButton);
            this.deviceGroupBox.Controls.Add(this.refreshButton);
            this.deviceGroupBox.Location = new System.Drawing.Point(12, 12);
            this.deviceGroupBox.Name = "deviceGroupBox";
            this.deviceGroupBox.Size = new System.Drawing.Size(526, 80);
            this.deviceGroupBox.TabIndex = 0;
            this.deviceGroupBox.TabStop = false;
            this.deviceGroupBox.Text = "Device";
            // 
            // deviceCountLabel
            // 
            this.deviceCountLabel.AutoSize = true;
            this.deviceCountLabel.Location = new System.Drawing.Point(6, 22);
            this.deviceCountLabel.Name = "deviceCountLabel";
            this.deviceCountLabel.Size = new System.Drawing.Size(95, 13);
            this.deviceCountLabel.TabIndex = 0;
            this.deviceCountLabel.Text = "พบอุปกรณ์: 0 เครื่อง";
            // 
            // deviceComboBox
            // 
            this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceComboBox.FormattingEnabled = true;
            this.deviceComboBox.Location = new System.Drawing.Point(6, 38);
            this.deviceComboBox.Name = "deviceComboBox";
            this.deviceComboBox.Size = new System.Drawing.Size(200, 21);
            this.deviceComboBox.TabIndex = 1;
            // 
            // connectButton
            // 
            this.connectButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.connectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.connectButton.ForeColor = System.Drawing.Color.White;
            this.connectButton.Location = new System.Drawing.Point(212, 36);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 25);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = false;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(318, 36);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 25);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // flashGroupBox
            // 
            this.flashGroupBox.Controls.Add(this.readIdButton);
            this.flashGroupBox.Controls.Add(this.flashIdLabel);
            this.flashGroupBox.Controls.Add(this.icLabel);
            this.flashGroupBox.Controls.Add(this.icNameLabel);
            this.flashGroupBox.Controls.Add(this.sizeLabel);
            this.flashGroupBox.Controls.Add(this.chipSizeLabel);
            this.flashGroupBox.Controls.Add(this.themeLabel);
            this.flashGroupBox.Controls.Add(this.chipThemeLabel);
            this.flashGroupBox.Controls.Add(this.readFlashButton);
            this.flashGroupBox.Controls.Add(this.writeFlashButton);
            this.flashGroupBox.Controls.Add(this.eraseButton);
            this.flashGroupBox.Controls.Add(this.resetButton);
            this.flashGroupBox.Controls.Add(this.progressBar);
            this.flashGroupBox.Controls.Add(this.progressLabel);
            this.flashGroupBox.Location = new System.Drawing.Point(12, 98);
            this.flashGroupBox.Name = "flashGroupBox";
            this.flashGroupBox.Size = new System.Drawing.Size(526, 200);
            this.flashGroupBox.TabIndex = 1;
            this.flashGroupBox.TabStop = false;
            this.flashGroupBox.Text = "Flash Operations";
            // 
            // readIdButton
            // 
            this.readIdButton.Location = new System.Drawing.Point(6, 22);
            this.readIdButton.Name = "readIdButton";
            this.readIdButton.Size = new System.Drawing.Size(100, 30);
            this.readIdButton.TabIndex = 0;
            this.readIdButton.Text = "Refresh";
            this.readIdButton.UseVisualStyleBackColor = true;
            this.readIdButton.Click += new System.EventHandler(this.readIdButton_Click);
            // 
            // flashIdLabel
            // 
            this.flashIdLabel.AutoSize = true;
            this.flashIdLabel.Location = new System.Drawing.Point(112, 30);
            this.flashIdLabel.Name = "flashIdLabel";
            this.flashIdLabel.Size = new System.Drawing.Size(50, 13);
            this.flashIdLabel.TabIndex = 1;
            this.flashIdLabel.Text = "Chip ID: -";
            // 
            // icLabel
            // 
            this.icLabel.AutoSize = true;
            this.icLabel.Location = new System.Drawing.Point(6, 60);
            this.icLabel.Name = "icLabel";
            this.icLabel.Size = new System.Drawing.Size(24, 13);
            this.icLabel.TabIndex = 9;
            this.icLabel.Text = "IC:";
            // 
            // icNameLabel
            // 
            this.icNameLabel.AutoSize = true;
            this.icNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.icNameLabel.Location = new System.Drawing.Point(36, 60);
            this.icNameLabel.Name = "icNameLabel";
            this.icNameLabel.Size = new System.Drawing.Size(12, 13);
            this.icNameLabel.TabIndex = 10;
            this.icNameLabel.Text = "-";
            // 
            // sizeLabel
            // 
            this.sizeLabel.AutoSize = true;
            this.sizeLabel.Location = new System.Drawing.Point(150, 60);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(30, 13);
            this.sizeLabel.TabIndex = 11;
            this.sizeLabel.Text = "Size:";
            // 
            // chipSizeLabel
            // 
            this.chipSizeLabel.AutoSize = true;
            this.chipSizeLabel.Location = new System.Drawing.Point(186, 60);
            this.chipSizeLabel.Name = "chipSizeLabel";
            this.chipSizeLabel.Size = new System.Drawing.Size(10, 13);
            this.chipSizeLabel.TabIndex = 12;
            this.chipSizeLabel.Text = "-";
            // 
            // themeLabel
            // 
            this.themeLabel.AutoSize = true;
            this.themeLabel.Location = new System.Drawing.Point(6, 80);
            this.themeLabel.Name = "themeLabel";
            this.themeLabel.Size = new System.Drawing.Size(43, 13);
            this.themeLabel.TabIndex = 20;
            this.themeLabel.Text = "Theme:";
            // 
            // chipThemeLabel
            // 
            this.chipThemeLabel.AutoSize = true;
            this.chipThemeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chipThemeLabel.Location = new System.Drawing.Point(55, 80);
            this.chipThemeLabel.Name = "chipThemeLabel";
            this.chipThemeLabel.Size = new System.Drawing.Size(12, 13);
            this.chipThemeLabel.TabIndex = 21;
            this.chipThemeLabel.Text = "-";
            // 
            // readFlashButton
            // 
            this.readFlashButton.Location = new System.Drawing.Point(6, 100);
            this.readFlashButton.Name = "readFlashButton";
            this.readFlashButton.Size = new System.Drawing.Size(120, 35);
            this.readFlashButton.TabIndex = 6;
            this.readFlashButton.Text = "อ่าน Chip";
            this.readFlashButton.UseVisualStyleBackColor = true;
            this.readFlashButton.Click += new System.EventHandler(this.readFlashButton_Click);
            // 
            // writeFlashButton
            // 
            this.writeFlashButton.Location = new System.Drawing.Point(132, 100);
            this.writeFlashButton.Name = "writeFlashButton";
            this.writeFlashButton.Size = new System.Drawing.Size(120, 35);
            this.writeFlashButton.TabIndex = 7;
            this.writeFlashButton.Text = "เขียน Chip";
            this.writeFlashButton.UseVisualStyleBackColor = true;
            this.writeFlashButton.Click += new System.EventHandler(this.writeFlashButton_Click);
            // 
            // eraseButton
            // 
            this.eraseButton.BackColor = System.Drawing.Color.OrangeRed;
            this.eraseButton.ForeColor = System.Drawing.Color.White;
            this.eraseButton.Location = new System.Drawing.Point(258, 100);
            this.eraseButton.Name = "eraseButton";
            this.eraseButton.Size = new System.Drawing.Size(120, 35);
            this.eraseButton.TabIndex = 8;
            this.eraseButton.Text = "ลบข้อมูลทั้งหมด";
            this.eraseButton.UseVisualStyleBackColor = false;
            this.eraseButton.Click += new System.EventHandler(this.eraseButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.resetButton.ForeColor = System.Drawing.Color.White;
            this.resetButton.Location = new System.Drawing.Point(384, 100);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(120, 35);
            this.resetButton.TabIndex = 19;
            this.resetButton.Text = "Reset/Unlimit";
            this.resetButton.UseVisualStyleBackColor = false;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 145);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(526, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 17;
            this.progressBar.Visible = false;
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new System.Drawing.Point(6, 171);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(0, 13);
            this.progressLabel.TabIndex = 18;
            this.progressLabel.Visible = false;
            // 
            // logGroupBox
            // 
            this.logGroupBox.Controls.Add(this.logTextBox);
            this.logGroupBox.Location = new System.Drawing.Point(12, 306);
            this.logGroupBox.Name = "logGroupBox";
            this.logGroupBox.Size = new System.Drawing.Size(526, 170);
            this.logGroupBox.TabIndex = 2;
            this.logGroupBox.TabStop = false;
            this.logGroupBox.Text = "Log";
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            // Use Segoe UI for better emoji and text consistency
            this.logTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.Location = new System.Drawing.Point(3, 16);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(554, 181);
            this.logTextBox.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 490);
            this.Controls.Add(this.logGroupBox);
            this.Controls.Add(this.flashGroupBox);
            this.Controls.Add(this.deviceGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tama Smart App - CH347 SPI Flash Reader/Writer";
            this.deviceGroupBox.ResumeLayout(false);
            this.deviceGroupBox.PerformLayout();
            this.flashGroupBox.ResumeLayout(false);
            this.flashGroupBox.PerformLayout();
            this.logGroupBox.ResumeLayout(false);
            this.logGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
