using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.Instagram
{
    public partial class InstaForm : Form
    {

        DataCollector client = new DataCollector();

        public InstaForm()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            client.Start();
            this.Text += "(Started)";
        }
    }
}
