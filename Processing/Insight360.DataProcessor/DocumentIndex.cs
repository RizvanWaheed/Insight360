using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Insight360.DAL;
using Insight360.BO;
using Insight360.Util;
using System.Data;
using System.Linq;
using System.Configuration;
using System.Globalization;
using Insight360.Common;
//using Insight360.DataProcessor.LanguageService;

namespace Insight360.DataProcessor
{
    public static class DocumentIndex
    {
        static List<string> Parse(DocumentInfo document)
        {
            List<string> terms = null;

            try
            {

                string filteredText = CommonFilters.FilterText(document.ContentText);

                //// Nabeel
                //if( !string.IsNullOrEmpty( document.AuthorName ) )
                //    filteredText += " " + document.AuthorName;
                /**********C # Language Service Block ******/
                //using (LanguageServiceClient languageClient = new LanguageServiceClient())
                //{
                //    var languageCode = languageClient.GetLanguage( filteredText );
                //    var parsed = filteredText.Split(' ');
                ////	if( parsed != null )
                //    {
                //        if( parsed.Length >0 )
                //        {
                //            terms = new List<string>( );
                //            for( int i = 0 ; i < parsed.Length ; i++ )
                //            {
                //                terms.Add( parsed[ i ] as string );
                //            }
                //        }
                //        document.Language = languageCode;// parsed.languageCode;
                //    }
                //}


                /************** JAva Language Block***************/
                //// Nabeel
                ////using (LanguageService.LangaugeDetectionClient languageClient = new Insight360.DataProcessor.LanguageService.LangaugeDetectionClient())
                ////{
                ////    var parsed = languageClient.parseAndTranslate(filteredText);
                ////    if (parsed != null)
                ////    {
                ////        if (parsed.tokens != null)
                ////        {
                ////            terms = new List<string>();
                ////            for (int i = 0; i < parsed.tokens.Length; i++)
                ////            {
                ////                terms.Add(parsed.tokens[i] as string);
                ////            }
                ////        }
                ////        document.Language = parsed.languageCode;
                ////    }
                ////}
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DocumentIndex), "Parse", ex);
            }

            return terms;
        }
        public static bool Persist(DocumentInfo document, bool onlyUpdateDocument, Dictionary<string, DataTable> PersistDocumentQueue, Dictionary<string, DataTable> PersistIndexQueue)
        {
            bool result = false;
            long sentimentTime = 0;
            long parseTime = 0;
            long indexTime = 0;
            long stuckTime = 0;
            long documentTime = 0;
            int termCount = 0;
            if (document != null)
            {
                Guid id = Guid.NewGuid();
                DateTime startTime = DateTime.Now;

                if (AppSettings.EnableSentiment && !onlyUpdateDocument && !AppSettings.EnableSentimentThread)
                {
                    document.SentimentScore = Sentiment.GetSentimentScore(document.ContentText);
                    sentimentTime = (long)(DateTime.Now - startTime).TotalMilliseconds;
                    Log.WriteOperationCompleted(typeof(DocumentIndex), "Persist", "SentimentAnalysis " + document.DocumentSourceViewName + " " + document.OrganizationName + " " + sentimentTime + " " + id);
                }


                if (!AppSettings.EnableSentiment || AppSettings.EnableSentimentThread || document.SentimentScore != null)
                {
                    bool webIndex = false;

                    startTime = DateTime.Now;
                    List<string> terms = Parse(document);
                    parseTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                    if (terms != null)
                    {
                        termCount = terms.Count;

                        startTime = DateTime.Now;
                        var guid = Guid.NewGuid().ToString();
                        long documentID = IndexDAL.PersistDocument(document, webIndex, PersistDocumentQueue, guid);
                        //String[] IDArray = result2.Split(',');
                        //String documentID = IDArray[0];
                        //String contentId = IDArray[1];
                        documentTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                        if (!onlyUpdateDocument)
                        {
                            startTime = DateTime.Now;
                            if (terms.Count > 0)
                                result = IndexDAL.PersistIndex(document, documentID, terms, webIndex, PersistIndexQueue, guid);
                            indexTime = (long)(DateTime.Now - startTime).TotalMilliseconds;
                        }
                    }

                    if (!result)
                        stuckTime = (long)(DateTime.Now - startTime).TotalMilliseconds;
                }

                //if (result)
                //    DAL.IndexDAL.PersistTimes(document.DocumentSourceViewName, document.StagingId, sentimentTime, documentTime, parseTime, indexTime, stuckTime);


                Log.WriteOperationCompleted(typeof(DocumentIndex), "Persist", "Indexing "
                    + document.DocumentSourceViewName
                    + " " + document.OrganizationName
                    + " " + indexTime + "(" + termCount + ") "
                    + documentTime + "(" + document.ContentText.Length + ") "
                    + parseTime
                    + " " + id);

            }
            return result;
        }


        public static void ProcessingThread(object obj)
        {
            DocumentSourceTypeIndexingInfo documentSourceType = obj as DocumentSourceTypeIndexingInfo;
            Util.Log.WriteOperationStarted(typeof(DocumentIndex), "ProcessingThread", "Thread Enter " + documentSourceType.ViewName);

            var PersistDocumentQueue = new Dictionary<string, DataTable>();

            var PersistIndexQueue = new Dictionary<string, DataTable>();

            try
            {


                bool exit = false;
                exit = DataProcessor.StopEvent.WaitOne(1);

                while (!exit)
                {
                    if (ProcessData(documentSourceType, DataProcessingState.UnProcessed, false, false, PersistDocumentQueue, PersistIndexQueue))
                        exit = DataProcessor.StopEvent.WaitOne(1);
                    else
                        exit = DataProcessor.StopEvent.WaitOne((int)AppSettings.DataProcessor_WaitInterval_NothingFetched.TotalMilliseconds);

                    //					FlushData( PersistDocumentQueue , PersistIndexQueue , new List<DocumentSourceTypeIndexingInfo> { documentSourceType } , DataPersistenceMode.SingleDataSource );
                }

                FlushData(PersistDocumentQueue, PersistIndexQueue, new List<DocumentSourceTypeIndexingInfo> { documentSourceType }, DataPersistenceMode.SingleDataSource);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DocumentIndex), "ProcessingThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationCompleted(typeof(DocumentIndex), "ProcessingThread", "Thread Exit " + documentSourceType.ViewName);
            }
        }

        public static void ProcessingThread_Single()
        {
            Util.Log.WriteOperationStarted(typeof(DocumentIndex), "ProcessingThread", "Thread Enter ");

            var PersistDocumentQueue = new Dictionary<string, DataTable>();

            var PersistIndexQueue = new Dictionary<string, DataTable>();

            try
            {
                List<DocumentSourceTypeIndexingInfo> documentSourceTypes = DAL.ConfigDAL.FetchDocumentSourceType(null);

                bool exit = false;
                exit = DataProcessor.StopEvent.WaitOne(1);
                var sourceNames = new List<DocumentSourceTypeIndexingInfo>();

                while (!exit)
                {
                    bool somethingProcessed = false;
                    for (int i = 0; i < documentSourceTypes.Count; i++)
                    {
                        if (documentSourceTypes[i].Enabled)
                        {
                            sourceNames.Add(documentSourceTypes[i]);
                            somethingProcessed |= ProcessData(documentSourceTypes[i], DataProcessingState.UnProcessed, false, true, PersistDocumentQueue, PersistIndexQueue);
                            exit = DataProcessor.StopEvent.WaitOne(1);

                        }
                        if (exit)
                            break;
                    }

                    if (!somethingProcessed)
                        exit = DataProcessor.StopEvent.WaitOne((int)AppSettings.DataProcessor_WaitInterval_NothingFetched.TotalMilliseconds);
                }

                FlushData(PersistDocumentQueue, PersistIndexQueue, sourceNames, DataPersistenceMode.MultipleDataSources);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DocumentIndex), "ProcessingThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationCompleted(typeof(DocumentIndex), "ProcessingThread", "Thread Exit ");
            }
        }

        public static bool ProcessData(DocumentSourceTypeIndexingInfo documentSourceType, DataProcessingState state, bool onlyUpdateDocument, bool flushRecords, Dictionary<string, DataTable> PersistDocumentQueue, Dictionary<string, DataTable> PersistIndexQueue)
        {
            Util.Log.WriteOperationStarted(typeof(DocumentIndex), "ProcessData", documentSourceType.ViewName + " Fetching " + state.ToString());
            var fetched = DAL.StagingDAL.FetchStagingDocuments(documentSourceType, state, !onlyUpdateDocument ? documentSourceType.WhereCondition : string.Empty, Util.DataProcessingState.Processed);
            Util.Log.WriteOperationCompleted(typeof(DocumentIndex), "ProcessData", documentSourceType.ViewName + " Fetched " + state.ToString() + " " + fetched.Count);

            foreach (DocumentInfo document in fetched)
            {
                if (!PersistDocumentQueue.ContainsKey(document.OrganizationName))
                {
                    PersistDocumentQueue[document.OrganizationName] = GetAddDocumentTVP();
                }

                if (!PersistIndexQueue.ContainsKey(document.OrganizationName))
                {
                    PersistIndexQueue[document.OrganizationName] = GetAddIndexTVP();
                }

                if (DocumentIndex.Persist(document, onlyUpdateDocument, PersistDocumentQueue, PersistIndexQueue))
                {
                    List<DocumentInfo> list = new List<DocumentInfo>() { document };
                    //    DAL.StagingDAL.UpdateDocumentProcessingState(documentSourceType.ViewName, list, DataProcessingState.Processed);
                }

            }

            if (flushRecords)
            {
                FlushData(PersistDocumentQueue, PersistIndexQueue, new List<DocumentSourceTypeIndexingInfo> { documentSourceType }, DataPersistenceMode.SingleDataSource);
            }
            Util.Log.WriteOperationCompleted(typeof(DocumentIndex), "ProcessData", documentSourceType.ViewName + " Processed " + state.ToString() + " " + fetched.Count);

            return fetched.Count > 0;
        }

        static DataTable GetAddIndexTVP()
        {
            var table = new DataTable("indexes");

            table.Columns.AddRange(new[]
            {
                new DataColumn( "Document_Id" ) ,
                new DataColumn( "Term" ) ,
                new DataColumn( "Position" ) ,
                new DataColumn( "Document_Time" ) ,
                new DataColumn( "Document_TermCount" ) ,
                new DataColumn( "DocumentSource_Id" ) ,
                new DataColumn( "DocumentSourceType_Id" ) ,
                new DataColumn( "TempIdentifier" ) ,
            });

            return table;
        }

        static DataTable GetAddDocumentTVP()
        {
            var table = new DataTable("documents");

            table.Columns.AddRange(new[]
            {
                new DataColumn( "DocumentSourceType_Id" , typeof(int) ) ,
                  new DataColumn( "DocumentSource_Id" , typeof(int)) ,
                new DataColumn( "Staging_Id" , typeof(long)) ,

                new DataColumn( "Content_Id" , typeof(string)) ,
                new DataColumn( "Content_UniqueId" , typeof(string)) ,
                new DataColumn( "Content_Uri" , typeof(string)) ,
                new DataColumn( "Content_Text", typeof(string) ) ,
                new DataColumn( "Followup" , typeof(bool)) ,
                new DataColumn( "Content_Time" , typeof(DateTime)) ,
                new DataColumn( "Author_UniqueId" , typeof(string)) ,
                new DataColumn( "Author_Id" , typeof(string)) ,
                new DataColumn( "Author_Name" , typeof(string)) ,
                new DataColumn( "Author_Uri" , typeof(string)) ,
                new DataColumn( "Author_Uri_Image" , typeof(string)) ,

                new DataColumn( "Sentiment_Score", typeof(short) ) ,
                new DataColumn( "Language" , typeof(string)) ,
                new DataColumn( "CommentsCount" , typeof(int)) ,
                new DataColumn( "LikesCount" , typeof(int)) ,
                new DataColumn( "UniqueCommentersCount" , typeof(int)) ,
                new DataColumn( "Parent_Staging_Id" , typeof(long)) ,
                new DataColumn( "Parent_Content_Id" , typeof(string)) ,
                new DataColumn( "Parent_Content_UniqueId" , typeof(string)) ,
                new DataColumn( "Parent_DocumentSource_Id" , typeof(int)) ,
                new DataColumn( "TempIdentifier" , typeof(string)) ,

                new DataColumn( "ViewsCount" , typeof(int)) ,
                new DataColumn( "SharesCount" , typeof(int)) ,
                new DataColumn( "Author_Age" , typeof(int)) ,
                new DataColumn( "Author_Gender" , typeof(string)) ,
                new DataColumn( "Author_Location" , typeof(string)) ,
                new DataColumn( "Author_TimeZone" , typeof(string)) ,
                new DataColumn( "Followers" , typeof(string)) ,
                new DataColumn( "Following" , typeof(string)) ,
                new DataColumn("Type",typeof(string)),
                new DataColumn("SearchTerm_Id" , typeof(string)) ,
                new DataColumn( "To_Author_UniqueId" , typeof(string)) ,
                new DataColumn( "To_Author_Id" , typeof(string)) ,
                new DataColumn( "To_Author_Name" , typeof(string)) ,
                new DataColumn( "To_Author_Uri" , typeof(string)) ,
                new DataColumn( "To_Author_Uri_Image" , typeof(string)) ,
                new DataColumn( "To_Author_Age" , typeof(int)) ,
                new DataColumn( "To_Author_Gender" , typeof(string)) ,
                new DataColumn( "To_Author_Location" , typeof(string)) ,
                new DataColumn( "To_Author_TimeZone" , typeof(string)) ,
                new DataColumn( "To_Followers" , typeof(string)) ,
                new DataColumn( "To_Following" , typeof(string)) ,

            });

            table.Columns[14].AllowDBNull =
                table.Columns[15].AllowDBNull =
                table.Columns[19].AllowDBNull =
                table.Columns[20].AllowDBNull =
                table.Columns[21].AllowDBNull =
                table.Columns[22].AllowDBNull =
                table.Columns["To_Author_Age"].AllowDBNull =
                table.Columns["To_Author_Gender"].AllowDBNull =
                table.Columns["To_Author_Location"].AllowDBNull =
                table.Columns["To_Author_TimeZone"].AllowDBNull =

                table.Columns["Author_Age"].AllowDBNull =
                table.Columns["Author_Gender"].AllowDBNull =
                table.Columns["Author_Location"].AllowDBNull =
                table.Columns["Author_TimeZone"].AllowDBNull =
                table.Columns["Type"].AllowDBNull =
                true;

            return table;
        }

        static void FlushData(Dictionary<string, DataTable> PersistDocumentQueue, Dictionary<string, DataTable> PersistIndexQueue, List<DocumentSourceTypeIndexingInfo> viewName, DataPersistenceMode persistenceMode)
        {
            var persist = false;

            if ((from record in PersistDocumentQueue.Keys
                 select PersistDocumentQueue[record].Rows.Count)
                 .Sum() >= AppSettings.DocumentPesistenceQueueLength)
                persist = true;

            if ((from record in PersistIndexQueue.Keys
                 select PersistIndexQueue[record].Rows.Count)
                .Sum() >= AppSettings.IndexPersistenceQueueLength)
                persist = true;

            //	if( persist )
            IndexDAL.PersistData(PersistDocumentQueue, PersistIndexQueue, viewName, persistenceMode);
        }

    }
}
