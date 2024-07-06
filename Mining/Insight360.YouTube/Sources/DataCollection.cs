using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.YouTube;
using Google.GData.YouTube;
using Google.GData.Client;
using System.Configuration;
using System.IO;
using Insight360.DAL;
using Insight360.BO;
using System.Data;
using System.Collections;
using Insight360.Util;
using System.Text.RegularExpressions;
using Insight360.Common;

namespace Insight360.YouTube.Sources
{
    public class DataCollection
    {
        #region Configuration Settings
        private static int pageSize
        {
            get { return AppSettings.Youtube_PageSize; }
        }

        //private int orgID
        //{
        //    get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["OrgID"].ToString()); }
        //}

        private static string prodkey
        {
            get { return AppSettings.Youtube_ProdKey; }
        }

        public static string ApiKey { get; set; }

        private static string AppName
        {
            get { return AppSettings.Youtube_AppName; }
        }
        private static int maxData
        {
            get { return AppSettings.Youtube_MaxData; }
        }

        private static string YoutubeUserName
        {
            get { return AppSettings.Youtube_UserName; }
        }

        private static string YoutubePassword
        {
            get { return AppSettings.Youtube_Password; }
        }

        private static string FetchTime
        {
            get { return AppSettings.Youtube_FetchTime; }
        }
        #endregion

        internal static BO.YouTubeFeed CreateFeed(Video obj)
        {

            BO.YouTubeFeed VideoFeed = new Insight360.BO.YouTubeFeed()
            {
                VideoTitle = obj.Title,
                VideoID = obj.VideoId,
                YoutubeURL = obj.WatchPage.AbsoluteUri,
                ViewCount = obj.ViewCount,
                Author = obj.Author,
                CommentCount = obj.CommmentCount,
                keywords = obj.Keywords,
                Description = obj.Description,
                Updated = obj.Updated,
                Published = obj.YouTubeEntry.Published.ToUniversalTime(),
                Language = Helpers.DetectLanguage(obj.Description), //obj.YouTubeEntry.Language,
                Staging_InsertTime = DateTime.Now,
                Staging_UpdateTime = DateTime.Now,
                Duration = obj.YouTubeEntry.Duration.Seconds,
                Comments = new List<Insight360.BO.YouTubeCommentInfo>(),
                User = new YouTubeUserInfo()
            };
            if (obj.YouTubeEntry.Rating != null)
            {
                VideoFeed.Rating = obj.YouTubeEntry.Rating.Average;
                VideoFeed.NumRaters = obj.YouTubeEntry.Rating.NumRaters;
            }
            if (obj.YouTubeEntry.Statistics != null)
                if (obj.YouTubeEntry.Statistics.FavoriteCount != null)
                    VideoFeed.FavoriteCount = obj.YouTubeEntry.Statistics.FavoriteCount;

            if (obj.YouTubeEntry.YtRating != null)
            {
                VideoFeed.Likes = obj.YouTubeEntry.YtRating.NumLikes;
                VideoFeed.DisLikes = obj.YouTubeEntry.YtRating.NumDislikes;
            }
            string thumbs = "";
            var count = 0;
            foreach (var thumb in obj.Thumbnails)
            {
                if (count == 0)
                    thumbs = thumbs + thumb.Url;
                else
                    thumbs = thumbs + "," + thumb.Url;
                count++;
            }
            VideoFeed.Thumbnails = thumbs;





            return VideoFeed;
        }
        internal static BO.YouTubeUserInfo CreateUser(ProfileEntry User)
        {
            if (User.UserName != null)
            {
                BO.YouTubeUserInfo UserInfo = new Insight360.BO.YouTubeUserInfo()
                {
                    Age = User.Age,
                    Company = User.Company,
                    Etag = User.Etag,
                    FeedUri = User.FeedUri,
                    FirstName = User.Firstname,
                    LastName = User.Lastname,
                    Gender = Helpers.DetectGenderAsync((User.Firstname != null ? User.Firstname : "") + " " + (User.Lastname != null ? User.Lastname : "")).Result,  //User.Gender
                    Hobbies = User.Hobbies,
                    Hometown = User.Hometown,
                    Language = User.Language,
                    Location = (User.Location != null ? User.Location : "Global"),  //User.Location,
                    Movies = User.Movies,
                    Music = User.Music,
                    Occupation = User.Occupation,
                    Relationship = User.Relationship,
                    School = User.School,
                    Username = User.UserName,

                };
                if (User.Statistics != null)
                {
                    UserInfo.SubscriberCount = User.Statistics.SubscriberCount;
                    UserInfo.ViewCount = User.Statistics.ViewCount;
                }
                return UserInfo;
            }
            else
            {
                YouTubeUserInfo UserInfo = new Insight360.BO.YouTubeUserInfo();
                return UserInfo;
            }


        }
        internal static BO.YouTubeCommentInfo CreateComments(Comment Comment)
        {

            BO.YouTubeCommentInfo CommentInfo = new Insight360.BO.YouTubeCommentInfo()
            {
                YouTubeCommentID = Comment.Id,
                Title = Comment.Title,
                Author = Comment.Author,
                Content = Comment.Content,
                Updated = Comment.Updated,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now

            };
            if (Comment.CommentEntry != null)
            {
                CommentInfo.Language = Comment.CommentEntry.Language;
                CommentInfo.Published = Comment.CommentEntry.Published;
            }
            else
            {
                CommentInfo.Language = null;
                CommentInfo.Published = DateTime.Now;
            }
            return CommentInfo;
        }
        public static List<BO.YouTubeCommentInfo> VideoCommets(List<Comment> Comments, string videoID)
        {
            List<BO.YouTubeCommentInfo> ComentList = new List<Insight360.BO.YouTubeCommentInfo>();
            foreach (var comment in Comments)
            {
                var newcomment = CreateComments(comment);
                newcomment.Staging_VideoId = videoID;
                ComentList.Add(newcomment);
            }
            return ComentList;
        }


        internal static BO.YouTubeFeed CreateFeedv3(Google.Apis.YouTube.v3.Data.Video video)
        {
            try
            {
                BO.YouTubeFeed VideoFeed = new Insight360.BO.YouTubeFeed()
                {
                    VideoID = video.Id,
                    YoutubeURL = "http://www.youtube.com/watch?v=" + video.Id,
                    Staging_InsertTime = DateTime.Now,
                    Staging_UpdateTime = DateTime.Now,
                    Comments = new List<Insight360.BO.YouTubeCommentInfo>(),
                    User = new YouTubeUserInfo(),
                    Duration = "0",

                    DurationText = video.ContentDetails.Duration,

                };
                if (video.Snippet != null)
                {
                    VideoFeed.Author = video.Snippet.ChannelTitle;
                    VideoFeed.VideoTitle = video.Snippet.Title;
                    VideoFeed.Sentiment = Helpers.SentimentScore(video.Snippet.Title);
                    VideoFeed.Description = video.Snippet.Description;
                    VideoFeed.Updated = video.Snippet.PublishedAt.HasValue ? video.Snippet.PublishedAt.Value : DateTime.Now.AddYears(-100); // no update date in api given
                    VideoFeed.Published = video.Snippet.PublishedAt.HasValue ? video.Snippet.PublishedAt.Value : DateTime.Now.AddYears(-100);
                    VideoFeed.keywords = video.Snippet.Tags != null ? video.Snippet.Tags.Aggregate((i, j) => i + ", " + j) : null;//items.Aggregate((i, j) => i + delimeter + j)
                    VideoFeed.Language = Helpers.DetectLanguage(video.Snippet.Description); //video.Snippet.DefaultLanguage;

                    try
                    {
                        string thumbs = "";
                        if (video.Snippet.Thumbnails != null)
                        {
                            if (video.Snippet.Thumbnails.High != null)
                                thumbs += video.Snippet.Thumbnails.High.Url + " ";
                            if (video.Snippet.Thumbnails.Maxres != null)
                                thumbs += video.Snippet.Thumbnails.Maxres.Url + " ";
                            if (video.Snippet.Thumbnails.Medium != null)
                                thumbs += video.Snippet.Thumbnails.Medium.Url + " ";
                            if (video.Snippet.Thumbnails.Standard != null)
                                thumbs += video.Snippet.Thumbnails.Standard.Url + " ";
                        }
                        VideoFeed.Thumbnails = thumbs;
                    }
                    catch (Exception e)
                    {
                    }
                }

                if (video.Statistics != null)
                {
                    VideoFeed.ViewCount = video.Statistics.ViewCount.HasValue ? long.Parse(video.Statistics.ViewCount.ToString()) : 0;
                    VideoFeed.CommentCount = video.Statistics.CommentCount.HasValue ? long.Parse(video.Statistics.CommentCount.ToString()) : 0;
                    VideoFeed.FavoriteCount = video.Statistics.FavoriteCount.HasValue ? video.Statistics.FavoriteCount.Value.ToString() : null;
                    VideoFeed.Likes = video.Statistics.LikeCount.HasValue ? video.Statistics.LikeCount.Value.ToString() : null;
                    VideoFeed.DisLikes = video.Statistics.DislikeCount.HasValue ? video.Statistics.DislikeCount.Value.ToString() : null;
                    VideoFeed.NumRaters = (video.Statistics.DislikeCount.HasValue && video.Statistics.LikeCount.HasValue ? long.Parse((video.Statistics.DislikeCount.Value + video.Statistics.LikeCount.Value).ToString()) : 0);

                    if (video.Statistics.DislikeCount.HasValue && video.Statistics.LikeCount.HasValue)
                    {
                        if (VideoFeed.NumRaters == 0)
                        {
                            VideoFeed.Rating = 0;
                        }
                        else
                        {
                            VideoFeed.Rating = (video.Statistics.LikeCount.Value / ulong.Parse(VideoFeed.NumRaters.ToString())) * 5;
                        }
                    }
                }
                if (video.RecordingDetails != null && video.RecordingDetails.Location != null)
                {
                    VideoFeed.Location.name = video.RecordingDetails.LocationDescription == null ? "" : video.RecordingDetails.LocationDescription;
                    VideoFeed.Location.longitude = video.RecordingDetails.Location.Longitude == null ? 0 : Convert.ToDecimal(video.RecordingDetails.Location.Longitude);
                    VideoFeed.Location.latitude = video.RecordingDetails.Location.Latitude == null ? 0 : Convert.ToDecimal(video.RecordingDetails.Location.Latitude);
                }
                return VideoFeed;
            }
            catch (Exception ex)
            {
                return null;
            }
            //return VideoFeed;
        }
        internal static BO.YouTubeUserInfo CreateUserv3(Google.Apis.YouTube.v3.Data.Channel Channel, Google.Apis.Plus.v1.Data.Person User)
        {
            if (Channel != null && Channel.Snippet != null && User != null)
            {
                BO.YouTubeUserInfo UserInfo = new Insight360.BO.YouTubeUserInfo()
                {
                    Author = User.DisplayName,
                    Age = (User.AgeRange != null ? (User.AgeRange.Min.HasValue ? User.AgeRange.Min.Value : 0) : 0),
                    Company = User.Occupation,
                    Etag = Channel.ETag,
                    FeedUri = User.Url,
                    FirstName = (User.Name != null ? User.Name.GivenName : ""),
                    LastName = (User.Name != null ? User.Name.FamilyName : ""),
                    Gender = Helpers.DetectGenderAsync(User.DisplayName).Result,  //User.Gender
                    Hobbies = "",
                    Hometown = User.Url,
                    Language = User.Language,
                    Location = (User.CurrentLocation != null ? User.CurrentLocation : "Global"),  //User.CurrentLocation
                    Movies = "",
                    Music = "",
                    Occupation = User.Occupation,
                    Relationship = User.RelationshipStatus,
                    School = "",
                    Username = User.Url,

                };
                if (Channel.Statistics != null)
                {
                    UserInfo.SubscriberCount = Channel.Statistics.SubscriberCount.HasValue ? Channel.Statistics.SubscriberCount.Value.ToString() : null;
                    UserInfo.ViewCount = Channel.Statistics.ViewCount.HasValue ? Channel.Statistics.ViewCount.Value.ToString() : null;
                }
                return UserInfo;
            }
            else
            {
                YouTubeUserInfo UserInfo = new Insight360.BO.YouTubeUserInfo();
                return UserInfo;
            }
        }
        internal static BO.YouTubeCommentInfo CreateCommentv3(Google.Apis.YouTube.v3.Data.Comment Comment)
        {
            BO.YouTubeCommentInfo CommentInfo = new Insight360.BO.YouTubeCommentInfo()
            {
                YouTubeCommentID = Comment.Id,
                Title = Comment.Snippet.TextDisplay,
                Author = Comment.Snippet.AuthorDisplayName,
                Content = Comment.Snippet.TextOriginal,
                Updated = (Comment.Snippet.UpdatedAt.HasValue ? Comment.Snippet.UpdatedAt.Value : DateTime.Now.AddYears(-100)),
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                Published = (Comment.Snippet.PublishedAt.HasValue ? Comment.Snippet.PublishedAt.Value : DateTime.Now.AddYears(-100)),
                LanguageCode = Helpers.DetectLanguage(StripHTML(Comment.Snippet.TextOriginal)),
                Sentiment = Helpers.SentimentScore(StripHTML(Comment.Snippet.TextOriginal))
            };
            return CommentInfo;
        }

        public static List<BO.YouTubeCommentInfo> VideoCommentsv3(List<Google.Apis.YouTube.v3.Data.Comment> Comments, string videoID)
        {
            List<BO.YouTubeCommentInfo> ComentList = new List<Insight360.BO.YouTubeCommentInfo>();
            foreach (var comment in Comments)
            {
                var newcomment = CreateCommentv3(comment);
                newcomment.Staging_VideoId = videoID;
                ComentList.Add(newcomment);
            }
            return ComentList;
        }

        private static string StripHTML(string HTMLText)
        {
            if (HTMLText == null || HTMLText == "")
                return "";
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase); return reg.Replace(HTMLText, "");
        }

        public static List<BO.YouTubeFeed> SearchFeeds(SearchTermInfo searchTermInfo, DateTime sinceTime, out DateTime maximumTime, bool getComments)
        {
            //----------------------------------------adnan----------------------------------------//
            Google.Apis.Services.BaseClientService.Initializer initializer = new Google.Apis.Services.BaseClientService.Initializer();
            initializer.ApiKey = AppSettings.Youtube_APIKey;// "AIzaSyAdZZjpl7Akdwq3JP58pFFuGkkbCna-sM4"; //adi change
            initializer.ApplicationName = AppName;

            Google.Apis.YouTube.v3.YouTubeService youtubeService = new Google.Apis.YouTube.v3.YouTubeService(initializer);


            Google.Apis.YouTube.v3.SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = searchTermInfo.SearchTerm; // Replace with your search term.
            searchListRequest.MaxResults = 50;
            // Call the search.list method to retrieve results matching the specified query term.
            Google.Apis.YouTube.v3.Data.SearchListResponse searchListResponse = searchListRequest.Execute();

            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        //searchResult.Id.VideoId
                        Google.Apis.YouTube.v3.VideosResource.ListRequest videoQuery = youtubeService.Videos.List("snippet,contentDetails,statistics,status");
                        videoQuery.Id = searchResult.Id.VideoId;
                        var video = videoQuery.Execute();


                        Google.Apis.YouTube.v3.ChannelsResource.ListRequest channelQuery = youtubeService.Channels.List("snippet,status,ContentOwnerDetails,statistics,contentDetails,Localizations,topicDetails");//,auditDetails
                        channelQuery.Id = video.Items.First().Snippet.ChannelId;
                        Google.Apis.YouTube.v3.Data.ChannelListResponse channel = channelQuery.Execute();
                        //channel.Items.First().ContentOwnerDetails.ContentOwner;

                        Google.Apis.YouTube.v3.CommentThreadsResource.ListRequest commentThreadsQuery = youtubeService.CommentThreads.List("snippet");
                        commentThreadsQuery.VideoId = searchResult.Id.VideoId;
                        Google.Apis.YouTube.v3.Data.CommentThreadListResponse commentThreads = commentThreadsQuery.Execute();

                        foreach (var id in commentThreads.Items)
                        {
                            Google.Apis.YouTube.v3.CommentsResource.ListRequest commentsQuery = youtubeService.Comments.List("snippet");
                            commentsQuery.Id = id.Id;
                            //commentsQuery.VideoId = searchResult.Id.VideoId;
                            Google.Apis.YouTube.v3.Data.CommentListResponse comments = commentsQuery.Execute();
                        }

                        break;

                    case "youtube#channel":
                        break;

                    case "youtube#playlist":
                        break;
                }
            }
            //----------------------------------------adnan end----------------------------------------//

            YouTubeRequestSettings settings = new YouTubeRequestSettings(AppName, prodkey);

            YouTubeRequest request = new YouTubeRequest(settings);

            DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
            List<BO.YouTubeFeed> Feeds = new List<Insight360.BO.YouTubeFeed>();
            maximumTime = sinceTime;
            DateTime? untilltime = null;

            try
            {
                int Offset = 1;
                int i = pageSize;
                while (Offset != 0)
                {

                    YouTubeQuery myQuery = null;
                    Feed<Video> videoFeed = null;
                    if (Offset >= maxData || untilltime <= sinceTime)
                    {
                        Offset = 0;
                        break;
                    }

                    if (searchTermInfo.Type.Equals(SearchTermInfo.Types.SearchTerm))
                    {
                        myQuery = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
                        myQuery.Query = searchTermInfo.SearchTerm;

                        myQuery.StartIndex = Offset;
                        myQuery.OrderBy = "published";

                        if (FetchTime.Equals(YouTubeQuery.UploadTime.Today))
                        {
                            myQuery.Time = YouTubeQuery.UploadTime.Today;
                        }


                        myQuery.NumberToRetrieve = pageSize;

                        DAL.StagingDAL.LogMiner(4, "Youtube", "Started", "Starting Searching, Offset: " + Offset.ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);
                        Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchFeeds", "Starting Searching, Offset: " + Offset.ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);
                        videoFeed = request.Get<Video>(myQuery);
                    }
                    else if (searchTermInfo.Type.Equals(SearchTermInfo.Types.YoutubeUser))
                    {
                        //   Uri uri = new Uri("http://gdata.youtube.com/feeds/api/users/" + searchTermInfo.SearchTerm + "/uploads");
                        myQuery = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
                        //        myQuery.Query = searchTermInfo.SearchTerm;
                        myQuery.Author = searchTermInfo.SearchTerm;
                        myQuery.StartIndex = Offset;
                        myQuery.OrderBy = "published";

                        if (FetchTime.Equals(YouTubeQuery.UploadTime.Today))
                        {
                            myQuery.Time = YouTubeQuery.UploadTime.Today;
                        }


                        myQuery.NumberToRetrieve = pageSize;
                        videoFeed = request.Get<Video>(myQuery);
                        //  videoFeed = request.Get<Video>(uri);

                        DAL.StagingDAL.LogMiner(4, "Youtube", "Started", "Starting Searching, Offset: " + Offset.ToString() + " maxDate: " + sinceTime.ToString() + " key: " + myQuery.Query);
                        Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchFeeds", "Starting Searching, Offset: " + Offset.ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);
                    }

                    if (videoFeed.Entries != null && videoFeed.Entries.Count() > 0)
                    {
                        DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "Videos Found for " + searchTermInfo.SearchTerm + ": " + videoFeed.Entries.Count().ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);
                        Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "Videos Found for " + searchTermInfo.SearchTerm +  ": " + videoFeed.Entries.Count().ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);

                        foreach (Video vdo in videoFeed.Entries)
                        {
                            if (vdo.Updated <= sinceTime)
                            {
                                untilltime = vdo.Updated;
                                break;
                            }
                            var Feed = CreateFeed(vdo);

                            if (Feed.Updated > maximumTime)
                                maximumTime = Feed.Updated;

                            Feeds.Add(Feed);
                            //Feed.Staging_OrgName = organizationName;


                            if (getComments)
                            {
                                Feed.Comments = VideoCommets(GetVideoComments(request, vdo), vdo.VideoId);


                            }
                            else
                            {
                                Feed.Comments = new List<Insight360.BO.YouTubeCommentInfo>();

                            }


                            Feed.User = CreateUser(GetVideoUser(vdo.Uploader));
                            Feed.User.Author = Feed.Author;


                            StringBuilder sb = new StringBuilder();
                            sb.Append(Feeds.Count);
                            sb.Append("/");
                            sb.Append(i);
                            sb.Append(" completed");

                            DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "SearchPosts" + sb.ToString());
                            Util.Log.WriteOperationCompleted(typeof(DataCollection), "SearchPosts", sb.ToString());
                        }
                        Offset += videoFeed.Entries.Count();
                        i += videoFeed.Entries.Count();
                    }
                    else
                    {
                        DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "No Video Found for " + searchTermInfo.SearchTerm + ": " + videoFeed.Entries.Count().ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);
                        Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "No Video Found for " + searchTermInfo.SearchTerm + ": " + videoFeed.Entries.Count().ToString() + " maxDate: " + sinceTime.ToString() + " key:" + myQuery.Query);
                        break;
                    }

                }

            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Exception" + ex.Message);
                Util.Log.WriteOperationCompleted(typeof(DataCollection), "Exception", ex.Message);

            }
            return Feeds;

        }

        public static List<BO.YouTubeFeed> SearchChannels(SearchTermInfo searchTermInfo, string channelName, string userName, DateTime sinceTime, bool getComments)
        {

            Google.Apis.Services.BaseClientService.Initializer initializer = new Google.Apis.Services.BaseClientService.Initializer();
            initializer.ApiKey = AppSettings.Youtube_APIKey;// "AIzaSyAdZZjpl7Akdwq3JP58pFFuGkkbCna-sM4"; //adi change
            initializer.ApplicationName = AppName;
            Google.Apis.Plus.v1.PlusService plusService = new Google.Apis.Plus.v1.PlusService(initializer);
            Google.Apis.YouTube.v3.YouTubeService youtubeService = new Google.Apis.YouTube.v3.YouTubeService(initializer);
            List<Google.Apis.YouTube.v3.Data.Video> VideosResult = new List<Google.Apis.YouTube.v3.Data.Video>();
            List<BO.YouTubeFeed> Feeds = new List<Insight360.BO.YouTubeFeed>();



            var chaList = youtubeService.Channels.List("snippet");

            if (!String.IsNullOrEmpty(channelName))
                chaList.Id = channelName;
            else if (!String.IsNullOrEmpty(channelName))
                chaList.ForUsername = userName;

            var channelv = chaList.Execute();

            Google.Apis.YouTube.v3.SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = channelv.Items.First().Id; // giving channel ID so fetch all videos of this channel
            searchListRequest.MaxResults = 50;
            searchListRequest.PublishedAfter = sinceTime;
            Google.Apis.YouTube.v3.Data.SearchListResponse searchListResponse = searchListRequest.Execute();

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Snippet.Title.ToLower().Contains(searchTermInfo.SearchTerm.ToLower()) || searchResult.Snippet.Description.ToLower().Contains(searchTermInfo.SearchTerm.ToLower()))
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            Google.Apis.YouTube.v3.VideosResource.ListRequest videoQuery = null;
                            Google.Apis.YouTube.v3.Data.VideoListResponse video = null;
                            string nextPageToken = null;

                            do
                            {
                                videoQuery = youtubeService.Videos.List("snippet,contentDetails,statistics,status,recordingDetails");
                                videoQuery.Id = searchResult.Id.VideoId;
                                videoQuery.PageToken = nextPageToken;
                                video = videoQuery.Execute();

                                nextPageToken = video.NextPageToken;

                                VideosResult.AddRange(video.Items);
                            } while (nextPageToken != null);
                            break;
                    }
                }
            }

            if (VideosResult != null && VideosResult.Count > 0)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "Videos Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "Videos Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
                foreach (Google.Apis.YouTube.v3.Data.Video v in VideosResult)
                {
                    var feed = CreateFeedv3(v);

                    if (getComments)
                    {
                        feed.Comments = VideoCommentsv3(GetVideoCommentsv3(youtubeService, feed.VideoID), feed.VideoID);
                    }

                    //getting video User Info
                    Google.Apis.YouTube.v3.ChannelsResource.ListRequest channelQuery = youtubeService.Channels.List("snippet,statistics,contentDetails");//,Localizations,topicDetails,auditDetails,status,ContentOwnerDetails,
                    channelQuery.Id = v.Snippet.ChannelId;
                    Google.Apis.YouTube.v3.Data.ChannelListResponse channel = channelQuery.Execute();
                    if (channel.Items != null && channel.Items.Count > 0)
                    {
                        try
                        {
                            Google.Apis.Plus.v1.PeopleResource.GetRequest getUserQuery = plusService.People.Get(channel.Items.First().ContentDetails.GooglePlusUserId);

                            Google.Apis.Plus.v1.Data.Person user = getUserQuery.Execute();
                            feed.User = CreateUserv3(channel.Items.First(), user);
                        }
                        catch (Exception ex)
                        {
                            DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "No User Found for " + searchTermInfo.SearchTerm + ": " + channel.Items.First().ContentDetails.GooglePlusUserId);
                            Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "No User Found for " + searchTermInfo.SearchTerm + ": " + channel.Items.First().ContentDetails.GooglePlusUserId);
                        }
                        finally
                        {
                            if (String.IsNullOrEmpty(feed.User.Author) && channel.Items[0] != null)
                            {
                                if (channel.Items[0].Snippet == null)
                                    feed.User.Author = "unknown";
                                else
                                    feed.User.Author = channel.Items[0].Snippet.Title;

                                if (channel.Items[0].Statistics != null)
                                {
                                    feed.User.ViewCount = channel.Items[0].Statistics.ViewCount.HasValue ? channel.Items[0].Statistics.ViewCount.Value.ToString() : "0";
                                    feed.User.SubscriberCount = channel.Items[0].Statistics.SubscriberCount.HasValue ? channel.Items[0].Statistics.SubscriberCount.Value.ToString() : "0";
                                }
                            }
                            if (channel.Items[0] != null)
                            {
                                feed.User.ChannelId = channel.Items[0].Id;
                            }
                        }
                    }

                    Feeds.Add(feed);
                }
            }
            else
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "No Video Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "No Video Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
            }

            return Feeds;
        }



        public static BO.YouTubeFeed GetVideo(string videID, bool getComments)
        {
            YouTubeRequestSettings settings = new YouTubeRequestSettings(AppName, prodkey);

            YouTubeRequest request = new YouTubeRequest(settings);

            DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
            BO.YouTubeFeed Feed = new Insight360.BO.YouTubeFeed();

            Uri videoEntryUrl = new Uri("http://gdata.youtube.com/feeds/api/videos/" + videID);
            Video vdo = request.Retrieve<Video>(videoEntryUrl);

            if (vdo == null)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "No search Found : " + videoEntryUrl.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchFeeds", "No search Found: " + videoEntryUrl.ToString());
            }
            else
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "videos Found: " + videoEntryUrl.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchFeeds", "videos Found: " + videoEntryUrl.ToString());

                try
                {
                    Feed = CreateFeed(vdo);
                    //Feed.Staging_OrgName = organizationName;


                    if (getComments)
                    {
                        Feed.Comments = VideoCommets(GetVideoComments(request, vdo), vdo.VideoId);


                    }
                    else
                    {
                        Feed.Comments = new List<Insight360.BO.YouTubeCommentInfo>();

                    }

                    Feed.User = CreateUser(GetVideoUser(Feed.Author));
                    Feed.User.Author = Feed.Author;

                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Exception: " + ex.Message);
                    Util.Log.WriteOperationCompleted(typeof(DataCollection), "Exception", ex.Message);

                }

            }
            return Feed;

        }
        public static ProfileEntry GetVideoUser(string author)
        {
            YouTubeRequestSettings settings = new YouTubeRequestSettings(AppName, prodkey);

            YouTubeRequest request = new YouTubeRequest(settings);

            YouTubeService service = new YouTubeService(AppName, prodkey);

            try
            {
                author = author.Replace(" ", "");
                ProfileEntry profile = (ProfileEntry)service.Get("http://gdata.youtube.com/feeds/api/users/" + author);
                return profile;

            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Exception"+ ex.Message);
                Util.Log.WriteOperationCompleted(typeof(DataCollection), "Exception", ex.Message);
                ProfileEntry User = new ProfileEntry();
                return User;
            }

        }
        private static List<Comment> GetVideoComments(YouTubeRequest request, Video vdo)
        {
            List<Comment> comments = new List<Comment>();

            try
            {
                if (vdo.CommmentCount > 0)
                {
                    Feed<Comment> coms = request.GetComments(vdo);
                    comments.AddRange(coms.Entries);
                    while (coms != null && coms.Entries != null)
                    {
                        coms = request.Get<Comment>(coms, FeedRequestType.Next);
                        if (coms != null && coms.Entries != null)
                            comments.AddRange(coms.Entries);
                    }

                }
            }

            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Exception" + ex.Message);
                Util.Log.WriteOperationCompleted(typeof(DataCollection), "Exception", ex.Message);

            }
            return comments;
        }


        public static List<BO.YouTubeFeed> SearchFeedsv3(SearchTermInfo searchTermInfo, DateTime sinceTime, out DateTime maximumTime, bool getComments)
        {
            #region ADNAN
            //----------------------------------------adnan----------------------------------------//
            Google.Apis.Services.BaseClientService.Initializer initializer = new Google.Apis.Services.BaseClientService.Initializer();
            initializer.ApiKey = AppSettings.Youtube_APIKey;// "AIzaSyCa_rF-txBCPihl6pGVAWsu8gA2rlmVllc";// "AIzaSyAdZZjpl7Akdwq3JP58pFFuGkkbCna-sM4"; //adi change
            //initializer.ApplicationName = AppName;
            Google.Apis.YouTube.v3.YouTubeService youtubeService = new Google.Apis.YouTube.v3.YouTubeService(initializer);

            Google.Apis.Plus.v1.PlusService plusService = new Google.Apis.Plus.v1.PlusService(initializer);

            //----------------------------------------adnan end----------------------------------------//
            #endregion

            List<BO.YouTubeFeed> Feeds = new List<Insight360.BO.YouTubeFeed>();
            List<Google.Apis.YouTube.v3.Data.Video> VideosResult = new List<Google.Apis.YouTube.v3.Data.Video>();

            maximumTime = sinceTime;

            if (searchTermInfo.Type.Equals(SearchTermInfo.Types.SearchTerm))
            {
                string nextPageToken = null;
                Google.Apis.YouTube.v3.SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Type = "video";
                searchListRequest.Q = searchTermInfo.SearchTerm; // Replace with your search term.
                searchListRequest.MaxResults = 50;
                searchListRequest.Order = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Date;
                searchListRequest.PublishedAfter = sinceTime; //.AddYears(-5);
                //searchListRequest.LocationRadius = "1000km";
                //searchListRequest.Location = "52.132633000,5.291265999";

                do
                {
                    searchListRequest.PageToken = nextPageToken;
                    Google.Apis.YouTube.v3.Data.SearchListResponse searchListResponse = searchListRequest.Execute();

                    foreach (var searchResult in searchListResponse.Items)
                    {
                        if (searchResult.Snippet.Title.ToLower().Contains(searchTermInfo.SearchTerm.ToLower()) || searchResult.Snippet.Description.ToLower().Contains(searchTermInfo.SearchTerm.ToLower()))
                        {
                            switch (searchResult.Id.Kind)
                            {
                                case "youtube#video":
                                    Google.Apis.YouTube.v3.VideosResource.ListRequest videoQuery = null;
                                    Google.Apis.YouTube.v3.Data.VideoListResponse video = null;
                                    videoQuery = youtubeService.Videos.List("snippet,contentDetails,statistics,status,recordingDetails");
                                    videoQuery.Id = searchResult.Id.VideoId;
                                    video = videoQuery.Execute();

                                    VideosResult.AddRange(video.Items);
                                    break;
                            }
                        }
                    }
                    nextPageToken = searchListResponse.NextPageToken;
                } while (nextPageToken != null);

                DAL.StagingDAL.LogMiner(4, "Youtube", "Started", "Starting Searching for " + searchTermInfo.SearchTerm + ", maxDate: " + sinceTime.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchFeeds", "Starting Searching for " + searchTermInfo.SearchTerm + ", maxDate: " + sinceTime.ToString());
            }
            else if (searchTermInfo.Type.Equals(SearchTermInfo.Types.YoutubeUser))
            {
                if (searchTermInfo.Type.Equals(SearchTermInfo.Types.SearchTerm))
                {
                    //getting channel ID from username
                    var chaList = youtubeService.Channels.List("snippet");
                    chaList.ForUsername = searchTermInfo.SearchTerm; //channel username here
                    var channel = chaList.Execute();

                    Google.Apis.YouTube.v3.SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
                    searchListRequest.ChannelId = channel.Items.First().Id; // giving channel ID so fetch all videos of this channel
                    searchListRequest.MaxResults = 50;
                    searchListRequest.PublishedAfter = sinceTime;
                    Google.Apis.YouTube.v3.Data.SearchListResponse searchListResponse = searchListRequest.Execute();

                    foreach (var searchResult in searchListResponse.Items)
                    {
                        switch (searchResult.Id.Kind)
                        {
                            case "youtube#video":
                                Google.Apis.YouTube.v3.VideosResource.ListRequest videoQuery = null;
                                Google.Apis.YouTube.v3.Data.VideoListResponse video = null;
                                string nextPageToken = null;

                                do
                                {
                                    videoQuery = youtubeService.Videos.List("snippet,contentDetails,statistics,status,recordingDetails");
                                    videoQuery.Id = searchResult.Id.VideoId;
                                    videoQuery.PageToken = nextPageToken;
                                    video = videoQuery.Execute();

                                    nextPageToken = video.NextPageToken;

                                    VideosResult.AddRange(video.Items);
                                } while (nextPageToken != null);
                                break;
                        }
                    }
                    DAL.StagingDAL.LogMiner(4, "Youtube", "Started", "Starting Searching for " + searchTermInfo.SearchTerm + ", maxDate: " + sinceTime.ToString());
                    Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchFeeds", "Starting Searching for " + searchTermInfo.SearchTerm + ", maxDate: " + sinceTime.ToString());
                }
            }

            if (VideosResult != null && VideosResult.Count > 0)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "Videos Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "Videos Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
                foreach (Google.Apis.YouTube.v3.Data.Video v in VideosResult)
                {
                    var feed = CreateFeedv3(v);

                    if (getComments)
                    {
                        feed.Comments = VideoCommentsv3(GetVideoCommentsv3(youtubeService, feed.VideoID), feed.VideoID);
                    }

                    //getting video User Info
                    Google.Apis.YouTube.v3.ChannelsResource.ListRequest channelQuery = youtubeService.Channels.List("snippet,status,ContentOwnerDetails,statistics,contentDetails,Localizations,topicDetails");//,auditDetails
                    channelQuery.Id = v.Snippet.ChannelId;
                    Google.Apis.YouTube.v3.Data.ChannelListResponse channel = channelQuery.Execute();
                    if (channel.Items != null && channel.Items.Count > 0)
                    {
                        try
                        {
                            Google.Apis.Plus.v1.PeopleResource.GetRequest getUserQuery = plusService.People.Get(channel.Items.First().ContentDetails.GooglePlusUserId);

                            Google.Apis.Plus.v1.Data.Person user = getUserQuery.Execute();
                            feed.User = CreateUserv3(channel.Items.First(), user);
                        }
                        catch (Exception ex)
                        {
                            DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "No User Found for " + searchTermInfo.SearchTerm + ": " + channel.Items.First().ContentDetails.GooglePlusUserId);
                            Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "No User Found for " + searchTermInfo.SearchTerm + ": " + channel.Items.First().ContentDetails.GooglePlusUserId);
                        }
                        finally
                        {
                            if (String.IsNullOrEmpty(feed.User.Author) && channel.Items[0] != null)
                            {
                                if (channel.Items[0].Snippet == null)
                                {
                                    feed.User.Author = "unknown";
                                    feed.User.Gender = "unspecified";
                                }
                                else
                                {
                                    feed.User.Author = channel.Items[0].Snippet.Title;
                                    feed.User.Gender = Helpers.DetectGenderAsync(feed.User.Author).Result;
                                }

                                if (channel.Items[0].Statistics != null)
                                {
                                    feed.User.ViewCount = channel.Items[0].Statistics.ViewCount.HasValue ? channel.Items[0].Statistics.ViewCount.Value.ToString() : "0";
                                    feed.User.SubscriberCount = channel.Items[0].Statistics.SubscriberCount.HasValue ? channel.Items[0].Statistics.SubscriberCount.Value.ToString() : "0";
                                }
                            }
                            if (channel.Items[0] != null)
                            {
                                feed.User.ChannelId = channel.Items[0].Id;
                            }
                        }
                    }

                    maximumTime = feed.Staging_InsertTime;

                    Feeds.Add(feed);
                }
            }
            else
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Processing", "No Video Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
                Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "No Video Found for " + searchTermInfo.SearchTerm + ": " + VideosResult.Count.ToString() + " maxDate: " + sinceTime.ToString());
            }

            return Feeds;
        }
        private static List<Google.Apis.YouTube.v3.Data.Comment> GetVideoCommentsv3(Google.Apis.YouTube.v3.YouTubeService youtubeService, string videoId)
        {
            string nextPageToken = null;
            List<Google.Apis.YouTube.v3.Data.Comment> commentsReturn = new List<Google.Apis.YouTube.v3.Data.Comment>();

            try
            {
                do
                {
                    Google.Apis.YouTube.v3.CommentThreadsResource.ListRequest commentThreadsQuery = youtubeService.CommentThreads.List("snippet");
                    commentThreadsQuery.PageToken = nextPageToken;
                    commentThreadsQuery.VideoId = videoId;
                    Google.Apis.YouTube.v3.Data.CommentThreadListResponse commentThreads = commentThreadsQuery.Execute();

                    foreach (var commentThread in commentThreads.Items)
                    {
                        Google.Apis.YouTube.v3.CommentsResource.ListRequest commentsQuery = youtubeService.Comments.List("snippet");
                        commentsQuery.Id = commentThread.Id;
                        Google.Apis.YouTube.v3.Data.CommentListResponse comments = commentsQuery.Execute();

                        if (comments.Items != null && comments.Items.Count > 0)
                        {
                            commentsReturn.Add(comments.Items.First());
                        }
                    }

                    nextPageToken = commentThreads.NextPageToken;
                } while (nextPageToken != null);
            }

            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(4, "Youtube", "Error", "Y1004 GetVideoComments" + ex);
                Util.Log.WriteException(typeof(DataCollection), "Y1004 GetVideoComments", ex);
            }
            return commentsReturn;
        }

    }
}
