using System;
using System.Collections.Generic;
using System.Threading;
using Insight360.Common;

namespace Insight360.YouTube
{
    public class DataCollector
    {
        ManualResetEvent stopEvent = new ManualResetEvent(false);

        List<Thread> threads = new List<Thread>();
        private int Frequency;
        public DataCollector()
        {
            //Dec 12,2015 Ahsen Mehmood: Getting the frequency from DB
            //var doc = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Youtube");
            //Frequency = doc[0].Frequency * 60000; // converting in milliseconds
        }
        public void Start()
        {
            if (AppSettings.Youtube_EnableSearchThread)
                threads.Add(new Thread(new ThreadStart(SearchThread)));

            if (AppSettings.Youtube_EnableDataRefreshThread)
                threads.Add(new Thread(new ThreadStart(DataRefreshThread)));

            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        public void SearchThread()
        {
            try
            {
                Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Thread Enter");

                YouTubeClient searchClient = new YouTubeClient(stopEvent);

                bool exit = false;
                // exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Youtube");
                    Frequency = DAL.ConfigDAL.GetFrequency("Youtube") * 60000; // converting in milliseconds

                    int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(4);
                    DAL.StagingDAL.LogMiner(4, "Youtube", "Started", "Youtube Miner started with " + (docSources.Count - lastRowId) + " channels");

                    for (int i = lastRowId + 1; i <= docSources.Count; i++)  //foreach (BO.DocumentSourceInfo source in docSources)
                    {
                        if (docSources[i-1].IsEnabled == 1)
                            for (int j = 0; j < docSources[i-1].SearchTerms.Count; j++)
                            {
                                if (!exit)  // && source.SearchTerms[i].IsTermEnabled == 1 [commented on  Dec 08, 2016 by Tooba on requirement placed by Azfer]
                                {
                                    DateTime dt = searchClient.FetchVideoFeeds(docSources[i-1].SearchTerms[j], docSources[i-1]);
                                }
                            }
                    }

                    DAL.ConfigDAL.UpdateLastFetchConfigId(4, 0);
                    DAL.StagingDAL.LogMiner(4, "Youtube", "Completed", "Youtube Miner Cycle completed");

                    // exit = stopEvent.WaitOne(Frequency);

                    //if (docSources.Count == 0)
                    //    exit = stopEvent.WaitOne(5 * 60 * 1000);
                }
            }

            catch (Exception ex)
            {
                Util.Log.WriteException(this.GetType(), "SearchThread", ex);
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Miner issue in \"SearchThread\"");
            }

            finally
            {
                Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Thread Exit");
            }
        }

        void DataRefreshThread()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "DataRefreshThread", "Thread Enter");
            try
            {
                YouTubeClient searchClient = new YouTubeClient(stopEvent);

                bool exit = false;
                exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    var feeds = DAL.YoutubeDbOperations.FetchVideosToUpdate(AppSettings.Youtube_TopCount_DataRefresh);
                    Util.Log.WriteOperationCompleted(this.GetType(), "DataRefreshThread", "Fetched " + feeds.Count);
                    foreach (var feed in feeds)
                    {
                        var p = Sources.DataCollection.GetVideo(feed.VideoID, AppSettings.Youtube_GetComments);
                        if (p != null)
                        {
                            //p.Staging.OrgName = feed.Staging.OrgName;
                            p.Staging.StagingId = feed.Staging.StagingId;
                            p.Staging.DocumentSourceId = feed.Staging.DocumentSourceId;
                            DAL.YoutubeDbOperations.PersistYouTubeFeed(p, false, -1, AppSettings.Youtube_GetComments);
                        }
                        Thread.Sleep((int)AppSettings.Youtube_WaitInterval_ApiCalls.TotalMilliseconds);
                    }
                    Util.Log.WriteOperationCompleted(this.GetType(), "DataRefreshThread",
                        "Processed " + feeds.Count);
                    exit = stopEvent.WaitOne(feeds.Count > 0 ? 1 : (int)AppSettings.Youtube_WaitInterval_NothingFetched.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Miner issue in \"DataRefreshThread\"");
                Util.Log.WriteException(this.GetType(), "DataRefreshThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(this.GetType(), "DataRefreshThread", "Thread Exit");
            }
        }

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
