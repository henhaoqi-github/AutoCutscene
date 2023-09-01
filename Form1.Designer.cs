namespace Genshin_Auto_Start;

partial class Form1
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
        this.PictureBox0 = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.PictureBox0)).BeginInit();
        this.SuspendLayout();
        //
        // PictureBox0
        //
        this.PictureBox0.Text =  "PictureBox0";
        this.PictureBox0.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PictureBox0.Size = new System.Drawing.Size(464,362);
     //
     // form
     //
        this.Size = new System.Drawing.Size(480,400);
        this.Text =  "Auto Qidong";
        this.Controls.Add(this.PictureBox0);
        ((System.ComponentModel.ISupportInitialize)(this.PictureBox0)).EndInit();
        this.ResumeLayout(false);
    } 

    #endregion 

    private System.Windows.Forms.PictureBox PictureBox0;
}

