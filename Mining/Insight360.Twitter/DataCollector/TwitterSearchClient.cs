using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insight360.Common;
using Insight360.Util;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using System.Threading;
using Insight360.BO;

namespace Insight360.Twitter
{
    class TwitterSearchClient
    {
        private const int RESULTS_PER_PAGE = 100; //100;

        ManualResetEvent stopEvent;

        public TwitterSearchClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        public DateTime FetchAndPersist(BO.SearchTermInfo config, BO.DocumentSourceInfo source, BO.Location location, List<ExcludedDocumentSourceInfo> excludedHandels) /*, int organizationId,string organizationName*/
        {
            try
            {
                int documentSourceId = source.DocumentSourceId;
                Util.Log.WriteOperationStarted(typeof(TwitterSearchClient), "FetchAndPersist", /*organizationName + "-" + */config.SearchTerm + "-" + config.Type);

                if (source.ProfileType != Insight360.BO.ProfileTypes.TwitterHandle)
                {
                    // Tweet Count to use for paging in tweets
                    // To get one page of tweets from api and store it on db before second call
                    int TweetPages = 2;  //pages for each keyword:docsourceid association
                    string i = config.LastFetchId;
                    bool getHistory = long.Parse(config.LastFetchId) == 0;  //i == "0";

                    DAL.StagingDAL.LogMiner(1, "Twitter", "Processing", "Global Search started: KW: " + config.SearchTerm);

                    while (TweetPages > 0)
                    {
                        //Util.Log.WriteOperationStarted(typeof(TwitterSearchClient), "SearchTerm ", /*organizationName + "-" + */config.SearchTerm + "-" + config.Type);

                        long maximumId = 0, maximumId2 = 0;
                        DateTime maximumTime;
                        List<BO.TwitterStatus> allStatuses = Insight360.Sources.Twitter.DataCollection.SearchStatuses(
                            new BO.TwitterAuthentication
                            {
                                ConsumerKey = AppSettings.TwitterSearch_ConsumerKey,
                                ConsumerSecret = AppSettings.TwitterSearch_ConsumerSecret,
                                Token = AppSettings.TwitterSearch_Token,
                                TokenSecret = AppSettings.TwitterSearch_TokenSecret
                            },
                            config.Id,
                            config.SearchTerm,
                            long.Parse(config.LastFetchId),
                            long.Parse(config.LastFetchId2),
                            config.LastFetchTime,
                            out maximumId,
                            out maximumId2,
                            out maximumTime,
                            getHistory,
                            location,
                            excludedHandels); //, config.LastFetchTime

                        //Util.Log.WriteOperationStarted(typeof(TwitterSearchClient), "Status Fetched", /*organizationName + "-" + */config.SearchTerm + "-" + config.Type);

                        //TweetCount = allStatuses != null && allStatuses.Count > 1 ? allStatuses.Count : 0;
                        Util.Log.WriteOperationCompleted(this.GetType(), "Twitter Search Term", config.SearchTerm + "-" + config.Type + "-" + allStatuses.Count + "-" + maximumId);

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

                        List<BO.TwitterStatus> filteredStatus = new List<BO.TwitterStatus>();

                        try
                        {
                            if (allStatuses != null && allStatuses.Count > 0)
                            {
                                if (termCount > 0)
                                {
                                    filteredStatus = search(searchQuery, allStatuses);
                                }
                                else
                                {
                                    filteredStatus = allStatuses;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Log.WriteException(typeof(TwitterSearchClient), "FetchAndPersist LUCENE.NET ", ex);
                        }

                        Util.Log.WriteOperationCompleted(this.GetType(), "Twitter - Processing -  Search Term", config.SearchTerm + " brought " + filteredStatus.Count + " tweets");

                        if (filteredStatus != null && filteredStatus.Count > 0)
                        {
                            try
                            {
                                foreach (BO.TwitterStatus status in filteredStatus)
                                {
                                    try
                                    {
                                        // status.Sentiment = Helpers.SentimentScore(status.Text, "facebook");
                                        var taskSentiment = Helpers.SentimentScoreAsync(status.Text); //Helpers.SentimentScore(status.Text, "facebook");
                                        status.Sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Sentiment service");
                                        Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Sentiment service", ex);

                                        status.Sentiment = 3;
                                    }

                                    try
                                    {
                                        var taskgender = Helpers.DetectGenderAsync(status.UserDetails.Name);
                                        status.UserDetails.Gender = taskgender.Result;

                                        //if (status.UserDetails.Gender != "")
                                        // status.UserDetails.Gender = Helpers.DetectGender(status.UserDetails.Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Gender service");
                                        Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Gender service", ex);

                                        status.UserDetails.Gender = "unspecified";
                                    }

                                    try
                                    {
                                        var taskLang = Helpers.DetectLanguageAsync(status.Text);
                                        status.LanguageCode = taskLang.Result == null ? "na" : taskLang.Result.ToString();  //Helpers.DetectLanguage(status.Text);

                                    }
                                    catch (Exception ex)
                                    {
                                        DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Language service");
                                        Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Language service", ex);

                                        status.LanguageCode = "na";
                                    }

                                    if (source.FeedLabels.Count > 0)
                                    {
                                        status.FeedLabels = source.FeedLabels
                                                .Where(label => status.Text.ToLower()
                                                    .Contains(label.LabelName.ToLower()))
                                                        .Select(x => x.LabelId).ToList();
                                    }

                                    DAL.StagingDAL.PersistTwitterStatus(status, config.Id, false, true, documentSourceId);

                                }
                            }
                            catch (Exception ex)
                            {
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Error in Persist Twitter Status", ex);
                            }
                        }

                        config.LastFetchId = maximumId.ToString();
                        config.LastFetchId2 = maximumId2.ToString();
                        config.LastFetchTime = maximumTime;

                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);

                        DAL.StagingDAL.LogMiner(1, "Twitter", "Processing", "Row Id: " + source.RowId + ", KW: " + config.SearchTerm + ", Page " + TweetPages + ", Tweets:" + filteredStatus.Count);

                        // if(filteredStatus != null && filteredStatus.Count > 0)  //commnted on Apr 07, 2017
                        //{
                        //    config.LastFetchId = maximumId.ToString();
                        //    config.LastFetchId2 = maximumId2.ToString();
                        //    config.LastFetchTime = maximumTime;

                        //    DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);
                        //}

                        TweetPages--;
                    }
                }
                else
                {
                    //if (config.Type == Insight360.BO.SearchTermInfo.Types.TwitterUser
                    //|| config.Type == Insight360.BO.SearchTermInfo.Types.TwitterEngagementUser)

                    long maximumId = long.Parse(config.LastFetchId);
                    long maximumId2 = long.Parse(config.LastFetchId2);

                    if (maximumId == 0) maximumId = 1;
                    if (maximumId2 == 0) maximumId2 = 1;

                    DateTime maximumTime = config.LastFetchTime;
                    DateTime maximumTime2 = config.LastFetchTime;

                    List<BO.TwitterStatus> timeline = new List<BO.TwitterStatus>();


                    try
                    {
                        timeline = Insight360.Sources.Twitter.DataCollection.GetUserTimeline(
                                        new BO.TwitterAuthentication
                                        {
                                            ConsumerKey = AppSettings.TwitterMentions_ConsumerKey,
                                            ConsumerSecret = AppSettings.TwitterMentions_ConsumerSecret,
                                            Token = AppSettings.TwitterMentions_Token,
                                            TokenSecret = AppSettings.TwitterMentions_TokenSecret
                                        },
                                        config.SearchTerm.ToLowerInvariant(),
                                        source.Url,
                                        maximumId,
                                        out maximumId,
                                        out maximumTime);
                    }
                    catch (Exception ex)
                    {
                        //stopEvent.WaitOne(1000 * 60 * 15);
                    }

                    //timeline = timeline.Where(s => s.Text.ToLower().Contains(config.SearchTerm.ToLower())).ToList();

                    List<BO.TwitterStatus> mentions = null;
                    //if (config.Type == Insight360.BO.SearchTermInfo.Types.TwitterEngagementUser)
                    //{
                    //    mentions = Insight360.Sources.Twitter.DataCollection.GetUserMentions(
                    //        new BO.TwitterAuthentication
                    //        {
                    //            ConsumerKey = Properties.Settings.Default.TwitterMentions_ConsumerKey,
                    //            ConsumerSecret = Properties.Settings.Default.TwitterMentions_ConsumerSecret,
                    //            Token = Properties.Settings.Default.TwitterMentions_Token,
                    //            TokenSecret = Properties.Settings.Default.TwitterMentions_TokenSecret
                    //        },
                    //        maximumId2,
                    //        out maximumId2,
                    //        out maximumTime2);
                    //}                    


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
                    // adi change lucene.net


                    if (mentions != null)
                    {
                        // adi change lucene.net
                        List<BO.TwitterStatus> tempMentions = new List<BO.TwitterStatus>();
                        try
                        {
                            if (mentions != null && mentions.Count > 0 && termCount > 0)
                                tempMentions = search(searchQuery, mentions);
                        }
                        catch (Exception ex)
                        {
                            Util.Log.WriteException(typeof(TwitterSearchClient), "FetchAndPersist TIMELINE LUCENE.NET ", ex);
                        }
                        // adi change lucene.net end

                        foreach (BO.TwitterStatus status in tempMentions)
                        {
                            try
                            {
                                // status.Sentiment = Helpers.SentimentScore(status.Text, "facebook");
                                var taskSentiment = Helpers.SentimentScoreAsync(status.Text); //Helpers.SentimentScore(status.Text, "facebook");
                                status.Sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Sentiment service");
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Sentiment service", ex);

                                status.Sentiment = 3;
                            }

                            try
                            {
                                var taskgender = Helpers.DetectGenderAsync(status.UserDetails.Name);
                                status.UserDetails.Gender = taskgender.Result;

                                //if (status.UserDetails.Gender != "")
                                // status.UserDetails.Gender = Helpers.DetectGender(status.UserDetails.Name);
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Gender service");
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Gender service", ex);

                                status.UserDetails.Gender = "unspecified";
                            }

                            try
                            {
                                var taskLang = Helpers.DetectLanguageAsync(status.Text);
                                status.LanguageCode = taskLang.Result == null ? "na" : taskLang.Result.ToString(); //Helpers.DetectLanguage(status.Text);
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Language service");
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Language service", ex);

                                status.LanguageCode = "na";
                            }


                            if (source.FeedLabels.Count > 0)
                            {
                                status.FeedLabels = source.FeedLabels
                                        .Where(label => status.Text.ToLower()
                                            .Contains(label.LabelName.ToLower()))
                                                .Select(x => x.LabelId).ToList();
                            }

                            //status.Staging.OrgName = organizationName;
                            status.Staging.Followup = config.Type == Insight360.BO.SearchTermInfo.Types.TwitterEngagementUser;
                            DAL.StagingDAL.PersistTwitterStatus(status, config.Id, false, true, documentSourceId);

                        }
                    }

                    if (timeline != null)
                    {
                        // adi change lucene.net
                        List<BO.TwitterStatus> tempTimeline = new List<BO.TwitterStatus>();
                        try
                        {
                            if (timeline != null && timeline.Count > 0 && termCount > 0)
                                tempTimeline = search(searchQuery, timeline);
                        }
                        catch (Exception ex)
                        {
                            Util.Log.WriteException(typeof(TwitterSearchClient), "FetchAndPersist TIMELINE LUCENE.NET ", ex);
                        }
                        // adi change lucene.net end


                        DAL.StagingDAL.LogMiner(1, "Twitter", "Processing", "Timeline processing started: KW: " + config.SearchTerm + " , Profile: " + source.DisplayName);

                        foreach (BO.TwitterStatus status in tempTimeline)
                        {
                            try
                            {
                                // status.Sentiment = Helpers.SentimentScore(status.Text, "facebook");
                                var taskSentiment = Helpers.SentimentScoreAsync(status.Text); //Helpers.SentimentScore(status.Text, "facebook");
                                status.Sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());

                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Sentiment service");
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Sentiment service", ex);

                                status.Sentiment = 3;
                            }

                            try
                            {
                                var taskgender = Helpers.DetectGenderAsync(status.UserDetails.Name);
                                status.UserDetails.Gender = taskgender.Result;

                                //if (status.UserDetails.Gender != "")
                                // status.UserDetails.Gender = Helpers.DetectGender(status.UserDetails.Name);
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Gender service");
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Gender service", ex);

                                status.UserDetails.Gender = "unspecified";
                            }

                            try
                            {
                                var taskLang = Helpers.DetectLanguageAsync(status.Text);
                                status.LanguageCode = taskLang.Result == null ? "na" : taskLang.Result.ToString();  //Helpers.DetectLanguage(status.Text);
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Issue in Language service");
                                Util.Log.WriteException(typeof(TwitterSearchClient), "Issue in Language service", ex);

                                status.LanguageCode = "na";
                            }


                            if (source.FeedLabels.Count > 0)
                            {
                                status.FeedLabels = source.FeedLabels
                                        .Where(label => status.Text.ToLower()
                                            .Contains(label.LabelName.ToLower()))
                                                .Select(x => x.LabelId).ToList();
                            }

                            status.Staging.Followup = config.Type == Insight360.BO.SearchTermInfo.Types.TwitterEngagementUser;

                            DAL.StagingDAL.PersistTwitterStatus(status, config.Id, false, true, documentSourceId);

                        }
                    }

                    if ((timeline != null && timeline.Count > 0) || (mentions != null && mentions.Count > 0))
                    {
                        config.LastFetchId = maximumId.ToString();
                        config.LastFetchId2 = maximumId2.ToString();
                        config.LastFetchTime = maximumTime > maximumTime2 ? maximumTime : maximumTime2;

                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId/*, organizationId*/);
                    }

                    //Util.Log.WriteOperationCompleted(organizationName + "-" + config.SearchTerm + "-" + config.Type
                    //+ "-" + (timeline != null ? timeline.Count : 0) + "-" + (mentions != null ? mentions.Count : 0) + "-" + maximumId + "-" + maximumId2);

                    DAL.StagingDAL.LogMiner(1, "Twitter", "Processing", "Row Id: " + source.RowId + ", Profile: " + source.DisplayName + ", KW: " + config.SearchTerm + " , Tweets:" + timeline.Count);

                    Util.Log.WriteOperationStarted(typeof(TwitterSearchClient), "FetchAndPersist", /*organizationName + "-" + */config.SearchTerm + "-" + config.Type
                            + "-" + (timeline != null ? timeline.Count : 0) + "-" + (mentions != null ? mentions.Count : 0) + "-" + maximumId + "-" + maximumId2);
                }

                DAL.ConfigDAL.UpdateLastFetchConfigId(1, source.RowId);
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Twitter Miner Issue in \"FetchAndPersist\"");
                Util.Log.WriteException(typeof(TwitterSearchClient), "FetchAndPersist", ex);
            }

            return config.LastFetchTime;
        }


        #region Search Methods

        Directory createIndex(List<BO.TwitterStatus> tweets)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (BO.TwitterStatus tweet in tweets)
                {
                    var document = new Document();

                    document.Add(new Field("Text", tweet.Text.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                    document.Add(new Field("Id", tweet.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
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

        List<BO.TwitterStatus> search(string textSearch, List<BO.TwitterStatus> tweets)
        {
            List<BO.TwitterStatus> filteredTweets = new List<BO.TwitterStatus>();

            Directory Index = createIndex(tweets);

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

                        var selected = tweets.Where(x => x.Id.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredTweets.Add(selected);
                    }
                }
            }

            return filteredTweets;

        }

        #endregion


    }
}
