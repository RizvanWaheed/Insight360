using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insight360.BO;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Xml;
using Insight360.Util;
using System.ServiceModel.Syndication;
using Insight360.Common.BO;

namespace Insight360.DAL
{
    public static class StagingDAL
    {
        #region Twitter
        public static void PersistTwitterStatus(TwitterStatus status, int searchTermID, bool isRefresh, bool isCompleted, int documentsourceid)
        {
            try
            {
                XmlDocument xml = new XmlDocument();

                XmlNode root = xml.AppendChild(xml.CreateElement("Tweet"));

                root.AppendChild(xml.CreateElement("id")).InnerText = status.Id.ToString();
                root.AppendChild(xml.CreateElement("text")).InnerText = status.Text;
                root.AppendChild(xml.CreateElement("retweet_count")).InnerText = status.RetweetCount.ToString();
                root.AppendChild(xml.CreateElement("likes_count")).InnerText = status.LikesCount.ToString();
                root.AppendChild(xml.CreateElement("created_at")).InnerText = status.CreatedAt.ToString();
                root.AppendChild(xml.CreateElement("from_user")).InnerText = status.FromUser;
                root.AppendChild(xml.CreateElement("from_user_id")).InnerText = status.FromUserId;
                root.AppendChild(xml.CreateElement("to_user")).InnerText = status.ToUser ?? string.Empty;
                root.AppendChild(xml.CreateElement("to_user_id")).InnerText = status.ToUserId ?? string.Empty;
                root.AppendChild(xml.CreateElement("iso_language_code")).InnerText = status.IsoLanguageCode ?? string.Empty;
                root.AppendChild(xml.CreateElement("source")).InnerText = status.Source ?? string.Empty;
                root.AppendChild(xml.CreateElement("media_url")).InnerText = status.Media_Url ?? string.Empty;
                root.AppendChild(xml.CreateElement("media_url2")).InnerText = status.Media_Url2 ?? string.Empty;
                root.AppendChild(xml.CreateElement("media_url3")).InnerText = status.Media_Url3 ?? string.Empty;
                root.AppendChild(xml.CreateElement("media_cnt")).InnerText = status.Media_Cnt.ToString();

                //root.AppendChild(xml.CreateElement("profile_image_url")).InnerText = //status.ProgfileImageUrl ?? string.Empty;
                //root.AppendChild(xml.CreateElement("Staging_OrgName")).InnerText = //status.Staging.OrgName ?? string.Empty;

                root.AppendChild(xml.CreateElement("geo")).InnerText = status.Geo ?? string.Empty;
                root.AppendChild(xml.CreateElement("Staging_ProcessingState")).InnerText = ((int)Util.DataProcessingState.UnProcessed).ToString();
                root.AppendChild(xml.CreateElement("Staging_Followup")).InnerText = status.Staging.Followup.ToString();

                root.AppendChild(xml.CreateElement("sentiment")).InnerText = (status.Sentiment.HasValue ? status.Sentiment.Value.ToString() : null);
                root.AppendChild(xml.CreateElement("language_code")).InnerText = status.LanguageCode;
                root.AppendChild(xml.CreateElement("in_reply_to_status_id")).InnerText = status.InReplyToStatusId;
                root.AppendChild(xml.CreateElement("staging_documentsource_id")).InnerText = documentsourceid.ToString();

                if (status.FeedLabels != null && status.FeedLabels.Count != 0)
                    root.AppendChild(xml.CreateElement("FeedLabels")).InnerText = string.Join(",", status.FeedLabels);
                else
                    root.AppendChild(xml.CreateElement("FeedLabels")).InnerText = "0";

                //root.AppendChild(xml.CreateElement("isRefresh")).InnerText = //isRefresh.ToString();

                XmlNode user = root.AppendChild(xml.CreateElement("User"));
                user.AppendChild(xml.CreateElement("id")).InnerText = status.UserDetails.Id;
                user.AppendChild(xml.CreateElement("screen_name")).InnerText = status.UserDetails.ScreenName;

                user.AppendChild(xml.CreateElement("name")).InnerText = status.UserDetails.Name;
                user.AppendChild(xml.CreateElement("location")).InnerText = string.IsNullOrEmpty(status.UserDetails.Location) ? "Global" : status.UserDetails.Location;

                user.AppendChild(xml.CreateElement("description")).InnerText = status.UserDetails.Description;
                user.AppendChild(xml.CreateElement("profile_image_url")).InnerText = status.UserDetails.ProfileImageUrl;
                user.AppendChild(xml.CreateElement("url")).InnerText = status.UserDetails.Url;
                user.AppendChild(xml.CreateElement("protected")).InnerText = status.UserDetails.Protected.ToString();
                user.AppendChild(xml.CreateElement("followers_count")).InnerText = status.UserDetails.FollowersCount.ToString();
                user.AppendChild(xml.CreateElement("friends_count")).InnerText = status.UserDetails.FriendsCount.ToString();
                user.AppendChild(xml.CreateElement("created_at")).InnerText = status.UserDetails.CreatedAt.ToString();
                user.AppendChild(xml.CreateElement("favourites_count")).InnerText = status.UserDetails.FavouritesCount.ToString();
                user.AppendChild(xml.CreateElement("utc_offset")).InnerText = status.UserDetails.UtcOffset;
                user.AppendChild(xml.CreateElement("time_zone")).InnerText = status.UserDetails.TimeZone;
                user.AppendChild(xml.CreateElement("geo_enabled")).InnerText = status.UserDetails.GeoEnabled.ToString();
                user.AppendChild(xml.CreateElement("statuses_count")).InnerText = status.UserDetails.StatusesCount.ToString();

                user.AppendChild(xml.CreateElement("lang")).InnerText = status.UserDetails.Lang;
                user.AppendChild(xml.CreateElement("contributors_enabled")).InnerText = status.UserDetails.ContributorsEnabled.ToString();
                user.AppendChild(xml.CreateElement("listed_count")).InnerText = status.UserDetails.ListedCount.ToString();
                user.AppendChild(xml.CreateElement("following")).InnerText = status.UserDetails.Following.ToString();
                user.AppendChild(xml.CreateElement("notifications")).InnerText = status.UserDetails.Notifications.ToString();
                user.AppendChild(xml.CreateElement("is_verified")).InnerText = status.UserDetails.IsVerified.ToString();
                user.AppendChild(xml.CreateElement("profile_bg_color")).InnerText = status.UserDetails.ProfileBgColor;
                user.AppendChild(xml.CreateElement("profile_bg_img_url")).InnerText = status.UserDetails.ProfileBgImageUrl;
                user.AppendChild(xml.CreateElement("profile_link_color")).InnerText = status.UserDetails.ProfileLinkColor;
                user.AppendChild(xml.CreateElement("profile_bg_tiled")).InnerText = status.UserDetails.ProfileBgTiled.ToString();
                user.AppendChild(xml.CreateElement("profile_txt_color")).InnerText = status.UserDetails.ProfileTxtColor;
                user.AppendChild(xml.CreateElement("gender")).InnerText = status.UserDetails.Gender;

                //user.AppendChild(xml.CreateElement("isCompleted")).InnerText = //    isCompleted.ToString();

                root.AppendChild(user);

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@RecvReqMsg", xml.OuterXml));
                parameters.Add(new SqlParameter("@keywords_ids", searchTermID));
                parameters.Add(new SqlParameter("@user_isCompleted", isCompleted.ToString()));
                parameters.Add(new SqlParameter("@isRefresh", isRefresh.ToString()));

                var guid = Guid.NewGuid().ToString();
                DBUtil.ExecuteNonQuery(CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Twitter_AddStatus), parameters, null, CommandType.StoredProcedure, guid);
                DBUtil.CloseConnection(guid);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DBUtil), "Persist Twitter Status", ex);
            }
        }

        public static void UpdateTwitterStatusDetailsAndUser(TwitterStatus status)
        {
            var guid = Guid.NewGuid().ToString();
            string query = "UPDATE " + CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Twitter_Statuses)
            + " SET [Staging_ProcessingState] = " + (int)DataProcessingState.UnProcessed
            + " ,[Staging_UpdateTime] = GETDATE()    "
            + " ,[in_reply_to_status_id] = @in_reply_to_status_id"
            + " ,[in_reply_to_user_id] = @in_reply_to_user_id"
            + " ,[in_reply_to_screen_name] = @in_reply_to_screen_name"
            + " ,[favorited] = @favorited"
            + " ,[truncated] = @truncated"
            + " WHERE id =@id";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@in_reply_to_status_id", status.InReplyToStatusId ?? string.Empty));
            parameters.Add(new SqlParameter("@in_reply_to_user_id", status.InReplyToUserId ?? string.Empty));
            parameters.Add(new SqlParameter("@in_reply_to_screen_name", status.InReplyToScreenName ?? string.Empty));
            parameters.Add(new SqlParameter("@favorited", status.Favorited));
            parameters.Add(new SqlParameter("@truncated", status.Truncated));
            parameters.Add(new SqlParameter("@id", status.Id));

            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);

            parameters.Clear();
            parameters.Add(new SqlParameter("@id", status.UserDetails.Id));

            DBUtil.ExecuteNonQuery("delete from " + CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Twitter_Users)
                + " WHERE id=@id", parameters, null, CommandType.Text, guid);

            query = "INSERT INTO " + CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Twitter_Users)
                + " ([Id]"
                + " ,[name]"
                + " ,[screen_name]"
                + " ,[location]"
                + " ,[description]"
                + " ,[profile_image_url]"
                + " ,[url]"
                + " ,[protected]"
                + " ,[followers_count]"
                + " ,[friends_Count]"
                + " ,[created_at]"
                + " ,[favourites_count]"
                + " ,[utc_offset]"
                + " ,[time_zone]"
                + " ,[geo_enabled]"
                + " ,[statuses_count]"
                + " ,[lang]"
                + " ,[contributors_enabled]"
                + " ,[listed_count]"
                + " ,[following]"
                + " ,[is_verified]"
                + " ,[profile_bg_color]"
                + " ,[profile_bg_img_url]"
                + " ,[profile_bg_tiled]"
                + " ,[profile_link_color]"
                + " ,[profile_txt_color]"
                + " ,[notifications])"
                + " VALUES(@id"
                   + " ,@name"
                   + " ,@screen_name"
                   + " ,@location"
                   + " ,@description"
                   + " ,@profile_image_url"
                   + " ,@url"
                   + " ,@protected"
                   + " ,@followers_count"
                   + " ,@friends_Count"
                   + " ,@created_at"
                   + " ,@favourites_count"
                   + " ,@utc_offset"
                   + " ,@time_zone"
                   + " ,@geo_enabled"
                   + " ,@statuses_count"
                   + " ,@lang"
                   + " ,@contributors_enabled"
                   + " ,@listed_count"
                   + " ,@following"
                   + " ,@isverified"
                   + " ,@profilebgcolor"
                   + " ,@profilebgimageurl"
                   + " ,@profilebgtiled"
                   + " ,@profilelinkcolor"
                   + " ,@profiletxtcolor"
                   + " ,@notifications)";


            parameters.Clear();
            parameters.Add(new SqlParameter("@id", status.UserDetails.Id));
            parameters.Add(new SqlParameter("@name", status.UserDetails.Name ?? string.Empty));
            parameters.Add(new SqlParameter("@screen_name", status.UserDetails.ScreenName ?? string.Empty));
            parameters.Add(new SqlParameter("@location", string.IsNullOrEmpty(status.UserDetails.Location) ? "Global" : status.UserDetails.Location));
            parameters.Add(new SqlParameter("@description", status.UserDetails.Description ?? string.Empty));
            parameters.Add(new SqlParameter("@profile_image_url", status.UserDetails.ProfileImageUrl ?? string.Empty));
            parameters.Add(new SqlParameter("@url", status.UserDetails.Url ?? string.Empty));
            parameters.Add(new SqlParameter("@protected", status.UserDetails.Protected ?? false));
            parameters.Add(new SqlParameter("@followers_count", status.UserDetails.FollowersCount));
            parameters.Add(new SqlParameter("@friends_Count", status.UserDetails.FriendsCount));
            parameters.Add(new SqlParameter("@created_at", status.UserDetails.CreatedAt));
            parameters.Add(new SqlParameter("@favourites_count", status.UserDetails.FavouritesCount));
            parameters.Add(new SqlParameter("@utc_offset", status.UserDetails.UtcOffset ?? string.Empty));
            parameters.Add(new SqlParameter("@time_zone", status.UserDetails.TimeZone ?? string.Empty));
            parameters.Add(new SqlParameter("@geo_enabled", status.UserDetails.GeoEnabled ?? false));
            parameters.Add(new SqlParameter("@statuses_count", status.UserDetails.StatusesCount));
            parameters.Add(new SqlParameter("@lang", status.UserDetails.Lang ?? string.Empty));
            parameters.Add(new SqlParameter("@contributors_enabled", status.UserDetails.ContributorsEnabled ?? false));
            parameters.Add(new SqlParameter("@notifications", status.UserDetails.Notifications ?? false));
            parameters.Add(new SqlParameter("@following", status.UserDetails.Following ?? false));
            parameters.Add(new SqlParameter("@listed_count", status.UserDetails.ListedCount));
            parameters.Add(new SqlParameter("@isverified", status.UserDetails.IsVerified));
            parameters.Add(new SqlParameter("@profilebgcolor", status.UserDetails.ProfileBgColor));
            parameters.Add(new SqlParameter("@profilebgimageurl", status.UserDetails.ProfileBgImageUrl));
            parameters.Add(new SqlParameter("@profilebgtiled", status.UserDetails.ProfileBgTiled));
            parameters.Add(new SqlParameter("@profilelinkcolor", status.UserDetails.ProfileLinkColor));
            parameters.Add(new SqlParameter("@profiletxtcolor", status.UserDetails.ProfileTxtColor));

            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);
            DBUtil.CloseConnection(guid);
        }

        public static List<TwitterStatus> FetchTwitterStatusToUpdate(int topCount)
        {
            List<TwitterStatus> statuses = new List<TwitterStatus>();
            DataTable table = DBUtil.FetchRecords(
                string.Format("select top {0} id "
                    + " from {1} WITH(READPAST) "
                    + " where Staging_Refresh={2}"
                    + " order by Staging_UpdateTime"
                    , topCount
                    , CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Twitter_Statuses)
                    , (int)Util.DataProcessingState.Refresh
                    )
                , null, null, CommandType.Text);

            foreach (DataRow row in table.Rows)
            {
                statuses.Add(new TwitterStatus()
                {
                    Id = row["id"].ToString(),
                });

            }

            //UpdateTwitterDetailsProcessingState(
            //     statuses, DataProcessingState.Processing);
            CommonDAL.UpdateStagingUpdateTime(CommonDAL.Tables.Staging_Twitter_Statuses,
                    (from p in statuses
                     select p.Staging.StagingId).ToList());
            return statuses;
        }

        public static void UpdateTwitterDetailsProcessingState(List<TwitterStatus> statuses, Util.DataProcessingState state)
        {
            if (statuses.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" id in (");
                for (int i = 0; i < statuses.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append("'" + statuses[i].Id + "'");
                }
                sb.Append(")");
                CommonDAL.UpdateProcessingState(CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Twitter_Statuses),
                    "Staging_ProcessingState_Details", "Staging_UpdateTime", sb.ToString(), state);
            }
        }

        public static void InitializeTwitterDetailsCollection()
        {
            var guid = Guid.NewGuid().ToString();
            DBUtil.ExecuteNonQuery(CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Twitter_InitializeDetailsCollection), null, null, CommandType.StoredProcedure, guid);
            DBUtil.CloseConnection(guid);
        }
        #endregion

        #region Facebook
        public static void PersistFacebookPostComments(long stagingPostId, List<FacebookCommentInfo> comments)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            var guid = Guid.NewGuid().ToString();
            string query = string.Format(
                              "delete from {0} where Staging_Facebook_Post_Comment_Id in"
                              + "(select staging_id from {1} where Staging_Facebook_Post_Id = @Staging_Facebook_Post_Id)",
                              CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Comment_Likes),
                              CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Comments));

            parameters.Add(new SqlParameter("@Staging_Facebook_Post_Id", stagingPostId));
            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);

            parameters.Clear();
            query = string.Format(
                              "delete from {0} where Staging_Facebook_Post_Id = @Staging_Facebook_Post_Id",
                              CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Comments));

            parameters.Add(new SqlParameter("@Staging_Facebook_Post_Id", stagingPostId));
            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);

            query = string.Format(
                           "insert into {0} ([id],[from_name],[from_id],[message],[created_time],[Staging_Facebook_Post_Id],[Staging_LikesCount]) " +
                           " VALUES(@id,@from_name,@from_id,@message,@created_time,@Staging_Facebook_Post_Id,@Staging_LikesCount); select SCOPE_IDENTITY()",
                           CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Comments));

            for (int i = 0; i < comments.Count; i++)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@id", comments[i].Id));
                parameters.Add(new SqlParameter("@from_id", comments[i].From.Id));
                parameters.Add(new SqlParameter("@from_name", comments[i].From.Name));
                parameters.Add(new SqlParameter("@message", comments[i].Message));
                parameters.Add(new SqlParameter("@created_time", comments[i].CreatedTime));
                parameters.Add(new SqlParameter("@Staging_Facebook_Post_Id", stagingPostId));
                parameters.Add(new SqlParameter("@Staging_LikesCount", comments[i].Likes.Count));

                long id = -1;
                object obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.Text, guid);
                if (obj != null)
                    id = Convert.ToInt64(obj);

                if (id != -1)
                    PersistFacebookLikes(id, comments[i].Likes, false);
            }
            DBUtil.CloseConnection(guid);
        }

        public static void PersistFacebookLikes(long stagingId, List<FacebookLikeInfo> likes, bool post)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            string query = string.Format(
                              "delete from {0} where Staging_Facebook_Post_Id = @Staging_Facebook_Post_Id",
                              CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Likes));

            var guid = Guid.NewGuid().ToString();

            if (post)
            {
                parameters.Add(new SqlParameter("@Staging_Facebook_Post_Id", stagingId));
                DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);
            }

            parameters.Clear();
            query = string.Format(
                              "insert into {0} ([id],[name],{1}) " +
                              " VALUES(@id,@name,@{1})",
                              post ? CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Likes) : CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Post_Comment_Likes),
                              post ? "Staging_Facebook_Post_Id" : "Staging_Facebook_Post_Comment_Id");

            for (int i = 0; i < likes.Count; i++)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@id", likes[i].Id));
                parameters.Add(new SqlParameter("@name", likes[i].Name));
                parameters.Add(new SqlParameter(post ? "@Staging_Facebook_Post_Id" : "@Staging_Facebook_Post_Comment_Id", stagingId));
                DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);
            }

            DBUtil.CloseConnection(guid);
        }

        public static bool PersistFacebookPost(FacebookPost post, bool GetCommentsAndLikes, bool isSearch, int searchTermId)//,bool checkExisting)
        {
            long id = -1;
            List<SqlParameter> parameters = new List<SqlParameter>();
            object obj = null;

            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Facebook_AddPost);

            parameters.Clear();

            parameters.Add(new SqlParameter("@id", post.Id));
            parameters.Add(new SqlParameter("@from_id", post.From.Id));
            parameters.Add(new SqlParameter("@from_name", post.From.Name));
            parameters.Add(new SqlParameter("@message", post.Message));
            parameters.Add(new SqlParameter("@type", post.Type));
            parameters.Add(new SqlParameter("@picture", post.Picture));
            parameters.Add(new SqlParameter("@link", post.Link));
            parameters.Add(new SqlParameter("@name", post.Name));
            parameters.Add(new SqlParameter("@caption", post.Caption));
            parameters.Add(new SqlParameter("@description", post.Description));
            parameters.Add(new SqlParameter("@icon", post.Icon));
            parameters.Add(new SqlParameter("@attribution", post.Attribution));
            parameters.Add(new SqlParameter("@created_time", post.CreatedTime));
            parameters.Add(new SqlParameter("@updated_time", post.UpdatedTime));
            parameters.Add(new SqlParameter("@targeting_country", post.Targeting.Country));
            parameters.Add(new SqlParameter("@targeting_city", post.Targeting.City));
            parameters.Add(new SqlParameter("@targeting_region", post.Targeting.Region));
            parameters.Add(new SqlParameter("@targeting_locale", post.Targeting.Locale));
            parameters.Add(new SqlParameter("@likes_count", post.LikesCount));
            parameters.Add(new SqlParameter("@shares_count", post.SharesCount));
            parameters.Add(new SqlParameter("@comments_count", post.CommentsCount));
            parameters.Add(new SqlParameter("@Staging_Followup", post.Staging.Followup));
            parameters.Add(new SqlParameter("@Staging_CommentsCount", post.Comments.Count));
            parameters.Add(new SqlParameter("@Staging_LikesCount", post.Likes.Count));
            parameters.Add(new SqlParameter("@Staging_DocumentSource_Id", post.Staging.DocumentSourceId));
            parameters.Add(new SqlParameter("@Staging_SearchTerm_Id", searchTermId));
            parameters.Add(new SqlParameter("@GetCommentsAndLikes", GetCommentsAndLikes));
            parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
            parameters.Add(new SqlParameter("@isSearch", isSearch));
            parameters.Add(new SqlParameter("@Sentiment", post.Sentiment));
            parameters.Add(new SqlParameter("@LanguageCode", post.LanguageCode));
            parameters.Add(new SqlParameter("@location", post.PageLocation.name));

            //adding params related to user
            parameters.Add(new SqlParameter("@user_ID", post.user.Id));
            parameters.Add(new SqlParameter("@user_name", post.user.Name));
            parameters.Add(new SqlParameter("@user_first_name", post.user.FirstName));
            parameters.Add(new SqlParameter("@user_last_name", post.user.LastName));
            parameters.Add(new SqlParameter("@user_gender", post.user.Gender));
            parameters.Add(new SqlParameter("@user_locale", post.user.Locale));
            parameters.Add(new SqlParameter("@user_link", post.user.Link));
            parameters.Add(new SqlParameter("@user_third_party_id", post.user.ThirdPartyId));
            parameters.Add(new SqlParameter("@user_picture", post.user.Picture));
            parameters.Add(new SqlParameter("@user_updated_time", post.user.UpdatedTime));
            parameters.Add(new SqlParameter("@user_time_zone", post.user.TimeZone));
            parameters.Add(new SqlParameter("@user_followers_count", Convert.ToInt64(post.user.FollowersCount)));
            parameters.Add(new SqlParameter("@user_following_count", Convert.ToInt64(post.user.FollowingCount)));
            parameters.Add(new SqlParameter("@location_name", string.IsNullOrEmpty(post.user.Location) ? "Global" : post.user.Location));


            parameters.Add(new SqlParameter("@to_user_ID", post.toUser.Id));
            parameters.Add(new SqlParameter("@to_user_name", post.toUser.Name));
            parameters.Add(new SqlParameter("@to_user_first_name", post.toUser.FirstName));
            parameters.Add(new SqlParameter("@to_user_last_name", post.toUser.LastName));
            parameters.Add(new SqlParameter("@to_user_gender", post.toUser.Gender));
            parameters.Add(new SqlParameter("@to_user_locale", post.toUser.Locale));
            parameters.Add(new SqlParameter("@to_user_link", post.toUser.Link));
            parameters.Add(new SqlParameter("@to_user_third_party_id", post.toUser.ThirdPartyId));
            parameters.Add(new SqlParameter("@to_user_picture", post.toUser.Picture));
            parameters.Add(new SqlParameter("@to_user_updated_time", post.toUser.UpdatedTime));
            parameters.Add(new SqlParameter("@to_user_time_zone", post.toUser.TimeZone));
            parameters.Add(new SqlParameter("@to_user_followers_count", Convert.ToInt64(post.toUser.FollowersCount)));
            parameters.Add(new SqlParameter("@to_user_following_count", Convert.ToInt64(post.toUser.FollowingCount)));


            var uniqueCommentersCount = (from x in post.Comments
                                         select x.From.Id).Distinct().Count();

            parameters.Add(new SqlParameter("@Staging_UniqueCommentersCount", uniqueCommentersCount));

            DataTable likesDataTable = new DataTable();
            likesDataTable.Columns.Add(new DataColumn("id", typeof(String)));
            likesDataTable.Columns.Add(new DataColumn("name", typeof(string)));

            foreach (var like in post.Likes)
            {
                likesDataTable.Rows.Add(like.Id, like.Name);
            }

            DataTable commentsDataTable = new DataTable();
            commentsDataTable.Columns.Add(new DataColumn("id", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("from_name", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("from_id", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("message", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("created_time", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("Staging_LikesCount", typeof(int)));
            commentsDataTable.Columns.Add(new DataColumn("Sentiment", typeof(int)));
            commentsDataTable.Columns.Add(new DataColumn("LanguageCode", typeof(string)));

            DataTable commentLikesDataTable = new DataTable();
            commentLikesDataTable.Columns.Add(new DataColumn("id", typeof(String)));
            commentLikesDataTable.Columns.Add(new DataColumn("name", typeof(string)));
            commentLikesDataTable.Columns.Add(new DataColumn("comment_id", typeof(string)));

            DataTable postCommentLabelsDataTable = new DataTable();
            postCommentLabelsDataTable.Columns.Add(new DataColumn("Label_Id", typeof(string)));
            postCommentLabelsDataTable.Columns.Add(new DataColumn("Comment_Id", typeof(string)));

            foreach (var comment in post.Comments)
            {
                commentsDataTable.Rows.Add(comment.Id, comment.From.Name, comment.From.Id, comment.Message, comment.CreatedTime, 0, comment.Sentiment, comment.LanguageCode);

                foreach (var label in comment.FeedLabels)
                {
                    postCommentLabelsDataTable.Rows.Add(label, comment.Id);
                }

                //foreach (var like in comment.Likes)
                //{
                //    commentLikesDataTable.Rows.Add(like.Id, like.Name, comment.Id);
                //}
            }

            SqlParameter tvp = new SqlParameter("@Likes", likesDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            parameters.Add(tvp);

            tvp = new SqlParameter("@Comments", commentsDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            parameters.Add(tvp);

            tvp = new SqlParameter("@CommentLikes", commentLikesDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            parameters.Add(tvp);

            tvp = new SqlParameter("@PostCommentLabels", postCommentLabelsDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            parameters.Add(tvp);


            obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);
            if (obj != null)
                id = Convert.ToInt64(obj);
            else
                return false;

            return true;
        }

        public static void UpdateFacebookUser(FacebookUser user)
        {
            string query = "UPDATE " + CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Users)
                + " SET [name] = @name"
                + " ,[first_name] = @first_name"
                + " ,[last_name] = @last_name"
                + " ,[gender] = @gender"
                + " ,[locale] = @locale"
                + " ,[link] = @link"
                + " ,[third_party_id] = @third_party_id"
                + " ,[updated_time] = @updated_time"
                + " ,[picture] = @picture"
                + " ,[Staging_UpdateTime] = GETDATE()"
                + " ,[Staging_ProcessingState] = " + (int)Util.DataProcessingState.Processed
                + " WHERE id=@id";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@name", user.Name));
            parameters.Add(new SqlParameter("@first_name", user.FirstName));
            parameters.Add(new SqlParameter("@last_name", user.LastName));
            parameters.Add(new SqlParameter("@gender", user.Gender));
            parameters.Add(new SqlParameter("@locale", user.Locale));
            parameters.Add(new SqlParameter("@link", user.Link));
            parameters.Add(new SqlParameter("@third_party_id", user.ThirdPartyId));
            parameters.Add(new SqlParameter("@updated_time", user.UpdatedTime));
            parameters.Add(new SqlParameter("@picture", user.Picture));
            parameters.Add(new SqlParameter("@id", user.Id));

            var guid = Guid.NewGuid().ToString();
            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.Text, guid);
            DBUtil.CloseConnection(guid);
        }

        public static List<FacebookUser> FetchFacebookUsersToUpdate(int topCount)
        {
            List<FacebookUser> users = new List<FacebookUser>();
            DataTable table = DBUtil.FetchRecords(
                string.Format("select top {0} * "
                    + " from {1} WITH(READPAST) "
                    + " where Staging_ProcessingState={2}"
                    + " order by updated_time desc"
                    , topCount
                    , CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Users)
                    , (int)Util.DataProcessingState.UnProcessed
                    )
                , null, null, CommandType.Text);

            foreach (DataRow row in table.Rows)
            {
                DateTime dt;
                DateTime.TryParse(row["updated_time"].ToString(), out dt);
                users.Add(new FacebookUser()
                {
                    Id = row["id"].ToString(),
                    FirstName = row["first_name"].ToString(),
                    LastName = row["last_name"].ToString(),
                    Link = row["link"].ToString(),
                    Locale = row["locale"].ToString(),
                    Name = row["name"].ToString(),
                    Picture = row["picture"].ToString(),
                    ThirdPartyId = row["third_party_id"].ToString(),
                    UpdatedTime = dt,
                });

            }

            UpdateFacebookUserProcessingState(
                 users, DataProcessingState.Processing);
            return users;
        }

        public static void UpdateFacebookUserProcessingState(List<FacebookUser> users, Util.DataProcessingState state)
        {
            if (users.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" id in (");
                for (int i = 0; i < users.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append("'" + users[i].Id + "'");
                }
                sb.Append(")");
                CommonDAL.UpdateProcessingState(CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Users),
                    "Staging_ProcessingState", "Staging_UpdateTime", sb.ToString(), state);
            }
        }
        public static string GetActiveSearchTerms()
        {
            return @" select Id from dbo.Config_SearchTerm  with (nolock) where IsEnabled  =1 ";
        }
        public static List<FacebookPost> FetchFacebookPostsToUpdate(int topCount)
        {
            List<FacebookPost> posts = new List<FacebookPost>();
            var query = string.Format("select top {0} staging_id, from_id, id,staging_followup,staging_orgname,Staging_DocumentSource_Id "
                    + " from {1} WITH(nolock) "
                    + " where Staging_refresh=1 and Staging_SearchTerm_Id in( " + GetActiveSearchTerms() + " )"
                    //	+ " --AND datediff(hour,Staging_UpdateTime,getdate())>=1 "
                    + " order by Staging_UpdateTime"
                    , topCount
                    , CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Posts)
                    , (int)Util.DataProcessingState.UnProcessed
                    );
            DataTable table = DBUtil.FetchRecords(query

                , null, null, CommandType.Text);

            foreach (DataRow row in table.Rows)
            {
                posts.Add(new FacebookPost()
                {
                    Id = row["id"].ToString(),
                    Staging = new StagingInfo()
                    {
                        Followup = Convert.ToBoolean(row["Staging_Followup"]),
                        //OrgName = row["Staging_OrgName"].ToString(),
                        StagingId = (long)row["Staging_Id"],
                        DocumentSourceId = Convert.ToInt32(row["Staging_DocumentSource_Id"]),
                    },
                    From = new FacebookUserInfo()
                    {
                        Id = row["from_id"].ToString()
                    }

                });

            }

            CommonDAL.UpdateStagingUpdateTime(CommonDAL.Tables.Staging_Facebook_Posts,
                    (from p in posts
                     select p.Staging.StagingId).ToList());
            return posts;
        }

        public static void UpdateFacebookPostsProcessingState(List<FacebookPost> posts, Util.DataProcessingState state)
        {
            if (posts.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" id in (");
                for (int i = 0; i < posts.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append("'" + posts[i].Id + "'");
                }
                sb.Append(")");
                CommonDAL.UpdateProcessingState(CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_Facebook_Posts),
                    "Staging_ProcessingState_Refresh", "Staging_UpdateTime", sb.ToString(), state);
            }
        }

        public static void InitializeFacebookUserCollection()
        {
            var guid = Guid.NewGuid().ToString();
            DBUtil.ExecuteNonQuery(CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Facebook_InitializeUserCollection), null, null, CommandType.StoredProcedure, guid);
            DBUtil.CloseConnection(guid);
        }
        #endregion

        #region Instagram
        public static void PersistInstagramUser()
        {

        }

        public static long PersistInstagramPost(Insight360.Common.BO.InstagramPost post, int searchTermId, bool isSearch, int DocumentSourceId)
        {
            try
            {

                List<SqlParameter> parameters = new List<SqlParameter>();
                string query;
                object obj = null;
                long id = -1;

                query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Instagram_AddPost);

                parameters.Clear();

                parameters.Add(new SqlParameter("@attribution", post.attribution == null ? null : post.attribution.ToString()));
                parameters.Add(new SqlParameter("@tags", post.alltags));
                parameters.Add(new SqlParameter("@type", post.type));
                parameters.Add(new SqlParameter("@filter", post.filter));
                parameters.Add(new SqlParameter("@created_time", post.created_time_obj));
                parameters.Add(new SqlParameter("@link", post.link));
                parameters.Add(new SqlParameter("@image_url", post.images.standard_resolution.url));
                parameters.Add(new SqlParameter("@caption", post.caption.text));
                parameters.Add(new SqlParameter("@post_id", post.id));
                parameters.Add(new SqlParameter("@updated", null));
                parameters.Add(new SqlParameter("@language", post.caption.language));
                parameters.Add(new SqlParameter("@location_name", post.location != null ? post.location.name : "Global"));
                parameters.Add(new SqlParameter("@location_latitude", post.location != null ? post.location.latitude : 0));
                parameters.Add(new SqlParameter("@location_longitude", post.location != null ? post.location.longitude : 0));
                parameters.Add(new SqlParameter("@location_id", post.location != null ? post.location.id : 0));
                parameters.Add(new SqlParameter("@sentiment", post.caption.sentiment));
                parameters.Add(new SqlParameter("@commentsUpdated", post.created_time_obj));
                parameters.Add(new SqlParameter("@type_Id", 1));
                parameters.Add(new SqlParameter("@staging_UserID", post.user.id));
                parameters.Add(new SqlParameter("@user_id", post.user.id));
                parameters.Add(new SqlParameter("@user_picture", post.user.profile_picture));
                parameters.Add(new SqlParameter("@likes_count", post.likes.count));
                parameters.Add(new SqlParameter("@comments_count", post.comments != null ? post.comments.count : 0));
                parameters.Add(new SqlParameter("@user_name", post.user.username));
                parameters.Add(new SqlParameter("@user_gender", post.user.gender));
                parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
                parameters.Add(new SqlParameter("@DocumentSourceId", DocumentSourceId));
                parameters.Add(new SqlParameter("@isSearch", isSearch));

                obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);

                if (obj != null)
                {
                    id = Convert.ToInt64(obj);
                }

                if (post.commentsData.data.Count > 0)
                {
                    long comment_id = 0;
                    for (int i = 0; i < post.commentsData.data.Count; i++)
                    {
                        var taskSentiment = Helpers.SentimentScoreAsync(post.caption == null ? "" : post.commentsData.data[i].text);
                        var taskLanguage = Helpers.DetectLanguageAsync(post.caption == null ? "" : post.commentsData.data[i].text);

                        post.commentsData.data[i].sentiment = taskSentiment.Result == null ? 3 : Convert.ToInt32(taskSentiment.Result.ToString());
                        post.commentsData.data[i].language = taskLanguage.Result == null ? "na" : taskLanguage.Result.ToString();

                        comment_id = PersistInstagramComment(post.commentsData.data[i], post.id);
                    }
                }

                //}
                return id;
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(StagingDAL), "PersistInstagramPost", ex);
                return -99;
            }

        }
        public static long PersistInstagramComment(Insight360.Common.BO.CommentData comment, string postId)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            string query;
            object obj = null;
            long id = -1;

            query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Instagram_AddComment);

            parameters.Clear();

            parameters.Add(new SqlParameter("@text", comment.text));
            parameters.Add(new SqlParameter("@created_time", UnixTimeStampToDateTime(Convert.ToDouble(comment.created_time))));
            parameters.Add(new SqlParameter("@comment_id", Convert.ToDouble(comment.id)));
            parameters.Add(new SqlParameter("@language", comment.language));
            parameters.Add(new SqlParameter("@sentiment", comment.sentiment));
            parameters.Add(new SqlParameter("@user_id", comment.from.id));
            parameters.Add(new SqlParameter("@user_name", comment.from.username));
            parameters.Add(new SqlParameter("@profile_picture", comment.from.profile_picture));
            parameters.Add(new SqlParameter("@user_gender", comment.from.gender));
            parameters.Add(new SqlParameter("@post_id", postId));

            obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);
            if (obj != null)
                id = Convert.ToInt64(obj);
            return id;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        #endregion

        #region Feedly

        public static long PersistFeedlyItem(Insight360.BO.FeedlyItem item)
        {
            /*
            long id = -1;
            List<SqlParameter> Parameters = new List<SqlParameter>();
            string query;
            object obj = null;

            query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Youtube_AddVideo);
            Parameters.Add(new SqlParameter("@VideoTitle", vdo.VideoTitle));
            Parameters.Add(new SqlParameter("@YoutubeID", vdo.VideoID));
            Parameters.Add(new SqlParameter("@YoutubeURL", vdo.YoutubeURL));
            Parameters.Add(new SqlParameter("@ViewCount", vdo.ViewCount));
            Parameters.Add(new SqlParameter("@Author", vdo.Author));
            Parameters.Add(new SqlParameter("@CommentCount", vdo.CommentCount));
            Parameters.Add(new SqlParameter("@Description", vdo.Description));
            Parameters.Add(new SqlParameter("@Keywords", vdo.keywords));
            Parameters.Add(new SqlParameter("@Updated", vdo.Updated));
            Parameters.Add(new SqlParameter("@Published", vdo.Published));
            Parameters.Add(new SqlParameter("@Language", vdo.Language));
            Parameters.Add(new SqlParameter("@DateCreated", vdo.Staging_InsertTime));
            Parameters.Add(new SqlParameter("@DateModified", vdo.Staging_UpdateTime));
            Parameters.Add(new SqlParameter("@CommentsUpdated", vdo.CommentsUpdated));
            Parameters.Add(new SqlParameter("@Rating", vdo.Rating));
            Parameters.Add(new SqlParameter("@NumRaters", vdo.NumRaters));
            Parameters.Add(new SqlParameter("@FavoriteCount", vdo.FavoriteCount));
            Parameters.Add(new SqlParameter("@Likes", vdo.Likes));
            Parameters.Add(new SqlParameter("@DisLikes", vdo.DisLikes));
            Parameters.Add(new SqlParameter("@Thumbnails", vdo.Thumbnails));
            Parameters.Add(new SqlParameter("@Duration", vdo.Duration));
            Parameters.Add(new SqlParameter("@DurationText", vdo.DurationText));
            Parameters.Add(new SqlParameter("@Sentiment", vdo.Sentiment));
            Parameters.Add(new SqlParameter("@isSearch", isSearch));
            Parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
            Parameters.Add(new SqlParameter("@GetComments", GetComments));

            DataTable commentsDataTable = new DataTable();
            commentsDataTable.Columns.Add(new DataColumn("YouTubeCommentID", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Author", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Content", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Language", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Published", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("Updated", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("DateCreated", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("DateModified", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("Sentiment", typeof(int)));
            commentsDataTable.Columns.Add(new DataColumn("LanguageCode", typeof(string)));

            foreach (var comment in vdo.Comments)
            {
                commentsDataTable.Rows.Add(
                         comment.YouTubeCommentID,
                         comment.Title,
                         comment.Author,
                         comment.Content,
                         comment.Language,
                         comment.Published,
                         comment.Updated,
                         comment.DateCreated,
                         comment.DateModified,
                         comment.Sentiment,
                         comment.LanguageCode);

            }

            DataTable UserDataTable = new DataTable();
            UserDataTable.Columns.Add(new DataColumn("Author", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Age", typeof(int)));
            UserDataTable.Columns.Add(new DataColumn("Company", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Etag", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("FeedUri", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("FirstName", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("LastName", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Gender", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Hobbies", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Hometown", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Language", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Location", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Movies", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Music", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Occupation", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Relationship", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("School", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Username", typeof(string)));
            UserDataTable.Columns.Add(new DataColumn("Staging_InsertTime", typeof(DateTime)));
            UserDataTable.Columns.Add(new DataColumn("Staging_UpdateTime", typeof(DateTime)));
            UserDataTable.Columns.Add(new DataColumn("SubscriberCount", typeof(long)));
            UserDataTable.Columns.Add(new DataColumn("ViewCount", typeof(long)));


            var user = vdo.User;

            UserDataTable.Rows.Add(
                     user.Author,
                     user.Age,
                     user.Company,
                     user.Etag,
                     user.FeedUri,
                     user.FirstName,
                     user.LastName,
                     user.Gender,
                     user.Hobbies,
                     user.Hometown,
                     user.Language,
                     user.Location,
                     user.Movies,
                     user.Music,
                     user.Occupation,
                     user.Relationship,
                     user.School,
                     user.Username,
                     DateTime.Now,
                     DateTime.Now,
                     user.SubscriberCount,
                     user.ViewCount);



            SqlParameter tvp = new SqlParameter("@Comments", commentsDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            Parameters.Add(tvp);

            tvp = new SqlParameter("@User", UserDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            Parameters.Add(tvp);
            obj = DBUtil.ExecuteScalar(query, Parameters, null, CommandType.StoredProcedure);
            if (obj != null)
                id = Convert.ToInt64(obj);
            else
                return false;

            return true;
            */
            List<SqlParameter> parameters = new List<SqlParameter>();
            string query;
            object obj = null;
            long id = -1;

            query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Feedly_AddItem);

            parameters.Clear();

            parameters.Add(new SqlParameter("@Id", item.Id));
            parameters.Add(new SqlParameter("@DocumentSourceId", item.Staging_DocumentSource_Id));
            parameters.Add(new SqlParameter("@BaseUri", item.BaseUri));
            parameters.Add(new SqlParameter("@FingerPrint", item.FingerPrint));
            parameters.Add(new SqlParameter("@OriginId", item.OriginId));
            parameters.Add(new SqlParameter("@Title", item.Title));
            parameters.Add(new SqlParameter("@SummaryText", item.Summary_Text));
            parameters.Add(new SqlParameter("@ContentText", item.Content_Text));
            parameters.Add(new SqlParameter("@Keywords", item.Keywords));
            parameters.Add(new SqlParameter("@IsUnread", item.IsUnread));
            parameters.Add(new SqlParameter("@Author", item.Author));
            parameters.Add(new SqlParameter("@UpdateDate", item.UpdateDate));
            parameters.Add(new SqlParameter("@PublishDate", item.PublishDate));
            parameters.Add(new SqlParameter("@CrawledDate", item.CrawledDate));
            parameters.Add(new SqlParameter("@Sentiment", item.Sentiment));
            parameters.Add(new SqlParameter("@LanguageCode", item.LanguageCode));
            parameters.Add(new SqlParameter("@FeedId", item.Feed.FeedId));
            parameters.Add(new SqlParameter("@SearchTermId", item.Staging_SearchTerm_Id));

            //Feed - Parent of Items
            DataTable feedDataTable = new DataTable();
            feedDataTable.Columns.Add(new DataColumn("FeedId", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Website", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Description", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Subscribers", typeof(long)));
            feedDataTable.Columns.Add(new DataColumn("FacebookUserName", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("FacebookLikes", typeof(long)));
            feedDataTable.Columns.Add(new DataColumn("TwitterScreenName", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("TwitterFollowers", typeof(long)));
            feedDataTable.Columns.Add(new DataColumn("Coverage", typeof(decimal)));
            feedDataTable.Columns.Add(new DataColumn("CoverageScore", typeof(decimal)));
            feedDataTable.Columns.Add(new DataColumn("EstimatedEngagement", typeof(decimal)));
            feedDataTable.Columns.Add(new DataColumn("Velocity", typeof(decimal)));
            feedDataTable.Columns.Add(new DataColumn("Score", typeof(decimal)));
            feedDataTable.Columns.Add(new DataColumn("DeliciousTags", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Language", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Hint", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("Scheme", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("VisualUrl", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("CoverUrl", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("IconUrl", typeof(string)));
            feedDataTable.Columns.Add(new DataColumn("LastUpdated", typeof(DateTime)));
            feedDataTable.Rows.Add(
                item.Feed.FeedId,
                item.Feed.Title,
                item.Feed.Website,
                item.Feed.Description,
                item.Feed.Subscirbers,
                item.Feed.FacebookUserName,
                item.Feed.FacebookLikes,
                item.Feed.TwitterScreenName,
                item.Feed.TwitterFollowers,
                item.Feed.Coverage,
                item.Feed.CoverageScore,
                item.Feed.EstimatedEngagement,
                item.Feed.Velocity,
                item.Feed.Score,
                item.Feed.DeliciousTags,
                item.Feed.Language,
                item.Feed.Hint,
                item.Feed.Scheme,
                item.Feed.VisualUrl,
                item.Feed.CoverUrl,
                item.Feed.IconUrl,
                item.Feed.LastUpdated);
            parameters.Add(new SqlParameter("@Feed", feedDataTable));

            //Item links
            DataTable linksDataTable = new DataTable();
            linksDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
            linksDataTable.Columns.Add(new DataColumn("Uri", typeof(string)));
            linksDataTable.Columns.Add(new DataColumn("MediaType", typeof(string)));
            linksDataTable.Columns.Add(new DataColumn("RelationshipType", typeof(string)));
            linksDataTable.Columns.Add(new DataColumn("Width", typeof(int)));
            linksDataTable.Columns.Add(new DataColumn("Height", typeof(int)));
            foreach (var link in item.Links)
            {
                linksDataTable.Rows.Add(link.Title, link.Uri, link.MediaType, link.RelationshipType, link.Width, link.Height);
            }
            parameters.Add(new SqlParameter("@Links", linksDataTable));


            obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);
            if (obj != null)
                id = Convert.ToInt64(obj);
            return id;
        }


        #endregion

        #region RSS
        static string GenerateInsertQueryFromParameters(CommonDAL.Tables table, List<SqlParameter> parameters)
        {
            StringBuilder insertInto = new StringBuilder();
            StringBuilder values = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Value != null)
                {
                    if (insertInto.Length > 0)
                    {
                        insertInto.Append(",");
                        values.Append(",");
                    }
                    insertInto.Append(parameters[i].ParameterName.Substring(1));
                    values.Append(parameters[i].ParameterName);
                }
                else
                {
                    parameters.RemoveAt(i);
                    i--;
                }
            }

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2}); select SCOPE_IDENTITY()",
                CommonDAL.GetCommonTableName(table), insertInto.ToString(), values.ToString());
        }

        public static void PersistRssItem(SyndicationItem item, /*string organizationName,*/ int documentSourceId, List<int> SearchTerms, int? Sentiment, string LanguageCode, SyndicationFeed feed)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            var guid = Guid.NewGuid().ToString();
            //parameters.Add( new SqlParameter( "@Staging_OrgName" , organizationName ) );
            parameters.Add(new SqlParameter("@Staging_DocumentSource_Id", documentSourceId));
            parameters.Add(new SqlParameter("@Staging_InsertTime", DateTime.Now.ToUniversalTime()));
            parameters.Add(new SqlParameter("@Staging_UpdateTime", DateTime.Now.ToUniversalTime()));
            parameters.Add(new SqlParameter("@Staging_ProcessingState", "0"));
            parameters.Add(new SqlParameter("@Sentiment", Sentiment));
            parameters.Add(new SqlParameter("@LanguageCode", LanguageCode));

            if (item.Id != null)
                parameters.Add(new SqlParameter("@Id", item.Id));
            if (item.BaseUri != null)
                parameters.Add(new SqlParameter("@BaseUri", item.BaseUri.ToString()));
            if (item.Copyright != null)
            {
                parameters.Add(new SqlParameter("@Copyright_Text", item.Copyright != null ? item.Copyright.Text : null));
                parameters.Add(new SqlParameter("@Copyright_Type", item.Copyright != null ? item.Copyright.Type : null));
            }
            if (item.LastUpdatedTime.UtcDateTime.Year > 2000)
                parameters.Add(new SqlParameter("@LastUpdateTime", item.LastUpdatedTime.UtcDateTime));
            if (item.PublishDate.UtcDateTime.Year > 2000)
                parameters.Add(new SqlParameter("@PublishDate", item.PublishDate.UtcDateTime));
            if (item.Summary != null)
            {
                parameters.Add(new SqlParameter("@Summary_Text", item.Summary.Text));
                parameters.Add(new SqlParameter("@Summary_Type", item.Summary.Type));
            }
            if (item.Title != null)
            {
                parameters.Add(new SqlParameter("@Title_Text", item.Title.Text));
                parameters.Add(new SqlParameter("@Title_Type", item.Title.Type));
            }

            long stagingId = Convert.ToInt64(DBUtil.ExecuteScalar(
                GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Items, parameters)
                , parameters, null, CommandType.Text));

            foreach (var searchTermID in SearchTerms)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@Staging_Id", stagingId));
                parameters.Add(new SqlParameter("@SearchTerm_Id", searchTermID));
                DBUtil.ExecuteNonQuery(GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Items_SearchTerms, parameters), parameters, null, CommandType.Text, guid);
            }

            foreach (var link in item.Links)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@Staging_RSS_Item_Id", stagingId));

                parameters.Add(new SqlParameter("@Uri", link.Uri.ToString()));

                parameters.Add(new SqlParameter("@RelationshipType", link.RelationshipType));

                parameters.Add(new SqlParameter("@Title", link.Title));
                parameters.Add(new SqlParameter("@MediaType", link.MediaType));
                parameters.Add(new SqlParameter("@Length", link.Length));
                parameters.Add(new SqlParameter("@BaseUri", link.BaseUri != null ? link.BaseUri.ToString() : null));

                DBUtil.ExecuteNonQuery(GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Links, parameters), parameters, null, CommandType.Text, guid);
            }

            foreach (var category in item.Categories)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@Staging_RSS_Item_Id", stagingId));
                parameters.Add(new SqlParameter("@Name", category.Name));
                parameters.Add(new SqlParameter("@Scheme", category.Scheme));
                parameters.Add(new SqlParameter("@Label", category.Label));

                DBUtil.ExecuteNonQuery(GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Categories, parameters), parameters, null, CommandType.Text, guid);
            }

            if (item.Authors.Count == 0)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@Staging_RSS_Item_Id", stagingId));
                parameters.Add(new SqlParameter("@Email", ""));
                /**************OLD edit on 18 May 2012 *******************/
                //parameters.Add(new SqlParameter("@Name", person.Name));

                parameters.Add(new SqlParameter("@Name", feed.Title.Text));
                parameters.Add(new SqlParameter("@Uri", feed.ImageUrl.AbsolutePath));
                parameters.Add(new SqlParameter("@Staging_IsAuthor", true));

                DBUtil.ExecuteNonQuery(GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Persons, parameters), parameters, null, CommandType.Text, guid);
            }
            else
            {
                foreach (var person in item.Authors)
                {
                    parameters.Clear();
                    parameters.Add(new SqlParameter("@Staging_RSS_Item_Id", stagingId));
                    parameters.Add(new SqlParameter("@Email", person.Email));
                    /**************OLD edit on 18 May 2012 *******************/
                    //parameters.Add(new SqlParameter("@Name", person.Name));

                    parameters.Add(new SqlParameter("@Name", person.Email.Substring(0, person.Email.IndexOf(")")).Substring(person.Email.IndexOf("(") + 1)));
                    parameters.Add(new SqlParameter("@Uri", person.Uri));
                    parameters.Add(new SqlParameter("@Staging_IsAuthor", true));

                    DBUtil.ExecuteNonQuery(GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Persons, parameters), parameters, null, CommandType.Text, guid);
                }
            }

            foreach (var person in item.Contributors)
            {
                parameters.Clear();
                parameters.Add(new SqlParameter("@Staging_RSS_Item_Id", stagingId));
                parameters.Add(new SqlParameter("@Email", person.Email));
                parameters.Add(new SqlParameter("@Name", person.Name));
                parameters.Add(new SqlParameter("@Uri", person.Uri));
                parameters.Add(new SqlParameter("@Staging_IsAuthor", false));

                DBUtil.ExecuteNonQuery(GenerateInsertQueryFromParameters(CommonDAL.Tables.Staging_RSS_Persons, parameters), parameters, null, CommandType.Text, guid);
            }
            DBUtil.CloseConnection(guid);
        }
        #endregion

        #region SkyTrax
        static string GetValue(Dictionary<string, string> obj, string id)
        {
            return (obj.Keys.Contains(id) ? obj[id].ToString() : string.Empty);
        }
        public static bool UpdateReviewsinDB(List<Dictionary<string, string>> reviewList, String searchTermId)
        {
            try
            {
                foreach (var reviewObj in reviewList)
                {
                    string authorName = GetValue(reviewObj, "authorName");
                    string authorLocation = GetValue(reviewObj, "location");
                    string AuthorUri = GetValue(reviewObj, "uri");
                    authorLocation = authorLocation.Substring(1, authorLocation.Length - 2);
                    long author_staging_Id = updateAuthorInDB(authorName, authorLocation, AuthorUri);
                    //author object completed
                    string createdDate = GetValue(reviewObj, "date");
                    string text = GetValue(reviewObj, "Text");
                    string uri = GetValue(reviewObj, "uri");
                    string cabin = GetValue(reviewObj, "Cabin Flown");
                    int valueForMoney = Convert.ToInt32(GetValue(reviewObj, "Value for Money"));
                    int comfort = Convert.ToInt32(GetValue(reviewObj, "Seat Comfort"));
                    int service = Convert.ToInt32(GetValue(reviewObj, "Staff Service"));
                    int catering = Convert.ToInt32(GetValue(reviewObj, "Catering"));
                    int entertainment = Convert.ToInt32(GetValue(reviewObj, "Entertainment"));
                    Boolean recomended = Convert.ToBoolean(GetValue(reviewObj, "Recommended"));
                    string rating = GetValue(reviewObj, "Rating");


                    var query = "dbo.AddSkyTraxReviews";
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@user_staging_Id", author_staging_Id));
                    parameters.Add(new SqlParameter("@review_createdDate", createdDate));
                    parameters.Add(new SqlParameter("@review_text", text));
                    parameters.Add(new SqlParameter("@review_cabin", cabin));
                    parameters.Add(new SqlParameter("@valueForMoney", valueForMoney));
                    parameters.Add(new SqlParameter("@comfort", comfort));
                    parameters.Add(new SqlParameter("@service", service));
                    parameters.Add(new SqlParameter("@entertainment", entertainment));
                    parameters.Add(new SqlParameter("@catering", catering));
                    parameters.Add(new SqlParameter("@recomended", recomended));
                    parameters.Add(new SqlParameter("@rating", rating));
                    parameters.Add(new SqlParameter("@uri", uri));
                    parameters.Add(new SqlParameter("@searchTerm_Id", searchTermId));


                    var table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);
                    //  long staging_Id =Convert.ToInt64(table.Rows[0][0].ToString());

                }
                return true;
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(StagingDAL), "UpdateReviewsinDB", ex);
            }

            return false;
        }

        static long updateAuthorInDB(string name, string location, string authorUri)
        {
            var query = "dbo.updateSkyTraxUser";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@name", name));
            parameters.Add(new SqlParameter("@location", location));
            parameters.Add(new SqlParameter("@authorUri", authorUri));

            var table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);
            return Convert.ToInt64(table.Rows[0][0].ToString());
        }

        public static bool PersistDawnNews(DawnNews news, int searchTermId, int documentSourceId)//,bool checkExisting)
        {
            long id = -1;
            List<SqlParameter> parameters = new List<SqlParameter>();
            string query;
            object obj = null;
            query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.StagingSkyTraxWeb_Insert);

            parameters.Clear();
            parameters.Add(new SqlParameter("@News_Id", news.NewsId));
            parameters.Add(new SqlParameter("@News_Title", news.NewsTitle));
            parameters.Add(new SqlParameter("@News_Url", news.NewsUrl));
            parameters.Add(new SqlParameter("@News_Author", news.NewsAuthor));
            parameters.Add(new SqlParameter("@News_Description", news.NewsDescription));
            parameters.Add(new SqlParameter("@News_CreatedAt", news.NewsCreatedAt));
            parameters.Add(new SqlParameter("@CommentsCount", news.CommentsCount));
            parameters.Add(new SqlParameter("@LikesCount", news.LikesCount));
            parameters.Add(new SqlParameter("@Staging_DocumentSource_Id", news.DocumentSourceId));
            parameters.Add(new SqlParameter("@Staging_SearchTerm_Id", news.SearchTermId));
            parameters.Add(new SqlParameter("@LanguageCode", news.LanguageCode));
            parameters.Add(new SqlParameter("@Sentiment", news.Sentiment));

            DataTable commentsDataTable = new DataTable();
            commentsDataTable.Columns.Add(new DataColumn("Author", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Content", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("PublishedDate", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("CreatedDate", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("UpdatedDate", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("Language", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("Sentiment", typeof(int)));

            foreach (var comment in news.Comments)
            {
                commentsDataTable.Rows.Add(
                         comment.Author,
                         comment.Content,
                         comment.PublishedDate,
                         comment.CreatedDate,
                         comment.UpdatedDate,
                         comment.Language,
                         comment.Sentiment);
            }

            SqlParameter comment_parameter = new SqlParameter("@Comments", commentsDataTable);
            comment_parameter.SqlDbType = SqlDbType.Structured;
            parameters.Add(comment_parameter);

            obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);
            if (obj != null)
                id = Convert.ToInt64(obj);
            else
                return false;

            return true;
        }

        public static bool PersistGoogleSearch(GoogleSearch googleSearch)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_WebGoogle_Insert);

            parameters.Clear();
            parameters.Add(new SqlParameter("@CacheId", googleSearch.Id));
            parameters.Add(new SqlParameter("@News_Title", googleSearch.Title));
            parameters.Add(new SqlParameter("@News_Url", googleSearch.Url));
            parameters.Add(new SqlParameter("@News_Author", googleSearch.Title));
            parameters.Add(new SqlParameter("@News_Description", googleSearch.Snippet));
            parameters.Add(new SqlParameter("@News_CreatedAt", googleSearch.CreatedAt));
            parameters.Add(new SqlParameter("@CommentsCount", googleSearch.CommentsCount));
            parameters.Add(new SqlParameter("@LikesCount", googleSearch.LikesCount));
            parameters.Add(new SqlParameter("@Staging_DocumentSource_Id", googleSearch.DocumentSourceId));
            parameters.Add(new SqlParameter("@Staging_SearchTerm_Id", googleSearch.SearchTermId));
            parameters.Add(new SqlParameter("@LanguageCode", googleSearch.LanguageCode));
            parameters.Add(new SqlParameter("@Sentiment", googleSearch.Sentiment));

            object obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);

            if (obj != null)
                return true;
            else
                return false;
        }


        #endregion

        #region Common
        static object GetNullableDBValue(DataRow row, string column)
        {
            object o = row[column];
            return o == DBNull.Value ? null : o;
        }
        static DocumentInfo CreateDocumentInfo(DataRow row, DocumentSourceTypeIndexingInfo documentSourceType, string orgName, int documentSourceId)
        {
            DocumentInfo document = new DocumentInfo()
            {
                DocumentSourceTypeId = documentSourceType.DocumentSourceTypeId,
                DocumentSourceViewName = documentSourceType.ViewName,
                OrganizationName = orgName,
                DocumentSourceId = documentSourceId,
                StagingId = (long)row["Staging_Id"],
                ContentId = row["Content_Id"].ToString(),
                ContentText = row["Content_Text"].ToString(),
                SearchTerm_Id = row["SearchTerm_Id"].ToString(),
                //ContentHtml = null,//row["Content_Html"].ToString(),
                ContentTime = (DateTime)row["Content_Time"],
                AuthorName = row["Author_Name"].ToString(),
                AuthorId = row["Author_Id"].ToString(),
                AuthorUri = (string)GetNullableDBValue(row, "Author_Uri"),
                AuthorAge = (int?)GetNullableDBValue(row, "Author_Age"),
                AuthorGender = (string)GetNullableDBValue(row, "Author_Gender"),
                AuthorLocation = (string)GetNullableDBValue(row, "Author_Location"),
                AuthorTimeZone = (string)GetNullableDBValue(row, "TimeZone"),
                Followers = Convert.ToInt32(GetNullableDBValue(row, "Followers")),
                Following = Convert.ToInt32(GetNullableDBValue(row, "Following")),
                ToAuthorName = row["To_Author_Name"].ToString(),
                ToAuthorId = row["To_Author_Id"].ToString(),
                ToAuthorUri = (string)GetNullableDBValue(row, "To_Author_Uri"),
                ToAuthorAge = (int?)GetNullableDBValue(row, "To_Author_Age"),
                ToAuthorGender = (string)GetNullableDBValue(row, "To_Author_Gender"),
                ToAuthorLocation = (string)GetNullableDBValue(row, "To_Author_Location"),
                ToAuthorTimeZone = (string)GetNullableDBValue(row, "To_TimeZone"),
                ToFollowers = Convert.ToInt32(GetNullableDBValue(row, "To_Followers")),
                ToFollowing = Convert.ToInt32(GetNullableDBValue(row, "To_Following")),
                ContentUri = row["Content_Uri"].ToString(),
                Followup = Convert.ToBoolean(row["Staging_Followup"]),
                CommentsCount = Convert.ToInt32(GetNullableDBValue(row, "CommentsCount")),
                LikesCount = Convert.ToInt32(GetNullableDBValue(row, "LikesCount")),
                ViewsCount = Convert.ToInt32(GetNullableDBValue(row, "ViewsCount")),
                SharesCount = Convert.ToInt32(GetNullableDBValue(row, "SharesCount")),
                UniqueCommentersCount = 0,//Convert.ToInt32(GetNullableDBValue(row, "UniqueCommentersCount")),
                ParentContentId = row["Parent_Content_Id"].ToString(),
                ParentStagingId = (long?)GetNullableDBValue(row, "Parent_Staging_Id"),
                ParentStagingDocumentSourceId = (int?)GetNullableDBValue(row, "Parent_Staging_DocumentSource_Id"),
                Type = (string)GetNullableDBValue(row, "type"),

            };
            return document;
        }

        static List<DocumentInfo> FindSearchTerms(DataRow row, DocumentSourceTypeIndexingInfo documentSourceType, int documentSourceId, string orgName)
        {
            List<DocumentInfo> found = new List<DocumentInfo>();
            string statusText = (string)row["Content_Text"];
            var docSources = ConfigDAL.FetchDocumentSourceConfiguration(documentSourceType.Type);
            foreach (var source in docSources)
            {
                if (documentSourceId == source.DocumentSourceId)
                {
                    //TODO:handle
                    // foreach (var org in source.Organization)
                    {
                        //   if (string.IsNullOrEmpty(orgName) || orgName == org.Name)
                        {
                            foreach (var search in source.SearchTerms)
                            {
                                if (search.Type == SearchTermInfo.Types.SearchTerm &&
                                    statusText.IndexOf(search.SearchTerm,
                                    StringComparison.InvariantCultureIgnoreCase) >= 0)
                                {
                                    found.Add(CreateDocumentInfo(
                                        row,
                                        documentSourceType,
                                       orgName,// org.Name,
                                        documentSourceId
                                        ));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return found;
        }

        public static List<DocumentInfo> FetchStagingDocuments(DocumentSourceTypeIndexingInfo documentSourceType, DataProcessingState state, string whereCondition, DataProcessingState documentState)
        {
            List<DocumentInfo> fetched = new List<DocumentInfo>();
            List<DocumentInfo> keywordsNotFound = new List<DocumentInfo>();

            try
            {
                //List<DocumentSourceTypeInfo> config = ConfigDAL.FetchDocumentSourceType(documentSourceTypeId);

                //var documentSourceType = config[0];

                StringBuilder query = new StringBuilder();
                DataTable table = new DataTable();
                if (documentSourceType.DocumentSourceTypeId != 1 && documentSourceType.DocumentSourceTypeId != 4 && documentSourceType.DocumentSourceTypeId != 1073741824)
                {

                    query.AppendFormat("select top({0}) * from dbo.{1} WITH(READPAST) where Staging_ProcessingState = {2}",
                        documentSourceType.TopCount, documentSourceType.ViewName, (int)state);

                    if (!string.IsNullOrEmpty(whereCondition))
                    {
                        query.Append(" AND " + whereCondition);
                    }

                    if (!string.IsNullOrEmpty(documentSourceType.OrderBy))
                    {
                        query.Append(" ORDER BY " + documentSourceType.OrderBy);
                    }


                    table = DBUtil.FetchRecords(query.ToString(), null, null, CommandType.Text);
                }
                else if (documentSourceType.DocumentSourceTypeId == 1073741824)
                {
                    query = new StringBuilder();
                    query.AppendFormat("dbo.{0}", "Staging_process_Web");
                    table = DBUtil.FetchRecords(query.ToString(), new List<SqlParameter>(), null, CommandType.StoredProcedure);

                }
                else if (documentSourceType.DocumentSourceTypeId == 1)
                {
                    query = new StringBuilder();
                    query.AppendFormat("dbo.{0}", "Staging_process_Twitter");
                    table = DBUtil.FetchRecords(query.ToString(), new List<SqlParameter>(), null, CommandType.StoredProcedure);

                }
                else if (documentSourceType.DocumentSourceTypeId == 4)
                {
                    query = new StringBuilder();
                    query.AppendFormat("dbo.{0}", "Staging_process_Youtube");
                    table = DBUtil.FetchRecords(query.ToString(), new List<SqlParameter>(), null, CommandType.StoredProcedure);

                }

                if (table != null)
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        bool keywordFound = false;
                        if (!string.IsNullOrEmpty(table.Rows[i]["To_User_StagingId"].ToString()))
                        {
                            DataTable ToUserTable = FetchToUserInfo(table.Rows[i]["To_User_StagingId"].ToString(), documentSourceType.DocumentSourceTypeId);
                            table.Rows[i]["To_Author_Name"] = ToUserTable.Rows[0]["To_Author_Name"].ToString();
                            table.Rows[i]["To_Author_Id"] = ToUserTable.Rows[0]["To_Author_Id"].ToString();
                            table.Rows[i]["To_Author_Uri"] = ToUserTable.Rows[0]["To_Author_Uri"].ToString();
                            table.Rows[i]["To_Author_Age"] = ToUserTable.Rows[0]["To_Author_Age"].ToString();
                            table.Rows[i]["To_Author_Gender"] = ToUserTable.Rows[0]["To_Author_Gender"].ToString();
                            table.Rows[i]["To_Author_Location"] = ToUserTable.Rows[0]["To_Author_Location"].ToString();
                            table.Rows[i]["To_TimeZone"] = ToUserTable.Rows[0]["To_TimeZone"].ToString();
                            table.Rows[i]["To_Followers"] = ToUserTable.Rows[0]["To_Followers"].ToString();
                            table.Rows[i]["To_Following"] = ToUserTable.Rows[0]["To_Following"].ToString();
                        }
                        //if (documentSourceType.DocumentSourceTypeId == 0x40000000)
                        //{
                        //    string domain = GetDomainFromUri((string)table.Rows[i]["Content_Uri"]);
                        //    List<SqlParameter> param = new List<SqlParameter>();
                        //    param.Add(new SqlParameter("@Domain", domain));

                        //    int documentSourceId = (int)DBUtil.ExecuteScalar(CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Config_AssignDocumentSource),
                        //      param, null, CommandType.StoredProcedure);


                        //    List<DocumentInfo> found = FindSearchTerms(
                        //        table.Rows[i], documentSourceType, documentSourceId, null);
                        //    if (found.Count > 0)
                        //    {
                        //        keywordFound = true;
                        //        fetched.AddRange(found);
                        //    }


                        //}
                        //else
                        {
                            int documentSourceId = (int)table.Rows[i]["Staging_DocumentSource_Id"];
                            string orgName = Convert.ToString(table.Rows[i]["Staging_OrgName"]);

                            //   if (string.IsNullOrEmpty(orgName) || documentSourceType.FindSearchTerms)
                            //{
                            //if (documentSourceId == 1)
                            //{
                            //    List<DocumentInfo> found = FindSearchTerms(
                            //        table.Rows[i], documentSourceType, documentSourceId, orgName);
                            //    if (found.Count > 0)
                            //    {
                            //        keywordFound = true;
                            //        fetched.AddRange(found);
                            //    }
                            //}
                            if (documentSourceType.DocumentSourceTypeId == 2)
                            {
                                if (GetSearchTermType(Convert.ToInt32(table.Rows[i]["Staging_Id"].ToString())) == 0)
                                {
                                    List<DocumentInfo> found = FindSearchTerms(
                                        table.Rows[i], documentSourceType, documentSourceId, orgName);
                                    if (found.Count > 0)
                                    {
                                        keywordFound = true;
                                        fetched.AddRange(found);
                                    }
                                }
                                else
                                {
                                    keywordFound = true;
                                    fetched.Add(CreateDocumentInfo(
                                                  table.Rows[i],
                                                  documentSourceType,
                                                  orgName,
                                                  documentSourceId
                                                  ));
                                }
                            }
                            //}
                            else
                            {
                                keywordFound = true;
                                fetched.Add(CreateDocumentInfo(
                                              table.Rows[i],
                                              documentSourceType,
                                              orgName,
                                              documentSourceId
                                              ));
                            }
                        }

                        if (!keywordFound)
                        {
                            keywordsNotFound.Add(new BO.DocumentInfo
                            {
                                StagingId = (long)table.Rows[i]["Staging_Id"]
                            });
                        }
                    }
                    if (documentSourceType.DocumentSourceTypeId == 1073741824)
                    {
                        UpdateSkyTraxProcessingState(fetched);
                    }
                    else
                    {

                        UpdateDocumentProcessingState(documentSourceType.ViewName,
                            fetched,
                             documentState);
                        UpdateDocumentProcessingState(documentSourceType.ViewName,
                            keywordsNotFound,
                            Util.DataProcessingState.KeywordsNotFound);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(StagingDAL), "FetchStagingDocuments", ex);
            }

            return fetched;
        }
        public static DataTable FetchToUserInfo(string To_User_StagingId, int documentsourcetype_Id)

        {
            var query = @"dbo.FetchToUserInfo ";
            SqlParameter staging_Id = new SqlParameter("@staging_Id", To_User_StagingId);
            SqlParameter documentSourceType_Id = new SqlParameter("@documentSourceType_Id", @documentsourcetype_Id);
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(staging_Id);
            parameters.Add(documentSourceType_Id);
            DataTable table = DBUtil.FetchRecords(query.ToString(), parameters, null, CommandType.StoredProcedure);
            return table;

        }
        public static int GetSearchTermType(long staging_id)
        {
            string query = @"select SearchTermType_Id from dbo.Config_SearchTerm with (nolock) 
                            where Id in( select Staging_SearchTerm_Id from 
                            dbo.staging_facebook_posts with (nolock) where staging_id = {0}) ";// query.AppendFormat("WebMiner_Common.dbo.{0}", "Staging_process_Twitter");
            query = string.Format(query, staging_id);
            DataTable table = DBUtil.FetchRecords(query.ToString(), new List<SqlParameter>(), null, CommandType.Text);
            int typeId = (table.Rows.Count > 0) ? Convert.ToInt32(table.Rows[0][0].ToString()) : 1;
            return typeId;
        }

        public static void UpdateSkyTraxProcessingState(List<DocumentInfo> documents)
        {
            var groups = documents.GroupBy(x => x.OrganizationName);

            foreach (var group in groups)
            {

                List<DocumentInfo> di = group.ToList<DocumentInfo>();
                String orgName = di[0].OrganizationName;
                StringBuilder sb = new StringBuilder();
                sb.Append(" Staging_review_Id in (");

                for (int i = 0; i < di.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append(di[i].StagingId);
                }
                sb.Append(") and organization_Id = (select Id from dbo.config_organization where name = '" + orgName + "' ) ");

                Log.WriteOperationCompleted(typeof(StagingDAL), "UpdateDocumentProcessingState", "Updating Ids in query: " + sb);

                CommonDAL.UpdateProcessingState(
                      "dbo." + "Staging_SkyTrax_Review_organization", "isProcessed", "staging_updateTime", sb.ToString(), Util.DataProcessingState.Refresh);
            }

        }

        public static void UpdateDocumentProcessingState(string viewName, List<DocumentInfo> documents, Util.DataProcessingState state)
        {
            if (documents.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" Staging_Id in (");

                for (int i = 0; i < documents.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append(documents[i].StagingId);
                }
                sb.Append(")");

                Log.WriteOperationCompleted(typeof(StagingDAL), "UpdateDocumentProcessingState", "Updating Ids in query: " + sb);

                CommonDAL.UpdateProcessingState(
                    "dbo." + viewName
                    , "Staging_ProcessingState", "Staging_UpdateTime", sb.ToString(), state);
            }
        }

        public static DataTable GetSkyTraxSearchTerm()
        {

            var query = @"select * from dbo.staging_skytrax_searchTerm with (nolock) where IsActive =1 ";
            DataTable table = DBUtil.FetchRecords(query.ToString(), new List<SqlParameter>(), null, CommandType.Text);

            return table;
        }

        public static bool LogMiner(int DocsourceTypeId, string DocumentSourceType, string MinerStep, string MinerMessage)
        {
            string query = "MinerLog_Add";
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@DocumentSourceTypeId", DocsourceTypeId));
            parameters.Add(new SqlParameter("@DocumentSourceTypeName", DocumentSourceType));
            parameters.Add(new SqlParameter("@MinerStep", MinerStep));
            parameters.Add(new SqlParameter("@MinerMessage", MinerMessage));

            DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);

            return true;
        }

        static string GetDomainFromUri(string uri)
        {
            Uri u = new Uri(uri);

            return u.Host;
        }

        #endregion
    }
}
