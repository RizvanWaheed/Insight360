using Insight360.Common;
using Insight360.FeedlySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insight360.Feedly
{
    public class FeedlyClient
    {
        ManualResetEvent stopEvent;

        public FeedlyClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        public DateTime FetchFeedlyFeeds(BO.SearchTermInfo config, BO.DocumentSourceInfo info)
        {
            int documentSourceId = info.DocumentSourceId;

            bool exit = false;
            exit = stopEvent.WaitOne(1);
            if (exit)
                return config.LastFetchTime;

            //try
            //{
            DateTime maximumTime = config.LastFetchTime;
            DateTime lastFetchTime = config.LastFetchTime;
            List<BO.FeedlyItem> Items = null;
            Util.Log.WriteOperationStarted(this.GetType(), "FetchFeedlyFeeds", config.SearchTerm + "-" + config.Type + "-" + config.LastFetchTime);

            Insight360.FeedlySharp.FeedlyClient client = new Insight360.FeedlySharp.FeedlyClient(CloudEnvironment.Production, "", "", "");
            List<FeedlySharp.Models.FeedlySearchFeed> feeds = null;
            FeedlySharp.Models.FeedlyFeed rssFeed = null;

            if (info.ProfileType != BO.ProfileTypes.FeedlyRss)
                feeds = client.FindFeeds(config.SearchTerm, 100).Result;  // feeds = client.FindFeeds(config.SearchTerm, 10000).Result;
            else
            {
                rssFeed = client.GetFeed("feed/" + info.Url).Result;
                feeds = new List<FeedlySharp.Models.FeedlySearchFeed>();
                feeds.Add(Map(rssFeed));
            }

            int feedCount = 0;
            if (feeds != null)
            {
                foreach (FeedlySharp.Models.FeedlySearchFeed feed in feeds)
                {
                    if (feed.Id == null)
                        continue;

                    Items = Sources.DataCollection.SearchFeeds(config, info, lastFetchTime, out maximumTime, client, feed);

                    if (Items != null)
                    {
                        foreach (BO.FeedlyItem Feed in Items)
                        {
                            //Feed.Staging_OrgName = organizationName;
                            Feed.Staging_DocumentSource_Id = documentSourceId;
                            Feed.Staging_SearchTerm_Id = config.Id;
                            feedCount++;
                        }

                        config.LastFetchTime = maximumTime;
                        if (Items == null || Items.Count == 0)
                        {
                            DAL.ConfigDAL.UpdateDocumentSourceConfiguration(config, documentSourceId);
                        }
                        else if (Items != null)
                        {
                            ThreadPool.QueueUserWorkItem(PersistFeeds, new object[] { config, Items, documentSourceId, false });
                            Util.Log.WriteOperationStarted(this.GetType(), "FetchFeedlyFeeds", "Queued Persisting Feed " + Items.Count + " searchterm=" + config.SearchTerm);
                        }

                        Util.Log.WriteOperationCompleted(this.GetType(), "FetchFeedlyFeeds", config.SearchTerm + "-" + config.Type + "-" + Items.Count + "-" + config.LastFetchTime);
                    }
                }
            }

            DAL.ConfigDAL.UpdateLastFetchConfigId(99, info.RowId);
            DAL.StagingDAL.LogMiner(99, "Feedly", "Processing", "Row Id: " + info.RowId + ", KW: " + config.SearchTerm + ", Src: " + info.DisplayName + ", Posts: " + feedCount);

            //}
            //catch (Exception ex)
            //{
            //    DAL.StagingDAL.LogMiner(99, "Feedly", "Error", "Feedly Miner Issue in \"FetchFeedlyFeeds\"");
            //    Util.Log.WriteException(this.GetType(), "F1001 FetchFeedlyFeeds | setting delay of 25min | SearchTerm: "+ config.SearchTerm +" | DocumentSourceUrl: " + info.Url, ex);
            //    stopEvent.WaitOne(1000 * 60 * 20);
            //}

            return config.LastFetchTime;
        }

        private void PersistFeeds(object param)
        {
            lock (this)
            {
                try
                {
                    BO.SearchTermInfo config = ((object[])param)[0] as BO.SearchTermInfo;
                    List<BO.FeedlyItem> Feeds = ((object[])param)[1] as List<BO.FeedlyItem>;
                    int documentSourceId = (int)((object[])param)[2];
                    bool recurse = (bool)((object[])param)[3];

                    Util.Log.WriteOperationStarted(this.GetType(), "PersistFeeds", "Persisting Feeds " + Feeds.Count + " searchterm=" + config.SearchTerm);
                    bool error = false;
                    foreach (BO.FeedlyItem feed in Feeds)
                    {
                        if (DAL.StagingDAL.PersistFeedlyItem(feed) == -1)
                        {
                            error = true;
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
                    DAL.StagingDAL.LogMiner(99, "Feedly", "Error", "Feedly Miner Issue in \"PersistFeeds\"");
                    Util.Log.WriteException(this.GetType(), "F1002 PersistFeeds", ex);
                }
            }
        }

        private FeedlySharp.Models.FeedlySearchFeed Map(FeedlySharp.Models.FeedlyFeed model)
        {
            FeedlySharp.Models.FeedlySearchFeed ret = new FeedlySharp.Models.FeedlySearchFeed();

            ret.CoverColor = model.CoverColor;
            ret.CoverImage = model.CoverImage;
            ret.Description = model.Description;
            ret.FacebookLikes = model.FacebookLikes;
            ret.FacebookName = model.FacebookName;
            ret.IconImage = model.IconImage;
            ret.Id = model.Id;
            ret.Image = model.Image;
            ret.Language = model.Language;
            ret.Name = model.Name;
            ret.Subscribers = model.Subscribers;
            ret.Topics = model.Topics;
            ret.TwitterFollowers = model.TwitterFollowers;
            ret.TwitterName = model.TwitterName;
            ret.Uri = model.Uri;
            ret.Velocity = model.Velocity;

            return ret;
        }

    }
}
