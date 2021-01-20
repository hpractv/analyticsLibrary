using analyticsLibrary.dbObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace analyticsLibrary.library
{

    public partial class userNamePassword : Form
    {
        public userNamePassword()
        {
            InitializeComponent();
        }

        private IEnumerable<Control> controlsByTag(string tag)
        {
            return this.Controls.Cast<Control>()
                .Where(c => c.Tag != null && c.Tag.ToString() == tag);
        }
        private void setControlsVisible(string tag, bool visible)
        {
            this.controlsByTag(tag).forEach(c => c.Visible = visible);
        }

        private void save_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static login getUserNamePasswordDsn(string name, string dsn)
        {
            return getUserNamePassword(dbConnection.loginType.dsn, name, dsn, null, null, null, null);
        }

        public static login getUserNamePasswordServer(string name, string server, string domain)
        {
            return getUserNamePassword(dbConnection.loginType.server, name, null, server, domain, null, null);
        }


        public static login getUserNamePasswordOracle(string name, string server, string service, int port)
        {
            return getUserNamePassword(dbConnection.loginType.tns, name, null, server, null, service, port);
        }

        internal static login getUserNamePassword(dbConnection.loginType type, string name, string dsn, string server, string domain, string service, int? port)
        {
            var dialogue = new userNamePassword();
            var info = new login();
            try
            {
                dialogue.Text = string.Format("{0} {1}", name, dialogue.Text);

                switch (type)
                {
                    case dbConnection.loginType.server:
                        dialogue.setControlsVisible("server", true);
                        dialogue.setControlsVisible("domain", true);
                        dialogue.server.Text = server;
                        dialogue.domain.Text = domain;
                        break;
                    case dbConnection.loginType.tns:
                        dialogue.setControlsVisible("server", true);
                        dialogue.setControlsVisible("domain", true);
                        dialogue.setControlsVisible("port", true);
                        dialogue.server.Text = server;
                        dialogue.domainLabel.Text = "Service";
                        dialogue.domain.Text = service;
                        dialogue.port.Text = (port ?? 1521).ToString();
                        break;
                    case dbConnection.loginType.dsn:
                    default:
                        dialogue.setControlsVisible("domain", true);
                        dialogue.domainLabel.Text = "DSN";
                        dialogue.domain.Text = dsn;
                        break;
                }


                var inputs = dialogue.Controls.Cast<Control>()
                    .Where(c => c is TextBox && c.Visible)
                    .OrderBy(c => c.TabIndex);

                dialogue.ShowDialog();

                foreach (var i in inputs)
                {
                    if (string.IsNullOrWhiteSpace(((TextBox)i).Text))
                    {
                        i.Focus();
                        break;
                    }
                }

                if (dialogue.DialogResult == DialogResult.OK)
                {
                    switch (type)
                    {
                        case dbConnection.loginType.server:
                            info.server = dialogue.server.Text;
                            info.domain = dialogue.domain.Text;
                            break;
                        case dbConnection.loginType.tns:
                            info.server = dialogue.server.Text;
                            info.serviceName = dialogue.domain.Text;
                            info.port = int.Parse(dialogue.port.Text);
                            break;
                        case dbConnection.loginType.dsn:
                        default:
                            info.dsn = dialogue.domain.Text;
                            break;
                    }

                    info.userId = dialogue.userId.Text;
                    info.password = dialogue.password.Text;
                    info.sessionPersist = dialogue.session.Checked;
                }
                else
                {
                    throw new ApplicationException("Connection credential save error.");
                }
            }
            finally
            {
                dialogue.Dispose();
            }
            return info;
        }
    }
}
