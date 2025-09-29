using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnFixText;
        private System.Windows.Forms.Button btnCopyFixedText;
        private System.Windows.Forms.Button btnChangePrompt;
        private System.Windows.Forms.Button btnFloatingMode;
        private System.Windows.Forms.Label lblEnterText;
        private System.Windows.Forms.Label lblFixedText;

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

        #region Windows Form

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtInput = new System.Windows.Forms.TextBox();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnFixText = new System.Windows.Forms.Button();
            this.btnCopyFixedText = new System.Windows.Forms.Button();
            this.btnChangePrompt = new System.Windows.Forms.Button();
            this.btnFloatingMode = new System.Windows.Forms.Button();
            this.lblEnterText = new System.Windows.Forms.Label();
            this.lblFixedText = new System.Windows.Forms.Label();
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.layout.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.layout.SetColumnSpan(this.txtInput, 4);
            this.txtInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.txtInput.Location = new System.Drawing.Point(3, 29);
            this.txtInput.Multiline = true;
            this.txtInput.Name = "txtInput";
            this.txtInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInput.Size = new System.Drawing.Size(472, 115);
            this.txtInput.TabIndex = 1;
            this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged);
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.layout.SetColumnSpan(this.txtOutput, 4);
            this.txtOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.txtOutput.Location = new System.Drawing.Point(3, 216);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(472, 118);
            this.txtOutput.TabIndex = 7;
            // 
            // btnFixText
            // 
            this.btnFixText.AutoSize = true;
            this.btnFixText.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnFixText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFixText.Location = new System.Drawing.Point(3, 150);
            this.btnFixText.Name = "btnFixText";
            this.btnFixText.Size = new System.Drawing.Size(113, 34);
            this.btnFixText.TabIndex = 2;
            this.btnFixText.Text = "Fix Text";
            this.btnFixText.UseVisualStyleBackColor = true;
            this.btnFixText.Click += new System.EventHandler(this.btnFixText_Click);
            // 
            // btnCopyFixedText
            // 
            this.btnCopyFixedText.AutoSize = true;
            this.btnCopyFixedText.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCopyFixedText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCopyFixedText.Location = new System.Drawing.Point(122, 150);
            this.btnCopyFixedText.Name = "btnCopyFixedText";
            this.btnCopyFixedText.Size = new System.Drawing.Size(113, 34);
            this.btnCopyFixedText.TabIndex = 3;
            this.btnCopyFixedText.Text = "Copy Fixed Text";
            this.btnCopyFixedText.UseVisualStyleBackColor = true;
            this.btnCopyFixedText.Click += new System.EventHandler(this.btnCopyFixedText_Click);
            // 
            // btnChangePrompt
            // 
            this.btnChangePrompt.AutoSize = true;
            this.btnChangePrompt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnChangePrompt.Location = new System.Drawing.Point(241, 150);
            this.btnChangePrompt.Name = "btnChangePrompt";
            this.btnChangePrompt.Size = new System.Drawing.Size(113, 34);
            this.btnChangePrompt.TabIndex = 4;
            this.btnChangePrompt.Text = "Change Prompt";
            this.btnChangePrompt.UseVisualStyleBackColor = true;
            this.btnChangePrompt.Click += new System.EventHandler(this.btnChangePrompt_Click);
            // 
            // btnFloatingMode
            // 
            this.btnFloatingMode.AutoSize = true;
            this.btnFloatingMode.BackColor = System.Drawing.Color.ForestGreen;
            this.btnFloatingMode.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnFloatingMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFloatingMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFloatingMode.ForeColor = System.Drawing.Color.White;
            this.btnFloatingMode.Location = new System.Drawing.Point(360, 150);
            this.btnFloatingMode.Name = "btnFloatingMode";
            this.btnFloatingMode.Size = new System.Drawing.Size(115, 34);
            this.btnFloatingMode.TabIndex = 5;
            this.btnFloatingMode.Text = "Floating Mode";
            this.btnFloatingMode.UseVisualStyleBackColor = false;
            this.btnFloatingMode.Click += new System.EventHandler(this.btnFloatingMode_Click);
            // 
            // lblEnterText
            // 
            this.lblEnterText.AutoSize = true;
            this.layout.SetColumnSpan(this.lblEnterText, 4);
            this.lblEnterText.Location = new System.Drawing.Point(3, 0);
            this.lblEnterText.Name = "lblEnterText";
            this.lblEnterText.Size = new System.Drawing.Size(72, 17);
            this.lblEnterText.TabIndex = 0;
            this.lblEnterText.Text = "Enter text:";
            // 
            // lblFixedText
            // 
            this.lblFixedText.AutoSize = true;
            this.layout.SetColumnSpan(this.lblFixedText, 4);
            this.lblFixedText.Location = new System.Drawing.Point(3, 187);
            this.lblFixedText.Name = "lblFixedText";
            this.lblFixedText.Size = new System.Drawing.Size(71, 17);
            this.lblFixedText.TabIndex = 6;
            this.lblFixedText.Text = "Fixed text:";
            // 
            // layout
            // 
            this.layout.AutoSize = true;
            this.layout.ColumnCount = 4;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layout.Controls.Add(this.lblEnterText, 0, 0);
            this.layout.Controls.Add(this.txtInput, 0, 1);
            this.layout.Controls.Add(this.btnFixText, 0, 2);
            this.layout.Controls.Add(this.btnCopyFixedText, 1, 2);
            this.layout.Controls.Add(this.btnChangePrompt, 2, 2);
            this.layout.Controls.Add(this.btnFloatingMode, 3, 2);
            this.layout.Controls.Add(this.lblFixedText, 0, 3);
            this.layout.Controls.Add(this.txtOutput, 0, 4);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(3, 3);
            this.layout.Name = "layout";
            this.layout.RowCount = 5;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.layout.Size = new System.Drawing.Size(478, 337);
            this.layout.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(484, 343);
            this.Controls.Add(this.layout);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "English Fixer";
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel layout;
    }
}

