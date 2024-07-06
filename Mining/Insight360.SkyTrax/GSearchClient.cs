using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Requests;
using Google.Apis.Services;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Insight360.BO;
using Insight360.Common.BO;
using Insight360.Common;
using Insight360.Util;


namespace Insight360.SkyTrax
{
    public class GSearchClient
    {
        ManualResetEvent stopEvent;
        public GSearchClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        public DateTime GoogleSearchCrawler(BO.DocumentSourceInfo documentSource)
        {
            string searchEngineId = AppSettings.GoogleCustomSearch_SearchEngineId;   // "010719371917914901911:pxey413ioya";
            string ApiKey = AppSettings.GoogleCustomSearch_APIKey; // "AIzaSyCa_rF-txBCPihl6pGVAWsu8gA2rlmVllc";
            int googleMaxResults = AppSettings.GoogleCustomSearch_MaxAPIResults;

            //bool exit = false;
            //exit = stopEvent.WaitOne(1);

            //if (exit)
            //    return DateTime.Now;

            //try
            //{
            //Util.Log.WriteOperationStarted(this.GetType(), "Google Custom Search", "Miner started for Source: " + documentSource.DisplayName);
            //DAL.StagingDAL.LogMiner(1073741824, "Web", "Started", "Web Miner Google Search started");

            BaseClientService.Initializer initializer = new BaseClientService.Initializer();
            initializer.ApiKey = ApiKey;

            CustomsearchService service = new CustomsearchService(initializer);

            foreach (var searchTerm in documentSource.SearchTerms.OrderBy(x => x.LastFetchId))
            {
                long pageNum = 0, totalResult = 0;

                bool IsValidPageNumber = long.TryParse(searchTerm.LastFetchId, out pageNum);
                bool IsValidTotalResults = long.TryParse(searchTerm.LastFetchId2, out totalResult);

                if (pageNum > 0 && ((totalResult == 0 || pageNum * 10 > totalResult) || pageNum >= googleMaxResults))
                {
                    DAL.StagingDAL.LogMiner(1073741824, "Web", "Processing", "Google: Row Id: " + documentSource.RowId + ", KW: " + documentSource.SearchTerms[0].SearchTerm);
                    DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, documentSource.RowId);
                    continue;
                }

                string SearchQuery = searchTerm.SearchTerm;
                //List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(searchTerm.Id);
                GoogleProximity proximity = DAL.ConfigDAL.GetGoogleProximity(searchTerm.Id);

                //for (int i = 0; i < terms.Count; i++)
                //{
                //    SearchQuery = (i > 0) ? SearchQuery + " OR " + "(" + terms[i].Name + ")" : "(" + terms[i].Name + ")";
                //}

                CseResource.ListRequest listRequest = service.Cse.List(SearchQuery);
                listRequest.Cx = searchEngineId;

                listRequest.Cr = string.IsNullOrEmpty(proximity.CountryRestrict) ? null : proximity.CountryRestrict; //"countryPK";

                if (proximity.IsSiteSearch && !string.IsNullOrEmpty(proximity.SiteUrls))
                {
                    listRequest.SiteSearch = proximity.SiteUrls; //"Linkedin.com telenor.com.pk www.Wikipedia.com Easypaisa.com.pk pakvisit.com scribd.com jobz.pk";
                    listRequest.SiteSearchFilter = CseResource.ListRequest.SiteSearchFilterEnum.E;
                }

                listRequest.Gl = string.IsNullOrEmpty(proximity.Geolocation) ? null : proximity.Geolocation; // "pk";
                listRequest.ExactTerms = proximity.IsExactTerms && !string.IsNullOrEmpty(proximity.ExactTermString) ? proximity.ExactTermString : null; //"Telenor";
                listRequest.ExcludeTerms = proximity.IsExcludeTerms && !string.IsNullOrEmpty(proximity.ExcludeTermString) ? proximity.ExcludeTermString : null;
                listRequest.Googlehost = string.IsNullOrEmpty(proximity.GoogleHost) ? null : proximity.GoogleHost;

                listRequest.Start = pageNum > 0 ? pageNum * 10 : (long?)null;

                if (searchTerm.UntillFetchTime != DateTime.MinValue)
                {
                    string FromDate = searchTerm.LastFetchTime.ToString("yyyyMMdd");
                    string ToDate = searchTerm.UntillFetchTime.ToString("yyyyMMdd");

                    //"date-sdate:d:s,date-sdate:r:20100825:20100907";
                    listRequest.DateRestrict = "date-sdate:d:s,date-sdate:r:" + FromDate + ":" + ToDate;
                }

                while (true)
                {
                    try
                    {
                        Google.Apis.Customsearch.v1.Data.Search search = listRequest.Execute();
                        Common.AppSettings.GoogleQuota++;

                        DAL.StagingDAL.LogMiner(1073741824, "Web", "Info", "Google Quota " + Common.AppSettings.GoogleQuota + " out of 100");
                        Util.Log.WriteOperationStarted(this.GetType(), "Google Customer Search", "Google Quota " + Common.AppSettings.GoogleQuota + " out of 100");

                        List<GoogleSearch> searchList = new List<GoogleSearch>();

                        if (search != null && search.Items != null && search.Items.Count > 0)
                        {
                            foreach (Google.Apis.Customsearch.v1.Data.Result result in search.Items)
                            {
                                if (result.Snippet.ToLower().Contains(searchTerm.SearchTerm.ToLower()))
                                {
                                    GoogleSearch googleSearch = new GoogleSearch();

                                    googleSearch.Id = string.IsNullOrEmpty(result.CacheId) ? Guid.NewGuid().ToString() : result.CacheId;
                                    googleSearch.Title = result.Title;
                                    googleSearch.Snippet = result.Snippet;
                                    googleSearch.SearchTermId = searchTerm.Id;
                                    googleSearch.DocumentSourceId = documentSource.DocumentSourceId;
                                    googleSearch.DisplayLink = result.Link;
                                    googleSearch.Url = result.Link;
                                    googleSearch.FormattedUrl = result.HtmlFormattedUrl;
                                    googleSearch.CreatedAt = DateTime.Now;
                                    googleSearch.LanguageCode = Helpers.DetectLanguage(googleSearch.Snippet);
                                    googleSearch.Sentiment = Helpers.SentimentScore(googleSearch.Title);

                                    searchList.Add(googleSearch);
                                }
                            }
                        }

                        pageNum = pageNum + 1;
                        searchTerm.LastFetchId = pageNum.ToString();
                        searchTerm.LastFetchId2 = search.SearchInformation.TotalResults.ToString() ?? "0";

                        //commented on May 29, 2017
                        //List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(searchTerm.Id);
                        //string searchQuery = "Text:";
                        //int termCount = 0;

                        //foreach (var term in terms)
                        //{
                        //    if (termCount > 0)
                        //        searchQuery += " OR " + "(" + term.Name + ")";
                        //    else
                        //        searchQuery += "(" + term.Name + ")";
                        //    termCount++;
                        //}

                        //try
                        //{
                        //    if (searchList != null && searchList.Count > 0 && termCount > 0)
                        //        searchList = this.search(searchQuery, searchList);
                        //}
                        //catch (Exception ex)
                        //{
                        //    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner Google Issue in \"Lucene.NET\"");
                        //    Util.Log.WriteException(this.GetType(), "DawnCrawler LUCENE.NET " + searchTerm.SearchTerm, ex);
                        //}

                        if (searchList.Count > 0)
                        {
                            PersistSearch(new object[] { searchList, searchTerm, documentSource.DocumentSourceId });
                            //ThreadPool.QueueUserWorkItem(PersistSearch, new object[] { searchList, searchTerm, documentSource.DocumentSourceId });

                            //Util.Log.WriteOperationStarted(this.GetType(), "FetchNewsAndPersist",
                            //    "Queued Persisting News " + articles.Count + " searchterm=" + search.SearchTerm);
                        }

                        break;
                    }
                    catch (Google.GoogleApiException ex)
                    {
                        bool IsDailyLimitExceeded = ((ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)); // && (ex.Error.Message.Contains("Daily Limit Exceeded")

                        if (IsDailyLimitExceeded)
                        {
                            Common.AppSettings.GoogleQuota = 0;

                            //DateTime pacificTimeNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
                            //DateTime pacificNextMidnight = pacificTimeNow.Date.AddDays(1);
                            //TimeSpan timeToWaitMidnight = pacificNextMidnight - pacificTimeNow;

                            //string threadMessage = string.Format("Daily Limit Exceeded!",
                            //    timeToWaitMidnight.Hours, timeToWaitMidnight.Minutes, timeToWaitMidnight.Seconds); // Thread is sleeping for {0} Hours, {1} mins and {2} seconds.

                            Util.Log.WriteException(this.GetType(), "Google Custom Search Daily Limit Exceeded!", ex);
                           // Util.Log.WriteOperationStarted(this.GetType(), "Google Customer Search", threadMessage);
                            DAL.StagingDAL.LogMiner(1073741824, "Web", "Processing", "Web Miner GoogleSearch | Daily Limit Exceeded!");
                            break;

                           // stopEvent.WaitOne(timeToWaitMidnight);

                            //Util.Log.WriteOperationStarted(this.GetType(), "Google Custom Search", "Thread initiated again..");
                            //DAL.StagingDAL.LogMiner(1073741824, "Web", "Processing", "Web Miner GoogleSearch initiated again");
                        }
                        else
                        {
                            DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner GoogleSearch Issue in \"GoogleSearchCrawler\" KW: " + documentSource.SearchTerms[0].SearchTerm);
                            Util.Log.WriteException(this.GetType(), "Google Custom Search Exception while fetching", ex);
                        }

                    }
                    catch (Exception ex)
                    {
                        DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner GoogleSearch Issue in \"GoogleSearchCrawler\"");
                        Util.Log.WriteException(this.GetType(), "Google Custom Search Exception while fetching", ex);
                    }
                }
            }

            DAL.ConfigDAL.UpdateLastFetchConfigId(1073741824, documentSource.RowId);
            DAL.StagingDAL.LogMiner(1073741824, "Web", "Processing", "Google: Row Id: " + documentSource.RowId + ", KW: " + documentSource.SearchTerms[0].SearchTerm);

            //}
            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner GoogleSearch Issue in \"GoogleSearchCrawler\"");
            //    Util.Log.WriteException(this.GetType(), "SkyTrax - Google Custom Search - FetchSearchResultsAndPersist", ex);
            //}

            return DateTime.Now;
        }

        private void PersistSearch(object param)
        {
            lock (this)
            {
                try
                {
                    List<GoogleSearch> searchResults = ((object[])param)[0] as List<GoogleSearch>;
                    BO.SearchTermInfo searchTerm = ((object[])param)[1] as BO.SearchTermInfo;
                    int DocumentSourceId = Convert.ToInt32(((object[])param)[2]);

                    Util.Log.WriteOperationStarted(this.GetType(), "Persist Google Search", "Count: " + searchResults.Count);

                    bool error = false;

                    foreach (var search in searchResults)
                    {
                        if (!DAL.StagingDAL.PersistGoogleSearch(search))
                        {
                            error = true;
                            break;
                        }
                    }

                    if (!error)
                    {
                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(searchTerm, DocumentSourceId);
                    }
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(1073741824, "Web", "Error", "Web Miner GoogleSearch Issue in \"PersistPosts\"");
                    Util.Log.WriteException(this.GetType(), "PersistPosts", ex);
                }
            }
        }

        List<GoogleSearch> search(string textSearch, List<GoogleSearch> posts)
        {
            List<GoogleSearch> filteredPosts = new List<GoogleSearch>();

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

                        var selected = posts.Where(x => x.Id.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredPosts.Add(selected);
                    }
                }
            }

            return filteredPosts;
        }

        Lucene.Net.Store.Directory createIndex(List<GoogleSearch> posts)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (GoogleSearch post in posts)
                {
                    var document = new Document();

                    document.Add(new Field("Text", post.Title.ToLower() + " " + post.Snippet.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                    document.Add(new Field("Id", post.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    writer.AddDocument(document);
                }

                writer.Optimize();
                writer.Flush(true, true, true);
            }

            return directory;
        }

    }
}
