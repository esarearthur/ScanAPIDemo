namespace ScanAPIDemo
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
            this.m_btnSave = new System.Windows.Forms.Button();
            this.m_textMessage = new System.Windows.Forms.TextBox();
            this.m_picture = new System.Windows.Forms.PictureBox();
            this.m_Dev_Name = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_picture)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnSave
            // 
            this.m_btnSave.Enabled = false;
            this.m_btnSave.Location = new System.Drawing.Point(141, 340);
            this.m_btnSave.Margin = new System.Windows.Forms.Padding(2);
            this.m_btnSave.Name = "m_btnSave";
            this.m_btnSave.Size = new System.Drawing.Size(75, 36);
            this.m_btnSave.TabIndex = 10;
            this.m_btnSave.Text = "Save";
            this.m_btnSave.UseVisualStyleBackColor = true;
            this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
            // 
            // m_textMessage
            // 
            this.m_textMessage.Location = new System.Drawing.Point(15, 347);
            this.m_textMessage.Margin = new System.Windows.Forms.Padding(2);
            this.m_textMessage.Name = "m_textMessage";
            this.m_textMessage.Size = new System.Drawing.Size(122, 20);
            this.m_textMessage.TabIndex = 11;
            // 
            // m_picture
            // 
            this.m_picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_picture.Location = new System.Drawing.Point(15, 43);
            this.m_picture.Name = "m_picture";
            this.m_picture.Size = new System.Drawing.Size(201, 292);
            this.m_picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_picture.TabIndex = 6;
            this.m_picture.TabStop = false;
            // 
            // m_Dev_Name
            // 
            this.m_Dev_Name.AutoSize = true;
            this.m_Dev_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_Dev_Name.Location = new System.Drawing.Point(18, 9);
            this.m_Dev_Name.Name = "m_Dev_Name";
            this.m_Dev_Name.Size = new System.Drawing.Size(0, 29);
            this.m_Dev_Name.TabIndex = 12;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 387);
            this.Controls.Add(this.m_Dev_Name);
            this.Controls.Add(this.m_btnSave);
            this.Controls.Add(this.m_textMessage);
            this.Controls.Add(this.m_picture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Scan API Demo (C#)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_picture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox m_picture;
        private System.Windows.Forms.Button m_btnSave;
        private System.Windows.Forms.TextBox m_textMessage;
        private System.Windows.Forms.Label m_Dev_Name;
    }
}

