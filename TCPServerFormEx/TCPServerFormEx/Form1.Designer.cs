namespace TCPServerFormEx
{
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
            connectButton = new Button();
            disconnectButton = new Button();
            messageLabel = new Label();
            SuspendLayout();
            // 
            // connectButton
            // 
            connectButton.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            connectButton.Location = new Point(12, 12);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(162, 50);
            connectButton.TabIndex = 0;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += connectButton_Click;
            // 
            // disconnectButton
            // 
            disconnectButton.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            disconnectButton.Location = new Point(180, 12);
            disconnectButton.Name = "disconnectButton";
            disconnectButton.Size = new Size(162, 50);
            disconnectButton.TabIndex = 1;
            disconnectButton.Text = "Disconnect";
            disconnectButton.UseVisualStyleBackColor = true;
            disconnectButton.Click += disconnectButton_Click;
            // 
            // messageLabel
            // 
            messageLabel.AutoSize = true;
            messageLabel.Location = new Point(12, 77);
            messageLabel.Name = "messageLabel";
            messageLabel.Size = new Size(68, 20);
            messageLabel.TabIndex = 2;
            messageLabel.Text = "Message";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(354, 213);
            Controls.Add(messageLabel);
            Controls.Add(disconnectButton);
            Controls.Add(connectButton);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button connectButton;
        private Button disconnectButton;
        public Label messageLabel;
    }
}
