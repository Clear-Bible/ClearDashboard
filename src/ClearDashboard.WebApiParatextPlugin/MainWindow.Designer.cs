﻿
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
            this.btnExportUSFM.Location = new System.Drawing.Point(249, 7);
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
            this.btnVersificationTest.Location = new System.Drawing.Point(4, 7);
            this.btnVersificationTest.Name = "btnVersificationTest";
            this.btnVersificationTest.Size = new System.Drawing.Size(114, 23);
            this.btnVersificationTest.TabIndex = 26;
            this.btnVersificationTest.Text = "Versification Test";
            this.btnVersificationTest.UseVisualStyleBackColor = true;
            this.btnVersificationTest.Visible = false;
            this.btnVersificationTest.Click += new System.EventHandler(this.btnVersificationTest_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
    }
}
