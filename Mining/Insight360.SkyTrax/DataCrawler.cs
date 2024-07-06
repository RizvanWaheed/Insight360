using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Insight360.SkyTrax.Sources;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Insight360.BO;

namespace Insight360.SkyTrax
{
    public class DataCrawler
    {
        ManualResetEvent stopEvent = new ManualResetEvent(false);
        List<Thread> threads = new List<Thread>();
        public void Start()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "Web", "Started");
            //threads.Add(new Thread(new ThreadStart(SearchThread)));
            threads.Add(new Thread(new ThreadStart(ScrappingThread)));

            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        public void ScrappingThread()
        {
            List<Thread> searchTermThreads = new List<Thread>();
            Util.Log.WriteOperationStarted(this.GetType(), "Scrapping Thread ", "Thread Enter");

            DateTime dt;

            //try
            //{
            List<Task> dawnTasks = new List<Task>();
            List<Task> googleSearchTasks = new List<Task>();

            bool exit = false;
            // exit = stopEvent.WaitOne(1);

            while (!exit)
            {
                var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Web");
                int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(1073741824);
                DAL.StagingDAL.LogMiner(1073741824, "Web", "Started", "Web Miner started with " + (docSources.Count - lastRowId) + " Search types");

                for (int i = lastRowId + 1; i <= docSources.Count; i++)//foreach (var source in docSources)
                {
                    switch (docSources[i - 1].ProfileType)
                    {
                        case ProfileTypes.WebDawn:

                            //var tasks = dawnTasks.Where(x => x.Status == TaskStatus.Running).ToList();

                            //if (tasks.Count == 0)
                            //{
                                try
                                {
                                    //foreach (var search in docSources[i - 1].SearchTerms)
                                    //{
                                    DataCollection dc = new DataCollection();
                                   dc.DawnCrawler(docSources[i - 1].SearchTerms[0], docSources[i - 1]);
                                    //dawnTasks.Add(Task.Factory.StartNew(() => dc.DawnCrawler(docSources[i - 1].SearchTerms[0], docSources[i - 1])));
                                    // }
                                }
                                catch (Exception ex)
                                {
                                    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web- Dawn Miner issue for KW: " + docSources[i - 1].SearchTerms[0].SearchTerm);
                                    Util.Log.WriteException(this.GetType(), "FetchPostsAndPersist " + docSources[i - 1].DisplayName, ex);
                                }
                                //finally
                                //{
                                //    DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, docSources[i - 1].RowId);
                                //}
                           // }

                            break;

                        case ProfileTypes.WebGoogleSearch:
                            try
                            {
                                //if (googleSearchTasks.Any(x => x.Status == TaskStatus.RanToCompletion))
                                //{
                                //    googleSearchTasks.Clear();
                                //}

                                //if (googleSearchTasks.Count == 0)
                                //{
                                GSearchClient client = new GSearchClient(stopEvent);
                                dt = client.GoogleSearchCrawler(docSources[i - 1]);
                                   // googleSearchTasks.Add(Task.Factory.StartNew(() => client.GoogleSearchCrawler(docSources[i - 1])));
                                   // }
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web- Google Miner issue for KW: " + docSources[i - 1].SearchTerms[0].SearchTerm);
                                Util.Log.WriteException(this.GetType(), "FetchPostsAndPersist " + docSources[i - 1].DisplayName, ex);

                                //DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, docSources[i - 1].RowId);
                            }
                            //finally
                            //{
                            //    DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, docSources[i - 1].RowId);
                            //}

                            break;
                    }
                }

                DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, 0);
                DAL.StagingDAL.LogMiner(1073741824, "Web", "Completed", "Web Miner Cycle completed");
                // exit = stopEvent.WaitOne(30000);
            }
            //}
            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner Miner Issue in \"SearchThread\"");
            //    Util.Log.WriteException(typeof(DataCrawler), "ScrappingThread", ex);
            //}
            //finally
            //{
            //    Util.Log.WriteOperationStarted(typeof(DataCrawler), "ScrappingThread", "Thread Exit");
            //}
        }

        public void SearchThread()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "Search Thread ", "Thread Enter");
            try
            {
                DataCollection dc = new DataCollection();

                bool exit = false;
                exit = stopEvent.WaitOne(1);
                while (!exit)
                {

                    DataTable skyTraxURL = DAL.StagingDAL.GetSkyTraxSearchTerm();

                    var URLlist = new List<string>();// DAL.ConfigDAL.FetchBaseURLs("SkyTrax");
                                                     //   URLlist = ConfigurationSettings.AppSettings["SkyTraxURLList"].ToString().Split(';').ToList();
                                                     //  URLlist.Add("http://www.airlinequality.com/Forum/emrts.htm");

                    foreach (DataRow row in skyTraxURL.Rows)
                    {
                        if (!exit)
                        {
                            List<Dictionary<string, string>> reviewList = dc.CrawlReviews(row["Term"].ToString());

                            if (reviewList.Count > 0)
                            {
                                UpdateReviewsinDB(reviewList, row["Id"].ToString());
                            }
                        }
                        // exit = stopEvent.WaitOne((int)Properties.Settings.Default.WaitInterval_ApiCalls.TotalMilliseconds);
                        exit = stopEvent.WaitOne(30 * 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCrawler), "SearchThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(typeof(DataCrawler), "SearchThread", "Thread Exit");
            }
        }

        public bool UpdateReviewsinDB(List<Dictionary<string, string>> reviewList, String searchTermId)
        {
            bool updateReviews = DAL.StagingDAL.UpdateReviewsinDB(reviewList, searchTermId);
            return updateReviews;
        }

        public void Stop()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "On Stop", "Thread Enter");
            stopEvent.Set();

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }
    }
}
