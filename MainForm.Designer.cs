namespace WebApplication;

partial class MainForm
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
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        notifyIcon = new NotifyIcon(components);
        SuspendLayout();
        // 
        // notifyIcon
        // 
        notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
        notifyIcon.Text = "WebApplications";
        notifyIcon.Visible = true;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        Text = "WebApplication";
        Load += MainForm_Load;
        ResumeLayout(false);
    }

    #endregion

    private NotifyIcon notifyIcon;
}
