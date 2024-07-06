namespace Insight360.DataProcessor
{
    partial class MainForm
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
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.cmbOrgs = new System.Windows.Forms.ComboBox();
            this.btnProcessSurveyComments = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 25);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(93, 25);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // cmbOrgs
            // 
            this.cmbOrgs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOrgs.FormattingEnabled = true;
            this.cmbOrgs.Location = new System.Drawing.Point(12, 143);
            this.cmbOrgs.Name = "cmbOrgs";
            this.cmbOrgs.Size = new System.Drawing.Size(198, 21);
            this.cmbOrgs.TabIndex = 2;
            this.cmbOrgs.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // btnProcessSurveyComments
            // 
            this.btnProcessSurveyComments.Location = new System.Drawing.Point(12, 182);
            this.btnProcessSurveyComments.Name = "btnProcessSurveyComments";
            this.btnProcessSurveyComments.Size = new System.Drawing.Size(156, 23);
            this.btnProcessSurveyComments.TabIndex = 3;
            this.btnProcessSurveyComments.Text = "Process Survey Comments";
            this.btnProcessSurveyComments.UseVisualStyleBackColor = true;
            this.btnProcessSurveyComments.Click += new System.EventHandler(this.btnProcessSurveyComments_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.btnProcessSurveyComments);
            this.Controls.Add(this.cmbOrgs);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "MainForm";
            this.Text = "DataProcessor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ComboBox cmbOrgs;
        private System.Windows.Forms.Button btnProcessSurveyComments;

    }
}