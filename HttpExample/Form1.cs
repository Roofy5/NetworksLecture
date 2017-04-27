using System;
using System.Windows.Forms;

namespace HttpExample
{
    public partial class Form1 : Form
    {
        private Sender mySender;
        private AlgorithmsDownloader downloader;

        public Form1()
        {
            InitializeComponent();
            mySender = new Sender();
            downloader = new AlgorithmsDownloader();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            outputPasswords.Items.Clear();
            GetAndEncryptAllThePasswords();
        }

        private async void GetAndEncryptAllThePasswords()
        {
            string[] passwords = inputPasswords.Text.Split('\n');
            string selectedAlgoritm = inputAlgorithm.Enabled ? inputAlgorithm.SelectedItem as string : "md5";

            foreach (var password in passwords)
            {
                string hash = await mySender.SendPostRequest(password.Trim(), selectedAlgoritm);
                AddEnryptedPassword(password, hash);
            }
            outputPasswords.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            outputPasswords.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddEnryptedPassword(string password, string hash)
        {
            outputPasswords.Items.Add(new ListViewItem(new string[] { password, hash }));
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            string [] algoritms = await downloader.GetAlgorithms();
            inputAlgorithm.Items.AddRange(algoritms);
            inputAlgorithm.SelectedIndex = 0;
            inputAlgorithm.Enabled = true;
        }
    }
}
