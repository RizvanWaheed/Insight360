using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.SkyTrax
{
    public partial class WinForm : Form
    {
        public WinForm()
        {
            InitializeComponent();
        }
        DataCrawler skyTrax = new DataCrawler();
        private void WinForm_Load(object sender, EventArgs e)
        {

        }
        private void Start_Click(object sender, EventArgs e)
        {
            skyTrax.Start();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            skyTrax.Stop();
        }

    }
}
