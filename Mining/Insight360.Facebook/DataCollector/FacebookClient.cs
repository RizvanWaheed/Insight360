using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Threading;
using Facebook;
using System.Configuration;
using Insight360.Common;
using System.Diagnostics;
using Insight360.BO;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace Insight360.Facebook
{
    class FacebookClient
    {
        ManualResetEvent stopEvent;

        public FacebookClient()
        {
        }

        public FacebookClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        // BO.SearchTermInfo config, 
        public bool FetchPostsAndPersist(BO.DocumentSourceInfo documentSource)
        {
            //below code commented on May 15, 2017
            //bool exit = false;
            //exit = stopEvent.WaitOne(1);

            //if (exit)
            //    return DateTime.Now;

            //try
            //{
            bool limitFB = true;
            Util.Log.WriteOperationStarted(this.GetType(), "FetchPostsAndPersist", "Miner started for Source: " + documentSource.DisplayName);

            string fbFields = "?fields=id,message,icon,link,name,type,targeting,full_picture,caption,description,created_time,updated_time,from,shares,attachments,comments.limit(0).summary(true), likes.limit(0).summary(true),place";
            DateTime maximumTime = documentSource.SearchTerms.Min(x => x.LastFetchTime);
            DateTime? untilTime = null;

            List<object> labels = new List<object>();
            Dictionary<int, List<JsonObject>> searchTermChannels = new Dictionary<int, List<JsonObject>>();
            Dictionary<JsonObject, List<int>> channels = new Dictionary<JsonObject, List<int>>();
            string accessToken = "";

            // Add All channels to single dictionary
            foreach (var item in documentSource.SearchTerms)
            {
                var result = Insight360.Sources.Facebook.DataCollection.GetAllChannels(documentSource, item, out accessToken);

                foreach (var channelObject in result)
                {
                    string channelKey = channelObject["id"].ToString();


                    if (!Insight360.DAL.ConfigDAL.isFBChannelExcluded(channelKey))
                    {
                        var jsonChannel = channels.Keys.Where(x => x["id"].ToString() == channelKey).SingleOrDefault();

                        if (jsonChannel != null)
                        {
                            channels[jsonChannel].Add(item.Id);
                        }
                        else
                        {
                            channels.Add(channelObject, new List<int>() { item.Id });
                        }
                    }

                }
            }

            Util.Log.WriteOperationStarted(this.GetType(), "FetchPostsAndPersist", "Total Channels to Crawl: " + channels.Count);
            DAL.StagingDAL.LogMiner(2, "Facebook", "Process", "Src: " + documentSource.DisplayName + ", KW: " + documentSource.SearchTerms[0].SearchTerm + ", Channels: " + channels.Count);
            bool IsGetComments = documentSource.ProfileType == ProfileTypes.FacebookChannel;

            int posts = 0;
            foreach (var channel in channels.Keys)
            {
                bool IsLocationIncluded = channel.Keys.Contains("location") && ((channel["location"] as JsonObject).Keys.Contains("city") || (channel["location"] as JsonObject).Keys.Contains("country"));

                if (string.IsNullOrEmpty(Convert.ToString(channel["id"].ToString())))
                    continue;

                string channelPath = channel["id"].ToString() + "/feed" + fbFields;

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                Dictionary<string, object> Pparams = new Dictionary<string, object>();

                parameters.Add("since", maximumTime.ToString("yyyy-MM-dd HH:mm:ss"));
                parameters.Add("limit", "50");  //parameters.Add("limit", "100"); //commented on Jan 15, 2017 for checking purpose...data crawling was taking long
                bool nextPageAvailable = true;

                Pparams = parameters;

                Stopwatch watch = Stopwatch.StartNew();

                //below loop is commented on Jan 15, 2017 for checking purpose..data crawling was taking long 
                //while (nextPageAvailable)
                //{
                parameters = Pparams;
                List<JsonObject> feeds = null;

                feeds = Insight360.Sources.Facebook.DataCollection.GetPagedItems(channelPath, accessToken, parameters, maximumTime, untilTime, out nextPageAvailable, out Pparams,out limitFB);

                var Posts = new List<BO.FacebookPost>();
                int feedCount = feeds.Count;

                if (feedCount > 0)
                {
                    for (int i = 0; i < feedCount; i++)
                    {
                        var fbMessage = feeds[i].Keys.Contains("message") ? feeds[i]["message"].ToString().ToLower() : "";
                        var fbPostDate = Insight360.Sources.Facebook.DataCollection.GetDateValue(feeds[i], "created_time").ToLocalTime();
                        var termWithinDateRange = documentSource.SearchTerms.Where(x => channels[channel].Contains(x.Id) && x.LastFetchTime < fbPostDate);

                        if (termWithinDateRange.Any(x => fbMessage.ToLower().Contains(x.SearchTerm.ToLower()) && search(x, fbMessage)))
                        {
                            //Adding Channel location so that it takes it to the database
                            if (IsLocationIncluded)
                            {
                                feeds[i].Add("location", channel["location"]);
                            }

                            var post = Insight360.Sources.Facebook.DataCollection.CreateLocalPost(feeds[i], accessToken, IsGetComments, maximumTime, out maximumTime);

                            if (IsGetComments && documentSource.FeedLabels.Count > 0)
                            {
                                foreach (var comment in post.Comments)
                                {
                                    comment.FeedLabels = documentSource.FeedLabels
                                        .Where(label => comment.Message.ToLower()
                                            .Contains(" " + label.LabelName.ToLower() + " "))
                                                .Select(x => x.LabelId).ToList();
                                }
                            }

                            foreach (var term in termWithinDateRange)
                            {
                                //below line was comented on Jan 11, 2017...it was useless
                                //if (fbMessage.ToLower().Contains(term.SearchTerm.ToLower()) && search(term, fbMessage))
                                //{
                                if (post != null)
                                {
                                    post.Staging.Followup = false; //(config.Type == Insight360.BO.SearchTermInfo.Types.FacebookEngagementUser);
                                                                   //post.Staging.OrgName = organizationName;
                                    post.Staging.DocumentSourceId = documentSource.DocumentSourceId;
                                    post.Staging.SearchTermId = term.Id;

                                    Posts.Add(post);
                                }
                            }
                        }
                    }
                }

                if (untilTime != null)
                    maximumTime = untilTime.Value > maximumTime ? untilTime.Value : maximumTime;

                if (Posts != null && Posts.Count > 0)
                {
                    posts += Posts.Count;
                    ThreadPool.QueueUserWorkItem(PersistPosts, new object[] { Posts, documentSource.DocumentSourceId, documentSource });
                }
            }
            // }

            foreach (var term in documentSource.SearchTerms)
            {
                term.LastFetchTime = maximumTime;
                DAL.ConfigDAL.UpdateDocumentSourceConfiguration(term, documentSource.DocumentSourceId);
            }

            DAL.ConfigDAL.UpdateLastFetchConfigId(2, documentSource.RowId);

            Util.Log.WriteOperationCompleted(this.GetType(), "FetchPostsAndPersist", "Process completed for Source " + documentSource.DisplayName);
            DAL.StagingDAL.LogMiner(2, "Facebook", "Process", "Row Id: " + documentSource.RowId + ", KW: " + documentSource.SearchTerms[0].SearchTerm + ", Src: " + documentSource.DisplayName + ", Posts: " + posts);
            //}

            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"FetchPostsAndPersist\" for keyword: " + documentSource.SearchTerms[0].SearchTerm);
            //    Util.Log.WriteException(this.GetType(), "FetchPostsAndPersist " + documentSource.DisplayName, ex);
            //}

            //return DateTime.Now;
            return limitFB;
        }

        public bool FetchPostsAndPersistOptimize(BO.DocumentSourceInfo documentSource,string accessToken)
        {
            bool limitFB = true;
            Util.Log.WriteOperationStarted(this.GetType(), "FetchPostsAndPersist", "Miner started for Source: " + documentSource.DisplayName);
            string fbFields = "?fields=id,message,icon,link,name,type,targeting,full_picture,caption,description,created_time,updated_time,from,shares,attachments,comments.limit(0).summary(true), likes.limit(0).summary(true),place";
            DateTime maximumTime = documentSource.SearchTerms.Min(x => x.LastFetchTime);
            DateTime? untilTime = null;
            List<object> labels = new List<object>();
            Dictionary<int, List<JsonObject>> searchTermChannels = new Dictionary<int, List<JsonObject>>();
            Dictionary<JsonObject, List<int>> channels = new Dictionary<JsonObject, List<int>>();
            // Add All channels to single dictionary
            foreach (var item in documentSource.SearchTerms)
            {
                var result = Insight360.Sources.Facebook.DataCollection.GetAllChannelsWithShareToken(documentSource, item, accessToken);
                foreach (var channelObject in result)
                {
                    string channelKey = channelObject["id"].ToString();
                    if (!Insight360.DAL.ConfigDAL.isFBChannelExcluded(channelKey))
                    {
                        var jsonChannel = channels.Keys.Where(x => x["id"].ToString() == channelKey).SingleOrDefault();
                        if (jsonChannel != null)
                        {
                            channels[jsonChannel].Add(item.Id);
                        }
                        else
                        {
                            channels.Add(channelObject, new List<int>() { item.Id });
                        }
                    }
                }
            }
            Util.Log.WriteOperationStarted(this.GetType(), "FetchPostsAndPersist", "Total Channels to Crawl: " + channels.Count);
            DAL.StagingDAL.LogMiner(2, "Facebook", "Process", "Src: " + documentSource.DisplayName + ", KW: " + documentSource.SearchTerms[0].SearchTerm + ", Channels: " + channels.Count);
            bool IsGetComments = documentSource.ProfileType == ProfileTypes.FacebookChannel;
            int posts = 0;
            foreach (var channel in channels.Keys)
            {
                bool IsLocationIncluded = channel.Keys.Contains("location") && ((channel["location"] as JsonObject).Keys.Contains("city") || (channel["location"] as JsonObject).Keys.Contains("country"));
                if (string.IsNullOrEmpty(Convert.ToString(channel["id"].ToString())))
                    continue;
                string channelPath = channel["id"].ToString() + "/feed" + fbFields;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                Dictionary<string, object> Pparams = new Dictionary<string, object>();
                parameters.Add("since", maximumTime.ToString("yyyy-MM-dd HH:mm:ss"));
                parameters.Add("limit", "50");  //parameters.Add("limit", "100"); //commented on Jan 15, 2017 for checking purpose...data crawling was taking long
                bool nextPageAvailable = true;
                Pparams = parameters;
                Stopwatch watch = Stopwatch.StartNew();
                parameters = Pparams;
                List<JsonObject> feeds = null;
                feeds = Insight360.Sources.Facebook.DataCollection.GetPagedItemsWithFBLimit(channelPath, accessToken, parameters, maximumTime, untilTime, documentSource.DisplayName, documentSource.SearchTerms[0].SearchTerm , out nextPageAvailable, out Pparams, out limitFB);
                var Posts = new List<BO.FacebookPost>();
                int feedCount = feeds.Count;
                if (feedCount > 0)
                {
                    for (int i = 0; i < feedCount; i++)
                    {
                        var fbMessage = feeds[i].Keys.Contains("message") ? feeds[i]["message"].ToString().ToLower() : "";
                        var fbPostDate = Insight360.Sources.Facebook.DataCollection.GetDateValue(feeds[i], "created_time").ToLocalTime();
                        var termWithinDateRange = documentSource.SearchTerms.Where(x => channels[channel].Contains(x.Id) && x.LastFetchTime < fbPostDate);
                        if (termWithinDateRange.Any(x => fbMessage.ToLower().Contains(x.SearchTerm.ToLower()) && search(x, fbMessage)))
                        {
                            //Adding Channel location so that it takes it to the database
                            if (IsLocationIncluded)
                            {
                                feeds[i].Add("location", channel["location"]);
                            }
                            var post = Insight360.Sources.Facebook.DataCollection.CreateLocalPost(feeds[i], accessToken, IsGetComments, maximumTime, out maximumTime);
                            if (IsGetComments && documentSource.FeedLabels.Count > 0)
                            {
                                foreach (var comment in post.Comments)
                                {
                                    comment.FeedLabels = documentSource.FeedLabels
                                        .Where(label => comment.Message.ToLower()
                                            .Contains(" " + label.LabelName.ToLower() + " "))
                                                .Select(x => x.LabelId).ToList();
                                }
                            }
                            foreach (var term in termWithinDateRange)
                            {
                                if (post != null)
                                {
                                    post.Staging.Followup = false;
                                    post.Staging.DocumentSourceId = documentSource.DocumentSourceId;
                                    post.Staging.SearchTermId = term.Id;
                                    Posts.Add(post);
                                }
                            }
                        }
                    }
                }
                if (untilTime != null)
                    maximumTime = untilTime.Value > maximumTime ? untilTime.Value : maximumTime;
                if (Posts != null && Posts.Count > 0)
                {
                    posts += Posts.Count;
                    ThreadPool.QueueUserWorkItem(PersistPosts, new object[] { Posts, documentSource.DocumentSourceId, documentSource });
                }
            }
            foreach (var term in documentSource.SearchTerms)
            {
                term.LastFetchTime = maximumTime;
                DAL.ConfigDAL.UpdateDocumentSourceConfiguration(term, documentSource.DocumentSourceId);
            }
            DAL.ConfigDAL.UpdateLastFetchConfigId(2, documentSource.RowId);
            Util.Log.WriteOperationCompleted(this.GetType(), "FetchPostsAndPersist", "Process completed for Source " + documentSource.DisplayName);
            DAL.StagingDAL.LogMiner(2, "Facebook", "Process", "Row Id: " + documentSource.RowId + ", KW: " + documentSource.SearchTerms[0].SearchTerm + ", Src: " + documentSource.DisplayName + ", Posts: " + posts);
            return limitFB;
        }
        private void PersistPosts(object param)
        {
            lock (this)
            {
                try
                {
                    List<BO.FacebookPost> posts = ((object[])param)[0] as List<BO.FacebookPost>;
                    int documentSourceId = (int)((object[])param)[1];

                    bool error = false;

                    foreach (BO.FacebookPost post in posts)
                    {
                        if (!DAL.StagingDAL.PersistFacebookPost(post, AppSettings.GetCommentsAndLikes, true, post.Staging.SearchTermId))
                        {
                            error = true;
                            break;
                        }
                    }

                    //if (!error)
                    //{
                    //    DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);

                    //    if (recurse)
                    //    {
                    //        if ((DateTime.Now - config.LastFetchTime).TotalHours > AppSettings.UntilHoursCount)
                    //            FetchPostsAndPersist(info);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"PersistPosts\"");
                    Util.Log.WriteException(this.GetType(), "PersistPosts", ex);
                }
            }
        }

        private void PersistPost(object param)
        {
            lock (this)
            {
                try
                {
                    BO.SearchTermInfo config = ((object[])param)[0] as BO.SearchTermInfo;
                    BO.FacebookPost post = ((object[])param)[1] as BO.FacebookPost;
                    int documentSourceId = (int)((object[])param)[2];

                    //int organizationId = (int)((object[])param)[3];
                    //string organizationName = (string)((object[])param)[4];

                    bool recurse = (bool)((object[])param)[3];
                    BO.DocumentSourceInfo info = ((object[])param)[4] as BO.DocumentSourceInfo;

                    Util.Log.WriteOperationStarted(this.GetType(), "PersistPosts", "Persisting Post " + post.Id + " searchterm=" + config.SearchTerm);
                    bool error = false;

                    if (!DAL.StagingDAL.PersistFacebookPost(post, AppSettings.GetCommentsAndLikes, true, config.Id))
                    {
                        error = true;
                    }

                    if (!error)
                    {
                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);

                        if (recurse)
                        {
                            if ((DateTime.Now - config.LastFetchTime).TotalHours > AppSettings.UntilHoursCount)
                                FetchPostsAndPersist(info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"PersistPost\"");
                    Util.Log.WriteException(this.GetType(), "PersistPost", ex);
                }
            }
        }

        #region Search Methods

        Directory createIndex(string fbMessage)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                var document = new Lucene.Net.Documents.Document();

                document.Add(new Lucene.Net.Documents.Field("Text", fbMessage.ToLower(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                writer.AddDocument(document);

                writer.Optimize();
                writer.Flush(true, true, true);
            }

            return directory;
        }

        bool search(SearchTermInfo searchTerm, string fbMessage)
        {
            try
            {
                List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(searchTerm.Id);
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

                if (termCount == 0) // no query to search on then its ok
                    return true;


                List<string> filteredTweets = new List<string>();

                Directory Index = createIndex(fbMessage);

                using (var reader = IndexReader.Open(Index, true))
                using (var searcher = new IndexSearcher(reader))
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Text", analyzer);

                    //queryParser.AllowLeadingWildcard = true;

                    var query = queryParser.Parse(searchQuery);

                    var collector = TopScoreDocCollector.Create(1000, true);

                    searcher.Search(query, collector);

                    var matches = collector.TopDocs().ScoreDocs;

                    return (matches.Count() > 0 ? true : false);
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(this.GetType(), "FetchPostsAndPersist LUCENE.NET " + searchTerm.SearchTerm, ex);
                return true;
            }
        }

        #endregion
    }
}
