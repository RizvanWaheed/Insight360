using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;



namespace Insight360.DAL
{
    public class YoutubeDbOperations
    {
        public static SqlCommand GetCommand()
        {

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["YouTubeDS"].ConnectionString);
            con.Open();
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            return com;
        }

        public static DataTable GetKeywordsWithOrg()
        {
            SqlCommand com = GetCommand();

            com.CommandText = "ytGetKeyWordsWithOrgID";

            com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            return dt;
        }

        public static string GetVideoByCommentID(string CommentID)
        {
            SqlCommand com = GetCommand();

            com.CommandText = "GetVideoByCommentID";

            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.Add(new SqlParameter("@CommentID", CommentID));

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString();
            return string.Empty;
        }

        public static bool PersistYouTubeFeed(Insight360.BO.YouTubeFeed vdo, bool isSearch, int searchTermId, bool GetComments)
        {
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
            Parameters.Add(new SqlParameter("@DocumentSourceId", vdo.Staging.DocumentSourceId));
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
            UserDataTable.Columns.Add(new DataColumn("ChannelId", typeof(string)));


            var user = vdo.User;

            UserDataTable.Rows.Add(
       user.Author,
     user.Age,
     user.Company,
     user.Etag,
     user.FeedUri,
     user.FirstName,
     user.LastName,
     (user.Gender == null ? "unspecified" : user.Gender),
     user.Hobbies,
     user.Hometown,
     user.Language,
     (user.Location == null ? "Global" : user.Location),
     user.Movies,
     user.Music,
     user.Occupation,
     user.Relationship,
     user.School,
     user.Username,
     DateTime.Now,
     DateTime.Now,
     user.SubscriberCount,
     user.ViewCount,
     user.ChannelId);



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


        }


        public static DataTable ExecuteSP(string spName, Dictionary<string, string> parameters)
        {
            SqlCommand com = GetCommand();

            com.CommandType = CommandType.StoredProcedure;

            com.CommandText = spName;

            foreach (string key in parameters.Keys)
            {
                com.Parameters.Add(new SqlParameter(key, parameters[key]));
            }

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            return dt;
        }


        public static void UpdateSP(string spName, Dictionary<string, string> parameters)
        {
            SqlCommand com = GetCommand();

            com.CommandType = CommandType.StoredProcedure;

            com.CommandText = spName;

            foreach (string key in parameters.Keys)
            {
                com.Parameters.Add(new SqlParameter(key, parameters[key]));
            }

            com.ExecuteNonQuery();

            com.Connection.Close();
        }

        public static void UpdateStagingTime(string value)
        {

            SqlCommand com = GetCommand();

            string query = "UPDATE [dbo].[Staging_YouTube_Videos] set [Staging_UpdateTime] = GETDATE() WHERE YoutubeID IN (" + value + ")";

            com.CommandText = query;

            com.ExecuteNonQuery();

            com.Connection.Close();
        }


        public static DataTable GetExistingAuthors()
        {
            SqlCommand com = GetCommand();



            com.CommandText = "Select distinct author from Staging_YouTube_Videos";
            com.CommandType = CommandType.Text;

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            return dt;
        }

        public static List<Insight360.BO.YouTubeFeed> FetchVideosToUpdate(int topCount)
        {

            DataTable table = DBUtil.FetchRecords(
         string.Format("select top {0} Staging_Id,YoutubeID, Author, Staging_UpdateTime "
                   + " from {1} WITH(READPAST) "
                   + " where Staging_refresh = 'true' "
                   + " AND datediff(hour,Staging_UpdateTime,getdate())>=1 "
                   + " order by Staging_UpdateTime ASC"
                   , topCount
                   , CommonDAL.GetCommonTableName(CommonDAL.Tables.Staging_YouTube_Videos)
                   , (int)Util.DataProcessingState.UnProcessed
                   )
               , null, null, CommandType.Text);

            List<Insight360.BO.YouTubeFeed> feeds = new List<Insight360.BO.YouTubeFeed>();

            foreach (DataRow row in table.Rows)
            {
                DateTime dt;
                DateTime.TryParse(row["Staging_UpdateTime"].ToString(), out dt);
                feeds.Add(new Insight360.BO.YouTubeFeed()
                {
                    VideoID = row["YoutubeID"].ToString(),
                    Author = row["Author"].ToString(),
                    //Staging_OrgName = row["Staging_OrgName"].ToString(),
                    Staging_UpdateTime = dt
                });

            }

            CommonDAL.UpdateStagingUpdateTime(CommonDAL.Tables.Staging_YouTube_Videos,
                    (from p in feeds select p.Staging.StagingId).ToList());


            return feeds;


        }





    }
}
