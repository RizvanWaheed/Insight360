using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insight360.DAL;
using System.Data;
using System.Net;
using TweetSharp;
using Insight360.Util;
using System.Threading;
using Insight360.BO;

namespace Insight360.Sources.Twitter
{
    public class DataCollection
    {
        private const int RESULTS_PER_PAGE = 50; //100;

        public static List<BO.TwitterStatus> SearchStatuses(BO.TwitterAuthentication authentication,
            int searchtermid, string searchTerm, long sinceId, long maxId, DateTime lastfetchTime, out long maximumId, out long maximumId2, out DateTime maximumTime, bool getHistory, BO.Location location,
            List<ExcludedDocumentSourceInfo> excludedHandels) //, DateTime lastFetchTime
        {
            List<BO.TwitterStatus> results = new List<BO.TwitterStatus>();
            //int currentPage = 1;
            maximumId = sinceId;
            maximumId2 = maxId;
            maximumTime = DateTime.Now;
            float datediff = (lastfetchTime != null ? (maximumTime - lastfetchTime).Days : 0);

            maximumTime = lastfetchTime;

            SearchOptions so = new SearchOptions();

            if (location != null)
            {
                so.Geocode = new TwitterGeoLocationSearch()
                {
                    Coordinates = new TwitterGeoLocation.GeoCoordinates()
                    {
                        Latitude = Convert.ToDouble(location.latitude),
                        Longitude = Convert.ToDouble(location.longitude)
                    },

                    Radius = 1000,
                    UnitOfMeasurement = TwitterGeoLocationSearch.RadiusType.Km,
                    Type = "Point"
                };
            }

            so.Q = searchTerm;
            so.Count = RESULTS_PER_PAGE;

            if (maxId != 0)  //  && datediff <= 7
            {
                so.MaxId = Convert.ToInt64(maxId) - 1;
                so.SinceId = 0;
            }
            else if (sinceId != 0)
            {
                so.MaxId = 0;
                so.SinceId = Convert.ToInt64(sinceId);
            }
            else
                so.Until = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            TwitterService service = new TwitterService(authentication.ConsumerKey, authentication.ConsumerSecret);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            service.AuthenticateWith(authentication.Token, authentication.TokenSecret);
            service.TweetMode = TweetMode.Extended;
            service.IncludeEntities = true;
            //service.IncludeRetweets = true;
            List<TweetSharp.TwitterStatus> searchResult = new List<TweetSharp.TwitterStatus>();

            try
            {
                long id1 = 0; string query = "";
                query = "select top 1 websourceid from documents where documentsource_id=1 and SearchTermId=" + searchtermid + " order by time desc";
                id1 = Convert.ToInt64(DBUtil.ExecuteScalar(query, null, null, CommandType.Text));

                bool gethistory = ((maxId == 0 && sinceId == 0) || (maxId != 0 && Convert.ToInt64(maxId) <= Convert.ToInt64(sinceId)));

                if (Convert.ToInt64(maxId) > Convert.ToInt64(sinceId) || gethistory || maxId == 0)
                {
                    bool check = false;
                    if (Convert.ToInt64(maxId) > Convert.ToInt64(sinceId))
                        check = true;

                    if (excludedHandels.Count > 0)
                        searchResult = service.Search(so).Statuses.Where(s => !excludedHandels.Any(e => e.Url == s.User.ScreenName)).OrderByDescending(x => Convert.ToInt64(x.Id)).ToList();
                    else
                        searchResult = service.Search(so).Statuses.OrderByDescending(x => Convert.ToInt64(x.Id)).ToList();

                    if (searchResult != null)  // && searchResult.Count > 0
                    {
                        if (service.Response.StatusCode.ToString() == "429")//Rate Limit Exceeded
                        {
                            DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Rate Limit Exceed");
                            Log.WriteOperationCompleted(typeof(DataCollection), "Twitter", " Search Statuses Rate Limit Exceed");
                            Thread.Sleep(900000);
                        }
                        else if (service.Response.StatusCode == HttpStatusCode.OK)
                        {
                            if (searchResult.Count > 0)
                            {
                                foreach (TweetSharp.TwitterStatus ts in searchResult)
                                {
                                    //In case of Retweets, FullText gets cut off (if > 140 characters i.e why using RetweetedStatus to search complete text 
                                    //string Text_to_Search = "";
                                    //Text_to_Search = ts.RetweetedStatus != null ? ts.RetweetedStatus.FullText : ts.FullText;

                                    //if (Text_to_Search.ToLower().Contains(searchTerm.ToLower()))   
                                    if (ts.FullText.ToLower().Contains(searchTerm.ToLower()))
                                    {
                                        BO.TwitterStatus obj = new BO.TwitterStatus();
                                        obj.CreatedAt = ts.CreatedDate.ToLocalTime();//ts.CreatedDate;
                                        obj.Favorited = ts.IsFavorited;
                                        obj.FromUser = ts.User.ScreenName;
                                        obj.FromUserId = ts.User.Id.ToString();
                                        // obj.Geo = ts.Location == null || ts.Location.Coordinates == null ? "" : ts.Location.Coordinates.Latitude.ToString() + ", " + ts.Location.Coordinates.Longitude.ToString();
                                        obj.Geo = ts.Place != null ? ts.Place.FullName : "";
                                        obj.Id = ts.Id.ToString();
                                        obj.IsoLanguageCode = ts.Language;
                                        obj.Media_Cnt = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count : 0;
                                        obj.Media_Url = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count != 0 ? ts.ExtendedEntities.Media[0].MediaUrl.ToString() : null : null;
                                        obj.Media_Url2 = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count > 1 ? ts.ExtendedEntities.Media[1].MediaUrl.ToString() : null : null;
                                        obj.Media_Url3 = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count > 2 ? ts.ExtendedEntities.Media[2].MediaUrl.ToString() : null : null;
                                        obj.Source = ts.Source;
                                        obj.Text = ts.FullText; //ts.RetweetedStatus != null ? "RT @" + ts.RetweetedStatus.Author.ScreenName + ": " + ts.RetweetedStatus.FullText : ts.FullText; 
                                        obj.ToUserId = ts.InReplyToUserId.ToString();
                                        obj.ToUser = ts.InReplyToScreenName;
                                        obj.LikesCount = ts.RetweetedStatus != null ? ts.RetweetedStatus.FavoriteCount : ts.FavoriteCount;
                                        obj.InReplyToStatusId = ts.RetweetedStatus != null ? ts.RetweetedStatus.Id.ToString() : null;
                                        obj.RetweetCount = ts.RetweetCount;

                                        BO.TwitterUser usr = new BO.TwitterUser();

                                        usr.Location = "";
                                        if (ts.User.Location != "" && ts.User.Location != null)
                                            usr.Location = ts.User.Location;

                                        usr.CreatedAt = ts.User.CreatedDate;
                                        usr.Description = ts.User.Description;
                                        usr.FavouritesCount = ts.User.FavouritesCount;
                                        usr.FollowersCount = ts.User.FollowersCount;
                                        usr.Following = true;// ts.User.iIsFollowing,
                                        usr.FriendsCount = ts.User.FriendsCount;
                                        usr.GeoEnabled = ts.User.IsGeoEnabled;
                                        usr.Id = ts.User.Id.ToString();
                                        usr.Lang = ts.User.Language;
                                        usr.Name = ts.User.Name;
                                        usr.ProfileImageUrl = ts.User.ProfileImageUrl;
                                        usr.Protected = ts.User.IsProtected;
                                        usr.ScreenName = ts.User.ScreenName;
                                        usr.StatusesCount = ts.User.StatusesCount;
                                        usr.TimeZone = ts.User.TimeZone;
                                        usr.Url = ts.User.Url;
                                        usr.UtcOffset = ts.User.UtcOffset;
                                        usr.IsVerified = ts.User.IsVerified;
                                        usr.ProfileBgColor = ts.User.ProfileBackgroundColor;
                                        usr.ProfileBgImageUrl = ts.User.ProfileBackgroundImageUrl;
                                        usr.ProfileBgTiled = ts.User.IsProfileBackgroundTiled;
                                        usr.ProfileLinkColor = ts.User.ProfileLinkColor;
                                        usr.ProfileTxtColor = ts.User.ProfileTextColor;

                                        obj.UserDetails = usr;

                                        results.Add(obj);

                                        if (maxId == 0 && sinceId == 0)  //Initial case of Keyword
                                        {
                                            maximumId = maximumId2 = ts.Id;
                                            break;
                                        }
                                        else if (so.MaxId != 0)   //Backdate mining  (wrt LastFetchId)
                                        {
                                            maximumId = sinceId;
                                            maximumId2 = ts.Id;

                                            if (check)
                                            {
                                                if (maximumId2 < maximumId)
                                                {
                                                    maximumId2 = 0;
                                                    maximumId = id1;
                                                    maximumTime = ts.CreatedDate.ToLocalTime();

                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //maximumId = ts.Id;   //forward date mining (wrt LastFetchId)
                                            //maximumId2 = 0;
                                            maximumId2 = ts.Id;
                                        }
                                        maximumTime = ts.CreatedDate.ToLocalTime();
                                    }
                                }
                            }  // if (searchResult.Count > 0)
                            else
                            {
                                if (maxId != 0) //(so.MaxId != 0)
                                {
                                    maximumId2 = 0;
                                    maximumId = id1;
                                }
                            }
                        } //else if end
                    }  //if (searchResult != null)
                } // if (Convert.ToInt64(maxId) > Convert.ToInt64(sinceId) || gethistory || maxId == 0)
            }  //try end

            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Error in Global search for " + searchTerm);
                Util.Log.WriteException(typeof(DataCollection), "Error in Global search for " + searchTerm, ex);
            }

            return results;
        }

        public static BO.TwitterUser GetUser(BO.TwitterAuthentication authentication, string user_id)
        {
            BO.TwitterUser user = null;


            TwitterService service = new TwitterService(authentication.ConsumerKey, authentication.ConsumerSecret);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            service.AuthenticateWith(authentication.Token, authentication.TokenSecret);
            SearchForUserOptions suo = new SearchForUserOptions();
            suo.Q = user_id.ToString();

            var twUser = service.SearchForUser(suo);

            if (service.Response.StatusCode.ToString() == "429")//Rate Limit Exceeded
            {
                string a = "";

            }

            else
            {
                if (service.Response.StatusCode == HttpStatusCode.OK)
                {
                    foreach (TweetSharp.TwitterUser tu in twUser)
                    {

                        TweetSharp.TwitterUser twitter_user = tu;
                        //if (user_id.ToLower() == twitter_user.Name.ToLower()) //if an exact match, just return it, do not bother to process the rest of the matching users
                        //{
                        user = new BO.TwitterUser()
                        {
                            CreatedAt = twitter_user.CreatedDate,
                            Description = twitter_user.Description,
                            FavouritesCount = twitter_user.FavouritesCount,
                            FollowersCount = twitter_user.FollowersCount,
                            Following = true,// twitter_user.IsFollowing,
                            FriendsCount = twitter_user.FriendsCount,
                            GeoEnabled = twitter_user.IsGeoEnabled,
                            Id = twitter_user.Id.ToString(),
                            Lang = twitter_user.Language,
                            Location = twitter_user.Location,
                            Name = twitter_user.Name,
                            ProfileImageUrl = twitter_user.ProfileImageUrl,
                            Protected = twitter_user.IsProtected,
                            ScreenName = twitter_user.ScreenName,
                            StatusesCount = twitter_user.StatusesCount,
                            TimeZone = twitter_user.TimeZone,
                            Url = twitter_user.Url,
                            UtcOffset = twitter_user.UtcOffset,
                            IsVerified = twitter_user.IsVerified,
                            ProfileBgColor = twitter_user.ProfileBackgroundColor,
                            ProfileBgImageUrl = twitter_user.ProfileBackgroundImageUrl,
                            ProfileBgTiled = twitter_user.IsProfileBackgroundTiled,
                            ProfileLinkColor = twitter_user.ProfileLinkColor,
                            ProfileTxtColor = twitter_user.ProfileTextColor,
                        };
                        //return user;
                        //}
                    }

                }
            }

            return user;
        }

        public static BO.TwitterStatus GetDetailedStatus(BO.TwitterAuthentication authentication,
            long statusId)
        {
            BO.TwitterStatus status = new BO.TwitterStatus();
            try
            {
                TwitterClientInfo clientInfo = new TwitterClientInfo();
                clientInfo.ConsumerKey = authentication.ConsumerKey;
                clientInfo.ConsumerSecret = authentication.ConsumerSecret;

                TwitterService service = new TwitterService(authentication.ConsumerKey, authentication.ConsumerSecret);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                service.AuthenticateWith(authentication.Token, authentication.TokenSecret);
                GetTweetOptions gto = new GetTweetOptions();
                gto.Id = statusId;

                TweetSharp.TwitterStatus ts = new TweetSharp.TwitterStatus();
                ts = service.GetTweet(gto);

                if (service.Response.StatusCode.ToString() == "429")//Rate Limit Exceeded
                {
                    status = new BO.TwitterStatus() { SkippedDueToRateLimiting = true };
                }

                //else if (service.Response.Error.Code == 63) {//user account has been suspended
                //    status = new BO.TwitterStatus() { SkippedDueToRateLimiting = false };
                //}
                else
                {
                    if (service.Response.StatusCode == HttpStatusCode.OK)
                    {

                        //    TwitterStatus s =new TwitterStatus();
                        //    s. twitterResult.AsStatus();

                        status = new BO.TwitterStatus()
                        {
                            Id = ts.Id.ToString(),
                            //      Parent_id = ts.RetweetedStatus != null ? ts.RetweetedStatus.Id : 0,
                            Text = ts.FullText,
                            CreatedAt = ts.CreatedDate.ToLocalTime(),
                            Source = ts.Source,
                            Geo = ts.Location == null || ts.Location.Coordinates == null ? "" : ts.Location.Coordinates.Latitude.ToString() + ", " + ts.Location.Coordinates.Longitude.ToString(),
                            Favorited = ts.IsFavorited,
                            FromUser = ts.User.ScreenName,
                            FromUserId = ts.User.Id.ToString(),
                            ToUserId = ts.InReplyToUserId.ToString(),
                            ToUser = ts.InReplyToScreenName,
                            InReplyToScreenName = ts.InReplyToScreenName,
                            InReplyToStatusId = ts.InReplyToStatusId.ToString(),
                            InReplyToUserId = ts.InReplyToUserId.ToString(),
                            Truncated = ts.IsTruncated,
                            RetweetCount = ts.RetweetCount,
                            LikesCount = ts.RetweetedStatus != null ? ts.RetweetedStatus.FavoriteCount : ts.FavoriteCount,
                            SkippedDueToRateLimiting = false,
                            Sentiment = Helpers.SentimentScore(ts.FullText),
                            LanguageCode = Helpers.DetectLanguage(ts.FullText),
                            UserDetails = new BO.TwitterUser()
                            {
                                CreatedAt = ts.User.CreatedDate,
                                Description = ts.User.Description,
                                FavouritesCount = ts.User.FavouritesCount,
                                FollowersCount = ts.User.FollowersCount,
                                Following = true,// ts.User.iIsFollowing,
                                FriendsCount = ts.User.FriendsCount,
                                GeoEnabled = ts.User.IsGeoEnabled,
                                Id = ts.User.Id.ToString(),
                                Lang = ts.User.Language,
                                Location = ts.User.Location,
                                Name = ts.User.Name,
                                ProfileImageUrl = ts.User.ProfileImageUrl,
                                Protected = ts.User.IsProtected,
                                ScreenName = ts.User.ScreenName,
                                StatusesCount = ts.User.StatusesCount,
                                TimeZone = ts.User.TimeZone,
                                Url = ts.User.Url,
                                UtcOffset = ts.User.UtcOffset,
                                IsVerified = ts.User.IsVerified,
                                ProfileBgColor = ts.User.ProfileBackgroundColor,
                                ProfileBgImageUrl = ts.User.ProfileBackgroundImageUrl,
                                ProfileBgTiled = ts.User.IsProfileBackgroundTiled,
                                ProfileLinkColor = ts.User.ProfileLinkColor,
                                ProfileTxtColor = ts.User.ProfileTextColor,
                            }
                        };
                    }
                }

                return status;
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "Exception in Fetching Status", ex);
                return new BO.TwitterStatus();
            }
        }


        public static List<BO.TwitterStatus> SearchStatusesFromAndTo(BO.TwitterAuthentication authentication,
            string userName, long sinceId, out long maximumId, out DateTime maximumTime)
        {
            maximumId = 0;
            maximumTime = DateTime.Now;

            return null;
        }

        public static List<BO.TwitterStatus> GetUserMentions(BO.TwitterAuthentication authentication, string searchTerm, long sinceId, out long maximumId, out DateTime maximumTime)
        {
            List<BO.TwitterStatus> results = new List<BO.TwitterStatus>();
            int currentPage = 1;
            long currentId = 0;
            maximumId = sinceId;
            maximumTime = new DateTime(2010, 1, 1);

            TwitterService service = new TwitterService(authentication.ConsumerKey, authentication.ConsumerSecret);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            service.AuthenticateWith(authentication.Token, authentication.TokenSecret);
            service.TweetMode = TweetMode.Extended;
            while (true)
            {
                // List option = new ListTweetsMentioningMeOptions();
                SearchOptions options = new SearchOptions();
                options.Q = searchTerm;
                options.Count = 200;
                if (sinceId != 0)
                {
                    options.SinceId = sinceId;
                }

                if (currentId != 0)
                {
                    options.MaxId = (currentId > 0) ? currentId - 1 : currentId;// maximumId;
                }

                TwitterSearchResult result = service.Search(options);
                if (service.Response.StatusCode.ToString() == "429")//Rate Limit Exceeded
                {
                    string a = "";
                    break;
                    //   SkippedDueToRateLimiting = true;

                }
                // var result = twitterResult.AsStatuses();
                else if (service.Response.StatusCode == HttpStatusCode.OK)
                {
                    if (result != null)// && twitterResult.IsTwitterError == false && twitterResult.IsNetworkError == false)
                    {
                        bool check = false;
                        foreach (var ts in result.Statuses)
                        {
                            check = true;


                            results.Add(new BO.TwitterStatus()
                            {
                                CreatedAt = ts.CreatedDate.ToLocalTime(),
                                FromUser = ts.User.ScreenName,
                                FromUserId = ts.User.Id.ToString(),
                                Id = ts.Id.ToString(),
                                //   Parent_id = ts.RetweetedStatus != null ? ts.RetweetedStatus.Id : 0,
                                IsoLanguageCode = ts.User.Language,
                                //ProgfileImageUrl = ts.User.ProfileImageUrl,
                                Source = ts.Source,
                                Text = ts.FullText,
                                ToUserId = ts.InReplyToUserId.ToString(),
                                ToUser = ts.InReplyToScreenName,
                                RetweetCount = ts.RetweetCount,
                                LikesCount = ts.RetweetedStatus != null ? ts.RetweetedStatus.FavoriteCount : ts.FavoriteCount,
                                Sentiment = Helpers.SentimentScore(ts.FullText),
                                LanguageCode = Helpers.DetectLanguage(ts.FullText),
                                UserDetails = new BO.TwitterUser()
                                {
                                    CreatedAt = ts.User.CreatedDate,
                                    Description = ts.User.Description,
                                    FavouritesCount = ts.User.FavouritesCount,
                                    FollowersCount = ts.User.FollowersCount,
                                    Following = true,// ts.User.iIsFollowing,
                                    FriendsCount = ts.User.FriendsCount,
                                    GeoEnabled = ts.User.IsGeoEnabled,
                                    Id = ts.User.Id.ToString(),
                                    Lang = ts.User.Language,
                                    Location = ts.User.Location,
                                    Name = ts.User.Name,
                                    ProfileImageUrl = ts.User.ProfileImageUrl,
                                    Protected = ts.User.IsProtected,
                                    ScreenName = ts.User.ScreenName,
                                    StatusesCount = ts.User.StatusesCount,
                                    TimeZone = ts.User.TimeZone,
                                    Url = ts.User.Url,
                                    UtcOffset = ts.User.UtcOffset,
                                    IsVerified = ts.User.IsVerified,
                                    ProfileBgColor = ts.User.ProfileBackgroundColor,
                                    ProfileBgImageUrl = ts.User.ProfileBackgroundImageUrl,
                                    ProfileBgTiled = ts.User.IsProfileBackgroundTiled,
                                    ProfileLinkColor = ts.User.ProfileLinkColor,
                                    ProfileTxtColor = ts.User.ProfileTextColor,
                                }
                                //UserDetails = new BO.TwitterUser()
                                //{
                                //    Id = ts.User.Id.ToString(),
                                //    ScreenName = ts.User.ScreenName,
                                //    ProfileImageUrl = ts.User.ProfileImageUrl
                                //}
                            });

                            currentId = ts.Id;
                            if (ts.Id > maximumId)
                                maximumId = ts.Id;

                            if (ts.CreatedDate.ToLocalTime() > maximumTime)
                                maximumTime = ts.CreatedDate.ToLocalTime();
                        }
                        currentPage++;
                        if (!check) break;


                    }
                }
                else
                {
                    string a = "";
                    break;
                }
            }

            return results;
        }

        public static List<BO.TwitterStatus> GetUserTimeline(BO.TwitterAuthentication authentication, string searchTerm, string user, long sinceId, out long maximumId, out DateTime maximumTime)
        {
            Util.Log.WriteOperationStarted(typeof(DataCollection), "GetUserTimeline", "Fetching User Timeline");

            List<BO.TwitterStatus> results = new List<BO.TwitterStatus>();
            int currentPage = 1;
            long currentId = 0;
            maximumId = sinceId;
            maximumTime = new DateTime(2013, 11, 1);

            TwitterService service = new TwitterService(authentication.ConsumerKey, authentication.ConsumerSecret);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            service.AuthenticateWith(authentication.Token, authentication.TokenSecret);
            service.TweetMode = TweetMode.Extended;

            try
            {
                while (true)
                {
                    ListTweetsOnUserTimelineOptions option = new ListTweetsOnUserTimelineOptions();
                    option.ScreenName = user;
                    option.ExcludeReplies = false;
                    option.IncludeRts = true;
                    option.Count = RESULTS_PER_PAGE;

                    if (sinceId != 0)
                    {
                        option.SinceId = sinceId;
                    }

                    if (currentId != 0)
                    {
                        option.MaxId = (currentId > 0) ? currentId - 1 : currentId;// maximumId;
                    }

                    var resp = service.ListTweetsOnUserTimeline(option);

                    try
                    {
                        if (resp != null)
                        {
                            List<TweetSharp.TwitterStatus> result = service.ListTweetsOnUserTimeline(option).ToList();// twitterResult.AsStatuses();
                            if (service.Response.StatusCode.ToString() == "429")//Rate Limit Exceeded
                            {
                                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Rate Limit Exceed");
                                Log.WriteOperationCompleted(typeof(DataCollection), "Twitter", " User Timeline Rate Limit Exceed");
                                Thread.Sleep(900000);
                            }
                            else if (service.Response.StatusCode == HttpStatusCode.OK)
                            {
                                if (result != null)//&& twitterResult.IsTwitterError == false && twitterResult.IsNetworkError == false)
                                {
                                    Util.Log.WriteOperationStarted(typeof(DataCollection), "GetUserTimeline", "New results Count" + result.Count);

                                    bool check = false;
                                    foreach (var ts in result)
                                    {
                                        //string Text_to_Search = "";
                                        //Text_to_Search = ts.RetweetedStatus != null ? ts.RetweetedStatus.FullText : ts.FullText;

                                        ////In case of Retweets, FullText gets cut off (ig > 140 characters i.e why using RetweetedStatus to search complete text 
                                        //if (Text_to_Search.ToLower().Contains(searchTerm.ToLower()))   //
                                        if (ts.FullText.ToLower().Contains(searchTerm.ToLower()))
                                        {
                                            check = true;
                                            if (ts.CreatedDate.ToLocalTime() <= maximumTime)
                                            {
                                                check = false;
                                                break;
                                            }

                                            results.Add(new BO.TwitterStatus()
                                            {
                                                CreatedAt = ts.CreatedDate.ToLocalTime(), //ts.CreatedDate,
                                                FromUser = ts.User.ScreenName,
                                                FromUserId = ts.User.Id.ToString(),
                                                Id = ts.Id.ToString(),
                                                //Parent_id = ts.RetweetedStatus != null ? ts.RetweetedStatus.Id : 0,
                                                IsoLanguageCode = ts.User.Language,
                                                //ProgfileImageUrl = ts.User.ProfileImageUrl,
                                                Source = ts.Source,
                                                Text = ts.FullText,  //ts.RetweetedStatus != null ? "RT @" + ts.RetweetedStatus.Author.ScreenName + ": " + ts.RetweetedStatus.FullText : ts.FullText,//
                                                ToUserId = ts.InReplyToUserId.ToString(),
                                                ToUser = ts.InReplyToScreenName,
                                                RetweetCount = ts.RetweetCount,
                                                Media_Cnt = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count : 0,
                                                Media_Url = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count != 0 ? ts.ExtendedEntities.Media[0].MediaUrl.ToString() : null : null,
                                                Media_Url2 = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count > 1 ? ts.ExtendedEntities.Media[1].MediaUrl.ToString() : null : null,
                                                Media_Url3 = ts.ExtendedEntities != null ? ts.ExtendedEntities.Media.Count > 2 ? ts.ExtendedEntities.Media[2].MediaUrl.ToString() : null : null,
                                                LikesCount = ts.RetweetedStatus != null ? ts.RetweetedStatus.FavoriteCount : ts.FavoriteCount,

                                                UserDetails = new BO.TwitterUser()
                                                {
                                                    CreatedAt = ts.User.CreatedDate,
                                                    Description = ts.User.Description,
                                                    FavouritesCount = ts.User.FavouritesCount,
                                                    FollowersCount = ts.User.FollowersCount,
                                                    Following = true,// ts.User.iIsFollowing,
                                                    FriendsCount = ts.User.FriendsCount,
                                                    GeoEnabled = ts.User.IsGeoEnabled,
                                                    Id = ts.User.Id.ToString(),
                                                    Lang = ts.User.Language,
                                                    Location = ts.User.Location,
                                                    Name = ts.User.Name,
                                                    ProfileImageUrl = ts.User.ProfileImageUrl,
                                                    Protected = ts.User.IsProtected,
                                                    ScreenName = ts.User.ScreenName,
                                                    StatusesCount = ts.User.StatusesCount,
                                                    TimeZone = ts.User.TimeZone,
                                                    Url = ts.User.Url,
                                                    UtcOffset = ts.User.UtcOffset,
                                                    IsVerified = ts.User.IsVerified,
                                                    ProfileBgColor = ts.User.ProfileBackgroundColor,
                                                    ProfileBgImageUrl = ts.User.ProfileBackgroundImageUrl,
                                                    ProfileBgTiled = ts.User.IsProfileBackgroundTiled,
                                                    ProfileLinkColor = ts.User.ProfileLinkColor,
                                                    ProfileTxtColor = ts.User.ProfileTextColor,
                                                }
                                            });

                                            currentId = ts.Id;
                                        }

                                        if (ts.Id > maximumId)
                                            maximumId = ts.Id;
                                    }

                                    Util.Log.WriteOperationStarted(typeof(DataCollection), "GetUserTimeline", "Total results Count" + results.Count);

                                    currentPage++;
                                    if (!check) break;
                                }
                            }
                            else
                            {
                                string a = "";
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "Error in fetching user timeline for " + searchTerm);
                        Util.Log.WriteException(typeof(DataCollection), "Error in fetching user timeline for " + searchTerm, ex);

                    }
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(1, "Twitter", "Error", "GetUserTimeline | setting delay of 15min | " + "SearchTerm: " + searchTerm + " | " + "Handle: " + user + " | Service.Response.Error: " + service.Response.Errors.RawSource);
                Util.Log.WriteException(typeof(DataCollection), "GetUserTimeline | setting delay of 25min | " + "SearchTerm: " + searchTerm + " | " + "Handle: " + user + " | Service.Response.Error: " + service.Response.Errors.RawSource, ex);
                throw;
            }

            return results;
        }
    }
}
