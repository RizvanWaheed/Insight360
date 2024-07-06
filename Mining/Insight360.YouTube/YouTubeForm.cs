using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Insight360.Common;

namespace Insight360.YouTube
{
    public partial class YouTubeForm : Form
    {

        #region Configuration Settings

        private int pageSize
        {
            get { return AppSettings.Youtube_PageSize; }
        }

        //private int orgID
        //{
        //    get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["OrgID"].ToString()); }
        //}

        private string prodkey
        {
            get { return AppSettings.Youtube_ProdKey; }
        }

        private int maxData
        {
            get { return AppSettings.Youtube_MaxData; }
        }

        private string YoutubeUserName
        {
            get { return AppSettings.Youtube_UserName; }
        }

        private string YoutubePassword
        {
            get { return AppSettings.Youtube_Password; }
        }

        #endregion

        public YouTubeForm()
        {
            InitializeComponent();
        }
        DataCollector youtube = new DataCollector();
        private void btnStart_Click_1(object sender, EventArgs e)
        {
            youtube.Start();
            this.Text += "(Started)";
        }

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            youtube.Stop();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            youtube.Stop();
        }









    }
}
