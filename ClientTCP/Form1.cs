using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TCPLibrary;

namespace ClientTCP
{
    public partial class Form1 : Form
    {
        private Client client;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new Client(inputIpAddress.Text, (ushort)inputPort.Value);
                btnConnect.Enabled = false;
                client.MessageFunction += GetMessage;
                client.ClientConnect();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                client.SendMessage(inputMessage.Text);
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
