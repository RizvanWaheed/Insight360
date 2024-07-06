using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.Twitter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        DataCollector twitter = new DataCollector();

        private void btnStart_Click(object sender, EventArgs e)
        {
            twitter.Start();
            this.Text += "(Started)";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            twitter.Stop();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            twitter.Stop();
        }
    }
}
