using System.Diagnostics;
using System.Windows.Forms;

namespace CurePlease
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();

            label2.Text = Application.ProductVersion;
        }

        #region "== Form About"

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/atom0s/Cure-Please");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://ext.elitemmonetwork.com/downloads/eliteapi/");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://ext.elitemmonetwork.com/downloads/elitemmo_api/");
        }
    }

    #endregion "== Form About"
}