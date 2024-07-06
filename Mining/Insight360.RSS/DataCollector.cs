using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Insight360.Common;

namespace Insight360.RSS
{
   public class DataCollector
    {
        ManualResetEvent stopEvent = new ManualResetEvent(false);
        ManualResetEvent reconnectEvent = new ManualResetEvent(false);
        ManualResetEvent keywordsChangedEvent = new ManualResetEvent(false);

        Thread mainThread;
        Thread monitorConfigurationThread;
        private int Frequency;
        public DataCollector()
        {
            //Dec 12,2015 Ahsen Mehmood: Getting the frequency from DB
            //var doc = DAL.ConfigDAL.FetchDocumentSourceConfiguration("RSS");
            //Frequency = doc[0].Frequency * 60000; // converting in milliseconds
        }

        public void Start()
        {
            DAL.DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
            mainThread = new Thread(new ThreadStart(SearchThread));
            mainThread.Start();
        }

       public void SearchThread()
        {
            try
            {
                Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Thread Enter");
                RssSearchClient searchClient = new RssSearchClient();

                bool exit = false;
                exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("RSS");

                    var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("RSS");
                    Frequency = frequencyInMinutes * 60000;  // converting in milliseconds

                    DAL.StagingDAL.LogMiner(64, "RSS", "Started", "RSS Miner started with " + docSources.Count + " channels");

                    foreach (var source in docSources)
                    {
                        DAL.StagingDAL.LogMiner(64, "RSS", "Processing", "RSS Miner started for " + source.DisplayName);

                        if (!exit)
                        {
                            searchClient.FetchAndPersist(source);
                        }

                        DAL.StagingDAL.LogMiner(64, "RSS", "Processing", "RSS Miner completed for " + source.DisplayName);
                    }

                    DAL.StagingDAL.LogMiner(64, "RSS", "Completed", "RSS Miner Cycle completed. restarting in " + frequencyInMinutes + " mins.");
                    exit = stopEvent.WaitOne(Frequency);

                    if (docSources.Count == 0)
                        exit = stopEvent.WaitOne(5 * 60 * 1000);
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(64, "RSS", "Error", "RSS Miner Miner Issue in \"SearchThread\"");
                Util.Log.WriteException(this.GetType(), "SearchThread", ex);
            }

            Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Thread Exit");
        }        

        public void Stop()
        {
            stopEvent.Set();
            mainThread.Join();
        }
    }
}
