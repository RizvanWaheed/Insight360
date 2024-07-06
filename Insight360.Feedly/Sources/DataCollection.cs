using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Insight360.Util;
using Insight360.FeedlySharp;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Insight360.Feedly.Sources
{
    public class DataCollection
    {
        public static List<BO.FeedlyItem> SearchFeeds(BO.SearchTermInfo config, BO.DocumentSourceInfo info, DateTime lastFetchTime, out DateTime maximumTime, Insight360.FeedlySharp.FeedlyClient client, FeedlySharp.Models.FeedlySearchFeed feed)
        {
            List<BO.FeedlyItem> items = new List<BO.FeedlyItem>();
            List<BO.FeedlyItem> filteredItems = new List<BO.FeedlyItem>();

            maximumTime = config.LastFetchTime;
            string continuation = null;

            List<FeedlySharp.Models.FeedlyEntry> itemsOriginal = new List<FeedlySharp.Models.FeedlyEntry>();
            do
            {
                FeedlySharp.Models.FeedlyStreamEntriesResponse entries = client.GetStreamEntries(feed.Id, ContentType.Feed, 100, FeedSorting.Oldest, null, lastFetchTime, continuation).Result;
                
                if (entries.Items != null)
                {
                    itemsOriginal.AddRange(entries.Items);

                    continuation = entries.Continuation;
                }
                else
                {
                    continuation = null;
                }
            } while (continuation != null && continuation != "");

            if(itemsOriginal.Count==0 && info.ProfileType == BO.ProfileTypes.FeedlyRss) // api issue
            {
                continuation = null;
                do
                {
                    FeedlySharp.Models.FeedlyStreamEntriesResponse entries = client.GetStreamEntries(feed.Id, ContentType.Feed, 100, FeedSorting.Newest, null, null, continuation).Result;

                    if (entries.Items != null)
                    {
                        itemsOriginal.AddRange(entries.Items);

                        continuation = entries.Continuation;
                    }
                    else
                    {
                        continuation = null;
                    }
                } while (continuation != null && continuation != "");

                //FeedlySharp.Models.FeedlyStreamEntriesResponse entries = client.GetStreamEntries(feed.Id, ContentType.Feed, 100, FeedSorting.Newest, null, null, null).Result;

                //if (entries.Items != null)
                //{
                //    itemsOriginal.AddRange(entries.Items);
                //}
            }

            foreach (FeedlySharp.Models.FeedlyEntry itemOriginal in itemsOriginal)
            {
                try
                {
                    if ((itemOriginal.Summary != null && Regex.IsMatch(itemOriginal.Summary.Content.ToLower(), "\\b"+ config.SearchTerm.ToLower() + "\\b", RegexOptions.IgnoreCase)) ||
                        (itemOriginal.Content != null && Regex.IsMatch(itemOriginal.Content.Content.ToLower(), "\\b" + config.SearchTerm.ToLower() + "\\b", RegexOptions.IgnoreCase)) ||
                        (itemOriginal.Title != null && Regex.IsMatch(itemOriginal.Title.ToLower(), "\\b" + config.SearchTerm.ToLower() + "\\b", RegexOptions.IgnoreCase)))
                        items.Add(MapFeedlyItems(feed, itemOriginal));

                    if (itemOriginal.PublishedDate != null && itemOriginal.PublishedDate > maximumTime)
                        maximumTime = itemOriginal.PublishedDate.Value;
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(99, "Feedly", "Error", "Feedly Miner Issue in \"MapFeedlyItems\"");
                    Util.Log.WriteException(typeof(DataCollection), "F1121 MapFeedlyItems() | SearchTerm: " + config.SearchTerm + " | DocumentSourceUrl: " + info.Url, ex);
                }
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
                if (items != null && items.Count > 0 && termCount > 0)
                    items = search(searchQuery, items);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "F1101 LUCENE.NET | SearchTerm: " + config.SearchTerm + " | DocumentSourceUrl: " + info.Url, ex);
            }
            // adi change lucene.net end

            filteredItems.AddRange(items);

            return filteredItems;
        }

        public static List<BO.FeedlyItem> SearchFeeds(BO.SearchTermInfo config, out DateTime maximumTime)
        {
            List<BO.FeedlyItem> items = new List<BO.FeedlyItem>();
            List<BO.FeedlyItem> filteredItems = new List<BO.FeedlyItem>();

            maximumTime = config.LastFetchTime;
            string continuation = null;

            Insight360.FeedlySharp.FeedlyClient client = new Insight360.FeedlySharp.FeedlyClient(CloudEnvironment.Production, "", "", "");
            List<FeedlySharp.Models.FeedlySearchFeed> feeds = client.FindFeeds(config.SearchTerm, 10000).Result;

            foreach (FeedlySharp.Models.FeedlySearchFeed feed in feeds)
            {
                List<FeedlySharp.Models.FeedlyEntry> itemsOriginal = new List<FeedlySharp.Models.FeedlyEntry>();
                do
                {
                    FeedlySharp.Models.FeedlyStreamEntriesResponse entries = client.GetStreamEntries(feed.Id, ContentType.Feed, 100, FeedSorting.Oldest, null, config.LastFetchTime, continuation).Result;

                    itemsOriginal.AddRange(entries.Items);

                    continuation = entries.Continuation;
                } while (continuation != null && continuation != "");

                foreach(FeedlySharp.Models.FeedlyEntry itemOriginal in itemsOriginal)
                {
                    try
                    {
                        if((itemOriginal.Summary!=null && itemOriginal.Summary.Content.ToLower().Contains(config.SearchTerm.ToLower())) || 
                            (itemOriginal.Content!=null && itemOriginal.Content.Content.ToLower().Contains(config.SearchTerm.ToLower())) || 
                            (itemOriginal.Title!=null && itemOriginal.Title.ToLower().Contains(config.SearchTerm.ToLower())))
                            items.Add(MapFeedlyItems(feed, itemOriginal));

                        if (itemOriginal.PublishedDate != null && itemOriginal.PublishedDate > maximumTime)
                            maximumTime = itemOriginal.PublishedDate.Value;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
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
                    if (items != null && items.Count > 0 && termCount > 0)
                        items = search(searchQuery, items);
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(99, "Feedly", "Error", "Feedly Miner Issue in \"SearchFeeds\"");
                    Util.Log.WriteException(typeof(DataCollection), "F1101 LUCENE.NET ", ex);
                }
                // adi change lucene.net end

                filteredItems.AddRange(items);
            }

            return filteredItems;
        }

        private static BO.FeedlyItem MapFeedlyItems(FeedlySharp.Models.FeedlySearchFeed feed, FeedlySharp.Models.FeedlyEntry item)
        {
            BO.FeedlyItem ret = new BO.FeedlyItem();

            ret.Id = item.Id;
            ret.FingerPrint = item.Fingerprint;
            ret.OriginId = item.OriginId;
            ret.Author = item.Author;
            ret.Title = item.Origin.Name;
            ret.BaseUri = item.Origin != null ? item.Origin.Uri.AbsoluteUri : "" ;
            ret.PublishDate = item.PublishedDate;
            ret.CrawledDate = item.CrawledDate;
            ret.UpdateDate = item.UpdateDate;
            ret.IsUnread = item.IsUnread;
            ret.Keywords = (item.Keywords!=null?string.Join(", ", item.Keywords):"");
            ret.Summary_Text = item.Summary!=null? StripHTML(item.Summary.Content):"";
            ret.Content_Text = item.Content!=null? StripHTML(item.Content.Content):"";
            ret.Sentiment = Helpers.SentimentScore(StripHTML(ret.Summary_Text + " " + ret.Content_Text));
            ret.LanguageCode = Helpers.DetectLanguage(StripHTML(ret.Summary_Text + " " + ret.Content_Text));

            if(item.AlternateLinks!=null)
                ret.Links.AddRange(MapFeedlyLinks(item.AlternateLinks));
            if (item.CanonicalLinks != null)
                ret.Links.AddRange(MapFeedlyLinks(item.CanonicalLinks));

            ret.Feed.FeedId = feed.Id;
            ret.Feed.Title = feed.Name;
            ret.Feed.CoverUrl = feed.CoverImage!=null? feed.CoverImage.AbsoluteUri:"";
            ret.Feed.Coverage = null;// feed.co;
            ret.Feed.DeliciousTags = null;
            ret.Feed.Description = feed.Description;
            ret.Feed.FacebookLikes = feed.FacebookLikes;
            ret.Feed.FacebookUserName = feed.FacebookName;
            ret.Feed.TwitterFollowers = feed.TwitterFollowers;
            ret.Feed.TwitterScreenName = feed.TwitterName;
            ret.Feed.Velocity = (decimal?)feed.Velocity;
            ret.Feed.VisualUrl = null;
            ret.Feed.Website = feed.Uri != null ? feed.Uri.AbsoluteUri:"";
            ret.Feed.Language = feed.Language;

            if (ret.Summary_Text == "")
                ret.Summary_Text = ret.Content_Text;

            //following two conditions are to check if originId is valid url. If not get alternate url.
            if (!IsUrlValid(ret.OriginId))
            {
                if (item.AlternateLinks != null && item.AlternateLinks.Count > 0)
                {
                    var u = item.AlternateLinks.Where(x => x.ContentType == "text/html").First();
                    if (u != null)
                        ret.OriginId = item.AlternateLinks.Where(x => x.ContentType == "text/html").First().Uri.ToString();
                }
            }
            if (!IsUrlValid(ret.OriginId))
            {
                if(item.CanonicalLinks!=null && item.CanonicalLinks.Count > 0)
                {
                    var u = item.CanonicalLinks.Where(x => x.ContentType == "text/html").First();
                    if(u!=null)
                        ret.OriginId = item.CanonicalLinks.Where(x => x.ContentType == "text/html").First().Uri.ToString();
                }
            }

            return ret;
        }

        private static bool IsUrlValid(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
              && (uriResult.Scheme == Uri.UriSchemeHttp
                  || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static List<BO.FeedlyItemLink> MapFeedlyLinks(List<FeedlySharp.Models.FeedlyLink> links)
        {
            List<BO.FeedlyItemLink> retList = new List<BO.FeedlyItemLink>();

            foreach (var link in links)
            {
                BO.FeedlyItemLink l = new BO.FeedlyItemLink();
                l.MediaType = link.ContentType;
                l.Uri = link.Uri !=null ? link.Uri.AbsoluteUri : "";

                retList.Add(l);
            }

            return retList;
        }

        private static string StripHTML(string HTMLText)
        {
            if (HTMLText == null || HTMLText == "")
                return "";
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase); return reg.Replace(HTMLText, "");
        }



        #region Search Methods

        private static Directory createIndex(List<BO.FeedlyItem> items)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (BO.FeedlyItem item in items)
                {
                    var document = new Lucene.Net.Documents.Document();

                    string title, summary, content = "";
                    title = (item.Title != null ? item.Title.ToLower() : "");
                    summary = (item.Summary_Text != null ? item.Summary_Text.ToLower() : "");
                    content = (item.Content_Text != null ? item.Content_Text.ToLower() : "");

                    document.Add(new Lucene.Net.Documents.Field("Text", title + " " + summary + " " + content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                    document.Add(new Lucene.Net.Documents.Field("Id", item.Id.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
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

        private static List<BO.FeedlyItem> search(string textSearch, List<BO.FeedlyItem> items)
        {
            List<BO.FeedlyItem> filteredItems = new List<BO.FeedlyItem>();

            Directory Index = createIndex(items);

            using (var reader = IndexReader.Open(Index, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Text", analyzer);

                   // queryParser.AllowLeadingWildcard = true;

                    var query = queryParser.Parse(textSearch);

                    var collector = TopScoreDocCollector.Create(1000, true);

                    searcher.Search(query, collector);

                    var matches = collector.TopDocs().ScoreDocs;

                    foreach (var item in matches)
                    {
                        var id = item.Doc;
                        var doc = searcher.Doc(id);

                        var selected = items.Where(x => x.Id.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredItems.Add(selected);
                    }
                }
            }

            return filteredItems;
        }

        #endregion

    }
}
