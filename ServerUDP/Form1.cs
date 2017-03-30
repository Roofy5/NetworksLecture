using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using UDPLibrary;

namespace ServerUDP
{
    public partial class Form1 : Form
    {
        private Server server;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                server = new Server((ushort)inputPort.Value);
                btnStartServer.Enabled = false;
                server.MessageFunction += GetMessage;
                server.WaitForMessage();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            }

        private void GetMessage(string message)
        {
            outputList.Items.Add(message);
        }
    }
}
