using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Insight360.BO;
using Insight360.Util;
using System.Configuration;
using System.Globalization;
using Insight360.Common;

namespace Insight360.DataProcessor
{
    class QueueManager
    {
        static Dictionary<string, Queue<DocumentInfo>> orgQueues = new Dictionary<string, Queue<DocumentInfo>>();
        static Dictionary<string, bool> orgThreads = new Dictionary<string, bool>();
        static int totalQueuedCount;
        static int totalThreadsCount;
        static ManualResetEvent fetchEvent = new ManualResetEvent(true);

        static bool FetchOrganizationDocuments(string orgName)
        {
            lock (typeof(QueueManager))
            {
                if (orgQueues.Keys.Contains(orgName))
                    return orgQueues[orgName].Count < AppSettings.MinimumOrganizationQueued;
                else
                    return true;
            }
        }
        static bool FetchMore(Queue<DocumentInfo> queue)
        {
            return totalQueuedCount < AppSettings.MinimumTotalQueued;
            //&& (totalThreadsCount < Properties.Settings.Default.MinimumTotalThreads
            //|| queue.Count < Properties.Settings.Default.MinimumOrganizationQueued);
        }
        static void Enqueue(DocumentInfo document)
        {
            lock (typeof(QueueManager))
            {
                Queue<DocumentInfo> queue = null;
                if (orgQueues.Keys.Contains(document.OrganizationName))
                {
                    queue = orgQueues[document.OrganizationName];
                }
                else
                {
                    queue = new Queue<DocumentInfo>();
                    orgQueues.Add(document.OrganizationName, queue);
                }
                queue.Enqueue(document);
                totalQueuedCount++;
                if (!orgThreads.Keys.Contains(document.OrganizationName) || !orgThreads[document.OrganizationName])
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessDocument), document.OrganizationName);
                    totalThreadsCount++;
                    orgThreads[document.OrganizationName] = true;
                }
                if (!FetchMore(queue))
                    fetchEvent.Reset();
            }
        }

        static DocumentInfo Dequeue(string orgName)
        {
            lock (typeof(QueueManager))
            {
                var queue = orgQueues[orgName];
                DocumentInfo document = null;
                if (queue.Count > 0)
                    document = orgQueues[orgName].Dequeue();
                if (document != null)
                    totalQueuedCount--;
                if (FetchMore(queue))
                    fetchEvent.Set();
                Util.Log.WriteOperationCompleted(typeof(QueueManager), "Dequeue",
                    "Queued Items: " + totalQueuedCount + " Active Threads Count: " + totalThreadsCount + " Fetch State: " + fetchEvent.WaitOne(1));
                return document;
            }
        }

        static void ThreadExiting(string orgName)
        {
            lock (typeof(QueueManager))
            {
                totalThreadsCount--;
                orgThreads[orgName] = false;
            }
        }

        static void WrapUp()
        {
            bool allStopped = false;
            while (!allStopped)
            {
                Thread.Sleep(1000);
                allStopped = true;
                lock (typeof(QueueManager))
                {
                    foreach (var org in orgThreads.Keys)
                    {
                        if (orgThreads[org])
                        {
                            allStopped = false;
                            break;
                        }
                    }
                }
            }
            Util.Log.WriteOperationStarted(typeof(QueueManager), "WrapUp", "Clearing processing state");
            lock (typeof(QueueManager))
            {
                foreach (var org in orgThreads.Keys)
                {
                    DocumentInfo document = null;
                    while ((document = Dequeue(org)) != null)
                    {
                        List<DocumentInfo> list = new List<DocumentInfo>() { document };
                        DAL.StagingDAL.UpdateDocumentProcessingState(document.DocumentSourceViewName, list, DataProcessingState.UnProcessed);
                    }
                }
            }
            Util.Log.WriteOperationCompleted(typeof(QueueManager), "WrapUp", "Clearing processing state");


        }



        static void ProcessDocument(object data)
        {
            Util.Log.WriteOperationStarted(typeof(QueueManager), "ProcessDocument", "Thread Enter " + data);
            try
            {
                bool exit = DataProcessor.StopEvent.WaitOne(1);
                if (!exit)
                {
                    var document = Dequeue(data as string);
                    while (document != null && !exit)
                    {
                        try
                        {
                            if (DocumentIndex.Persist(document, false, null, null))
                            {
                                List<DocumentInfo> list = new List<DocumentInfo>() { document };
                                DAL.StagingDAL.UpdateDocumentProcessingState(document.DocumentSourceViewName, list, DataProcessingState.Processed);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Log.WriteException(typeof(QueueManager), "ProcessDocument", ex);
                        }
                        document = Dequeue(data as string);
                        exit = DataProcessor.StopEvent.WaitOne(1);
                    }
                }
            }

            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(QueueManager), "ProcessDocument", ex);
            }
            finally
            {
                ThreadExiting(data as string);
                Util.Log.WriteOperationCompleted(typeof(QueueManager), "ProcessDocument", "Thread Exit " + data);
            }
        }




        public static void FetchThread(object data)
        {
            DocumentSourceTypeIndexingInfo documentSourceType = data as DocumentSourceTypeIndexingInfo;
            Util.Log.WriteOperationStarted(typeof(QueueManager), "FetchThread", "Thread Enter ");
            try
            {
                List<DocumentSourceTypeIndexingInfo> documentSourceTypes = null;
                if (data == null)
                    documentSourceTypes = DAL.ConfigDAL.FetchDocumentSourceType(null);

                bool exit = DataProcessor.StopEvent.WaitOne(1);

                while (!exit)
                {
                    int count = 0;
                    if (data == null)
                    {
                        foreach (var source in documentSourceTypes)
                        {
                            if (source.Enabled)
                            {
                                count += Fetch(documentSourceType);
                            }

                            exit = DataProcessor.StopEvent.WaitOne(1);
                            if (exit)
                                break;
                        }
                    }
                    else
                        count = Fetch(documentSourceType);


                    Util.Log.WriteOperationStarted(typeof(QueueManager), "FetchThread", "..................Wait " + count + " " + data != null ? data.ToString() : string.Empty);
                    exit = ManualResetEvent.WaitAny(new WaitHandle[] { DataProcessor.StopEvent, fetchEvent }) == 0;
                    exit = DataProcessor.StopEvent.WaitOne(count > 100 ? AppSettings.WaitInterval_DataFetched : AppSettings.DataProcessor_WaitInterval_NothingFetched);
                    Util.Log.WriteOperationStarted(typeof(QueueManager), "FetchThread", "..................Wait " + count + " " + data != null ? data.ToString() : string.Empty);
                }

            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(QueueManager), "FetchThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationCompleted(typeof(QueueManager), "FetchThread", "Thread Exiting ");
                WrapUp();
                Util.Log.WriteOperationCompleted(typeof(QueueManager), "FetchThread", "Thread Exit ");
            }
        }

        private static int Fetch(DocumentSourceTypeIndexingInfo documentSourceType)
        {
            int count = 0;
            var orgs = DAL.ConfigDAL.FetchOrganizations();

            Util.Log.WriteOperationStarted(typeof(QueueManager), "Fetch", "Fetching " + documentSourceType.ViewName);
            DateTime start = DateTime.Now;
            List<DocumentInfo> fetched = DAL.StagingDAL.FetchStagingDocuments(documentSourceType, DataProcessingState.UnProcessed, documentSourceType.WhereCondition, Util.DataProcessingState.Processing);

            foreach (var document in fetched)
                Enqueue(document);

            Util.Log.WriteOperationCompleted(typeof(QueueManager), "Fetch",
                "documents queued " + fetched.Count + " " + (int)((DateTime.Now - start).TotalMilliseconds) + " " + documentSourceType.ViewName);

            bool exit = DataProcessor.StopEvent.WaitOne(1);
            if (!exit && !documentSourceType.FindSearchTerms & fetched != null && fetched.Count == documentSourceType.TopCount)
            {
                foreach (var org in orgs)
                {
                    if (FetchOrganizationDocuments(org))
                    {
                        string whereCondition = documentSourceType.WhereCondition;
                        if (!string.IsNullOrEmpty(whereCondition))
                            whereCondition += " AND ";
                        whereCondition += "Staging_OrgName = '" + org + "'";
                        fetched = DAL.StagingDAL.FetchStagingDocuments(documentSourceType, DataProcessingState.UnProcessed, whereCondition, Util.DataProcessingState.Processing);

                        foreach (var document in fetched)
                        {
                            Enqueue(document);
                            count++;
                        }

                        Util.Log.WriteOperationCompleted(typeof(QueueManager), "Fetch",
                            "documents queued " + fetched.Count + " " + (int)((DateTime.Now - start).TotalMilliseconds) + " " + documentSourceType.ViewName + " " + org);
                    }
                    exit = DataProcessor.StopEvent.WaitOne(1);
                    if (exit)
                        break;
                }
            }
            Util.Log.WriteOperationCompleted(typeof(QueueManager), "Fetch",
                "Fetching " + documentSourceType.ViewName + " " + count);
            return count;
        }
    }
}
