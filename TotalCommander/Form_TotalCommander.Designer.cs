using TotalCommander;

namespace TotalCommander
{
    partial class Form_TotalCommander
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_TotalCommander));
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.shellBrowser_Left = new TotalCommander.GUI.ShellBrowser();
            this.shellBrowser_Right = new TotalCommander.GUI.ShellBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.shellBrowser_Left);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.shellBrowser_Right);
            this.splMain.Size = new System.Drawing.Size(1264, 682);
            this.splMain.SplitterDistance = 630;
            this.splMain.TabIndex = 3;
            // 
            // shellBrowser_Left
            // 
            this.shellBrowser_Left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shellBrowser_Left.Location = new System.Drawing.Point(0, 0);
            this.shellBrowser_Left.Name = "shellBrowser_Left";
            this.shellBrowser_Left.Size = new System.Drawing.Size(630, 682);
            this.shellBrowser_Left.TabIndex = 0;
            // 
            // shellBrowser_Right
            // 
            this.shellBrowser_Right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shellBrowser_Right.Location = new System.Drawing.Point(0, 0);
            this.shellBrowser_Right.Name = "shellBrowser_Right";
            this.shellBrowser_Right.Size = new System.Drawing.Size(630, 682);
            this.shellBrowser_Right.TabIndex = 0;
            // 
            // Form_TotalCommander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 682);
            this.Controls.Add(this.splMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_TotalCommander";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Total Commander";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splMain;
        private GUI.ShellBrowser shellBrowser_Left;
        private GUI.ShellBrowser shellBrowser_Right;
    }
}


