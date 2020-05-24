using System.Windows.Forms;

namespace CSUnblockMeSolver {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.SolveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SolveButton
            // 
            this.SolveButton.Location = new System.Drawing.Point(0, 400);
            this.SolveButton.Name = "SolveButton";
            this.SolveButton.Size = new System.Drawing.Size(390, 40);
            this.SolveButton.TabIndex = 0;
            this.SolveButton.Text = "Solve (Num0)";
            this.SolveButton.UseVisualStyleBackColor = true;
            this.SolveButton.Click += new System.EventHandler(this.SolveButtonClick);

            //
            // MapBox
            //
            this.MapBox = new System.Windows.Forms.PictureBox();
            this.MapBox.Name = "MapBox";
            this.MapBox.Size = new System.Drawing.Size(390, 390);
            this.MapBox.Location = new System.Drawing.Point(0, 0);
            this.MapBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 440);
            this.Controls.Add(this.SolveButton);
            this.Controls.Add(this.MapBox);
            this.Name = "MainForm";
            this.Text = "CSUnblockMeSolver";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SolveButton;
        private System.Windows.Forms.PictureBox MapBox;
    }
}