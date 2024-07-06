using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading;
using Insight360.BO;
using Insight360.Util;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Insight360.Common;

namespace Insight360.SkyTrax.Sources
{
    public class DataCollection
    {
        ManualResetEvent stopEvent = new ManualResetEvent(false);

        public HtmlDocument GetWebResponse(string url)
        {
            //Util.Log.WriteOperationStarted(typeof(DataCollection), "Get Web Response ", "URL to browse is " + url);
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "GET";
                WebResponse myResponse = myRequest.GetResponse();
                HtmlDocument doc = new HtmlDocument();
                doc.Load(myResponse.GetResponseStream());
                return doc;
            }
            //catch (WebException ex)
            //{
            //    if(ex.Status == WebExceptionStatus.)
            //    stopEvent.WaitOne(new TimeSpan(6, 0, 0));
            //    Util.Log.WriteException(typeof(DataCollection), "Get Web Response ", ex);
            //}
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "Get Web Response ", ex);
                stopEvent.WaitOne(new TimeSpan(6, 0, 0));
            }
            return new HtmlDocument();
        }
        public List<Dictionary<string, string>> CrawlReviews(string url)
        {
            List<Dictionary<string, string>> reviewList = new List<Dictionary<string, string>>();
            string baseurl = url.Substring(0, url.LastIndexOf("/") + 1);
            HtmlDocument basedoc = GetWebResponse(url);
            if (basedoc.DocumentNode.HasChildNodes)
            {
                List<HtmlDocument> listDoc = new List<HtmlDocument>();
                listDoc.Add(basedoc);

                var pagingAncr = basedoc.DocumentNode.Descendants().Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("menu1"));

                List<string> pagingUrlList = new List<string>();

                foreach (HtmlNode link in pagingAncr)
                {
                    string pageurl = link.GetAttributeValue("href", "");
                    if (pagingUrlList.IndexOf(pageurl) == -1)
                    {
                        pagingUrlList.Add(pageurl);
                    }
                }
                foreach (var pageurl in pagingUrlList)
                {
                    HtmlDocument doc = GetWebResponse(baseurl + pageurl);
                    listDoc.Add(doc);
                }
                int i = 0;
                foreach (var doc in listDoc)
                {
                    //  if (i > 0) break;
                    List<HtmlNode> reviewNodeList = doc.DocumentNode.Descendants().Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("airport")).ToList<HtmlNode>();

                    foreach (var node in reviewNodeList)
                    {
                        try
                        {
                            string[] arr = node.Descendants().FirstOrDefault().InnerText.Replace("nbsp", "").Split(';');
                            arr[1] = arr[1].Replace("by", "by;");
                            String[] tempArr = arr[1].Split(';');
                            String date = tempArr[0].Substring(0, tempArr[0].Length - 3).Trim();
                            date = DateTime.Parse(date).ToShortDateString();
                            String authorName = tempArr[1].Replace("&", "").Trim();
                            String location = arr.Last().Trim();
                            HtmlNode parentDiv = node.ParentNode.ParentNode;// return the parent review Table
                            var reviewText = parentDiv.Descendants().Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("text2")).SingleOrDefault().ParentNode.InnerText.Trim();
                            Dictionary<string, string> reviewObj = new Dictionary<string, string>();
                            reviewObj.Add("Text", reviewText);
                            reviewObj.Add("date", date);
                            reviewObj.Add("uri", url);
                            reviewObj.Add("authorName", authorName);
                            reviewObj.Add("location", location);
                            var kpiList = parentDiv.Descendants().Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("text25")).ToList();
                            foreach (var kpi in kpiList)
                            {
                                string key = kpi.ParentNode.InnerText.Trim();
                                if (key.ToLower().Contains("rating"))
                                    key = "Rating";
                                if (key.ToLower() != "business" && key.ToLower() != "economy" && key.ToLower() != "first")
                                {
                                    string value = kpi.ParentNode.NextSibling.InnerText.Trim();
                                    if (value == "")
                                    {
                                        value = Regex.Match(kpi.ParentNode.ParentNode.Descendants().Where(d => d.Attributes.Contains("src")).SingleOrDefault().Attributes["src"].Value, @"\d+").Value;
                                        if (value.Trim() == "")
                                        {
                                            value = kpi.ParentNode.ParentNode.Descendants().Where(d => d.Attributes.Contains("src")).SingleOrDefault().Attributes["src"].Value;

                                            value = (value.IndexOf("yes", StringComparison.OrdinalIgnoreCase) == -1) ? "False" : "True";
                                        }
                                    }
                                    reviewObj.Add(key, value);
                                }
                            }
                            reviewList.Add(reviewObj);
                            i++;

                        }
                        catch (Exception ex)
                        {
                            Util.Log.WriteException(typeof(DataCollection), "Crawl Data", ex);
                        }
                    }
                }

                return reviewList;
            }
            return new List<Dictionary<string, string>>();
        }

        public void DawnCrawler(BO.SearchTermInfo search, BO.DocumentSourceInfo source)
        {
            DAL.StagingDAL.LogMiner(1073741824, "Web", "Started", "Web Miner Dawn started for KW: " + search.SearchTerm);

            List<DawnNews> newsColl = new List<DawnNews>();
            DateTime currentDate = DateTime.Now;
            //List<WebCommentInfo> Comments = GetWebComments("http://www.dawn.com/news/1280792/k-electric-confirms-acquisition-talks", 123);  //http://www.dawn.com/news/1280792/k-electric-confirms-acquisition-talks

            //try
            //{
            while (search.LastFetchTime <= currentDate)
            {
                string url = source.Url + search.LastFetchTime.ToString("yyyy-MM-dd");
                HtmlDocument basedoc = GetWebResponse(url);

                var articles = (from a in basedoc.DocumentNode.Descendants("article").ToList()
                                where a.InnerText.ToLower().Contains(search.SearchTerm.ToLower())
                                select new DawnNews
                                {
                                    NewsId = Convert.ToInt32(a.Attributes["data-id"].Value),
                                    NewsTitle = a.ChildNodes["h2"].Descendants("a").Select(d => d.InnerText).FirstOrDefault(),
                                    NewsDetails = a.ChildNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Trim().Equals("story__excerpt")).SingleOrDefault().InnerText.Split('\n'),
                                    NewsCreatedAt = Convert.ToDateTime(a.ChildNodes.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Trim().Equals("timestamp--time  timeago")).FirstOrDefault().Attributes["title"].Value),
                                    NewsUrl = a.ChildNodes["h2"].Descendants("a").Where(d => d.Attributes.Contains("href")).FirstOrDefault().Attributes["href"].Value,
                                    NewsAuthor = "Dawn News",
                                    LikesCount = 0,
                                    DocumentSourceId = source.DocumentSourceId,
                                    SearchTermId = search.Id,
                                }).Select(a =>
                                    {
                                        a.LanguageCode = Helpers.DetectLanguage(a.NewsTitle);
                                        a.CommentsCount = GetWebCommentCount(a.NewsUrl);
                                        a.Comments = (AppSettings.DawnNews_GetComments && a.CommentsCount > 0 ? GetWebComments(a.NewsUrl, a.NewsId) : null);  //http://www.dawn.com/news/1280792/k-electric-confirms-acquisition-talks
                                        a.Sentiment = Helpers.SentimentScore(a.NewsTitle);
                                        a.NewsDescription = a.NewsDetails[0].Trim();
                                        //a.NewsCreatedAt = Convert.ToDateTime(a.NewsDetails[1].Replace("Published", "").Replace("Updated", "").Trim());
                                        return a;
                                    }).ToList();

                // adi change lucene.net
                List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(search.Id);
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
                //commented on May 29, 2017
                //try
                //{
                //    if (articles != null && articles.Count > 0 && termCount > 0)
                //        articles = this.search(searchQuery, articles);
                //}
                //catch (Exception ex)
                //{
                //    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner Dawn Issue in \"Lucene.NET\"");
                //    Util.Log.WriteException(this.GetType(), "DawnCrawler LUCENE.NET " + search.SearchTerm, ex);
                //}
                // adi change lucene.net end


                Util.Log.WriteOperationStarted(this.GetType(), "FetchNews", "Dawn News " + search.LastFetchTime + " searchterm=" + search.SearchTerm);

                if (articles.Count > 0)
                {
                    ThreadPool.QueueUserWorkItem(PersistNews, new object[] { search, articles, source.DocumentSourceId, false, source });//untilTime!=null});
                    Util.Log.WriteOperationStarted(this.GetType(), "FetchNewsAndPersist",
                        "Queued Persisting News " + articles.Count + " searchterm=" + search.SearchTerm);
                }

                //if (search.LastFetchTime.Date < currentDate.Date)
                search.LastFetchTime = search.LastFetchTime.AddDays(1);
            }

            DAL.ConfigDAL.UpdateDocumentSourceConfiguration(search, source.DocumentSourceId);
            DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, source.RowId);
            DAL.StagingDAL.LogMiner(1073741824, "Web", "Processing", "Dawn News: Row Id: " + source.RowId + ", KW: " + search.SearchTerm);
            //}
            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner Dawn Issue " + search.SearchTerm);
            //    Util.Log.WriteException(this.GetType(), "DawnCrawler " + search.SearchTerm, ex);
            //}
        }

        private int GetWebCommentCount(string newsUrl)
        {
            HtmlDocument basedoc = GetWebResponse(newsUrl);
            string commentsText = basedoc.DocumentNode.Descendants("section").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Trim().Equals("comments")).
                Select(a => a.ChildNodes["h1"].InnerHtml).ToList().FirstOrDefault().ToString();
            string commentsCount = commentsText.Substring(commentsText.LastIndexOf('(') + 1, (commentsText.LastIndexOf(')') - 1) - commentsText.LastIndexOf('('));

            return Convert.ToInt32(commentsCount);
        }

        private List<WebCommentInfo> GetWebComments(string newsUrl, int newsID)
        {
            List<WebCommentInfo> webComments = new List<WebCommentInfo>();

            HtmlDocument basedoc = GetWebResponse(newsUrl);
            List<HtmlNode> scrapped_comments = basedoc.DocumentNode.Descendants("div").Where(b => b.Attributes.Contains("class")
                                    && b.Attributes["class"].Value.Trim().Contains("comment  pb-4")).Select(a => a.ChildNodes["div"]).ToList();

            foreach (var scrapped_comment in scrapped_comments)
            {
                //string comment_id = scrapped_comment.Descendants("div").Where(b => b.Attributes.Contains("id"))
                //                    .Select(a => a.Attributes["id"].Value).ToString();
                string comment_text = scrapped_comment.Descendants("div").Where(b => b.Attributes.Contains("class")
                                 && b.Attributes["class"].Value.Trim().Contains("comment__body")).Select(a => a.ChildNodes["p"].InnerHtml).ToList().FirstOrDefault().ToString();
                string comment_author = scrapped_comment.Descendants("span").Where(b => b.Attributes.Contains("class")
                                    && b.Attributes["class"].Value.Trim().Contains("comment__author")).Select(a => a.InnerHtml).ToList().FirstOrDefault().ToString();
                string comment_date = scrapped_comment.Descendants("span").Where(b => b.Attributes.Contains("class")
                                    && b.Attributes["class"].Value.Trim().Contains("comment__date")).Select(a => a.ChildNodes["span"].InnerHtml).ToList().FirstOrDefault().ToString();

                BO.WebCommentInfo comment = new Insight360.BO.WebCommentInfo()
                {
                    NewsId = newsID,
                    //  WebCommentID = comment_id,
                    Author = comment_author,
                    Content = comment_text,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    PublishedDate = comment_date,
                    // Language = Helpers.DetectLanguageAsync(comment_text).Result == null ? "na" : Helpers.DetectLanguageAsync(comment_text).Result.ToString(), //Helpers.DetectLanguage(comment_text), 
                    // commented purposely. taking long for large no of comments
                    Sentiment = Helpers.SentimentScoreAsync(comment_text).Result == null ? 3 : Convert.ToInt32(Helpers.SentimentScoreAsync(comment_text).Result.ToString())//Helpers.SentimentScore(comment_text)
                };

                webComments.Add(comment);
            }

            return webComments;
        }

        private void PersistNews(object param)
        {
            lock (this)
            {
                try
                {
                    BO.SearchTermInfo search = ((object[])param)[0] as BO.SearchTermInfo;
                    List<DawnNews> newsColl = ((object[])param)[1] as List<DawnNews>;
                    int documentSourceId = (int)((object[])param)[2];
                    //int organizationId = (int)((object[])param)[3];
                    //string organizationName = (string)((object[])param)[4];

                    //bool recurse = (bool)((object[])param)[3];
                    BO.DocumentSourceInfo source = ((object[])param)[4] as BO.DocumentSourceInfo;

                    Util.Log.WriteOperationStarted(this.GetType(), "PersistNews", "Persist News " + newsColl.Count + " searchterm=" + search.SearchTerm);
                    bool error = false;

                    foreach (var news in newsColl)
                    {
                        if (!DAL.StagingDAL.PersistDawnNews(news, search.Id, source.DocumentSourceId))
                        {
                            error = true;
                            break;
                        }
                    }

                    if (!error)
                    {
                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(search, documentSourceId);

                        //if (recurse)
                        //{
                        //    if ((DateTime.Now - config.LastFetchTime).TotalHours > AppSettings.UntilHoursCount)
                        //        FetchPostsAndPersist(config, info);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.WriteException(this.GetType(), "PersistPosts", ex);
                }
            }
        }



        #region Search Methods

        Lucene.Net.Store.Directory createIndex(List<DawnNews> posts)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (DawnNews post in posts)
                {
                    var document = new Document();

                    document.Add(new Field("Text", post.NewsTitle.ToLower() + " " + post.NewsDescription.ToLower() + " " + post.NewsDetails.ToString().ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                    document.Add(new Field("Id", post.NewsId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
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

        List<DawnNews> search(string textSearch, List<DawnNews> posts)
        {
            List<DawnNews> filteredPosts = new List<DawnNews>();

            Lucene.Net.Store.Directory Index = createIndex(posts);

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

                        var selected = posts.Where(x => x.NewsId.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredPosts.Add(selected);
                    }
                }
            }

            return filteredPosts;
        }

        #endregion
    }
}
