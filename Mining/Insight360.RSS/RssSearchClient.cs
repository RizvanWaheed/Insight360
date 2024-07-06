using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Net;
using System.IO;
using System.Xml;
using Insight360.Util;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace Insight360.RSS
{
    class RssSearchClient
    {
        public List<int> FindSearchTerms(string text, List<BO.SearchTermInfo> SearchTerms, DateTime itemTime)
        {
            List<int> foundSearchTermIDs = new List<int>();
            text = text.ToLower();
            foreach (var searchTerm in SearchTerms)
            {
                if (itemTime > searchTerm.LastFetchTime && text.ToLower().Contains(searchTerm.SearchTerm.ToLower()))
                {
                    //	searchTerm.LastFetchTime = itemTime;
                    foundSearchTermIDs.Add(searchTerm.Id);
                }
            }

            return foundSearchTermIDs;
        }

        public void FetchAndPersist(BO.DocumentSourceInfo config)
        {
            Util.Log.WriteOperationStarted(this.GetType(), "FetchAndPersist", config.Url);
            SyndicationFeedFormatter formatter = LoadFeed(config.Url);
            
            if (formatter != null)
            {
                int count = 0;

                foreach (var st in config.SearchTerms)
                {
                    // adi change lucene.net
                    List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(st.Id);
                    string searchQuery = "Text: (" + st.SearchTerm.ToLower() + ")";
                    int termCount = 0;

                    foreach (var term in terms)
                    {
                        if (termCount > 0)
                            searchQuery += " OR " + "(" + term.Name + ")";
                        else
                            searchQuery += "OR (" + term.Name + ")";

                        termCount++;
                    }

                    try
                    {
                        if (formatter.Feed.Items != null && formatter.Feed.Items.Count() > 0 && termCount > 0)
                            formatter.Feed.Items = search(searchQuery, formatter.Feed.Items.ToList());
                    }
                    catch (Exception ex)
                    {
                        Util.Log.WriteException(this.GetType(), "FetchAndPersist LUCENE.NET ", ex);
                    }
                }

                foreach (var item in formatter.Feed.Items)
                {
                    List<int> foundSearchTermIDs = new List<int>();

                    string searchText = (item.Title == null ? "" : item.Title.Text) + " " + (item.Summary == null ? "" : item.Summary.Text);
                    foundSearchTermIDs = FindSearchTerms(searchText, config.SearchTerms, item.PublishDate.UtcDateTime);

                    if (foundSearchTermIDs.Count > 0)
                    {
                        DAL.StagingDAL.PersistRssItem(item, /*org.Name,*/ config.DocumentSourceId, foundSearchTermIDs, Helpers.SentimentScore(item.Summary.Text), Helpers.DetectLanguage(item.Title.Text + " " + item.Summary.Text), formatter.Feed);
                        count++;
                    }
                }

                DAL.StagingDAL.LogMiner(64, "RSS", "Processing", "Search for Keyword: " + config.DisplayName + " brought " + count + " feeds");
                Util.Log.WriteOperationCompleted(this.GetType(), "FetchAndPersist", config.Url + "-" + count /*+ "-" + config.LastFetchTime*/);

                foreach (var searchTerm in config.SearchTerms)
                {
                    searchTerm.LastFetchTime = DateTime.Now;
                    DAL.ConfigDAL.UpdateDocumentSourceConfiguration(searchTerm, config.DocumentSourceId);
                }
            }
        }

        SyndicationFeedFormatter LoadFeed(string link)
        {
            HttpWebRequest httprequest = WebRequest.Create(link) as HttpWebRequest;
            //httprequest.Credentials = new NetworkCredential(username, password);

            HttpWebResponse httpresponse = null;
            try
            {
                httpresponse = httprequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                httpresponse = ex.Response as HttpWebResponse;
                Util.Log.WriteException(this.GetType(), "LoadFeed", ex);
                return null;
            }

            Stream stream = null;
            try
            {
                stream = httpresponse.GetResponseStream();
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(this.GetType(), "LoadFeed", ex);
            }

            SyndicationFeedFormatter formatter = null;

            try
            {
                XmlReader xmlreader = XmlReader.Create(stream, new XmlReaderSettings
                {
                    CheckCharacters = false
                });
                Rss20FeedFormatter rssformatter = new Rss20FeedFormatter();
                Atom10FeedFormatter atomformatter = new Atom10FeedFormatter();

                if (rssformatter.CanRead(xmlreader))
                {
                    formatter = rssformatter;
                    rssformatter.ReadFrom(xmlreader);
                }
                else if (atomformatter.CanRead(xmlreader))
                {
                    formatter = atomformatter;
                    atomformatter.ReadFrom(xmlreader);
                }
                xmlreader.Close();
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(this.GetType(), "LoadFeed", ex);
            }

            return formatter;
        }

        #region Search Methods

        Lucene.Net.Store.Directory createIndex(List<SyndicationItem> feeds)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(10000)))
            { // the writer and analyzer will popuplate the directory with documents

                foreach (SyndicationItem feed in feeds)
                {
                    var document = new Document();

                    document.Add(new Field("Text", (feed.Title == null ? "" : feed.Title.Text.ToLower()) + " " + (feed.Summary == null ? "" : feed.Summary.Text.ToLower()), Field.Store.YES, Field.Index.ANALYZED));
                    document.Add(new Field("Id", feed.Id, Field.Store.YES, Field.Index.NOT_ANALYZED));
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

        List<SyndicationItem> search(string textSearch, List<SyndicationItem> feeds)
        {
            List<SyndicationItem> filteredFeeds = new List<SyndicationItem>();

            Lucene.Net.Store.Directory Index = createIndex(feeds);

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

                        var selected = feeds.Where(x => x.Id.ToString() == doc.GetField("Id").StringValue).First();
                        if (selected != null)
                            filteredFeeds.Add(selected);
                    }
                }
            }

            return filteredFeeds;
        }

        #endregion
    }
}
