
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
            this.btnExportUSFM = new System.Windows.Forms.Button();
            this.btnVersificationTest = new System.Windows.Forms.Button();
            this.btnSwitchProject = new System.Windows.Forms.Button();
            this.ProjectListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // rtb
            // 
            this.rtb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb.Location = new System.Drawing.Point(4, 36);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(458, 335);
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
            this.btnRestart.Location = new System.Drawing.Point(387, 7);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(75, 23);
            this.btnRestart.TabIndex = 22;
            this.btnRestart.Text = "Clear Log";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnClearWindow_Click);
            // 
            // btnTest
            // 
            this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTest.Location = new System.Drawing.Point(114, 7);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(116, 23);
            this.btnTest.TabIndex = 23;
            this.btnTest.Text = "Send Message";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Visible = false;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnExportUSFM
            // 
            this.btnExportUSFM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportUSFM.Location = new System.Drawing.Point(195, 7);
            this.btnExportUSFM.Name = "btnExportUSFM";
            this.btnExportUSFM.Size = new System.Drawing.Size(118, 23);
            this.btnExportUSFM.TabIndex = 25;
            this.btnExportUSFM.Text = "Export USFM";
            this.btnExportUSFM.UseVisualStyleBackColor = true;
            this.btnExportUSFM.Visible = false;
            this.btnExportUSFM.Click += new System.EventHandler(this.btnExportUSFM_Click);
            // 
            // btnVersificationTest
            // 
            this.btnVersificationTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVersificationTest.Location = new System.Drawing.Point(4, 7);
            this.btnVersificationTest.Name = "btnVersificationTest";
            this.btnVersificationTest.Size = new System.Drawing.Size(104, 23);
            this.btnVersificationTest.TabIndex = 26;
            this.btnVersificationTest.Text = "Versification Test";
            this.btnVersificationTest.UseVisualStyleBackColor = true;
            this.btnVersificationTest.Visible = false;
            this.btnVersificationTest.Click += new System.EventHandler(this.btnVersificationTest_Click);
            // 
            // btnSwitchProject
            // 
            this.btnSwitchProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSwitchProject.Location = new System.Drawing.Point(265, 7);
            this.btnSwitchProject.Name = "btnSwitchProject";
            this.btnSwitchProject.Size = new System.Drawing.Size(116, 23);
            this.btnSwitchProject.TabIndex = 27;
            this.btnSwitchProject.Text = "Switch Project";
            this.btnSwitchProject.UseVisualStyleBackColor = true;
            this.btnSwitchProject.Click += new System.EventHandler(this.btnSwitchProject_Click);
            // 
            // ProjectListBox
            // 
            this.ProjectListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ProjectListBox.FormattingEnabled = true;
            this.ProjectListBox.Location = new System.Drawing.Point(6, 36);
            this.ProjectListBox.Margin = new System.Windows.Forms.Padding(2);
            this.ProjectListBox.Name = "ProjectListBox";
            this.ProjectListBox.Size = new System.Drawing.Size(119, 329);
            this.ProjectListBox.TabIndex = 28;
            this.ProjectListBox.Visible = false;
            this.ProjectListBox.SelectedIndexChanged += new System.EventHandler(this.ProjectListBox_SelectedIndexChanged);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ProjectListBox);
            this.Controls.Add(this.btnSwitchProject);
            this.Controls.Add(this.btnVersificationTest);
            this.Controls.Add(this.btnExportUSFM);
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
        private System.Windows.Forms.Button btnExportUSFM;
        private System.Windows.Forms.Button btnVersificationTest;
        private System.Windows.Forms.Button btnSwitchProject;
        private System.Windows.Forms.ListBox ProjectListBox;
    }
}
