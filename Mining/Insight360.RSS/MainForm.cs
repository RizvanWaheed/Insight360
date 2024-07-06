using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.RSS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        DataCollector rss = new DataCollector();

        private void btnStart_Click(object sender, EventArgs e)
        {
            rss.Start();
            this.Text += "(Started)";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            rss.Stop();
        }
    }
}
