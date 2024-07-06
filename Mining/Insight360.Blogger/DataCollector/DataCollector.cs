using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using Insight360.DAL;
using Insight360.BO;
using System.Data;
using System.Collections;
using System.Threading;
using System.Data.SqlClient;
using Insight360.Common;

namespace Insight360.Blogger
{
    public class DataCollector
    {
        #region Config Settings

        private ArrayList entryList;
        private string feedUri;
        private string feedContent;
        //public BloggerService _service;
        #endregion
        private int Frequency;
        public DataCollector()
        {
            //Dec 12,2015 Ahsen Mehmood: Getting the frequency from DB
            //var doc = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Blogger");
            //Frequency = doc[0].Frequency * 60000; // converting in milliseconds
        }
        ManualResetEvent stopEvent = new ManualResetEvent(false);
        List<Thread> threads = new List<Thread>();

        public void Start()
        {
            if (AppSettings.Blogger_EnableSearchThread)
                threads.Add(new Thread(new ThreadStart(SearchThread)));

            //if (Properties.Settings.Default.EnableDataRefreshThread)
            //    threads.Add(new Thread(new ThreadStart(DataRefreshThread)));

            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        public void SearchThread()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "Blogger SearchThread", "Thread Enter");

            try
            {
                BloggerClient searchClient = new BloggerClient(stopEvent);

                bool exit = false;
                exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Blogger");
                    var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("Blogger");

                    DAL.StagingDAL.LogMiner(32, "Blogger", "Started", "Blogger Miner started with " + docSources.Count + " Document Sources");

                    Frequency = frequencyInMinutes * 60000;

                    foreach (var source in docSources)
                    {
                        DAL.StagingDAL.LogMiner(32, "Blogger", "Processing", "Blogger Miner started for " + source.DisplayName);

                        if (!exit)
                        {
                            searchClient.FetchBlogPosts(source);
                        }

                        DAL.StagingDAL.LogMiner(32, "Blogger", "Processing", "Blogger Miner completed for " + source.DisplayName);
                    }

                    DAL.StagingDAL.LogMiner(32, "Blogger", "Completed", "Blogger Miner cycle completed. Restarting in " + frequencyInMinutes + " mins");
                    exit = stopEvent.WaitOne(Frequency);

                    if (docSources.Count == 0)
                        exit = stopEvent.WaitOne(5 * 60 * 1000);
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"SearchThread\"");
                Util.Log.WriteException(this.GetType(), "Blogger SearchThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(this.GetType(), "Blogger SearchThread", "Thread Exit");
            }
        }

        //void DataRefreshThread()
        //{
        //    Util.Log.WriteOperationStarted(this.GetType(), "DataRefreshThread", "Thread Enter");
        //    try
        //    {
        //        YouTubeClient searchClient = new YouTubeClient(stopEvent);

        //        bool exit = false;
        //        exit = stopEvent.WaitOne(1);

        //        while (!exit)
        //        {
        //            var feeds = DAL.YoutubeDbOperations.FetchVideosToUpdate(Properties.Settings.Default.TopCount_DataRefresh);
        //            Util.Log.WriteOperationCompleted(this.GetType(), "DataRefreshThread", "Fetched " + feeds.Count);
        //            foreach (var feed in feeds)
        //            {
        //                var p = Sources.Youtube.DataCollection.GetVideo(feed.VideoID, Properties.Settings.Default.GetComments, feed.Staging_OrgName);
        //                if (p != null)
        //                {
        //                    p.Staging.OrgName = feed.Staging.OrgName;
        //                    p.Staging.StagingId = feed.Staging.StagingId;
        //                    p.Staging.DocumentSourceId = feed.Staging.DocumentSourceId;
        //                    DAL.YoutubeDbOperations.PersistYouTubeFeed(p, false, -1, Properties.Settings.Default.GetComments);
        //                }
        //                Thread.Sleep((int)Properties.Settings.Default.WaitInterval_ApiCalls.TotalMilliseconds);
        //            }
        //            Util.Log.WriteOperationCompleted(this.GetType(), "DataRefreshThread",
        //                "Processed " + feeds.Count);
        //            exit = stopEvent.WaitOne(feeds.Count > 0 ? 1 : (int)Properties.Settings.Default.WaitInterval_NothingFetched.TotalMilliseconds);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Log.WriteException(this.GetType(), "DataRefreshThread", ex);
        //    }
        //    finally
        //    {
        //        Util.Log.WriteOperationStarted(this.GetType(), "DataRefreshThread", "Thread Exit");
        //    }
        //}

        public void Stop()
        {
            stopEvent.Set();

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }
    }
}
