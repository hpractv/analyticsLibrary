namespace analyticsLibrary.library
{
    partial class userNamePassword
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
            this.domain = new System.Windows.Forms.TextBox();
            this.userIdLabel = new System.Windows.Forms.Label();
            this.userId = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.session = new System.Windows.Forms.CheckBox();
            this.save = new System.Windows.Forms.Button();
            this.server = new System.Windows.Forms.TextBox();
            this.serverLabel = new System.Windows.Forms.Label();
            this.domainLabel = new System.Windows.Forms.Label();
            this.port = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // domain
            // 
            this.domain.Location = new System.Drawing.Point(127, 33);
            this.domain.MaxLength = 64;
            this.domain.Name = "domain";
            this.domain.Size = new System.Drawing.Size(153, 20);
            this.domain.TabIndex = 1;
            this.domain.Tag = "domain";
            this.domain.Visible = false;
            // 
            // userIdLabel
            // 
            this.userIdLabel.AutoSize = true;
            this.userIdLabel.Location = new System.Drawing.Point(77, 62);
            this.userIdLabel.Name = "userIdLabel";
            this.userIdLabel.Size = new System.Drawing.Size(41, 13);
            this.userIdLabel.TabIndex = 2;
            this.userIdLabel.Tag = "userid";
            this.userIdLabel.Text = "User Id";
            this.userIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // userId
            // 
            this.userId.Location = new System.Drawing.Point(127, 58);
            this.userId.MaxLength = 64;
            this.userId.Name = "userId";
            this.userId.Size = new System.Drawing.Size(153, 20);
            this.userId.TabIndex = 3;
            this.userId.Tag = "userid";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(65, 87);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(53, 13);
            this.passwordLabel.TabIndex = 4;
            this.passwordLabel.Tag = "password";
            this.passwordLabel.Text = "Password";
            this.passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(127, 83);
            this.password.MaxLength = 64;
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(153, 20);
            this.password.TabIndex = 5;
            this.password.Tag = "password";
            // 
            // session
            // 
            this.session.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.session.AutoSize = true;
            this.session.Checked = true;
            this.session.CheckState = System.Windows.Forms.CheckState.Checked;
            this.session.Location = new System.Drawing.Point(12, 149);
            this.session.Name = "session";
            this.session.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.session.Size = new System.Drawing.Size(106, 17);
            this.session.TabIndex = 6;
            this.session.Text = "Save for Session";
            this.session.UseVisualStyleBackColor = true;
            // 
            // save
            // 
            this.save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.save.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.save.Location = new System.Drawing.Point(205, 146);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(75, 23);
            this.save.TabIndex = 7;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // server
            // 
            this.server.Location = new System.Drawing.Point(127, 8);
            this.server.MaxLength = 64;
            this.server.Name = "server";
            this.server.Size = new System.Drawing.Size(153, 20);
            this.server.TabIndex = 0;
            this.server.Tag = "server";
            this.server.Visible = false;
            // 
            // serverLabel
            // 
            this.serverLabel.AutoSize = true;
            this.serverLabel.Location = new System.Drawing.Point(80, 12);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(38, 13);
            this.serverLabel.TabIndex = 10;
            this.serverLabel.Tag = "server";
            this.serverLabel.Text = "Server";
            this.serverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.serverLabel.Visible = false;
            // 
            // domainLabel
            // 
            this.domainLabel.AutoSize = true;
            this.domainLabel.Location = new System.Drawing.Point(75, 37);
            this.domainLabel.Name = "domainLabel";
            this.domainLabel.Size = new System.Drawing.Size(43, 13);
            this.domainLabel.TabIndex = 11;
            this.domainLabel.Tag = "domain";
            this.domainLabel.Text = "Domain";
            this.domainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.domainLabel.Visible = false;
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(127, 108);
            this.port.MaxLength = 64;
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(69, 20);
            this.port.TabIndex = 13;
            this.port.Tag = "port";
            this.port.Visible = false;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(92, 112);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(26, 13);
            this.portLabel.TabIndex = 12;
            this.portLabel.Tag = "port";
            this.portLabel.Text = "Port";
            this.portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.portLabel.Visible = false;
            // 
            // userNamePassword
            // 
            this.AcceptButton = this.save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 178);
            this.Controls.Add(this.port);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.domainLabel);
            this.Controls.Add(this.serverLabel);
            this.Controls.Add(this.server);
            this.Controls.Add(this.save);
            this.Controls.Add(this.session);
            this.Controls.Add(this.password);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.userId);
            this.Controls.Add(this.userIdLabel);
            this.Controls.Add(this.domain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(298, 200);
            this.MinimumSize = new System.Drawing.Size(298, 200);
            this.Name = "userNamePassword";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connection Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button save;
        private System.Windows.Forms.CheckBox session;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox userId;
        private System.Windows.Forms.Label userIdLabel;
        private System.Windows.Forms.TextBox domain;
        private System.Windows.Forms.TextBox server;
        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.Label domainLabel;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.Label portLabel;
    }
}