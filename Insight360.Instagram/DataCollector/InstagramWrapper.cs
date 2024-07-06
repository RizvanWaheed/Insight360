using Insight360.Common.BO;
using Insight360.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Insight360.Instagram
{
    public static class InstagramWrapper
    {
        public static List<InstagramPost> GetInstagramPosts(string accessToken, BO.SearchTermInfo searchTerm, ref Pagination pagination, BO.DocumentSourceInfo info)
        {
            Util.Log.WriteOperationStarted(typeof(InstagramWrapper), "GetInstagramPosts", "Search Term : " + searchTerm.SearchTerm + "  Started.");
            List<InstagramPost> instagramPosts = new List<InstagramPost>();
            try
            {
                string queryUrl = "";

                if (pagination != null && !string.IsNullOrEmpty(pagination.next_url))
                {
                    queryUrl = pagination.next_url;
                }
                else
                {
                    string Url = "";
                    if (info.ProfileType == BO.ProfileTypes.InstagramUser)
                    {
                        Url = "https://api.instagram.com/v1/users/{0}/media/recent/?access_token={1}";
                        queryUrl = string.Format(Url, info.Domain, accessToken);
                    }
                    else
                    {
                        Url = "https://api.instagram.com/v1/tags/{0}/media/recent?access_token={1}";
                        queryUrl = string.Format(Url, Regex.Replace(searchTerm.SearchTerm, "[^0-9a-zA-Z]+", ""), accessToken);
                    }
                    
                    if(info.ProfileType == BO.ProfileTypes.InstagramUser && !string.IsNullOrEmpty(searchTerm.LastFetchId) && searchTerm.LastFetchId != "0") // for user
                    {
                        queryUrl = string.Concat(queryUrl, "&max_id=", searchTerm.LastFetchId);
                    }
                    else if (info.ProfileType != BO.ProfileTypes.InstagramUser && !string.IsNullOrEmpty(searchTerm.LastFetchId) && searchTerm.LastFetchId != "0") // for public search
                    {
                        queryUrl = string.Concat(queryUrl, "&max_tag_id=", searchTerm.LastFetchId);
                    }
                }
              
                string result = Helpers.GetRequest(queryUrl);
                RootObject rootObjects = JsonConvert.DeserializeObject<RootObject>(result);
                
                if (rootObjects != null && rootObjects.data != null)
                {
                    pagination = rootObjects.pagination;

                    foreach (InstagramPost post in rootObjects.data)
                    {
                        try
                        {
                            if (post.caption != null && post.caption.text.ToLower().Contains(searchTerm.SearchTerm.ToLower()))
                            {
                                if (post.caption == null)
                                {
                                    post.caption = new Caption();
                                }
                                var taskSentiment = Helpers.SentimentScoreAsync(post.caption == null ? "" : post.caption.text);
                                var taskLanguage = Helpers.DetectLanguageAsync(post.caption == null ? "" : post.caption.text);
                                //var taskLanguage = Helpers.DetectLanguage(post.caption == null ? "" : post.caption.text);
                                var taskgender = Helpers.DetectGenderAsync(post.user.full_name == null ? "" : post.user.full_name);

                                post.created_time_obj = UnixTimeStampToDateTime(Convert.ToDouble(post.created_time));

                                post.alltags = "";
                                foreach (string tag in post.tags)
                                {
                                    post.alltags += tag + ",";
                                }
                                if (post.alltags.Length > 1)
                                    post.alltags = post.alltags.Remove(post.alltags.Length - 1);

                                Task.WaitAll(taskSentiment, taskLanguage, taskgender); //Task.WaitAll(taskSentiment, taskgender); 

                                post.caption.sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                                post.caption.language =  taskLanguage.Result == null ? "na" : taskLanguage.Result.ToString();
                               // post.caption.language = taskLanguage == null ? "" : taskLanguage.ToString();
                                post.user.gender = taskgender.Result;
                                post.commentsData = GetComments(post.id, accessToken);

                                if (post.created_time_obj.Date >= searchTerm.LastFetchTime.Date && post.created_time_obj.Date <=  searchTerm.UntillFetchTime.Date)
                                {
                                    instagramPosts.Add(post);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner Issue in \"GetInstagramPosts\"");
                            Util.Log.WriteException(typeof(InstagramWrapper), "GetInstagramPosts Search Term : " + searchTerm.SearchTerm, ex);
                        }
                    }

                    instagramPosts = ProximitySearch(searchTerm, instagramPosts);
                }
                Util.Log.WriteOperationCompleted(typeof(InstagramWrapper), "GetInstagramPosts", "Search Term :" + searchTerm.SearchTerm + "Completed.");
                return instagramPosts;
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner Issue in \"GetInstagramPosts\"");
                Util.Log.WriteException(typeof(InstagramWrapper), "GetInstagramPosts", ex);
                return instagramPosts;
            }

        }

        public static CommentRootObject GetComments(string postid, string accessToken)
        {
            string comment_Url = "https://api.instagram.com/v1/media/{0}/comments?access_token={1}";
            string comment_queryUrl = string.Format(comment_Url, postid, accessToken);
            string comments = Helpers.GetRequest(comment_queryUrl);
            CommentRootObject comment_list = JsonConvert.DeserializeObject<CommentRootObject>(comments);

            return comment_list;
        }

        public static List<InstagramPost> GetInstagramPostsByLocation(string accessToken, BO.SearchTermInfo searchTerm, ref Pagination pagination, BO.Location location)
        {
            Util.Log.WriteOperationStarted(typeof(InstagramWrapper), "GetInstagramPosts", "Search Term " + searchTerm.SearchTerm);
            List<InstagramPost> instagramPosts = new List<InstagramPost>();

            try
            {

                string queryUrl = "";

                if (pagination != null && !string.IsNullOrEmpty(pagination.next_url))
                {
                    queryUrl = pagination.next_url;
                }
                else
                {
                    //https://api.instagram.com/v1/media/search?lat=48.858844&lng=2.294351&access_token=ACCESS-TOKEN
                    string Url = "https://api.instagram.com/v1/media/search?access_token={0}&lat={1}&lng={2}";

                    if (location != null)
                    {

                        queryUrl = string.Format(Url, accessToken, location.latitude, location.longitude);
                    }
                    else
                    {
                        queryUrl = string.Format(Url, searchTerm.SearchTerm, accessToken);
                    }

                    if (!string.IsNullOrEmpty(searchTerm.LastFetchId) && searchTerm.LastFetchId != "0")
                    {
                        queryUrl = string.Concat(queryUrl, "&next_max_id=", searchTerm.LastFetchId);
                    }
                }

                string result = Helpers.GetRequest(queryUrl);

                RootObject rootObjects = JsonConvert.DeserializeObject<RootObject>(result);

                if (rootObjects != null && rootObjects.data != null)
                {
                    pagination = rootObjects.pagination;

                    foreach (InstagramPost post in rootObjects.data)
                    {
                        if (post.caption != null && post.caption.text.ToLower().Contains(searchTerm.SearchTerm.ToLower())) //if (post.tags != null && post.tags.Contains(searchTerm.SearchTerm))
                        {
                            try
                            {

                                if (post.caption == null)
                                {
                                    post.caption = new Caption();
                                }
                                var taskSentiment = Helpers.SentimentScoreAsync(post.caption == null ? "" : post.caption.text);
                                var taskLanguage = Helpers.DetectLanguageAsync(post.caption == null ? "" : post.caption.text);
                                //var taskLanguage = Helpers.DetectLanguage(post.caption == null ? "" : post.caption.text);
                                var taskgender = Helpers.DetectGenderAsync(post.user.full_name == null ? "" : post.user.full_name);

                                post.created_time_obj = UnixTimeStampToDateTime(Convert.ToDouble(post.created_time));

                                post.alltags = "";
                                foreach (string tag in post.tags)
                                {
                                    post.alltags += tag + ",";
                                }

                                if (post.alltags.Length > 1)
                                    post.alltags = post.alltags.Remove(post.alltags.Length - 1);

                                Task.WaitAll(taskSentiment, taskLanguage, taskgender);  //Task.WaitAll(taskSentiment, taskgender);//

                                post.caption.sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                                post.caption.language = taskLanguage.Result == null ? "na" : taskLanguage.Result.ToString();
                               // post.caption.language = taskLanguage == null ? "" : taskLanguage.ToString();
                                post.user.gender = taskgender.Result;
                                post.commentsData = GetComments(post.id, accessToken);

                                instagramPosts.Add(post);
                            }
                            catch (Exception ex)
                            {
                                Util.Log.WriteException(typeof(InstagramWrapper), "GetInstagramPosts", ex);
                                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner issue in \"GetInstagramPosts\" for Keyword: " + searchTerm.SearchTerm);

                            }
                        }
                    }
                }

                Util.Log.WriteOperationCompleted(typeof(InstagramWrapper), "GetInstagramPosts", "Search Term :" + searchTerm.SearchTerm + "Completed.");
                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Processing", "Search Term :" + searchTerm.SearchTerm + "Completed.");

                return instagramPosts;
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner Issue in \"GetInstagramPosts\"");
                Util.Log.WriteException(typeof(InstagramWrapper), "GetInstagramPosts", ex);
                return instagramPosts;
            }

        }

        public static List<InstagramPost> GetInstagramUserPosts(string accessToken, BO.SearchTermInfo searchTerm, ref Pagination pagination, string userId)
        {
            Util.Log.WriteOperationStarted(typeof(InstagramWrapper), "GetInstagramUserPosts", "User : " + userId);
            List<InstagramPost> instagramPosts = new List<InstagramPost>();
            try
            {

                string queryUrl = "";

                if (pagination != null && !string.IsNullOrEmpty(pagination.next_url))
                {
                    queryUrl = pagination.next_url;
                }
                else
                {
                    //https://api.instagram.com/v1/users/{0}/media/recent?access_token={1}
                    //https://api.instagram.com/v1/users/1112909426/media/recent?access_token=3137705600.c68a6b2.0a1ffc41dd604eac8520db662eb954f4
                    string Url = "https://api.instagram.com/v1/users/{0}/media/recent?access_token={1}";

                    queryUrl = string.Format(Url, userId, accessToken);


                    if (!string.IsNullOrEmpty(searchTerm.LastFetchId) && searchTerm.LastFetchId != "0")
                    {
                        queryUrl = string.Concat(queryUrl, "&next_max_id=", searchTerm.LastFetchId);
                    }
                }

                string result = Helpers.GetRequest(queryUrl);

                RootObject rootObjects = JsonConvert.DeserializeObject<RootObject>(result);

                if (rootObjects != null && rootObjects.data != null)
                {
                    pagination = rootObjects.pagination;

                    foreach (InstagramPost post in rootObjects.data)
                    {
                        if (post.caption != null && post.caption.text.ToLower().Contains(searchTerm.SearchTerm.ToLower()))// if (post.tags != null && post.tags.Contains(searchTerm.SearchTerm))
                        {
                            try
                            {

                                if (post.caption == null)
                                {
                                    post.caption = new Caption();
                                }
                                var taskSentiment = Helpers.SentimentScoreAsync(post.caption == null ? "" : post.caption.text);
                                var taskLanguage = Helpers.DetectLanguageAsync(post.caption == null ? "" : post.caption.text);
                                //var taskLanguage = Helpers.DetectLanguage(post.caption == null ? "" : post.caption.text);
                                var taskgender = Helpers.DetectGenderAsync(post.user.full_name == null ? "" : post.user.full_name);

                                post.created_time_obj = UnixTimeStampToDateTime(Convert.ToDouble(post.created_time));

                                post.alltags = "";
                                foreach (string tag in post.tags)
                                {
                                    post.alltags += tag + ",";
                                }
                                if (post.alltags.Length > 1)
                                    post.alltags = post.alltags.Remove(post.alltags.Length - 1);

                                Task.WaitAll(taskSentiment, taskLanguage, taskgender); //Task.WaitAll(taskSentiment, taskgender); //

                                post.caption.sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                                post.caption.language = taskLanguage.Result == null ? "na" : taskLanguage.Result.ToString();
                                //post.caption.language = taskLanguage == null ? "" : taskLanguage.ToString();
                                post.user.gender = taskgender.Result;
                                post.commentsData = GetComments(post.id, accessToken);

                                instagramPosts.Add(post);
                            }
                            catch (Exception ex)
                            {
                                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner issue in \"GetInstagramUserPosts\"");
                                Util.Log.WriteException(typeof(InstagramWrapper), "GetInstagramUserPosts", ex);
                            }
                        }
                    }
                }

                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Processing", "GetInstagramUserPosts- User :" + userId + "Completed.");
                Util.Log.WriteOperationCompleted(typeof(InstagramWrapper), "GetInstagramUserPosts", "User :" + userId + "Completed.");
                return instagramPosts;
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "Miner issue in \"GetInstagramUserPosts\"");
                Util.Log.WriteException(typeof(InstagramWrapper), "GetInstagramUserPosts", ex);
                return instagramPosts;
            }

        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static double ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return (double)span.TotalSeconds;
        }

        #region Search Methods

        private static List<InstagramPost> ProximitySearch(BO.SearchTermInfo searchTerm, List<InstagramPost> posts)
        {
            // adi change lucene.net
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
            try
            {
                if (posts != null && posts.Count > 0 && termCount > 0)
                {
                    posts = search(searchQuery, posts);
                    return posts;
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Error", "I1101 LUCENE.NET ");
                Util.Log.WriteException(typeof(InstagramWrapper), "I1101 LUCENE.NET ", ex);
            }

            return posts;
            // adi change lucene.net end
        }

        private static Directory createIndex(List<InstagramPost> items)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (InstagramPost item in items)
                {
                    var document = new Lucene.Net.Documents.Document();

                    string text = "";
                    text = (item.caption != null ? item.caption.text.ToLower() : "");

                    document.Add(new Lucene.Net.Documents.Field("Text", text, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                    document.Add(new Lucene.Net.Documents.Field("Id", item.id.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
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

        private static List<InstagramPost> search(string textSearch, List<InstagramPost> items)
        {
            List<InstagramPost> filteredItems = new List<InstagramPost>();

            Directory Index = createIndex(items);

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

                        var selected = items.Where(x => x.id.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredItems.Add(selected);
                    }
                }
            }

            return filteredItems;
        }

        private static bool IsPostInDateRange(DateTime date, DateTime lastFetch, DateTime untill) {
            return date >= lastFetch && date < untill;
        }
        #endregion

    }
}
