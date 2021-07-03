namespace Character_Making_File_Tool
{
    partial class Form1
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
            this.maleButton = new System.Windows.Forms.RadioButton();
            this.femaleButton = new System.Windows.Forms.RadioButton();
            this.humanButton = new System.Windows.Forms.RadioButton();
            this.newmanButton = new System.Windows.Forms.RadioButton();
            this.castButton = new System.Windows.Forms.RadioButton();
            this.deumanButton = new System.Windows.Forms.RadioButton();
            this.heightNACheckBox = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.debugEncryptButton = new System.Windows.Forms.Button();
            this.debugBatchButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.fileButton = new System.Windows.Forms.Button();
            this.filePanel = new System.Windows.Forms.Panel();
            this.quitButton = new System.Windows.Forms.Button();
            this.fileButtonSubmenu = new System.Windows.Forms.Panel();
            this.setPathButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel8 = new System.Windows.Forms.Panel();
            this.versionLabel = new System.Windows.Forms.Label();
            this.unencryptCheckBox = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel7.SuspendLayout();
            this.filePanel.SuspendLayout();
            this.fileButtonSubmenu.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // maleButton
            // 
            this.maleButton.AutoSize = true;
            this.maleButton.Checked = true;
            this.maleButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.maleButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.maleButton.Location = new System.Drawing.Point(19, 43);
            this.maleButton.Name = "maleButton";
            this.maleButton.Size = new System.Drawing.Size(60, 23);
            this.maleButton.TabIndex = 0;
            this.maleButton.TabStop = true;
            this.maleButton.Tag = "0";
            this.maleButton.Text = "Male";
            this.maleButton.UseVisualStyleBackColor = true;
            this.maleButton.CheckedChanged += new System.EventHandler(this.genderButton_CheckedChanged);
            // 
            // femaleButton
            // 
            this.femaleButton.AutoSize = true;
            this.femaleButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.femaleButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.femaleButton.Location = new System.Drawing.Point(19, 73);
            this.femaleButton.Name = "femaleButton";
            this.femaleButton.Size = new System.Drawing.Size(74, 23);
            this.femaleButton.TabIndex = 1;
            this.femaleButton.Tag = "1";
            this.femaleButton.Text = "Female";
            this.femaleButton.UseVisualStyleBackColor = true;
            this.femaleButton.CheckedChanged += new System.EventHandler(this.genderButton_CheckedChanged);
            // 
            // humanButton
            // 
            this.humanButton.AutoSize = true;
            this.humanButton.Checked = true;
            this.humanButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.humanButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.humanButton.Location = new System.Drawing.Point(25, 42);
            this.humanButton.Name = "humanButton";
            this.humanButton.Size = new System.Drawing.Size(73, 23);
            this.humanButton.TabIndex = 4;
            this.humanButton.TabStop = true;
            this.humanButton.Tag = "0";
            this.humanButton.Text = "Human";
            this.humanButton.UseVisualStyleBackColor = true;
            this.humanButton.CheckedChanged += new System.EventHandler(this.raceButton_CheckedChanged);
            // 
            // newmanButton
            // 
            this.newmanButton.AutoSize = true;
            this.newmanButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newmanButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.newmanButton.Location = new System.Drawing.Point(104, 42);
            this.newmanButton.Name = "newmanButton";
            this.newmanButton.Size = new System.Drawing.Size(84, 23);
            this.newmanButton.TabIndex = 5;
            this.newmanButton.Tag = "1";
            this.newmanButton.Text = "Newman";
            this.newmanButton.UseVisualStyleBackColor = true;
            this.newmanButton.CheckedChanged += new System.EventHandler(this.raceButton_CheckedChanged);
            // 
            // castButton
            // 
            this.castButton.AutoSize = true;
            this.castButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.castButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.castButton.Location = new System.Drawing.Point(25, 73);
            this.castButton.Name = "castButton";
            this.castButton.Size = new System.Drawing.Size(56, 23);
            this.castButton.TabIndex = 6;
            this.castButton.Tag = "2";
            this.castButton.Text = "Cast";
            this.castButton.UseVisualStyleBackColor = true;
            this.castButton.CheckedChanged += new System.EventHandler(this.raceButton_CheckedChanged);
            // 
            // deumanButton
            // 
            this.deumanButton.AutoSize = true;
            this.deumanButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deumanButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.deumanButton.Location = new System.Drawing.Point(104, 73);
            this.deumanButton.Name = "deumanButton";
            this.deumanButton.Size = new System.Drawing.Size(81, 23);
            this.deumanButton.TabIndex = 7;
            this.deumanButton.Tag = "3";
            this.deumanButton.Text = "Deuman";
            this.deumanButton.UseVisualStyleBackColor = true;
            this.deumanButton.CheckedChanged += new System.EventHandler(this.raceButton_CheckedChanged);
            // 
            // heightNACheckBox
            // 
            this.heightNACheckBox.AutoSize = true;
            this.heightNACheckBox.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.heightNACheckBox.ForeColor = System.Drawing.Color.Gainsboro;
            this.heightNACheckBox.Location = new System.Drawing.Point(244, 112);
            this.heightNACheckBox.Name = "heightNACheckBox";
            this.heightNACheckBox.Size = new System.Drawing.Size(197, 23);
            this.heightNACheckBox.TabIndex = 11;
            this.heightNACheckBox.Text = "Set to NA minimum height";
            this.heightNACheckBox.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel7);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.deumanButton);
            this.panel1.Controls.Add(this.castButton);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.humanButton);
            this.panel1.Controls.Add(this.newmanButton);
            this.panel1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(243, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(223, 99);
            this.panel1.TabIndex = 12;
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(67)))));
            this.panel7.Controls.Add(this.debugEncryptButton);
            this.panel7.Controls.Add(this.debugBatchButton);
            this.panel7.Controls.Add(this.label2);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(0, 0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(223, 37);
            this.panel7.TabIndex = 17;
            // 
            // debugEncryptButton
            // 
            this.debugEncryptButton.Location = new System.Drawing.Point(77, 14);
            this.debugEncryptButton.Name = "debugEncryptButton";
            this.debugEncryptButton.Size = new System.Drawing.Size(75, 23);
            this.debugEncryptButton.TabIndex = 18;
            this.debugEncryptButton.Text = "DebugEncrypt";
            this.debugEncryptButton.UseVisualStyleBackColor = true;
            this.debugEncryptButton.Click += new System.EventHandler(this.debugEncrypt_Click);
            // 
            // debugBatchButton
            // 
            this.debugBatchButton.Location = new System.Drawing.Point(-9, 12);
            this.debugBatchButton.Name = "debugBatchButton";
            this.debugBatchButton.Size = new System.Drawing.Size(75, 23);
            this.debugBatchButton.TabIndex = 18;
            this.debugBatchButton.Text = "DebugBatchConvert";
            this.debugBatchButton.UseVisualStyleBackColor = true;
            this.debugBatchButton.Click += new System.EventHandler(this.debugBatchButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 15.75F);
            this.label2.ForeColor = System.Drawing.Color.Gainsboro;
            this.label2.Location = new System.Drawing.Point(72, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 26);
            this.label2.TabIndex = 17;
            this.label2.Text = "Race";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(37)))));
            this.panel2.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel2.Location = new System.Drawing.Point(1, 35);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(360, 1);
            this.panel2.TabIndex = 13;
            // 
            // fileButton
            // 
            this.fileButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(37)))));
            this.fileButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.fileButton.FlatAppearance.BorderSize = 0;
            this.fileButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.fileButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fileButton.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.fileButton.Location = new System.Drawing.Point(0, 0);
            this.fileButton.Name = "fileButton";
            this.fileButton.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.fileButton.Size = new System.Drawing.Size(145, 37);
            this.fileButton.TabIndex = 13;
            this.fileButton.Text = "File";
            this.fileButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.fileButton.UseVisualStyleBackColor = false;
            this.fileButton.Click += new System.EventHandler(this.fileButton_Click);
            // 
            // filePanel
            // 
            this.filePanel.AutoScroll = true;
            this.filePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(47)))));
            this.filePanel.Controls.Add(this.quitButton);
            this.filePanel.Controls.Add(this.fileButtonSubmenu);
            this.filePanel.Controls.Add(this.fileButton);
            this.filePanel.Location = new System.Drawing.Point(-4, 0);
            this.filePanel.Name = "filePanel";
            this.filePanel.Size = new System.Drawing.Size(145, 157);
            this.filePanel.TabIndex = 14;
            // 
            // quitButton
            // 
            this.quitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(37)))));
            this.quitButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.quitButton.FlatAppearance.BorderSize = 0;
            this.quitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.quitButton.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.quitButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.quitButton.Location = new System.Drawing.Point(0, 126);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(145, 31);
            this.quitButton.TabIndex = 15;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = false;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // fileButtonSubmenu
            // 
            this.fileButtonSubmenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(50)))), ((int)(((byte)(57)))));
            this.fileButtonSubmenu.Controls.Add(this.setPathButton);
            this.fileButtonSubmenu.Controls.Add(this.saveButton);
            this.fileButtonSubmenu.Controls.Add(this.openButton);
            this.fileButtonSubmenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.fileButtonSubmenu.Location = new System.Drawing.Point(0, 37);
            this.fileButtonSubmenu.Name = "fileButtonSubmenu";
            this.fileButtonSubmenu.Size = new System.Drawing.Size(145, 89);
            this.fileButtonSubmenu.TabIndex = 16;
            this.fileButtonSubmenu.Visible = false;
            // 
            // setPathButton
            // 
            this.setPathButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.setPathButton.FlatAppearance.BorderSize = 0;
            this.setPathButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.setPathButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setPathButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.setPathButton.Location = new System.Drawing.Point(0, 59);
            this.setPathButton.Name = "setPathButton";
            this.setPathButton.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.setPathButton.Size = new System.Drawing.Size(145, 29);
            this.setPathButton.TabIndex = 16;
            this.setPathButton.Text = "Set pso2_bin";
            this.setPathButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.setPathButton.UseVisualStyleBackColor = true;
            this.setPathButton.Click += new System.EventHandler(this.setPathButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.saveButton.FlatAppearance.BorderSize = 0;
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.saveButton.Location = new System.Drawing.Point(0, 29);
            this.saveButton.Name = "saveButton";
            this.saveButton.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.saveButton.Size = new System.Drawing.Size(145, 30);
            this.saveButton.TabIndex = 15;
            this.saveButton.Text = "Save As...";
            this.saveButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // openButton
            // 
            this.openButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.openButton.FlatAppearance.BorderSize = 0;
            this.openButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openButton.ForeColor = System.Drawing.Color.Gainsboro;
            this.openButton.Location = new System.Drawing.Point(0, 0);
            this.openButton.Name = "openButton";
            this.openButton.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.openButton.Size = new System.Drawing.Size(145, 29);
            this.openButton.TabIndex = 14;
            this.openButton.Text = "Open";
            this.openButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel6);
            this.panel4.Controls.Add(this.maleButton);
            this.panel4.Controls.Add(this.femaleButton);
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Location = new System.Drawing.Point(142, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(99, 99);
            this.panel4.TabIndex = 15;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(67)))));
            this.panel6.Controls.Add(this.label1);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(99, 37);
            this.panel6.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 15.75F);
            this.label1.ForeColor = System.Drawing.Color.Gainsboro;
            this.label1.Location = new System.Drawing.Point(14, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 26);
            this.label1.TabIndex = 16;
            this.label1.Text = "Gender";
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(37)))));
            this.panel5.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel5.Location = new System.Drawing.Point(3, 36);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(360, 1);
            this.panel5.TabIndex = 15;
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(37)))));
            this.panel8.Location = new System.Drawing.Point(151, 105);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(290, 1);
            this.panel8.TabIndex = 16;
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionLabel.ForeColor = System.Drawing.Color.Gainsboro;
            this.versionLabel.Location = new System.Drawing.Point(156, 112);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(65, 19);
            this.versionLabel.TabIndex = 17;
            this.versionLabel.Text = "Version: ";
            // 
            // unencryptCheckBox
            // 
            this.unencryptCheckBox.AutoSize = true;
            this.unencryptCheckBox.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unencryptCheckBox.ForeColor = System.Drawing.Color.Gainsboro;
            this.unencryptCheckBox.Location = new System.Drawing.Point(145, 134);
            this.unencryptCheckBox.Name = "unencryptCheckBox";
            this.unencryptCheckBox.Size = new System.Drawing.Size(298, 23);
            this.unencryptCheckBox.TabIndex = 18;
            this.unencryptCheckBox.Text = "Save Unencrypted File (Unusable ingame)";
            this.unencryptCheckBox.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(-9, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "DebugBatchConvert";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.debugDecrypt_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(47)))));
            this.ClientSize = new System.Drawing.Size(444, 169);
            this.Controls.Add(this.unencryptCheckBox);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.panel8);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.filePanel);
            this.Controls.Add(this.heightNACheckBox);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Salon File Tool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.filePanel.ResumeLayout(false);
            this.fileButtonSubmenu.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton maleButton;
        private System.Windows.Forms.RadioButton femaleButton;
        private System.Windows.Forms.RadioButton humanButton;
        private System.Windows.Forms.RadioButton newmanButton;
        private System.Windows.Forms.RadioButton castButton;
        private System.Windows.Forms.RadioButton deumanButton;
        private System.Windows.Forms.CheckBox heightNACheckBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button fileButton;
        private System.Windows.Forms.Panel filePanel;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Panel fileButtonSubmenu;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.CheckBox unencryptCheckBox;
        private System.Windows.Forms.Button setPathButton;
        private System.Windows.Forms.Button debugBatchButton;
        private System.Windows.Forms.Button debugEncryptButton;
        private System.Windows.Forms.Button button1;
    }
}

