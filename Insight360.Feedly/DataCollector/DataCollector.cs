using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insight360.Feedly
{
    public class DataCollector
    {
        public DataCollector() { }
        private int Frequency;
        ManualResetEvent stopEvent = new ManualResetEvent(false);

        List<Thread> threads = new List<Thread>();

        public void Start()
        {
            threads.Add(new Thread(new ThreadStart(SearchThread)));
            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        public void SearchThread()
        {
            DateTime dt = DateTime.Now;
            Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Thread Enter Feedly");
            //try
            //{
            FeedlyClient searchClient = new FeedlyClient(stopEvent);
            bool exit = false;
            //exit = stopEvent.WaitOne(1);

            while (!exit)
            {
                var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Feedly");
                var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("Feedly");
                Frequency = frequencyInMinutes * 60000;

                int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(99);
                DAL.StagingDAL.LogMiner(99, "Feedly", "Started", "Feedly Miner started with " + (docSources.Count - lastRowId) + " channels");

                for (int i = lastRowId + 1; i <= docSources.Count; i++) //foreach (var source in docSources)
                {
                    try
                    {
                        foreach (var search in docSources[i - 1].SearchTerms)
                        {
                            if (!exit)   // && search.IsTermEnabled == 1 [commented on  Dec 08, 2016 by Tooba on requirement placed by Azfer]
                            {
                                dt = searchClient.FetchFeedlyFeeds(search, docSources[i - 1]);
                            }
                        }

                        //Thread.Sleep(60000);
                    }

                    catch (Exception ex)
                    {
                        DAL.StagingDAL.LogMiner(99, "Feedly", "Error 429", "Too many requests- Row Id: " + docSources[i - 1].RowId + ", Src: " + docSources[i - 1].DisplayName + ", KW: " + docSources[i-1].SearchTerms[0].SearchTerm);
                        Util.Log.WriteException(this.GetType(), "FetchPostsAndPersist " + docSources[i - 1].DisplayName, ex);

                        DAL.ConfigDAL.UpdateLastFetchConfigId(99, docSources[i - 1].RowId);
                    }
                }

                DAL.ConfigDAL.UpdateLastFetchConfigId(99, 0);
                DAL.StagingDAL.LogMiner(99, "Feedly", "Completed", "Feedly Miner Cycle completed");
                //exit = stopEvent.WaitOne(Frequency);

                //if (docSources.Count == 0)
                //    exit = stopEvent.WaitOne(5 * 60 * 1000);
            }
            // }
            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(99, "Feedly", "Error", "Feedly Miner Issue in \"SearchThread\"");
            //    Util.Log.WriteException(this.GetType(), "SearchThread FEEDLY", ex);
            //}
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
