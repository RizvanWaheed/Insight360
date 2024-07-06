using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using Insight360.Common;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace Insight360.Blogger
{
    public class BloggerClient
    {
        ManualResetEvent stopEvent;


        public BloggerClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        public void FetchBlogPosts(BO.DocumentSourceInfo config)
        {
            bool exit = false;
            exit = stopEvent.WaitOne(1);
            //if (exit)
            //    return config.LastFetchTime;
            try
            {
                List<BO.BloggerPost> Posts = null;
                int documentSourceId = config.DocumentSourceId;
                Util.Log.WriteOperationStarted(this.GetType(), "FetchBloggPost", config.Url.ToString() + "-" + config.SearchTerms);

                //DateTime minUpdateDate = DateTime.Now;
                //foreach (var search in config.SearchTerms)
                //{
                //    if (search.LastFetchTime < minUpdateDate)
                //        minUpdateDate = search.LastFetchTime;
                //}
                //Posts = WebMiner.Sources.Blogger.DataCollection.GetPosts(config, Properties.Settings.Default.GetComments,
                //    Properties.Settings.Default.UserName, Properties.Settings.Default.Password,
                //    Properties.Settings.Default.AppName,
                //    minUpdateDate);
                //config.SearchTerms.First().LastFetchTime = config.SearchTerms.First().LastFetchTime.AddMonths(-1);
                Posts = DataCollection.GetPosts(config, AppSettings.GetComments, AppSettings.GetAPIKey);

                if (Posts != null)
                {
                    //bool error = false;
                    foreach (BO.BloggerPost post in Posts)
                    {
                        post.Staging.DocumentSourceId = documentSourceId;
                        // post.Staging.SearchTermId = config.Id;
                    }
                    {
                        //config.LastFetchTime = maxUpdateDate;
                        if (Posts == null || Posts.Count == 0)
                        {
                            //DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);
                        }
                        else if (Posts != null)
                        {
                            ThreadPool.QueueUserWorkItem(PersistFeeds, new object[] { config, Posts, documentSourceId, false });//
                            Util.Log.WriteOperationStarted(this.GetType(), "FetchBlogPosts", "Queued Persisting Feed " + Posts.Count);
                        }
                    }

                    Util.Log.WriteOperationCompleted(this.GetType(), "FetchBlogPosts", "-" + Posts.Count);
                }
            }

            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"FetchBlogPosts\"");
                Util.Log.WriteException(this.GetType(), "B1011 FetchBlogPosts", ex);
            }

            //return config.LastFetchTime;
        }

        private void PersistFeeds(object param)
        {
            lock (this)
            {
                try
                {
                    BO.DocumentSourceInfo config = ((object[])param)[0] as BO.DocumentSourceInfo;
                    List<BO.BloggerPost> Posts = ((object[])param)[1] as List<BO.BloggerPost>;
                    int documentSourceId = (int)((object[])param)[2];
                    //int organizationId = (int)((object[])param)[3];
                    //string organizationName = (string)((object[])param)[4];
                    bool recurse = (bool)((object[])param)[3];

                    Util.Log.WriteOperationStarted(this.GetType(), "PersistFeeds", "Persisting Feeds " + Posts.Count + " url=" + config.Url.ToString());
                    bool error = false;

                    foreach (BO.BloggerPost post in Posts)
                    {

                        if (!DAL.BloggerDbOperation.PersistBloggerFeed(post, true, AppSettings.GetComments))
                        {
                            error = true;
                            //   break;
                        }
                    }

                    if (!error)
                    {
                        foreach (var searchTerm in config.SearchTerms)
                        {
                            DAL.ConfigDAL.UpdateDocumentSourceConfiguration(searchTerm, config.DocumentSourceId);
                        }

                        if (recurse)
                        {
                            if (config.SearchTerms != null)
                                FetchBlogPosts(config);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"PersistFeeds\"");
                    Util.Log.WriteException(this.GetType(), "B1010 PersistFeeds", ex);
                }
            }
        }
    }
}
