using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Globalization;
using Insight360.Common;
namespace Insight360.Instagram
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
            Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Thread Enter Instagram");

            //try
            //{
            InstagramClient searchClient = new InstagramClient(stopEvent);
            bool exit = false;
            exit = stopEvent.WaitOne(1);

            while (!exit)
            {
                var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Instagram");
                var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("Instagram");
                Frequency = frequencyInMinutes * 60000;

                int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(1073741826);

                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Started", "Miner started for Instagram with " + (docSources.Count - lastRowId) + " search types");

                for (int i = lastRowId + 1; i <= docSources.Count; i++) // foreach (var source in docSources)
                {
                   // DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Processing", "Miner started for " + docSources[i - 1].DisplayName);

                    foreach (var search in docSources[i - 1].SearchTerms)
                    {
                        try
                        {
                            if (!exit)
                            {
                                List<BO.Location> locations = DAL.ConfigDAL.GetLocations(search.Id);
                                if (locations.Count > 0)
                                {
                                    foreach (BO.Location loc in locations)
                                    {
                                        dt = searchClient.FetchPostsAndPersist(search, docSources[i - 1], loc);
                                    }
                                }
                                else
                                {
                                    dt = searchClient.FetchPostsAndPersist(search, docSources[i - 1], null);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner Issue in \"SearchThread\" for KW: " + search.SearchTerm);
                        }

                        finally
                        {
                            DAL.ConfigDAL.UpdateLastFetchConfigId(1073741826, docSources[i - 1].RowId);
                        }

                    }

                    //DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Processing", "Mining completed for " + docSources[i - 1].DisplayName);
                }

                DAL.ConfigDAL.UpdateLastFetchConfigId(1073741826, 0);
                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Completed", "Miner Cycle completed. restarting in " + frequencyInMinutes + " mins.");
                //exit = stopEvent.WaitOne(Frequency);

                //if (docSources.Count == 0)
                //    exit = stopEvent.WaitOne(5 * 60 * 1000);

            }

            //}
            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner Issue in \"SearchThread\"");
            //}
        }
    }
}
