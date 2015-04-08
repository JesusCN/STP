namespace SimpleTaskPlanner
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.txtConsole = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.gboxConsole = new System.Windows.Forms.GroupBox();
			this.nupConsoleThreshold = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.cmbCommands = new System.Windows.Forms.ComboBox();
			this.gboxExecuteCommand = new System.Windows.Forms.GroupBox();
			this.btnExecute = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnMove = new System.Windows.Forms.Button();
			this.gbxSceneMap = new System.Windows.Forms.GroupBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.btnLoadSceneMap = new System.Windows.Forms.Button();
			this.cmbUseArm = new System.Windows.Forms.ComboBox();
			this.gboxConsole.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nupConsoleThreshold)).BeginInit();
			this.gboxExecuteCommand.SuspendLayout();
			this.gbxSceneMap.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtConsole
			// 
			this.txtConsole.Location = new System.Drawing.Point(6, 38);
			this.txtConsole.Multiline = true;
			this.txtConsole.Name = "txtConsole";
			this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtConsole.Size = new System.Drawing.Size(593, 457);
			this.txtConsole.TabIndex = 0;
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(623, 330);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(184, 57);
			this.btnStart.TabIndex = 1;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// gboxConsole
			// 
			this.gboxConsole.Controls.Add(this.txtConsole);
			this.gboxConsole.Controls.Add(this.nupConsoleThreshold);
			this.gboxConsole.Controls.Add(this.label1);
			this.gboxConsole.Location = new System.Drawing.Point(12, 12);
			this.gboxConsole.Name = "gboxConsole";
			this.gboxConsole.Size = new System.Drawing.Size(605, 501);
			this.gboxConsole.TabIndex = 2;
			this.gboxConsole.TabStop = false;
			this.gboxConsole.Text = "Console";
			// 
			// nupConsoleThreshold
			// 
			this.nupConsoleThreshold.Location = new System.Drawing.Point(562, 14);
			this.nupConsoleThreshold.Margin = new System.Windows.Forms.Padding(1);
			this.nupConsoleThreshold.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
			this.nupConsoleThreshold.Name = "nupConsoleThreshold";
			this.nupConsoleThreshold.Size = new System.Drawing.Size(37, 20);
			this.nupConsoleThreshold.TabIndex = 2;
			this.nupConsoleThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.nupConsoleThreshold.ValueChanged += new System.EventHandler(this.nupConsoleThreshold_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(501, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Verbosity :";
			// 
			// cmbCommands
			// 
			this.cmbCommands.AutoCompleteCustomSource.AddRange(new string[] {
            "juana"});
			this.cmbCommands.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.cmbCommands.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.cmbCommands.FormattingEnabled = true;
			this.cmbCommands.Location = new System.Drawing.Point(6, 19);
			this.cmbCommands.Name = "cmbCommands";
			this.cmbCommands.Size = new System.Drawing.Size(172, 21);
			this.cmbCommands.TabIndex = 4;
			// 
			// gboxExecuteCommand
			// 
			this.gboxExecuteCommand.Controls.Add(this.btnExecute);
			this.gboxExecuteCommand.Controls.Add(this.cmbCommands);
			this.gboxExecuteCommand.Location = new System.Drawing.Point(623, 12);
			this.gboxExecuteCommand.Name = "gboxExecuteCommand";
			this.gboxExecuteCommand.Size = new System.Drawing.Size(184, 73);
			this.gboxExecuteCommand.TabIndex = 5;
			this.gboxExecuteCommand.TabStop = false;
			this.gboxExecuteCommand.Text = "Command";
			// 
			// btnExecute
			// 
			this.btnExecute.Location = new System.Drawing.Point(89, 46);
			this.btnExecute.Name = "btnExecute";
			this.btnExecute.Size = new System.Drawing.Size(89, 23);
			this.btnExecute.TabIndex = 6;
			this.btnExecute.Text = "Execute";
			this.btnExecute.UseVisualStyleBackColor = true;
			this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
			// 
			// btnStop
			// 
			this.btnStop.Location = new System.Drawing.Point(623, 393);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(184, 57);
			this.btnStop.TabIndex = 6;
			this.btnStop.Text = "Stop";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnMove
			// 
			this.btnMove.Location = new System.Drawing.Point(623, 456);
			this.btnMove.Name = "btnMove";
			this.btnMove.Size = new System.Drawing.Size(184, 57);
			this.btnMove.TabIndex = 7;
			this.btnMove.Text = "Move";
			this.btnMove.UseVisualStyleBackColor = true;
			this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
			// 
			// gbxSceneMap
			// 
			this.gbxSceneMap.Controls.Add(this.textBox1);
			this.gbxSceneMap.Controls.Add(this.btnLoadSceneMap);
			this.gbxSceneMap.Location = new System.Drawing.Point(623, 99);
			this.gbxSceneMap.Name = "gbxSceneMap";
			this.gbxSceneMap.Size = new System.Drawing.Size(184, 74);
			this.gbxSceneMap.TabIndex = 7;
			this.gbxSceneMap.TabStop = false;
			this.gbxSceneMap.Text = "SceneMap";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(6, 19);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(172, 20);
			this.textBox1.TabIndex = 7;
			// 
			// btnLoadSceneMap
			// 
			this.btnLoadSceneMap.Location = new System.Drawing.Point(89, 45);
			this.btnLoadSceneMap.Name = "btnLoadSceneMap";
			this.btnLoadSceneMap.Size = new System.Drawing.Size(89, 23);
			this.btnLoadSceneMap.TabIndex = 6;
			this.btnLoadSceneMap.Text = "Load Map";
			this.btnLoadSceneMap.UseVisualStyleBackColor = true;
			// 
			// cmbUseArm
			// 
			this.cmbUseArm.FormattingEnabled = true;
			this.cmbUseArm.Items.AddRange(new object[] {
            "Use Both Arms",
            "Use Left Arm",
            "Use Right Arm"});
			this.cmbUseArm.Location = new System.Drawing.Point(670, 225);
			this.cmbUseArm.Name = "cmbUseArm";
			this.cmbUseArm.Size = new System.Drawing.Size(121, 21);
			this.cmbUseArm.TabIndex = 8;
			this.cmbUseArm.Tag = "Use Both Hands";
			this.cmbUseArm.Text = "Use Both Arms";
			this.cmbUseArm.SelectedIndexChanged += new System.EventHandler(this.cmbUseArm_SelectedIndexChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(819, 525);
			this.Controls.Add(this.cmbUseArm);
			this.Controls.Add(this.gbxSceneMap);
			this.Controls.Add(this.btnMove);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.gboxExecuteCommand);
			this.Controls.Add(this.gboxConsole);
			this.Controls.Add(this.btnStart);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "Simple Task Planner";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.gboxConsole.ResumeLayout(false);
			this.gboxConsole.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nupConsoleThreshold)).EndInit();
			this.gboxExecuteCommand.ResumeLayout(false);
			this.gbxSceneMap.ResumeLayout(false);
			this.gbxSceneMap.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.GroupBox gboxConsole;
        private System.Windows.Forms.NumericUpDown nupConsoleThreshold;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbCommands;
		private System.Windows.Forms.GroupBox gboxExecuteCommand;
		private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.GroupBox gbxSceneMap;
        private System.Windows.Forms.Button btnLoadSceneMap;
        private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ComboBox cmbUseArm;
    }
}

