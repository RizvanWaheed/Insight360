using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Insight360.DAL;
using Insight360.Util;
using Insight360.BO;
using Insight360.Common;
using System.Data;
using System.Threading.Tasks;
using Quartz;

namespace Insight360.Scheduling
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Scheduling" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Scheduling.svc or Scheduling.svc.cs at the Solution Explorer and start debugging.

    [DisallowConcurrentExecution]
    public class SchedulingInitiator : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
            //Task.Factory.StartNew(() => MineFacebook());
            Task.Factory.StartNew(() => MineBlogger());
            Task.Factory.StartNew(() => MineTwitter());
            Task.Factory.StartNew(() => MineRSS());
            Task.Factory.StartNew(() => MineYoutube());
            Task.Factory.StartNew(() => MineAllWeb());
            Task.Factory.StartNew(() => MineFeedly());
            Task.Factory.StartNew(() => MineInstagram());
        }

        public void Process()
        {
            string query;
            query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Web_Processor);
            query = query.Replace("Common", "Airways");

            try
            {
                DBUtil.ExecuteNonQuery(query, null, null, CommandType.StoredProcedure);
                Log.WriteOperationStarted(this.GetType(), "Process", "** Scheduling Service::::Process.......**");
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "Process", ex);
            }

            try
            {
                query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.WebMiner_ProcessDashboard);
                query = query.Replace("Common", "Airways");
                Log.WriteOperationStarted(this.GetType(), "Process", "** Scheduling Service::::Process.......**");

                DBUtil.ExecuteNonQuery(query, null, null, CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "Process", ex);
            }
        }

        public string StopMining()
        {
            //StopFacebook();

            //StopTwitter();

            //StopBlogger();

            //StopRSS();

            //StopYoutube();

            //StopAllWeb();

            //StopFeedly();

            return "";
        }

        #region Start Mining

        public string MineFacebook()
        {
            try
            {
                //check if fb limit reached hold to call this scrapper for 20 minutes.
                if (System.IO.File.Exists("/listening/fblimit.txt"))
                {
                    var lines=System.IO.File.ReadAllLines("/listening/fblimit.txt");
                    var limitTime = lines.FirstOrDefault();
                    DateTime fbLimitTime;
                    if (!string.IsNullOrEmpty(limitTime)&&DateTime.TryParseExact(limitTime, "dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out fbLimitTime))
                    {
                        if (fbLimitTime < DateTime.Now)
                        {
                            System.IO.File.Delete("/listening/fblimit.txt");
                        }
                        else
                        {
                            return "FB limit Reached!!!";
                        }
                    }
                    else
                    {
                        System.IO.File.Delete("/listening/fblimit.txt");
                    }
                }

                Facebook.DataCollector dc = new Facebook.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "MineFacebook", "Service for Facebook Started.....");
                dc.Start();
                //Log.WriteOperationCompleted(this.GetType(), "MineFacebook", "** Scheduling Service::::Facebook Completed.......**");

                return "Facebook has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineFacebook", ex);
                return "FACEBOOK----" + ex.Message;
            }
        }
        public string MineInstagram()
        {
            try
            {
                Instagram.DataCollector dc = new Instagram.DataCollector();
                Insight360.Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Instagram Started.....");
                dc.Start();
                //logger.Info("** Scheduling Service::::Instagram Started.......**");

                return "Instagram has been started..";
            }
            catch (Exception ex)
            {
                //logger.Error("** Exception:::::Scheduling Service::::Instagram Started.......**");
                return "Instagram----" + ex.Message;
            }
        }
        public string MineBlogger()
        {
            try
            {
                Blogger.DataCollector BloggerDC = new Blogger.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "MineBlogger", "Service for Blogger Started.....");
                BloggerDC.Start();
                //Log.WriteOperationCompleted(this.GetType(), "MineBlogger", "Service for Blogger Completed.....");

                return "Blogger has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineBlogger", ex);
                return "Blogger----" + ex.Message;
            }
        }

        public string MineTwitter()
        {
            try
            {
                Twitter.DataCollector TwitterDC = new Twitter.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "MineTwitter", "Service for Twitter Started.....");
                TwitterDC.Start();
                //Log.WriteOperationCompleted(this.GetType(), "MineTwitter", "Service for Twitter Completed.....");

                return "Twitter has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineTwitter", ex);
                return "Twitter----" + ex.Message;
            }
        }

        public string MineRSS()
        {
            try
            {
                RSS.DataCollector RssDC = new RSS.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "MineRSS", "Service for RSS Started.....");
                RssDC.Start();
                //Log.WriteOperationStarted(this.GetType(), "MineRSS", "Service for RSS Completed.....");

                return "RSS has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineRSS", ex);
                return "RSS----" + ex.Message;
            }
        }

        public string MineYoutube()
        {
            try
            {
                YouTube.DataCollector YoutubeDC = new YouTube.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "MineYoutube", "Service for Youtube Started.....");
                YoutubeDC.Start();
                //Log.WriteOperationCompleted(this.GetType(), "MineYoutube", "Service for Youtube Completed.....");


                return "Youtube has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineYoutube", ex);

                return "Youtube----" + ex.Message;
            }
        }

        public string MineAllWeb()
        {
            try
            {
                SkyTrax.DataCrawler dc = new SkyTrax.DataCrawler();
                Log.WriteOperationStarted(this.GetType(), "MineAllWeb", "Service for Web Started.....");
                dc.Start();
                //Log.WriteOperationCompleted(this.GetType(), "MineAllWeb", "Service for Web Completed.....");


                return "Web has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineAllWeb", ex);
                return "WEB----" + ex.Message;
            }
        }

        public string MineFeedly()
        {
            try
            {
                Insight360.Feedly.DataCollector dc = new Insight360.Feedly.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Web Started.....");
                dc.Start();

                return "FEEDLY has been started..";
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineFeedly", ex);
                return "FEEDLY----" + ex.Message;
            }
        }

        #endregion

        //#region Stop Mining

        //public string StopFacebook()
        //{
        //    try
        //    {
        //        Facebook.DataCollector dc = new Facebook.DataCollector();
        //        Insight360.Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Facebook Started.....");
        //        dc.Stop();

        //        logger.Info("** Scheduling Service::::Facebook Started.......**");

        //        return "Facebook has been started..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("** Exception:::::Scheduling Service::::FACEBOOK Started.......**");
        //        return "FACEBOOK----" + ex.Message;
        //    }
        //}

        //public string StopBlogger()
        //{
        //    try
        //    {
        //        Blogger.DataCollector BloggerDC = new Blogger.DataCollector();
        //        Insight360.Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Blogger Started.....");
        //        BloggerDC.Stop();
        //        logger.Info("** Scheduling Service::::Blogger Started.......**");

        //        return "Blogger has been started..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("** Exception:::::Scheduling Service::::Blogger Started.......**");
        //        return "Blogger----" + ex.Message;
        //    }
        //}

        //public string StopTwitter()
        //{
        //    try
        //    {
        //        Twitter.DataCollector TwitterDC = new Twitter.DataCollector();
        //        Insight360.Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Twitter Started.....");
        //        TwitterDC.Stop();

        //        logger.Info("** Scheduling Service::::Twitter Started.......**");

        //        return "Twitter has been started..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("**Exception::::: Scheduling Service::::RSS Started.......**");
        //        return "Twitter----" + ex.Message;
        //    }
        //}

        //public string StopRSS()
        //{
        //    try
        //    {
        //        RSS.DataCollector RssDC = new RSS.DataCollector();
        //        Insight360.Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for RSS Started.....");
        //        RssDC.Stop();
        //        logger.Info("** Scheduling Service::::RSS Started.......**");

        //        return "RSS has been started..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("**Exception::::: Scheduling Service::::RSS Started.......**");
        //        return "RSS----" + ex.Message;
        //    }
        //}

        //public string StopYoutube()
        //{
        //    try
        //    {
        //        YouTube.DataCollector YoutubeDC = new YouTube.DataCollector();
        //        Insight360.Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Youtube Started.....");

        //        YoutubeDC.Stop();

        //        logger.Info("** Scheduling Service::::Youtube Started.......**");

        //        return "Youtube has been started..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("**Exception::::: Scheduling Service::::Youtube.......**");

        //        return "Youtube----" + ex.Message;
        //    }
        //}

        //public string StopAllWeb()
        //{
        //    try
        //    {
        //        SkyTrax.DataCrawler dc = new SkyTrax.DataCrawler();
        //        Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for Web Stopped.....");
        //        dc.Stop();

        //        logger.Info("** Scheduling Service::::Web Stopped.......**");

        //        return "Web has been stopped..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("** Exception:::::Scheduling Service::::WEB Stopped.......**");
        //        return "WEB----" + ex.Message;
        //    }
        //}

        //public string StopFeedly()
        //{
        //    try
        //    {
        //        Insight360.Feedly.DataCollector dc = new Insight360.Feedly.DataCollector();
        //        Log.WriteOperationStarted(this.GetType(), "SearchThread", "Service for FEEDLY Stopped.....");
        //        dc.Stop();

        //        logger.Info("** Scheduling Service::::FEEDLY Stopped.......**");

        //        return "FEEDLY has been stopped..";
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("** Exception:::::Scheduling Service::::FEEDLY Stopped.......**");
        //        return "FEEDLY----" + ex.Message;
        //    }
        //}

        //#endregion
    }
}
