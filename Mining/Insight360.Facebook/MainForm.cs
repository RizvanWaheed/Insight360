using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.Facebook
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        DataCollector facebook = new DataCollector();

        private void btnStart_Click(object sender, EventArgs e)
        {
            facebook.Start();
            this.Text += "(Started)";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            facebook.Stop();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            facebook.Stop();
        }
    }
}
