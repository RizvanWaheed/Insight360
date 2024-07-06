using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Insight360.Common.BO;
//using Insight360.BO;

namespace Insight360.DAL
{
    public static class PostDaywiseEngagementDAL
    {
        /// <summary>
        /// Daywise Engagement Trend Recording
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPostsForDaywiseEngagement()
        {
            // string query = "select Id, DocumentSourceType_Id, DocumentSource_Id, time, SearchTermId, WebSourceId from documents where isTracked=1 order by time desc";
            string query = @"select Id [Doc_Id], 0 [Doc_MediaId], DocumentSourceType_Id, DocumentSource_Id, time, SearchTermId, WebSourceId 
                            from documents where isTracked = 1
                            union
                            select 0 [Doc_Id], doc.Id, doc.DocumentSourceType_Id, 0 [DocumentSource_Id], time, 0 [SearchTermId], doc.WebSourceId
                            from Documents_MediaUrl doc
                            inner join config_mediaurl m on m.media_id = doc.mediaurl_id
                            --where m.isActive = 1
                            order by time desc";
            return DBUtil.FetchRecords(query, null, null, CommandType.Text);
        }

        public static void UpdatePostsDaywiseEngagement(PostDaywiseEngagementBO postEngagement)
        {
            string query = "Update_PostDaywiseEngagement";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Clear();
            parameters.Add(new SqlParameter("@documentId", postEngagement.DocumentId));
            parameters.Add(new SqlParameter("@media_documentid", postEngagement.Media_DocumentId));
            parameters.Add(new SqlParameter("@documentSourceId", postEngagement.DocumentSourceId));
            parameters.Add(new SqlParameter("@searchTermId", postEngagement.SearchTermId));
            parameters.Add(new SqlParameter("@date", postEngagement.Date));
            parameters.Add(new SqlParameter("@hour", postEngagement.Hour));
            parameters.Add(new SqlParameter("@likes", postEngagement.Likes));
            parameters.Add(new SqlParameter("@comments", postEngagement.Comments));
            parameters.Add(new SqlParameter("@shares", postEngagement.Shares));
            parameters.Add(new SqlParameter("@views", postEngagement.Views));
            parameters.Add(new SqlParameter("@retweets", postEngagement.Retweets));

            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Facebook Comment Recording (for Manually Added urls)
        /// </summary>
        /// <returns></returns>
        public static DataTable GetFBPosts()
        {
            string query = @"select 1 as isMediaUrl, doc.Id, doc.WebSourceId from Documents_MediaUrl doc
                            where datediff(dd, time, getdate()) <= 7 and doc.DocumentSourceType_Id = 2

                            union

                            select 0 as isMediaUrl, doc.Staging_Id, doc.WebSourceId from Documents doc
                            where datediff(dd, time, getdate()) <= 3 and doc.DocumentSourceType_Id = 2";

            return DBUtil.FetchRecords(query, null, null, CommandType.Text);
        }

        public static bool PersistMediaUrl_FacebookComment(int Doc_Id, List<Insight360.BO.FacebookCommentInfo> Comments, bool isMediaUrl)//,bool checkExisting)
        {
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Facebook_MediaUrl_AddComment);

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Clear();

            parameters.Add(new SqlParameter("@DocId", Doc_Id));
            parameters.Add(new SqlParameter("@IsMediaUrl", isMediaUrl));

            DataTable commentsDataTable = new DataTable();
            commentsDataTable.Columns.Add(new DataColumn("id", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("from_name", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("from_id", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("message", typeof(string)));
            commentsDataTable.Columns.Add(new DataColumn("created_time", typeof(DateTime)));
            commentsDataTable.Columns.Add(new DataColumn("Staging_LikesCount", typeof(int)));
            commentsDataTable.Columns.Add(new DataColumn("Sentiment", typeof(int)));
            commentsDataTable.Columns.Add(new DataColumn("LanguageCode", typeof(string)));

            foreach (var comment in Comments)
                commentsDataTable.Rows.Add(comment.Id, comment.From.Name, comment.From.Id, comment.Message, comment.CreatedTime, 0, comment.Sentiment, comment.LanguageCode);

            SqlParameter tvp = new SqlParameter("@Comments", commentsDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            parameters.Add(tvp);

            Object obj = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);

            if (obj != null)
                return true;

            return false;
        }

    }


}
