
namespace ClearDashboard.WebApiParatextPlugin
{
	partial class MainWindow
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.listBoxClients = new System.Windows.Forms.ListBox();
            this.btnExportUSFM = new System.Windows.Forms.Button();
            this.btnVersificationTest = new System.Windows.Forms.Button();
            this.ProjectsListBox = new System.Windows.Forms.ListBox();
            this.textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // rtb
            // 
            this.rtb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb.Location = new System.Drawing.Point(4, 76);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(291, 282);
            this.rtb.TabIndex = 20;
            this.rtb.Text = "";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(3, 9);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(35, 13);
            this.lblVersion.TabIndex = 21;
            this.lblVersion.Text = "label1";
            // 
            // btnRestart
            // 
            this.btnRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestart.Location = new System.Drawing.Point(374, 18);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(75, 23);
            this.btnRestart.TabIndex = 22;
            this.btnRestart.Text = "Restart Pipe";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnTest
            // 
            this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTest.Location = new System.Drawing.Point(236, 47);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(213, 23);
            this.btnTest.TabIndex = 23;
            this.btnTest.Text = "Send Message";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // listBoxClients
            // 
            this.listBoxClients.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxClients.FormattingEnabled = true;
            this.listBoxClients.Location = new System.Drawing.Point(301, 76);
            this.listBoxClients.Name = "listBoxClients";
            this.listBoxClients.Size = new System.Drawing.Size(161, 56);
            this.listBoxClients.TabIndex = 24;
            // 
            // btnExportUSFM
            // 
            this.btnExportUSFM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportUSFM.Location = new System.Drawing.Point(236, 18);
            this.btnExportUSFM.Name = "btnExportUSFM";
            this.btnExportUSFM.Size = new System.Drawing.Size(132, 23);
            this.btnExportUSFM.TabIndex = 25;
            this.btnExportUSFM.Text = "Export USFM";
            this.btnExportUSFM.UseVisualStyleBackColor = true;
            this.btnExportUSFM.Click += new System.EventHandler(this.btnExportUSFM_Click);
            // 
            // btnVersificationTest
            // 
            this.btnVersificationTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVersificationTest.Location = new System.Drawing.Point(6, 47);
            this.btnVersificationTest.Name = "btnVersificationTest";
            this.btnVersificationTest.Size = new System.Drawing.Size(124, 23);
            this.btnVersificationTest.TabIndex = 26;
            this.btnVersificationTest.Text = "Versification Test";
            this.btnVersificationTest.UseVisualStyleBackColor = true;
            this.btnVersificationTest.Click += new System.EventHandler(this.btnVersificationTest_Click);
            // 
            // ProjectsListBox
            // 
            this.ProjectsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProjectsListBox.FormattingEnabled = true;
            this.ProjectsListBox.Location = new System.Drawing.Point(302, 137);
            this.ProjectsListBox.Margin = new System.Windows.Forms.Padding(2);
            this.ProjectsListBox.Name = "ProjectsListBox";
            this.ProjectsListBox.Size = new System.Drawing.Size(161, 121);
            this.ProjectsListBox.TabIndex = 27;
            this.ProjectsListBox.SelectedIndexChanged += new System.EventHandler(this.ProjectListBox_SelectedIndexChanged);
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Location = new System.Drawing.Point(302, 262);
            this.textBox.Margin = new System.Windows.Forms.Padding(2);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox.Size = new System.Drawing.Size(160, 96);
            this.textBox.TabIndex = 28;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.ProjectsListBox);
            this.Controls.Add(this.btnVersificationTest);
            this.Controls.Add(this.btnExportUSFM);
            this.Controls.Add(this.listBoxClients);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.rtb);
            this.Name = "MainWindow";
            this.Size = new System.Drawing.Size(465, 374);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.ListBox listBoxClients;
        private System.Windows.Forms.Button btnExportUSFM;
        private System.Windows.Forms.Button btnVersificationTest;
        private System.Windows.Forms.ListBox ProjectsListBox;
        private System.Windows.Forms.TextBox textBox;
    }
}
