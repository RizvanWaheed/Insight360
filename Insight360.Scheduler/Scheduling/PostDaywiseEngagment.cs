using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
using Insight360.Common;
using Insight360.DAL;
using System.Data;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using TweetSharp;
using Facebook;
using Insight360.Common.BO;

namespace Insight360.Scheduling
{
    [DisallowConcurrentExecution]
    public class PostDaywiseEngagement : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //try
            //{
            Util.Log.WriteOperationStarted(GetType(), "Execute", "Post Daywise Recording Called");

            DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
            var PostsTracked = PostDaywiseEngagementDAL.GetPostsForDaywiseEngagement();

            if (PostsTracked.Rows.Count > 0)
            {
                foreach (DataRow row in PostsTracked.Rows)
                {
                    DateTime postDate = Convert.ToDateTime(row["time"]);
                    DateTime currentDate = DateTime.Now.Date;
                    int currentHour = DateTime.Now.Hour;
                    int postDurationinDays = (currentDate - postDate).Days;
                    bool isError = false;

                    if (postDurationinDays < 7)
                    {
                        PostDaywiseEngagementBO daywiseEngagement = new PostDaywiseEngagementBO();
                        daywiseEngagement.Hour = currentHour;
                        daywiseEngagement.Date = currentDate;
                        daywiseEngagement.DocumentId = Convert.ToInt32(row["Doc_Id"]);
                        daywiseEngagement.Media_DocumentId = Convert.ToInt32(row["Doc_MediaId"]);
                        daywiseEngagement.DocumentSourceId = Convert.ToInt32(row["DocumentSource_Id"]);
                        daywiseEngagement.SearchTermId = Convert.ToInt32(row["SearchTermId"]);

                        //Twitter
                        if (row["DocumentSourceType_Id"].ToString() == "1")
                        {
                            String cK = AppSettings.TwitterSearch_ConsumerKey;
                            String cS = AppSettings.TwitterSearch_ConsumerSecret;
                            String aT = AppSettings.TwitterSearch_Token;
                            String aS = AppSettings.TwitterSearch_TokenSecret;

                            try
                            {
                                isError = false;

                                TweetSharp.TwitterService service = new TweetSharp.TwitterService(cK, cS);
                                service.AuthenticateWith(aT, aS);
                                TweetSharp.SearchOptions so = new TweetSharp.SearchOptions();
                                TweetSharp.TwitterStatus tweet = service.GetTweet(new GetTweetOptions() { Id = Convert.ToInt64(row["WebSourceId"]) });

                                daywiseEngagement.Likes = (tweet.RetweetedStatus != null ? tweet.RetweetedStatus.FavoriteCount : tweet.FavoriteCount);
                                daywiseEngagement.Retweets = (tweet.RetweetedStatus != null ? tweet.RetweetedStatus.RetweetCount : tweet.RetweetCount);
                            }
                            catch
                            {
                                isError = true;
                            }
                        }

                        //Facebook
                        else if (row["DocumentSourceType_Id"].ToString() == "2")
                        {
                            try
                            {
                                isError = false;

                                JsonObject Views = GetPostEngagement(row["WebSourceId"].ToString(), "View");
                                if (Views.Keys.Contains("data"))
                                {
                                    JsonArray data = (JsonArray)Views["data"];
                                    if (data.Count != 0)
                                    {
                                        JsonObject obj = (JsonObject)data[0];
                                        if (obj.Keys.Contains("values"))
                                        {
                                            JsonArray obj2 = (JsonArray)obj["values"];
                                            if (obj2.Count != 0)
                                            {
                                                JsonObject obj_view = (JsonObject)obj2[0];
                                                if (obj_view.Keys.Contains("value"))
                                                    daywiseEngagement.Views = Convert.ToInt32(obj_view["value"]);
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                isError = true;
                            }

                            try
                            {
                                isError = false;

                                JsonObject Comments = GetPostEngagement(row["WebSourceId"].ToString(), "Comment");
                                if (Comments.Keys.Contains("summary"))
                                {
                                    JsonObject obj = (JsonObject)Comments["summary"];
                                    daywiseEngagement.Comments = Convert.ToInt32(obj["total_count"]);
                                }
                            }
                            catch
                            {
                                isError = true;
                            }

                            try
                            {
                                isError = false;

                                JsonObject Likes = GetPostEngagement(row["WebSourceId"].ToString(), "Like");
                                if (Likes.Keys.Contains("summary"))
                                {
                                    JsonObject obj = (JsonObject)Likes["summary"];
                                    daywiseEngagement.Likes = Convert.ToInt32(obj["total_count"]);
                                }
                            }
                            catch
                            {
                                isError = true;
                            }

                            try
                            {
                                isError = false;

                                JsonObject Shares = GetPostEngagement(row["WebSourceId"].ToString(), "Share");
                                if (Shares.Keys.Contains("shares"))
                                {
                                    JsonObject obj = (JsonObject)Shares["shares"];
                                    daywiseEngagement.Shares = Convert.ToInt32(obj["count"]);
                                }
                            }
                            catch
                            {
                                isError = true;
                            }
                        }

                        if (!isError)
                            PostDaywiseEngagementDAL.UpdatePostsDaywiseEngagement(daywiseEngagement);
                    }
                }
            }

            Util.Log.WriteOperationStarted(GetType(), "Execute", "Post Daywise Engagement Recorded --- Posts Counts " + PostsTracked.Rows.Count);
            //}

            //catch (Exception ex)
            //{
            //    Util.Log.WriteException(GetType(), "Execute", ex);
            //}
        }

        Dictionary<string, object> GetParameters()
        {
            string accessToken = AppSettings.Facebook_AccessToken_User;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("access_token", accessToken);

            return parameters;
        }

        JsonObject GetPostEngagement(string WebSourceId, string type)
        {
            FacebookClient client = new FacebookClient();
            JsonObject obj = new JsonObject();

            if (type == "View")
                obj = (JsonObject)client.Get(WebSourceId + "/insights/post_video_views?summary=true&period=lifetime&access_token=", GetParameters());

            else if (type == "Comment")
                obj = (JsonObject)client.Get(WebSourceId + "/comments?summary=true&access_token=", GetParameters());

            else if (type == "Like")
                obj = (JsonObject)client.Get(WebSourceId + "/reactions?summary=true&access_token=", GetParameters());

            else if (type == "Share")
                obj = (JsonObject)client.Get(WebSourceId + "/?fields=shares&access_token=", GetParameters());

            return obj;
        }

    }
}