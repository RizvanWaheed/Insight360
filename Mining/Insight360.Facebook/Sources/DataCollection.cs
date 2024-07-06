using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook;
using System.Collections.Specialized;
using System.Web;
using System.Threading;
using System.Runtime.Serialization;
using Insight360.Util;
using System.Threading.Tasks;
using Insight360.Common;
using Insight360.BO;

namespace Insight360.Sources.Facebook
{
    public class DataCollection
    {
        static string GetValue(JsonObject obj, string id)
        {
            return (obj.Keys.Contains(id) ? obj[id].ToString() : string.Empty);
        }

        public static DateTime GetDateValue(JsonObject obj, string id)
        {
            DateTime dt;
            if (!DateTime.TryParse(GetValue(obj, id), out dt))
                dt = new DateTime(2000, 1, 1);
            dt = dt.ToUniversalTime();
            return dt;
        }

        internal static BO.FacebookPost CreatePost(JsonObject obj)
        {
            BO.FacebookPost post = new Insight360.BO.FacebookPost();

            post.Attribution = GetValue(obj, "attribution");
            post.Caption = GetValue(obj, "caption");
            post.CreatedTime = GetDateValue(obj, "created_time").ToLocalTime(); //GetDateValue(obj, "created_time").;
            post.Description = GetValue(obj, "description");

            post.From = new Insight360.BO.FacebookUserInfo()
            {
                Id = obj.Keys.Contains("from") ? GetValue(obj["from"] as JsonObject, "id") : string.Empty,
                Name = obj.Keys.Contains("from") ? GetValue(obj["from"] as JsonObject, "name") : string.Empty,
            };

            post.To = new Insight360.BO.FacebookUserInfo()
            {
                Id = obj.Keys.Contains("to") ? GetValue(((((obj["to"] as JsonObject))["data"] as JsonArray)[0] as JsonObject), "id") : string.Empty,
                Name = obj.Keys.Contains("to") ? GetValue(((((obj["to"] as JsonObject))["data"] as JsonArray)[0] as JsonObject), "name") : string.Empty,
            };

            post.Icon = GetValue(obj, "icon");
            post.Id = GetValue(obj, "id");
            post.Link = GetValue(obj, "link");
            post.Message = GetValue(obj, "message");
            post.Name = GetValue(obj, "name");
            post.Picture = GetValue(obj, "full_picture");   //post.Picture = GetValue(obj, "picture");

            post.Targeting = new Insight360.BO.FacebookPost.TargetingInfo()
            {
                City = obj.Keys.Contains("targeting") ? GetValue(obj["targeting"] as JsonObject, "city") : string.Empty,
                Country = obj.Keys.Contains("targeting") ? GetValue(obj["targeting"] as JsonObject, "country") : string.Empty,
                Locale = obj.Keys.Contains("targeting") ? GetValue(obj["targeting"] as JsonObject, "locale") : string.Empty,
                Region = obj.Keys.Contains("targeting") ? GetValue(obj["targeting"] as JsonObject, "region") : string.Empty,
            };

            post.Type = GetValue(obj, "type");
            post.UpdatedTime = GetDateValue(obj, "updated_time");

            post.Comments = new List<Insight360.BO.FacebookCommentInfo>();
            post.Likes = new List<Insight360.BO.FacebookLikeInfo>();

            if ((obj.Keys.Contains("comments"))
                && ((obj["comments"] as JsonObject).ContainsKey("summary"))
                && ((((obj["comments"] as JsonObject)["summary"]) as JsonObject).ContainsKey("total_count")))
            {
                post.CommentsCount = Convert.ToInt32((((obj["comments"] as JsonObject)["summary"]) as JsonObject)["total_count"]);
            }

            if ((obj.Keys.Contains("likes"))
                && ((obj["likes"] as JsonObject).ContainsKey("summary"))
                && ((((obj["likes"] as JsonObject)["summary"]) as JsonObject).ContainsKey("total_count")))
            {
                post.LikesCount = Convert.ToInt32((((obj["likes"] as JsonObject)["summary"]) as JsonObject)["total_count"]);
            }

            if ((obj.Keys.Contains("shares"))
                && ((obj["shares"] as JsonObject).ContainsKey("count")))
            {
                post.SharesCount = Convert.ToInt32(((obj["shares"] as JsonObject)["count"]));
            }

            post.PageLocation.name = "";

            if (obj.Keys.Contains("location"))
            {
                if ((obj["location"] as JsonObject).Keys.Contains("city"))
                {
                    post.PageLocation.name = ((obj["location"] as JsonObject)["city"]).ToString();
                }

                if ((obj["location"] as JsonObject).Keys.Contains("country"))
                {
                    post.PageLocation.name += ", " + ((obj["location"] as JsonObject)["country"]).ToString();
                }

                if ((obj["location"] as JsonObject).Keys.Contains("longitude"))
                {
                    post.PageLocation.longitude = Convert.ToDecimal(((obj["location"] as JsonObject)["longitude"]));
                }

                if ((obj["location"] as JsonObject).Keys.Contains("latitude"))
                {
                    post.PageLocation.latitude = Convert.ToDecimal(((obj["location"] as JsonObject)["latitude"]));
                }
            }

            if (obj.Keys.Contains("place"))
            {
                JsonObject place = obj["place"] as JsonObject;

                if (place.Keys.Contains("location"))
                {
                    JsonObject loc = place["location"] as JsonObject;

                    if (loc.Keys.Contains("city"))
                        post.PageLocation.name = (loc["city"]).ToString();

                    if (loc.Keys.Contains("country"))
                        post.PageLocation.name += ", " + (loc["country"]).ToString();

                    if (loc.Keys.Contains("longitude"))
                        post.PageLocation.longitude = Convert.ToDecimal(loc["longitude"]);

                    if (loc.Keys.Contains("latitude"))
                        post.PageLocation.latitude = Convert.ToDecimal(loc["latitude"]);
                }

                if (place.Keys.Contains("name"))
                {
                    post.PageLocation.name = (place["name"]).ToString();
                }

            }

            return post;
        }

        public static List<JsonObject> GetAllChannelsWithShareToken(DocumentSourceInfo documentSource, SearchTermInfo term,string accessToken)
        {
            string path = "search?fields=id,name,location";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var searchTerm = term.SearchTerm.ToLower() == "ke" ? "K-Electric" : term.SearchTerm.ToLower();
            switch (documentSource.ProfileType)
            {
                case ProfileTypes.FacebookPage:
                    parameters.Clear();
                    parameters.Add("type", "page");
                    parameters.Add("q", searchTerm);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);
                    break;
                case ProfileTypes.FacebookUser:
                    accessToken = AppSettings.Facebook_AccessToken_User;
                    parameters.Clear();
                    path = "me/friends";
                    return GetAllPagedItems(path, accessToken, parameters, null, null);
                    break;
                case ProfileTypes.FacebookEvent:
                    accessToken = AppSettings.Facebook_AccessToken_User;
                    path = "search?fields=id,name";
                    parameters.Clear();
                    parameters.Add("type", "event");
                    parameters.Add("q", term.SearchTerm);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);
                    break;
                case ProfileTypes.FacebookGroup:
                    accessToken = AppSettings.Facebook_AccessToken_User;
                    parameters.Clear();
                    parameters.Add("type", "group");
                    parameters.Add("q", searchTerm);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);
                    break;
                case ProfileTypes.FacebookPlace:
                    parameters.Clear();
                    parameters.Add("type", "place");
                    parameters.Add("q", term.SearchTerm);
                    if (term.latitude > 0 && term.longitude > 0)
                    {
                        parameters.Add("center", term.latitude + "," + term.longitude);
                        parameters.Add("distance", "16000");
                    }
                    return GetAllPagedItems(path, accessToken, parameters, null, null);
                    break;
                case ProfileTypes.FacebookChannel:
                    string pageName = documentSource.PageId;
                    List<JsonObject> channels = new List<JsonObject>();
                    JsonObject obj = new JsonObject();
                    obj.Add("id", pageName);
                    channels.Add(obj);
                    return channels;
                    break;
            }
            return null;
        }

        public static List<JsonObject> GetAllChannels(DocumentSourceInfo documentSource, SearchTermInfo term, out string accessToken)
        {
            string path = "search?fields=id,name,location";

            //commented on Jan 11, 2017
            //if (term.SearchTerm.StartsWith("#"))
            //{
            //    term.SearchTerm = term.SearchTerm.Replace("#", "%23");
            //}

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var searchTerm = term.SearchTerm.ToLower() == "ke" ? "K-Electric" : term.SearchTerm.ToLower(); // global search brings too many channels for term "KE"
            accessToken = "";

            switch (documentSource.ProfileType)
            {
                case ProfileTypes.FacebookPage:

                    //accessToken = Insight360.Sources.Facebook.DataCollection.GetToken();

                    parameters.Clear();
                    parameters.Add("type", "page");
                    parameters.Add("q", searchTerm);

                    return GetAllPagedItems(path, accessToken, parameters, null, null);
                    //return DAL.ConfigDAL.GetFacebookPagesBySearchTermId(term.Id); // List<string> from Database, for Databank

                    break;

                case ProfileTypes.FacebookUser:

                    accessToken = AppSettings.Facebook_AccessToken_User; 
                    
                    parameters.Clear();

                    //parameters.Add("type", "user");
                    //parameters.Add("q", term.SearchTerm);

                    path = "me/friends";

                    //return DAL.ConfigDAL.GetFacebookPagesBySearchTermId(term.Id);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);

                    break;

                case ProfileTypes.FacebookEvent:

                    accessToken = AppSettings.Facebook_AccessToken_User; 
                    path = "search?fields=id,name";
                    parameters.Clear();

                    parameters.Add("type", "event");
                    parameters.Add("q", term.SearchTerm);

                    //return DAL.ConfigDAL.GetFacebookPagesBySearchTermId(term.Id);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);

                    break;

                case ProfileTypes.FacebookGroup:

                    accessToken = AppSettings.Facebook_AccessToken_User; 
                    parameters.Clear();
                    parameters.Add("type", "group");
                    parameters.Add("q", searchTerm);

                    //return DAL.ConfigDAL.GetFacebookPagesBySearchTermId(term.Id);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);

                    break;

                case ProfileTypes.FacebookPlace:

                    //accessToken = Insight360.Sources.Facebook.DataCollection.GetToken();

                    parameters.Clear();

                    parameters.Add("type", "place");
                    parameters.Add("q", term.SearchTerm);

                    //List<Location> locations = DAL.ConfigDAL.GetLocations(item.Id);

                    if (term.latitude > 0 && term.longitude > 0)
                    {
                        parameters.Add("center", term.latitude + "," + term.longitude);
                        parameters.Add("distance", "16000");
                    }

                    //return DAL.ConfigDAL.GetFacebookPagesBySearchTermId(term.Id);
                    return GetAllPagedItems(path, accessToken, parameters, null, null);

                    break;

                case ProfileTypes.FacebookChannel:

                   // accessToken = Insight360.Sources.Facebook.DataCollection.GetToken();
                    //string pageName = documentSource.Url.Substring(documentSource.Url.LastIndexOf('/') + 1);
                    //pageName = pageName.Substring(pageName.LastIndexOf('-') + 1);

                    string pageName = documentSource.PageId;

                    List<JsonObject> channels = new List<JsonObject>();
                    JsonObject obj = new JsonObject();
                    obj.Add("id", pageName);
                    channels.Add(obj);
                    return channels;

                    break;
            }

            return null;
        }

        public static Dictionary<string, object> GetParameters(string searchTerm, DateTime sinceTime, BO.DocumentSourceInfo info, List<Location> locations)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("since", sinceTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //if(locations != null && locations.Count > 0)
            //{
            //    parameters.Add("center", "40.714623,-74.006605");
            //    parameters.Add("distance", "16000");
            //}

            BO.ProfileTypes profileType = (BO.ProfileTypes)info.ProfileType;

            switch (profileType)
            {
                case BO.ProfileTypes.FacebookPage:

                    parameters.Add("q", searchTerm);
                    parameters.Add("type", "page");

                    break;

                case BO.ProfileTypes.FacebookUser:

                    parameters.Add("q", searchTerm);
                    parameters.Add("type", "user");

                    break;

                case BO.ProfileTypes.FacebookEvent:

                    parameters.Add("q", searchTerm);
                    parameters.Add("type", "event");

                    break;

                case BO.ProfileTypes.FacebookGroup:

                    parameters.Add("q", searchTerm);
                    parameters.Add("type", "group");

                    break;

                case BO.ProfileTypes.FacebookPlace:

                    parameters.Add("q", searchTerm);
                    parameters.Add("type", "place");

                    //if(locations != null && locations.Count > 0)
                    //{
                    //    parameters.Add("center", "40.714623,-74.006605");
                    //    parameters.Add("distance", "16000");
                    //}

                    break;

                case BO.ProfileTypes.FacebookChannel:
                    break;
            }

            return parameters;
        }

        private static List<BO.FacebookPost> GetAllPagedPosts(string path, string accessToken, Dictionary<string, object> parameters, DateTime sinceTime, DateTime? untilTime, bool getCommentAndLikes, out DateTime maximumTime, string url)
        {
            maximumTime = sinceTime;
            List<BO.FacebookPost> posts = new List<Insight360.BO.FacebookPost>();
            List<JsonObject> jsonPosts = new List<JsonObject>();
            List<JsonObject> pages = GetAllPagedItems(path, accessToken, parameters, sinceTime, untilTime);

            /********************************************************************Ahsen Mehmood*********************************************************/
            // in case of facebook channels
            if (url != "http://www.facebook.com")
            {
                jsonPosts = GetAllPagedItems(path, accessToken, parameters, sinceTime, untilTime);
                Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchPosts", "Getting comments and likes of channel" + path);
            }
            // in case of all profiles
            else if (url == "http://www.facebook.com")
            {
                /// getting all pages
                //  now get posts in each page
                foreach (var page in pages)
                {
                    parameters.Clear();

                    var feeds = GetAllPagedItems(page["id"].ToString() + "/feed", accessToken, parameters, sinceTime, untilTime);

                    jsonPosts.AddRange(feeds);
                    //#if DEBUG
                    //                    if (jsonPosts.Count > 0)
                    //                        break;
                    //#endif
                }
                Util.Log.WriteOperationStarted(typeof(DataCollection), "SearchPosts", "Getting comments and likes " + pages.Count);

            }

            /******************************************************************************************************************************************/
            for (int i = 0; i < jsonPosts.Count; i++)
            {
                var post = CreatePost(jsonPosts[i]);
                bool exists = false;
                foreach (var p in posts)
                {
                    if (p.Id == post.Id)
                    {
                        Util.Log.WriteOperationCompleted(typeof(DataCollection), "SearchPosts", "Duplicate post found");
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    post.CommentsCount = GetPostCommentsCount(post.Id, accessToken);
                    post.LikesCount = GetLikesCount(post.Id, accessToken);

                    posts.Add(post);
                    if (post.CreatedTime > maximumTime)
                        maximumTime = post.CreatedTime;

                    if (getCommentAndLikes)
                    {
                        post.Comments = GetPostComments(post.Id, accessToken);
                        post.Likes = GetLikes(post.Id, accessToken);
                    }
                    else
                    {
                        post.Comments = new List<Insight360.BO.FacebookCommentInfo>();
                        post.Likes = new List<Insight360.BO.FacebookLikeInfo>();
                    }

                    post.user = GetUserDetails(accessToken, new List<string>() { post.From.Id }, true)[0];
                    if (!string.IsNullOrEmpty(post.To.Id))
                    {
                        post.toUser = GetUserDetails(accessToken, new List<string>() { post.To.Id }, true)[0];
                    }
                    else post.toUser.UpdatedTime = DateTime.Now;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(i + 1);
                    sb.Append("/");
                    sb.Append(pages.Count);
                    sb.Append(" completed");
                    FillPathAndParameters(sb, path, parameters);
                    Util.Log.WriteOperationCompleted(typeof(DataCollection), "SearchPosts", sb.ToString());
                }
            }

            return posts;
        }

        public static List<BO.FacebookPost> CreatePosts(List<JsonObject> jsonPosts, string accessToken, bool getCommentAndLikes, DateTime sinceTime, out DateTime maximumTime)
        {
            maximumTime = sinceTime;
            List<BO.FacebookPost> posts = new List<Insight360.BO.FacebookPost>();

            for (int i = 0; i < jsonPosts.Count; i++)
            {
                var post = CreatePost(jsonPosts[i]);
                bool exists = false;
                foreach (var p in posts)
                {
                    if (p.Id == post.Id)
                    {
                        Util.Log.WriteOperationCompleted(typeof(DataCollection), "SearchPosts", "Duplicate post found");
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    post.CommentsCount = GetPostCommentsCount(post.Id, accessToken);
                    post.LikesCount = GetLikesCount(post.Id, accessToken);

                    posts.Add(post);
                    if (post.CreatedTime > maximumTime)
                        maximumTime = post.CreatedTime;

                    if (getCommentAndLikes)
                    {
                        post.Comments = GetPostComments(post.Id, accessToken);
                        post.Likes = GetLikes(post.Id, accessToken);
                    }
                    else
                    {
                        post.Comments = new List<Insight360.BO.FacebookCommentInfo>();
                        post.Likes = new List<Insight360.BO.FacebookLikeInfo>();
                    }

                    post.user = GetUserDetails(accessToken, new List<string>() { post.From.Id })[0];

                    if (!string.IsNullOrEmpty(post.To.Id))
                    {
                        post.toUser = GetUserDetails(accessToken, new List<string>() { post.To.Id })[0];
                    }
                    else post.toUser.UpdatedTime = DateTime.Now;
                    //StringBuilder sb = new StringBuilder();
                    //sb.Append(i + 1);
                    //sb.Append("/");
                    //sb.Append(pages.Count);
                    //sb.Append(" completed");
                    //FillPathAndParameters(sb, path, parameters);
                    Util.Log.WriteOperationCompleted(typeof(DataCollection), "SearchPosts", post.Id.ToString());
                }
            }

            return posts;
        }

        public static BO.FacebookPost CreateLocalPost(JsonObject jsonPost, string accessToken, bool getCommentAndLikes, DateTime sinceTime, out DateTime maximumTime)
        {
            maximumTime = sinceTime;

            try
            {
                BO.FacebookPost post = CreatePost(jsonPost);

                //post.CommentsCount = GetPostCommentsCount(post.Id, accessToken);
                //post.LikesCount = GetLikesCount(post.Id, accessToken);

                if (post.CreatedTime > maximumTime)
                    maximumTime = post.CreatedTime;

                try
                {
                    post.user = GetUserDetails(accessToken, new List<string>() { post.From.Id }, true)[0];
                }
                catch (Exception ex)
                {
                    post.user = GetUserDetails(accessToken, new List<string>() { post.From.Id })[0];

                    //return null;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(post.To.Id))
                    {
                        post.toUser = GetUserDetails(accessToken, new List<string>() { post.To.Id })[0];
                    }
                    else post.toUser.UpdatedTime = DateTime.Now;

                    var taskSentiment = Helpers.SentimentScoreAsync(post.Message, "facebook");
                    var taskLanguage = Helpers.DetectLanguageAsync(post.Message);  //Helpers.DetectLanguage(post.Message);
                    var taskgender = Helpers.DetectGenderAsync(post.user.Name);

                    Task.WaitAll(taskSentiment, taskLanguage, taskgender);

                    post.Sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                    post.LanguageCode = taskLanguage.Result == null ? "na" : taskLanguage.Result.ToString();
                    post.user.Gender = taskgender.Result == null || taskgender.Result == "" ? "unspecified" : taskgender.Result.ToString();

                    //post.Sentiment = 3;
                    //post.LanguageCode = "na";
                    //post.user.Gender = "unspecified";

                    if (getCommentAndLikes)
                    {
                        post.Comments = GetPostComments(post.Id, accessToken);
                    }
                }

                //Util.Log.WriteOperationCompleted(typeof(DataCollection), "SearchPosts", post.Id.ToString());

                return post;
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"CreateLocalPost\"");
                Util.Log.WriteException(typeof(DataCollection), "Facebook CreateLocalPost", ex);

                return null;
            }
        }

        static int GetPostCommentsCount(string postId, string accessToken)
        {
            try
            {
                FacebookClient app = new FacebookClient();
                app.AccessToken = accessToken;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("access_token", accessToken);
                var result = (IDictionary<string, object>)app.Get(postId + "/comments?summary=true", parameters);
                JsonObject data = result["summary"] as JsonObject;
                return Convert.ToInt32(GetValue(data, "total_count"));
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "Get Post Comments Count", ex);
                return 0;
            }
        }

        static int GetLikesCount(string postId, string accessToken)
        {
            try
            {
                FacebookClient app = new FacebookClient();
                app.AccessToken = accessToken;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("access_token", accessToken);
                var result = (IDictionary<string, object>)app.Get(postId + "/likes?summary=true", parameters);
                JsonObject data = result["summary"] as JsonObject;
                return Convert.ToInt32(GetValue(data, "total_count"));
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "Get Post Likes Count", ex);
                return 0;
            }
        }

        static BO.FacebookUser CreateUser(JsonObject obj, string id, string followersCount, string followingCount)
        {
            string Loc = "Global", picture = null;

            //if (GetValue(obj, "location") != String.Empty)
            //    Loc = GetValue((obj["location"] as JsonObject), "city") + ", " + GetValue((obj["location"] as JsonObject), "country");
            //else
            //    Loc = "Global";

            if (obj.Keys.Contains("fan_count"))
            {
                followersCount = (obj["fan_count"]).ToString();
            }

            if (obj.Keys.Contains("hometown"))
            {
                Loc = (obj["hometown"]).ToString();
            }

            if (obj.Keys.Contains("location"))
            {
                if (obj.Keys.Contains("location") && (obj["location"] as JsonObject).Keys.Contains("city"))
                {
                    Loc = ((obj["location"] as JsonObject)["city"]).ToString();
                }

                if (obj.Keys.Contains("location") && (obj["location"] as JsonObject).Keys.Contains("country"))
                {
                    Loc += ", " + ((obj["location"] as JsonObject)["country"]).ToString();
                }
            }


            if (obj.Keys.Contains("picture"))
            {
                if (obj.Keys.Contains("picture") && (obj["picture"] as JsonObject).Keys.Contains("data"))
                {
                    JsonObject pict = (JsonObject)((obj["picture"] as JsonObject)["data"]);

                    if (pict.Keys.Contains("url"))
                        picture = pict["url"].ToString();
                }
            }

            BO.FacebookUser user = new Insight360.BO.FacebookUser()
            {
                Id = GetValue(obj, "id"),
                Name = GetValue(obj, "name"),
                FirstName = GetValue(obj, "first_name"),
                LastName = GetValue(obj, "last_name"),
                Gender = null,//GetValue(obj, "gender"),
                Link = GetValue(obj, "link"),
                Locale = GetValue(obj, "locale"),
                Picture = picture,  // GetValue(obj, "picture"),
                ThirdPartyId = GetValue(obj, "third_party_id"),
                UpdatedTime = GetDateValue(obj, "updated_time"),
                TimeZone = obj.Keys.Contains("timezone") ? GetValue(obj, "timezone") : null,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                Location = Loc //GetValue((obj["location"] as JsonObject), "city") + ", " + GetValue((obj["location"] as JsonObject), "country")
            };

            return user;
        }

        //https://graph.facebook.com/?ids=100000755914395,100001253508531&access_token=169095506475698|2e66dc828704143b13c3d8b1-100002075274719|tjsEs3-4AcbRU7UAfT1v6W5lAeA&fields=id,name,first_name,last_name,gender,locale,link,third_party_id,updated_time,verified,picture
        public static List<BO.FacebookUser> GetUserDetails(string accessToken, List<string> userIds, bool ispage = false)
        {
            List<BO.FacebookUser> results = new List<Insight360.BO.FacebookUser>();

            FacebookClient app = new FacebookClient();
            app.AccessToken = accessToken;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder();

            foreach (string userId in userIds)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(userId);
            }

            IDictionary<string, object> result;
            parameters.Add("ids", sb.ToString());
            parameters.Add("access_token", accessToken);

            if (ispage)
                parameters.Add("fields", "location,hometown, name, id, picture, fan_count");
            else
                parameters.Add("fields", "location,hometown, name, id, picture");

            //parameters.Add("fields", "id,name,first_name,last_name,gender,locale,link,third_party_id,updated_time,verified,picture");

            result = (IDictionary<string, object>)app.Get("", parameters);
            foreach (var r in result.Values)
            {
                //Ahsen Mehmood Nov 26, 2015 fields= subscribedto and susbscribers is depricated so not calling the following methods and returning 0
                //string followersCount = (!obj.Keys.Contains("likes") ? GetUserFollowersCount(sb.ToString(), accessToken) : "0");
                //string followingCount = (!obj.Keys.Contains("likes") ? GetUsersFollowingCount(sb.ToString(), accessToken) : "0");
                string followersCount = "0";
                string followingCount = "0";
                results.Add(CreateUser(r as JsonObject, null, followersCount, followingCount));
            }

            return results;
        }

        public static string GetUserFollowersCount(string userId, string accessToken)
        {
            try
            {
                FacebookClient app = new FacebookClient();
                app.AccessToken = accessToken;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("access_token", accessToken);
                var result = (IDictionary<string, object>)app.Get(userId + "/subscribers", parameters);
                if (result.Keys.Contains("summary"))
                {
                    JsonObject data = result["summary"] as JsonObject;
                    return GetValue(data, "total_count");
                }
                else return "0";
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "Get User Followers Count", ex);
                return "0";
            }

        }

        public static string GetUsersFollowingCount(string userId, string accessToken)
        {
            try
            {
                FacebookClient app = new FacebookClient();
                app.AccessToken = accessToken;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("access_token", accessToken);
                var result = (IDictionary<string, object>)app.Get(userId + "?fields=subscribedto%2Csubscribers", parameters);
                if (result.Keys.Contains("subscribedto"))
                {
                    JsonObject data = result["subscribedto"] as JsonObject;

                    return GetValue(data["summary"] as JsonObject, "total_count");
                }
                else return "0";
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "Get User Following Count", ex);
                return "0";
            }

        }

        static BO.FacebookLikeInfo CreateLike(JsonObject obj)
        {
            BO.FacebookLikeInfo likeInfo = new Insight360.BO.FacebookLikeInfo()
            {
                Id = GetValue(obj as JsonObject, "id"),
                Name = GetValue(obj as JsonObject, "name"),
            };
            return likeInfo;
        }

        //https://graph.facebook.com/100002171651511_103326983083023_19607/likes
        //https://graph.facebook.com/100001292791875_185783794775630/likes
        static List<BO.FacebookLikeInfo> GetLikes(string id, string accessToken)
        {
            List<BO.FacebookLikeInfo> likes = new List<Insight360.BO.FacebookLikeInfo>();
            List<JsonObject> items = GetAllPagedItems(id + "/likes", accessToken, new Dictionary<string, object>(), null, null);
            for (int i = 0; i < items.Count; i++)
            {
                likes.Add(CreateLike(items[i]));
            }
            return likes;
        }

        static BO.FacebookCommentInfo CreateComment(JsonObject obj)
        {
            BO.FacebookCommentInfo commentInfo = new Insight360.BO.FacebookCommentInfo()
            {
                Id = GetValue(obj as JsonObject, "id"),
                From = new Insight360.BO.FacebookUserInfo()
                {
                    Name = obj.Keys.Contains("from") ? GetValue(obj["from"] as JsonObject, "name") : string.Empty,
                    Id = obj.Keys.Contains("from") ? GetValue(obj["from"] as JsonObject, "id") : string.Empty,
                },
                CreatedTime = GetDateValue(obj, "created_time").ToLocalTime(),
                Message = GetValue(obj as JsonObject, "message"),
                Sentiment = Helpers.SentimentScore(GetValue(obj as JsonObject, "message"), "facebook"),
            };

            return commentInfo;
        }

        //https://graph.facebook.com/100001292791875_185783794775630/comments
        public static List<BO.FacebookCommentInfo> GetPostComments(string postId, string accessToken)
        {
            List<BO.FacebookCommentInfo> comments = new List<Insight360.BO.FacebookCommentInfo>();
            List<JsonObject> items = GetAllPagedItems(postId + "/comments", AppSettings.Facebook_AccessToken_User, new Dictionary<string, object>(), null, null);  //accessToken
            for (int i = 0; i < items.Count; i++)
            {
                var comment = CreateComment(items[i]);
                //bool exists = false;
                //foreach (var c in comments)
                //{
                //    if (c.Id == comment.Id)
                //    {
                //        Util.Log.WriteOperationCompleted(typeof(DataCollection), "GetPostComments", "Duplicate comment found");
                //        exists = true;
                //        break;
                //    }
                //}
                //if (!exists)
                //{

                comments.Add(comment);
                ////comment.Likes = GetLikes(comment.Id, accessToken);
                // }
            }

            return comments;
        }

        //https://graph.facebook.com/100001292791875_185783794775630
        public static BO.FacebookPost GetPost(string accessToken, string postId, bool getCommentAndLikes)
        {
            BO.FacebookPost post = null;
            try
            {
                FacebookClient app = new FacebookClient();
                app.AccessToken = accessToken;
                var result = (IDictionary<string, object>)app.Get(postId, null);

                post = CreatePost(result as JsonObject);

                var taskSentiment = Helpers.SentimentScoreAsync(post.Message, "facebook");
                var taskLanguage = Helpers.DetectLanguageAsync(post.Message);
                // var taskLanguage = Helpers.DetectLanguage(post.Message);

                if (getCommentAndLikes)
                {
                    post.Comments = GetPostComments(post.Id, accessToken);
                    post.Likes = GetLikes(post.Id, accessToken);
                }

                post.user = GetUserDetails(accessToken, new List<string>() { post.From.Id })[0];

                if (post.To.Id != "")
                {
                    post.toUser = GetUserDetails(accessToken, new List<string>() { post.To.Id })[0];
                }

                Task.WaitAll(taskSentiment, taskLanguage);

                post.Sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                post.LanguageCode = taskLanguage.Result == null ? "" : taskLanguage.Result.ToString();
                //post.LanguageCode = taskLanguage == null ? "" : taskLanguage.ToString();
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetPost\"");
                Util.Log.WriteException(typeof(DataCollection), "GetPost-" + accessToken + "-" + postId, ex);
            }

            return post;
        }

        static Dictionary<string, object> FillNextPageParameters(IDictionary<string, object> result)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (result.Keys.Contains("paging"))
            {
                JsonObject y = result["paging"] as JsonObject;

                if (y.Keys.Contains("next"))
                {
                    Uri uri = new Uri(y["next"].ToString());
                    NameValueCollection query = new NameValueCollection();
                    query = HttpUtility.ParseQueryString(uri.Query);

                    for (int i = 0; i < query.Keys.Count; i++)
                    {
                        parameters.Add(query.Keys[i], query[query.Keys[i]]);
                    }
                }
            }

            return parameters;
        }

        public static List<JsonObject> GetAllPagedItems(string path, string accessToken, Dictionary<string, object> parameters, DateTime? since, DateTime? until)
        {
            FacebookClient app = new FacebookClient();
            app.AccessToken = accessToken;

            List<JsonObject> items = new List<JsonObject>();
            bool nextPageAvailable = true;
            parameters.Add("access_token", accessToken);

            //parameters.Add("limit", 200);
            //     if (until != null && !parameters.Keys.Contains("until"))
            //       parameters.Add("until", until.Value.Date.ToString("yyyy-MM-dd HH:mm:ss"));

            while (nextPageAvailable)
            {
                //facebook removes since from pervious query params
                if (since != null && !parameters.Keys.Contains("since"))
                {
                    parameters.Add("since", since.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                try
                {
                    var result = (IDictionary<string, object>)app.Get(path, parameters);
                    JsonArray data = result["data"] as JsonArray;

                    int count = 0;

                    for (int i = 0; i < data.Count; i++)
                    {
                        bool exists = false;
                        if ((data[i] as JsonObject).Keys.Contains("id"))
                        {
                            foreach (var item in items)
                            {
                                if (GetValue(data[i] as JsonObject, "id") == GetValue(item, "id"))
                                {
                                    Util.Log.WriteOperationCompleted(typeof(DataCollection), "GetAllPagedItems", "Duplicate found");
                                    exists = true;
                                    break;
                                }
                            }
                        }
                        if (!exists)
                        {
                            count++;
                            items.Add(data[i] as JsonObject);
                        }
                    }

                    var oldParams = parameters;
                    parameters = FillNextPageParameters(result);
                    nextPageAvailable = parameters.Count > 0 && data.Count > 0 && count > 0;
                    //int c = (from y in items select GetValue(y, "id")).Distinct().Count();
                    //Util.Log.WriteOperationCompleted(x.Count + "-" + c + "-" + items.Count);

                    StringBuilder sb = new StringBuilder();
                    sb.Append(count);
                    sb.Append(" - ");
                    sb.Append(data.Count);
                    sb.Append(" - ");
                    sb.Append(items.Count);
                    sb.Append(" - ");

                    try
                    {
                        if (items.Count > 0)
                            sb.Append(GetDateValue(items[items.Count - 1], "created_time").ToLocalTime());
                    }
                    catch { }

                    FillPathAndParameters(sb, path, oldParams);

                    //commented on May 17, 2017
                    //if (count <= 5)
                    //    Thread.Sleep(800);
                    //else
                    //    Thread.Sleep(300);

                }
                catch (FacebookApiException ex)
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetAllPagedItems\"");
                    Util.Log.WriteException(ex.GetType(), "Facebook GetAllPagedItems", ex);
                    nextPageAvailable = false;
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetAllPagedItems\"");
                    Util.Log.WriteException(ex.GetType(), "Facebook GetAllPagedItems", ex);
                    nextPageAvailable = false;
                }
            }

            return items;
        }

        public static List<JsonObject> GetPagedItems(string path, string accessToken, Dictionary<string, object> parameters, DateTime? since, DateTime? until, out bool nextPageAvailable, out Dictionary<string, object> oldParams,out bool hasFbLimit)
        {
            FacebookClient app = new FacebookClient();
            app.AccessToken = accessToken;
            List<JsonObject> items = new List<JsonObject>();
            if (!parameters.Keys.Contains("access_token"))
                parameters.Add("access_token", accessToken);
            nextPageAvailable = true;
            hasFbLimit = true;
            //facebook removes since from pervious query params
            if (since != null && !parameters.Keys.Contains("since"))
                parameters.Add("since", since.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            try
            {
                var result = (IDictionary<string, object>)app.Get(path, parameters);
                JsonArray data = result["data"] as JsonArray;
                int count = data.Count;
                data.ForEach(item => items.Add(item as JsonObject));
                oldParams = parameters;
                parameters = FillNextPageParameters(result);
                nextPageAvailable = parameters.Count > 0 && data.Count > 0;
                StringBuilder sb = new StringBuilder();
                sb.Append(count);
                sb.Append(" - ");
                sb.Append(data.Count);
                sb.Append(" - ");
                sb.Append(items.Count);
                sb.Append(" - ");
                if (items.Count > 0)
                    sb.Append(GetDateValue(items[items.Count - 1], "created_time").ToLocalTime());
                FillPathAndParameters(sb, path, oldParams);
            }
            catch (FacebookApiException ex)
            {
                //Application request limit reached
                if (ex != null & !string.IsNullOrEmpty(ex.Message) && ex.Message.ToLower().Contains("limit reached"))
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Limit Reached ", "Facebook Miner Cycle Stopped Graph API Limit.");
                    Thread.Sleep(30 * 60000);
                    //if (!System.IO.File.Exists("/listening/fblimit.txt"))
                    //{
                    //    System.IO.File.WriteAllText("/listening/fblimit.txt", DateTime.Now.AddMinutes(30).ToString("dd/MM/yyyy hh:mm:ss tt"));
                    //    hasFbLimit = false;
                    //}
                    //else
                    //{
                    //    System.IO.File.Delete("/listening/fblimit.txt");
                    //    System.IO.File.WriteAllText("/listening/fblimit.txt", DateTime.Now.AddMinutes(30).ToString("dd/MM/yyyy hh:mm:ss tt"));
                    //    hasFbLimit = false;
                    //}
                }
                else
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetPagesItems\"");
                    Util.Log.WriteException(ex.GetType(), "Facebook GetPagedItems", ex);
                    nextPageAvailable = false;
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetPagesItems\"");
                Util.Log.WriteException(typeof(FacebookClient), "Facebook GetPagedItems", ex);
                nextPageAvailable = false;
            }
            oldParams = parameters;
            return items;
        }

        public static List<JsonObject> GetPagedItemsWithFBLimit(string path, string accessToken, Dictionary<string, object> parameters, DateTime? since, DateTime? until,string docSource,string searchTerm, out bool nextPageAvailable, out Dictionary<string, object> oldParams, out bool hasFbLimit)
        {
            FacebookClient app = new FacebookClient();
            app.AccessToken = accessToken;
            List<JsonObject> items = new List<JsonObject>();
            if (!parameters.Keys.Contains("access_token"))
                parameters.Add("access_token", accessToken);
            nextPageAvailable = true;
            hasFbLimit = true;
            //facebook removes since from pervious query params
            if (since != null && !parameters.Keys.Contains("since"))
                parameters.Add("since", since.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            try
            {
                var result = (IDictionary<string, object>)app.Get(path, parameters);
                JsonArray data = result["data"] as JsonArray;
                int count = data.Count;
                data.ForEach(item => items.Add(item as JsonObject));
                oldParams = parameters;
                parameters = FillNextPageParameters(result);
                nextPageAvailable = parameters.Count > 0 && data.Count > 0;
                StringBuilder sb = new StringBuilder();
                sb.Append(count);
                sb.Append(" - ");
                sb.Append(data.Count);
                sb.Append(" - ");
                sb.Append(items.Count);
                sb.Append(" - ");
                if (items.Count > 0)
                    sb.Append(GetDateValue(items[items.Count - 1], "created_time").ToLocalTime());
                FillPathAndParameters(sb, path, oldParams);
            }
            catch (FacebookApiException ex)
            {
                //Application request limit reached
                if (ex != null & !string.IsNullOrEmpty(ex.Message) && ex.Message.ToLower().Contains("limit reached"))
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Limit Reached ", "Facebook Miner cycle Graph API Limit at source: "+docSource+" , searchTerm: "+searchTerm);
                    Thread.Sleep(50 * 60000);
                    //if (!System.IO.File.Exists("/listening/fblimit.txt"))
                    //{
                    //    System.IO.File.WriteAllText("/listening/fblimit.txt", DateTime.Now.AddMinutes(30).ToString("dd/MM/yyyy hh:mm:ss tt"));
                    //    hasFbLimit = false;
                    //}
                    //else
                    //{
                    //    System.IO.File.Delete("/listening/fblimit.txt");
                    //    System.IO.File.WriteAllText("/listening/fblimit.txt", DateTime.Now.AddMinutes(30).ToString("dd/MM/yyyy hh:mm:ss tt"));
                    //    hasFbLimit = false;
                    //}
                }
                else
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetPagesItems\"");
                    Util.Log.WriteException(ex.GetType(), "Facebook GetPagedItems", ex);
                    nextPageAvailable = false;
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"GetPagesItems\"");
                Util.Log.WriteException(typeof(FacebookClient), "Facebook GetPagedItems", ex);
                nextPageAvailable = false;
            }
            oldParams = parameters;
            return items;
        }

        static void FillPathAndParameters(StringBuilder sb, string path, Dictionary<string, object> parameters)
        {
            sb.Append(" - ");
            sb.Append(path);
            foreach (var x in parameters.Keys)
            {
                sb.Append(" - ");
                sb.Append(x);
                sb.Append(":");
                sb.Append(parameters[x].ToString());
            }
        }

        public static string GetToken()
        {

            var fb = new FacebookClient();
            var result = fb.Get("oauth/access_token", new
            {
                client_id = AppSettings.Facebook_ClientId,
                client_secret = AppSettings.Facebook_ClientSecret,
                grant_type = "client_credentials"
            }) as JsonObject;
            return result["access_token"].ToString();

            //FacebookClient fbClient = new FacebookClient();
            //Dictionary<string, object> fbParams = new Dictionary<string, object>();
            //fbParams["client_id"] = "749622071786940";
            //fbParams["grant_type"] = "fb_exchange_token";
            //fbParams["client_secret"] = "5058c117fcb3ba8695ec79aa1384f687";
            //if (string.IsNullOrEmpty(currentAccessToken))
            //{
            //    currentAccessToken = System.Configuration.ConfigurationSettings.AppSettings["Facebook_AccessToken_User"];
            //}
            //fbParams["fb_exchange_token"] = currentAccessToken;
            //JsonObject publishedResponse = fbClient.Get("/oauth/access_token", fbParams) as JsonObject;
            //currentAccessToken = publishedResponse["access_token"].ToString();

        }
    }
}
