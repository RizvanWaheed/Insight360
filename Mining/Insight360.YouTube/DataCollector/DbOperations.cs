using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
//using Google.YouTube;
//using Google.GData.Extensions;
//using Google.GData.YouTube;

/// <summary>
/// Summary description for DbOperations
/// </summary>
namespace Insight360.YouTube.DataCollector
{
    public class DbOperations
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

        public string GetVideoByCommentID(string CommentID)
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

        public static int InsertVideo(Video vdo, string OrgSettingID)
        {
            SqlCommand com = GetCommand();

            com.Parameters.Add("@OrgSettingID", OrgSettingID);
            com.Parameters.Add("@VideoTitle", vdo.Title);
            com.Parameters.Add("@YoutubeID", vdo.VideoId);
            com.Parameters.Add("@YoutubeURL", vdo.WatchPage.AbsoluteUri);
            com.Parameters.Add("@ViewCount", vdo.ViewCount);
            com.Parameters.Add("@Author", vdo.Author);
            com.Parameters.Add("@CommentCount", vdo.CommmentCount);
            com.Parameters.Add("@Description", vdo.Description);
            com.Parameters.Add("@Keywords", vdo.Keywords);
            com.Parameters.Add("@Updated", vdo.Updated);
            com.Parameters.Add("@Published", vdo.YouTubeEntry.Published);
            com.Parameters.Add("@Language", vdo.YouTubeEntry.Language);
            com.Parameters.Add("@DateCreated", DateTime.Now);
            com.Parameters.Add("@DateModified", DateTime.Now);

            com.CommandText = "ytAddVideo";
            com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            if (dt != null && dt.Rows.Count > 0)
                return int.Parse(dt.Rows[0][0].ToString());
            return 0;


        }

        public static int InsertVideoComment(Int64 vdoID, Comment comment)
        {
            try
            {
                SqlCommand com = GetCommand();

                com.Parameters.Add("@VideoID", vdoID);
                com.Parameters.Add("@YouTubeCommentID", comment.CommentEntry.SelfUri.Content);
                com.Parameters.Add("@Title", comment.Title);
                com.Parameters.Add("@Author", comment.Author);
                com.Parameters.Add("@Content", comment.Content);
                com.Parameters.Add("@Language", comment.CommentEntry.Language);
                com.Parameters.Add("@Published", comment.CommentEntry.Published);
                com.Parameters.Add("@Updated", comment.Updated);
                com.Parameters.Add("@DateCreated", DateTime.Now);
                com.Parameters.Add("@DateModified", DateTime.Now);


                com.CommandText = "ytAddVideoComment";
                com.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(com);

                DataTable dt = new DataTable();
                da.Fill(dt);

                com.Connection.Close();

                if (dt != null && dt.Rows.Count > 0)
                    return int.Parse(dt.Rows[0][0].ToString());
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }


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

        public static void InsertUserInfo(ProfileEntry profile, string author)
        {
            SqlCommand com = GetCommand();

            com.Parameters.Add("@Author", author);
            com.Parameters.Add("@Age", profile.Age);
            com.Parameters.Add("@Company", profile.Company);
            com.Parameters.Add("@Etag", profile.Etag);
            com.Parameters.Add("@FeedUri", profile.FeedUri);
            com.Parameters.Add("@FirstName", profile.Firstname);
            com.Parameters.Add("@LastName", profile.Lastname);
            com.Parameters.Add("@Gender", profile.Gender);
            com.Parameters.Add("@Hobbies", profile.Hobbies);
            com.Parameters.Add("@Hometown", profile.Hometown);
            com.Parameters.Add("@Language", profile.Language);
            com.Parameters.Add("@Location", profile.Location);
            com.Parameters.Add("@Movies", profile.Movies);
            com.Parameters.Add("@Music", profile.Music);
            com.Parameters.Add("@Occupation", profile.Occupation);
            com.Parameters.Add("@Relationship", profile.Relationship);
            com.Parameters.Add("@School", profile.School);
            com.Parameters.Add("@Username", profile.UserName);


            com.CommandText = "ytAddUser";
            com.CommandType = CommandType.StoredProcedure;

            com.ExecuteNonQuery();
            com.Connection.Close();
        }

    }
}
