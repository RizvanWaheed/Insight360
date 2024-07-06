using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insight360.DataProcessor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        DataProcessor processDataManager = new DataProcessor();
        private void btnStart_Click(object sender, EventArgs e)
        {
            processDataManager.Start();
            this.Text += " (Started)";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            processDataManager.Stop();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var orgs = DAL.ConfigDAL.FetchOrganizations();

            for (int i = 0; i < orgs.Count; i++)
            {
                int j = this.cmbOrgs.Items.Add(orgs[i]);
            }
        }

        private void btnProcessSurveyComments_Click(object sender, EventArgs e)
        {
            //processDataManager.ProcessSurveyComments(
            //    int.Parse(this.cmbOrgs.Text.Split(',')[0]),
            //    this.cmbOrgs.Text.Split(',')[1]);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            processDataManager.Stop();
        }




    }
}
