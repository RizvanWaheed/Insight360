using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Insight360.Common;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace Insight360.YouTube
{
    class YouTubeClient
    {
        private static int pageSize
        {
            get { return AppSettings.Youtube_PageSize; }
        }
        ManualResetEvent stopEvent;

        public YouTubeClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        public DateTime FetchVideoFeeds(BO.SearchTermInfo config, BO.DocumentSourceInfo info)
        {
            int documentSourceId = info.DocumentSourceId;

            bool exit = false;
            exit = stopEvent.WaitOne(1);
            if (exit)
                return config.LastFetchTime;
            try
            {
                DateTime maximumTime = config.LastFetchTime;
                List<BO.YouTubeFeed> Feeds = null;
                Util.Log.WriteOperationStarted(this.GetType(), "FetchVideoFeed",
                                     config.SearchTerm + "-" + config.Type + "-" + config.LastFetchTime);
                if (info.ProfileType == BO.ProfileTypes.YoutubeChannel)
                {
                    //https://www.youtube.com/channel/UCe3tPdpxTF7wdBG8uV4hbew
                    string channelName = "", userName = "";

                    if(info.Url.Contains("/channel/"))
                        channelName = info.Url.Replace("https://www.youtube.com/channel/","");
                    else if(info.Url.Contains("/user/"))
                        userName = info.Url.Replace("https://www.youtube.com/user/", "");

                    Feeds = Sources.DataCollection.SearchChannels(config, channelName, userName, config.LastFetchTime,  AppSettings.Youtube_GetComments);
                }
                else if (info.ProfileType == BO.ProfileTypes.YoutubeGlobal)
                { 
                    Feeds = Sources.DataCollection.SearchFeedsv3(config, config.LastFetchTime, out maximumTime, AppSettings.Youtube_GetComments); //adi change remove .AddYears(-1)
                }

                // adi change lucene.net
                List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(config.Id);
                string searchQuery = "Text:";
                int termCount = 0;
                foreach (var term in terms)
                {
                    if (termCount > 0)
                        searchQuery += " OR " + "(" + term.Name + ")";
                    else
                        searchQuery += "(" + term.Name + ")";
                    termCount++;
                }
                try
                {
                    if (Feeds != null && Feeds.Count > 0 && termCount > 0)
                        Feeds = search(searchQuery, Feeds);
                }
                catch (Exception ex) {
                    Util.Log.WriteException(this.GetType(), "Y1101 LUCENE.NET ", ex);
                }
                // adi change lucene.net end


                if (Feeds != null)
                {
                    //bool error = false;
                    foreach (BO.YouTubeFeed Feed in Feeds)
                    {
                        //Feed.Staging.OrgName = organizationName;
                        Feed.Staging.DocumentSourceId = documentSourceId;
                        Feed.Staging.SearchTermId = config.Id;
                    }

                    //if (!error)
                    {
                        config.LastFetchTime = maximumTime;
                        if (Feeds == null || Feeds.Count == 0)
                        {
                            DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);
                        }
                        else if (Feeds != null)
                        {
                            ThreadPool.QueueUserWorkItem(PersistFeeds,
                                new object[] { config, Feeds, documentSourceId, false });//untilTime!=null});
                            //Util.Log.WriteOperationStarted(this.GetType(), "FetchVideoFeeds",
                            //    "Queued Persisting Feed " + Feeds.Count + " searchterm=" + config.SearchTerm);
                        }
                    }

                    Util.Log.WriteOperationCompleted(this.GetType(), "FetchVideoFeeds",
                        config.SearchTerm + "-" + config.Type + "-" + Feeds.Count + "-" + config.LastFetchTime);
                }

                DAL.ConfigDAL.UpdateLastFetchConfigId(4, info.RowId);
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "Row Id: " + info.RowId + ", KW: " + info.SearchTerms[0].SearchTerm + ", Src: " + info.DisplayName + ", Posts: " + Feeds.Count);
            }

            catch (Google.GoogleApiException ex)
            {
                bool IsDailyLimitExceeded = ((ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden));

                if (IsDailyLimitExceeded)
                {
                    Common.AppSettings.GoogleQuota = 0;

                    DateTime pacificTimeNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
                    DateTime pacificNextMidnight = pacificTimeNow.Date.AddDays(1);
                    TimeSpan timeToWaitMidnight = pacificNextMidnight - pacificTimeNow;

                    string threadMessage = string.Format("Youtube Daily Limit Exceeded! Thread is sleeping for {0} Hours, {1} mins and {2} seconds.",
                        timeToWaitMidnight.Hours, timeToWaitMidnight.Minutes, timeToWaitMidnight.Seconds);

                    Util.Log.WriteException(this.GetType(), "Youtube Client " + threadMessage, ex);
                    Util.Log.WriteOperationStarted(this.GetType(), "Youtube Client", threadMessage);

                    stopEvent.WaitOne(timeToWaitMidnight);

                    DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Thread initiated again.");
                    Util.Log.WriteOperationStarted(this.GetType(), "Youtube Client", "Thread initiated again.");

                }
                else
                {
                    DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Miner issue in Youtube Client");
                    Util.Log.WriteException(this.GetType(), "Youtube Client Exception while fetching", ex);
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Miner issue in Y1001 FetchVideoFeeds");
                Util.Log.WriteException(this.GetType(), "Y1001 FetchVideoFeeds", ex);
            }


            return config.LastFetchTime;
        }

        private void PersistFeeds(object param)
        {
            lock (this)
            {
                try
                {
                    BO.SearchTermInfo config = ((object[])param)[0] as BO.SearchTermInfo;
                    List<BO.YouTubeFeed> Feeds = ((object[])param)[1] as List<BO.YouTubeFeed>;
                    int documentSourceId = (int)((object[])param)[2];
                    //int organizationId = (int)((object[])param)[3];
                    //string organizationName = (string)((object[])param)[4];
                    bool recurse = (bool)((object[])param)[3];

                    Util.Log.WriteOperationStarted(this.GetType(), "PersistFeeds", "Persisting Feeds " + Feeds.Count + " searchterm=" + config.SearchTerm);
                    bool error = false;
                    foreach (BO.YouTubeFeed feed in Feeds)
                    {

                        if (!DAL.YoutubeDbOperations.PersistYouTubeFeed(feed, true, config.Id, AppSettings.Youtube_GetComments))
                        {
                            error = true;
                            //   break;
                        }

                    }

                    if (!error)
                    {

                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);
                        if (recurse)
                        {
                            //if ((DateTime.Now - config.LastFetchTime).TotalHours > AppSettings.Youtube_MaxData)
                             //   FetchVideoFeeds(config, info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Miner issue in Y1002 PersistFeeds");
                    Util.Log.WriteException(this.GetType(), "Y1002 PersistFeeds", ex);
                }
            }
        }


        #region Search Methods

        Directory createIndex(List<BO.YouTubeFeed> videos)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (BO.YouTubeFeed video in videos)
                {
                    var document = new Document();

                    document.Add(new Field("Text", video.VideoTitle.ToLower() + " " + video.Description.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                    document.Add(new Field("Id", video.VideoID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    //document.Add(new Field("FullText",
                    //    string.Format("{0}", tweet.Text)
                    //    , Field.Store.YES, Field.Index.ANALYZED));
                    writer.AddDocument(document);
                }

                writer.Optimize();
                writer.Flush(true, true, true);
            }

            return directory;
        }

        List<BO.YouTubeFeed> search(string textSearch, List<BO.YouTubeFeed> videos)
        {
            List<BO.YouTubeFeed> filteredVideos = new List<BO.YouTubeFeed>();

            Directory Index = createIndex(videos);

            using (var reader = IndexReader.Open(Index, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Text", analyzer);

                    //queryParser.AllowLeadingWildcard = true;

                    var query = queryParser.Parse(textSearch);

                    var collector = TopScoreDocCollector.Create(1000, true);

                    searcher.Search(query, collector);

                    var matches = collector.TopDocs().ScoreDocs;

                    foreach (var item in matches)
                    {
                        var id = item.Doc;
                        var doc = searcher.Doc(id);

                        var selected = videos.Where(x => x.VideoID.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredVideos.Add(selected);
                    }
                }
            }

            return filteredVideos;
        }

        #endregion
    }
}
