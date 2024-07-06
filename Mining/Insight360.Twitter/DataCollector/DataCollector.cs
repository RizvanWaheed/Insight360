using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Insight360.Common;

namespace Insight360.Twitter
{
    public class DataCollector
    {
        ManualResetEvent stopEvent = new ManualResetEvent(false);
        //ManualResetEvent reconnectEvent = new ManualResetEvent(false);
        //ManualResetEvent keywordsChangedEvent = new ManualResetEvent(false);

        List<Thread> threads = new List<Thread>();
        private int Frequency;

        public DataCollector()
        {
            //Dec 12,2015 Ahsen Mehmood: Getting the frequency from DB
            //var doc = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Twitter");
            //Frequency = doc[0].Frequency * 60000; // converting in milliseconds
        }

        public void Start()
        {
            DAL.DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;

            if (AppSettings.Twitter_EnableSearchThread)
                threads.Add(new Thread(new ThreadStart(SearchThread)));

            //     if (Properties.Settings.Default.EnableDetailsThread)
            //        threads.Add(new Thread(new ThreadStart(DetailsThread)));

            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        public void SearchThread()
        {
            Util.Log.WriteOperationStarted(typeof(DataCollector), "Twitter SearchThread", "Thread Enter");

            try
            {
                TwitterSearchClient searchClient = new TwitterSearchClient(stopEvent);

                bool exit = false;
               // exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("Twitter");
                    Frequency = frequencyInMinutes * 60000;  // converting in milliseconds

                    int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(1);

                    var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Twitter");
                    var docExSources = DAL.ConfigDAL.FetchExcludedDocumentSourceConfiguration(1);
                    DAL.StagingDAL.LogMiner(1, "Twitter", "Started", "Twitter Miner started with " + (docSources.Count - lastRowId) + " channels");

                    for (int i = lastRowId + 1; i <= docSources.Count; i++)  //foreach (var source in docSources)
                    {
                       // DAL.StagingDAL.LogMiner(1, "Twitter", "Started", "Twitter Miner started for " + docSources[i - 1].DisplayName);

                        foreach (var search in docSources[i - 1].SearchTerms)
                        {
                            if (!exit)  // && search.IsTermEnabled == 1 [commented on  Dec 08, 2016 by Tooba on requirement placed by Azfer]
                            {

                                List<BO.Location> locations = DAL.ConfigDAL.GetLocations(search.Id);
                                if (locations.Count > 0)
                                {
                                    foreach (BO.Location location in locations)
                                    {
                                        DateTime dt = searchClient.FetchAndPersist(search, docSources[i - 1], location, docExSources);
                                    }
                                }
                                else
                                {
                                    DateTime dt = searchClient.FetchAndPersist(search, docSources[i - 1], null, docExSources);
                                }
                            }
                        }

                       // DAL.StagingDAL.LogMiner(1, "Twitter", "Processing", "Mining completed for " + docSources[i - 1].DisplayName);
                    }

                    DAL.ConfigDAL.UpdateLastFetchConfigId(1, 0);

                    DAL.StagingDAL.LogMiner(1, "Twitter", "Completed", "Twitter Miner Cycle completed");
                   // exit = stopEvent.WaitOne(Frequency);

                    //if (docSources.Count == 0)
                    //    exit = stopEvent.WaitOne(5 * 60 * 1000);
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Twitter Miner Issue in \"SearchThread\"");
                Util.Log.WriteException(typeof(DataCollector), "SearchThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(typeof(DataCollector), "SearchThread", "Thread Exit");
            }
        }

        void DetailsThread()
        {
            Util.Log.WriteOperationStarted(typeof(DataCollector), "DetailsThread", "Thread Enter");
            //Util.Log.WriteOperationStarted("Thread Enter");
            try
            {

                bool exit = false;
                exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    Util.Log.WriteOperationStarted(typeof(DataCollector), "DetailsThread", "TInitializeTwitterDetailsCollection");

                    //Util.Log.WriteOperationStarted("InitializeTwitterDetailsCollection");

                    //DAL.StagingDAL.InitializeTwitterDetailsCollection();

                    var statuses = DAL.StagingDAL.FetchTwitterStatusToUpdate(AppSettings.TopCount_StatusDetails);

                    while (!exit && statuses != null && statuses.Count > 0)
                    {
                        bool rateLimited = false;
                        foreach (var status in statuses)
                        {
                            long id;

                            if (long.TryParse(status.Id, out id))
                            {
                                var s = Insight360.Sources.Twitter.DataCollection.GetDetailedStatus(
                                    new BO.TwitterAuthentication
                                    {
                                        ConsumerKey = AppSettings.TwitterDetails_ConsumerKey,
                                        ConsumerSecret = AppSettings.TwitterDetails_ConsumerSecret,
                                        Token = AppSettings.TwitterDetails_Token,
                                        TokenSecret = AppSettings.TwitterDetails_TokenSecret
                                    }, id);
                                if (s != null)
                                {
                                    if (!s.SkippedDueToRateLimiting)
                                        //DAL.StagingDAL.UpdateTwitterStatusDetailsAndUser(s);
                                        DAL.StagingDAL.PersistTwitterStatus(s, -1, true, true, 0);
                                    else
                                        rateLimited |= s.SkippedDueToRateLimiting;
                                }
                            }
                        }

                        if (rateLimited)
                            DAL.StagingDAL.UpdateTwitterDetailsProcessingState(
                                statuses, Insight360.Util.DataProcessingState.UnProcessed);

                        //Util.Log.WriteOperationCompleted(statuses.Count + " tweets processed");
                        Util.Log.WriteOperationStarted(typeof(DataCollector), "DetailsThread", statuses.Count + " tweets processed");

                        // DAL.StagingDAL.UpdateFacebookUserProcessingState(userIds, Insight360.Util.DataProcessingState.Processed);
                        statuses = DAL.StagingDAL.FetchTwitterStatusToUpdate(AppSettings.TopCount_StatusDetails);

                        exit = stopEvent.WaitOne((int)AppSettings.Twitter_WaitInterval_ApiCalls.TotalMilliseconds);
                    }

                    exit = stopEvent.WaitOne((int)AppSettings.WaitInterval_DetailsCollection.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteOperationStarted(typeof(DataCollector), "Exception in Fetching Status", "Exception in Fetching Status");

                //Util.Log.WriteException(ex);
                Util.Log.WriteException(typeof(DataCollector), "Exception in Fetching Status", ex);

                Util.Log.WriteException(typeof(DataCollector), "DetailsThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(typeof(DataCollector), "DetailsThread", "Thread Exit");
                //Util.Log.WriteOperationStarted("Thread Exit");
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

        //List<BO.DocumentSourceConfiguration> configuration;
        //List<string> GetKeywords()
        //{
        //    List<string> keywords = new List<string>();
        //    configuration = DAL.ConfigDAL.FetchDocumentSourceConfiguration();
        //    foreach (var org in configuration)
        //    {
        //        if (org.Enabled && !string.IsNullOrEmpty(org.SearchKeywords))
        //            keywords.Add(org.SearchKeywords);
        //    }
        //    return keywords;
        //}

        //void StreamingThread()
        //{
        //    Util.Log.WriteOperationStarted("Thread Start");
        //    TwitterStreamingClient activeClient = null, reconnectClient = null;
        //    int retries = 0;
        //    DateTime connectionAttemptTime = DateTime.MinValue;
        //    DateTime failureTime = DateTime.MinValue;

        //    bool exit = false;
        //    while (!exit)
        //    {
        //        reconnectClient = new TwitterStreamingClient();

        //        reconnectEvent.Reset();
        //        keywordsChangedEvent.Reset();

        //        Util.Log.WriteOperationStarted("Connecting retry " + retries);
        //        connectionAttemptTime = DateTime.Now;
        //        if (reconnectClient.StartStreaming(GetKeywords(), reconnectEvent))
        //        {
        //            //if (activeClient != null)
        //                //activeClient.StopStreaming();

        //            activeClient = reconnectClient;

        //            int index = ManualResetEvent.WaitAny(
        //               new WaitHandle[] 
        //                { 
        //                    stopEvent,                        
        //                    keywordsChangedEvent,
        //                    reconnectEvent, 
        //                    activeClient.StreamingStatus.AsyncWaitHandle  
        //                },
        //               (int)Properties.Settings.Default.ReconnectInterval.TotalMilliseconds
        //               - (15 * 1000));

        //            switch (index)
        //            {
        //                case 0: //exit thread
        //                    exit = true;
        //                    break;
        //                case 1: //reconnect for keywords change
        //                    Util.Log.WriteOperationStarted("Reconnect for keywords change");
        //                    if ((DateTime.Now - connectionAttemptTime).TotalSeconds > 300)
        //                        retries = 0;
        //                    break;
        //                case ManualResetEvent.WaitTimeout://reconnect for expired interval
        //                    Util.Log.WriteOperationStarted("Reconnect for expired interval");
        //                    retries = 0;
        //                    break;
        //                default: //reconnect for failure
        //                    Util.Log.WriteOperationStarted("Reconnect for failure");
        //                    if ((DateTime.Now - connectionAttemptTime).TotalSeconds > 100)
        //                        retries = 0;

        //                    retries++;
        //                    failureTime = DateTime.Now;
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            retries++;
        //        }


        //        int retryWait = 1; //milliseconds
        //        switch (retries)
        //        {
        //            case 0:
        //            case 1:
        //                //no wait
        //                break;
        //            case 2:
        //                retryWait = 5 * 1000;
        //                break;
        //            default:
        //                retryWait = (int)Math.Pow(2, retries - 2) * 10 * 1000;
        //                if (retryWait > (300 * 1000))
        //                    retryWait = 300 * 1000;
        //                break;
        //        }

        //        retryWait -= (int)(DateTime.Now - connectionAttemptTime).TotalMilliseconds;
        //        if (retryWait < 0)
        //            retryWait = 1;

        //        Util.Log.WriteOperationStarted("Waiting " + (retryWait / 1000) + " seconds");
        //        if (stopEvent.WaitOne(retryWait))
        //            exit = true;
        //    }

        //    //stop streaming before exiting
        //    if (activeClient != null)
        //    {
        //        activeClient.StopStreaming();
        //    }

        //    Util.Log.WriteOperationStarted("Thread Exit");
        //}

        //void MonitorConfigurationThread()
        //{
        //    Util.Log.WriteOperationStarted("Thread Start");
        //    bool exit = false;
        //    exit = stopEvent.WaitOne((int)Properties.Settings.Default.CheckConfigurationInterval.TotalMilliseconds);

        //    while (!exit)
        //    {
        //        if (configuration != null)
        //        {
        //            Util.Log.WriteOperationStarted("Checking configuration");
        //            bool configurationChanged = false;
        //            var changed = DAL.ConfigDAL.FetchDocumentSourceConfiguration();
        //            for (int i = 0; i < changed.Count; i++)
        //            {
        //                bool found = false;
        //                for (int j = 0; j < configuration.Count; j++)
        //                {
        //                    if (changed[i].OrgName.Equals(configuration[j].OrgName, StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        found = true;
        //                        if (changed[i].Enabled != configuration[j].Enabled)
        //                        {
        //                            configurationChanged = true;
        //                            break;
        //                        }
        //                        if (!changed[i].SearchKeywords.Equals(configuration[j].SearchKeywords))
        //                        {
        //                            configurationChanged = true;
        //                            break;
        //                        }
        //                    }
        //                }
        //                if (!found)
        //                {
        //                    configurationChanged = true;
        //                    break;
        //                }
        //            }
        //            if (configurationChanged)
        //            {
        //                Util.Log.WriteOperationCompleted("Configuration changed");
        //                keywordsChangedEvent.Set();
        //            }
        //        }

        //        exit = stopEvent.WaitOne((int)Properties.Settings.Default.CheckConfigurationInterval.TotalMilliseconds);
        //    }
        //    Util.Log.WriteOperationStarted("Thread Exit");
        //}
    }
}
