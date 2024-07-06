using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Insight360.BO;
using Insight360.Util;
using System.Data;
using System.Configuration;
using Insight360.Common;

namespace Insight360.DataProcessor
{
    class DataProcessor
    {
        public static ManualResetEvent StopEvent { get; set; }
        Timer timer;
        List<Thread> threads = new List<Thread>();

        static DataProcessor()
        {
            StopEvent = new ManualResetEvent(false);
        }

        public void Start()
        {

            var enableProcessingThread = Convert.ToBoolean(AppSettings.EnableProcessingThread); //Properties.Settings.Default.EnableProcessingThread;
            var enableQueuedProcessing = Convert.ToBoolean(AppSettings.EnableQueuedProcessing);//Properties.Settings.Default.EnableQueuedProcessing;
            var enableSingleProcessingThread = Convert.ToBoolean(AppSettings.EnableSingleProcessingThread);//Properties.Settings.Default.EnableSingleProcessingThread;

            // debugging
            //enableQueuedProcessing = false;
            //enableSingleProcessingThread = true;
            // end debugging

            if (enableProcessingThread)
            {
                if (enableQueuedProcessing)
                {
                    if (!enableSingleProcessingThread)
                    {
                        List<DocumentSourceTypeIndexingInfo> documentSourceTypes = DAL.ConfigDAL.FetchDocumentSourceType(null);
                        for (int i = 0; i < documentSourceTypes.Count; i++)
                        {
                            if (documentSourceTypes[i].Enabled)
                            {
                                Thread thread = new Thread(new ParameterizedThreadStart(QueueManager.FetchThread));
                                thread.Name = "Thread for: " + documentSourceTypes[i].ViewName;
                                thread.Start(documentSourceTypes[i]);
                                threads.Add(thread);
                            }
                        }
                    }
                    else
                    {
                        threads.Add(new Thread(new ParameterizedThreadStart(QueueManager.FetchThread)));
                        threads[threads.Count - 1].Start(null);
                    }
                }
                else
                {

                    if (!enableSingleProcessingThread)
                    {
                        List<DocumentSourceTypeIndexingInfo> documentSourceTypes = DAL.ConfigDAL.FetchDocumentSourceType(null);
                        for (int i = 0; i < documentSourceTypes.Count; i++)
                        {
                            if (documentSourceTypes[i].Enabled)
                            {
                                Thread thread = new Thread(new ParameterizedThreadStart(DocumentIndex.ProcessingThread));
                                thread.Name = "Thread for: " + documentSourceTypes[i].ViewName;
                                thread.Start(documentSourceTypes[i]);
                                threads.Add(thread);
                            }
                        }
                    }
                    else
                    {
                        threads.Add(new Thread(new ThreadStart(DocumentIndex.ProcessingThread_Single)));
                        threads[threads.Count - 1].Start();
                    }
                }
            }
            if (Convert.ToBoolean(AppSettings.EnableSentimentThread))//Properties.Settings.Default.EnableSentimentThread
            {
                threads.Add(new Thread(new ThreadStart(Sentiment.SentimentThread)));
                threads[threads.Count - 1].Start();
            }

            timer = new Timer(_ =>
               {
                   try
                   {
                       timer.Change(Timeout.Infinite, Timeout.Infinite);
                       Insight360.DAL.DBUtil.WriteCachedConnections();
                   }
                   finally
                   {
                       timer.Change(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
                   }
               });

            //timer.Change( TimeSpan.FromMinutes( 10 ) , TimeSpan.FromMinutes( 10 ) );
        }

        public void Stop()
        {
            StopEvent.Set();

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

        }




        //void ProcessingThread_DocumentUpdate()
        //{
        //    Util.Log.WriteOperationStarted("Thread Enter ");

        //    bool exit = false;
        //    exit = stopEvent.WaitOne(1);

        //    while (!exit)
        //    {                
        //        List<DocumentSourceTypeInfo> documentSourceTypes = DAL.ConfigDAL.FetchDocumentSourceType(null);

        //        for (int i = 0; i < documentSourceTypes.Count; i++)
        //        {
        //            if (documentSourceTypes[i].IndexingEnabled)
        //            {
        //                if (ProcessData(documentSourceTypes[i].Type, DataProcessingState.UnProcessed_DocumentUpdate, true))
        //                    exit = stopEvent.WaitOne(1);
        //                else
        //                    exit = stopEvent.WaitOne((int)Properties.Settings.Default.WaitInterval_NothingFetched.TotalMilliseconds);
        //                if (exit)
        //                    break;
        //            }
        //        }

        //        exit = stopEvent.WaitOne(1);                
        //    }
        //    Util.Log.WriteOperationCompleted("Thread Exit ");
        //}










    }
}
