using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.Blogger
{
    public partial class BloggerForm : Form
    {
        public BloggerForm()
        {
            InitializeComponent();
        }

        DataCollector Blogger = new DataCollector();
        private void btnStart_Click(object sender, EventArgs e)
        {

            Blogger.Start();
            this.Text += "(Started)";

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Blogger.Stop();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Blogger.Stop();
        }
    }
}
