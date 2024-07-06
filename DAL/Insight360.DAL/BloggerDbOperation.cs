using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Insight360.BO;
namespace Insight360.DAL
{
   public class BloggerDbOperation
    {
        //public static bool PersistBloggerFeed(BloggerPost Post, bool isSearch, bool GetComments)
        //{
        //    long id = -1;
        //    List<SqlParameter> Parameters = new List<SqlParameter>();
        //    string query;
        //    object obj = null;

        //    query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Blogger_AddPost);
        //    Parameters.Add(new SqlParameter("@Staging_UpdateTime", Post.Staging_UpdateTime));
        //    Parameters.Add(new SqlParameter("@Staging_InsertTime", Post.Staging_InsertTime));
        //    Parameters.Add(new SqlParameter("@PostID", Post.PostID));
        //    Parameters.Add(new SqlParameter("@BlogID", Post.BlogID));
        //    Parameters.Add(new SqlParameter("@Uri", Post.Uri));
        //    Parameters.Add(new SqlParameter("@PublishedDate", Post.PublishedDate));
        //    Parameters.Add(new SqlParameter("@UpdatedDate", Post.UpdatedDate));
        //    Parameters.Add(new SqlParameter("@Content", Post.Content));
        //    Parameters.Add(new SqlParameter("@Title", Post.Title));
        //    Parameters.Add(new SqlParameter("@Src", Post.Src));
        //    Parameters.Add(new SqlParameter("@Type", Post.Type));
        //    Parameters.Add(new SqlParameter("@EditURI", Post.EditURI));
        //    Parameters.Add(new SqlParameter("@Feed", Post.Feed));
        //    Parameters.Add(new SqlParameter("@Post", Post.Post));
        //    Parameters.Add(new SqlParameter("@Self", Post.Self));
        //    Parameters.Add(new SqlParameter("@StartIndex", Post.StartIndex));
        //    Parameters.Add(new SqlParameter("@TotalResults", Post.TotalResults));
        //    Parameters.Add(new SqlParameter("@IdURI", Post.IdURI));
        //    Parameters.Add(new SqlParameter("@LinkCount", Post.LinkCount));
        //    Parameters.Add(new SqlParameter("@LinkReadOnly", Post.LinkReadOnly));
        //    Parameters.Add(new SqlParameter("@MediaURI", Post.MediaURI));
        //    Parameters.Add(new SqlParameter("@ReadOnly", Post.ReadOnly));
        //    Parameters.Add(new SqlParameter("@Rights", Post.Rights));
        //    Parameters.Add(new SqlParameter("@SelfURI", Post.SelfURI));
        //    Parameters.Add(new SqlParameter("@Summary", Post.Summary));
        //    Parameters.Add(new SqlParameter("@UserCount", Post.UserCount));
        //    //Parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
        //    Parameters.Add(new SqlParameter("@GetComments", GetComments));
        //    Parameters.Add(new SqlParameter("@isSearch", isSearch));
        //    Parameters.Add(new SqlParameter("@DocumentSource_id", Post.DocumentSource_ID));


        //    DataTable UserDataTable = new DataTable();
        //    UserDataTable.Columns.Add(new DataColumn("Name", typeof(string)));
        //    UserDataTable.Columns.Add(new DataColumn("Email", typeof(string)));
        //    UserDataTable.Columns.Add(new DataColumn("Language", typeof(string)));
        //    UserDataTable.Columns.Add(new DataColumn("URI", typeof(string)));

        //    foreach (var user in Post.Users)
        //    {
        //        UserDataTable.Rows.Add(
        //                 user.Name,
        //                 user.Email,
        //                 user.Language,
        //                 user.URI);

        //    }



        //    DataTable SearchtermDataTable = new DataTable();
        //    SearchtermDataTable.Columns.Add(new DataColumn("SearchTermId", typeof(string)));

        //    foreach (var sid in Post.SearchTerms)
        //    {
        //        SearchtermDataTable.Rows.Add(
        //             sid.SearchTermsID);

        //    }



        //    DataTable LinkDataTable = new DataTable();
        //    LinkDataTable.Columns.Add(new DataColumn("AbsoluteURI", typeof(string)));
        //    LinkDataTable.Columns.Add(new DataColumn("Rel", typeof(string)));
        //    LinkDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
        //    LinkDataTable.Columns.Add(new DataColumn("Type", typeof(string)));

        //    foreach (var link in Post.Links)
        //    {
        //        LinkDataTable.Rows.Add(
        //             link.AbsoluteURI,
        //             link.Rel,
        //             link.Title,
        //             link.Type);

        //    }


        //    DataTable CommentDataTable = new DataTable();
        //    CommentDataTable.Columns.Add(new DataColumn("CommentID", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("PostID", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Content", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("PublishedDate", typeof(DateTime)));
        //    CommentDataTable.Columns.Add(new DataColumn("UpdatedDate", typeof(DateTime)));
        //    CommentDataTable.Columns.Add(new DataColumn("Uri", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Src", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Type", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("EditURI", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Feed", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Post", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Self", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("StartIndex", typeof(int)));
        //    CommentDataTable.Columns.Add(new DataColumn("TotalResults", typeof(int)));
        //    CommentDataTable.Columns.Add(new DataColumn("IdURI", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("LinkCount", typeof(int)));
        //    CommentDataTable.Columns.Add(new DataColumn("LinkReadOnly", typeof(bool)));
        //    CommentDataTable.Columns.Add(new DataColumn("MediaURI", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("ReadOnly", typeof(bool)));
        //    CommentDataTable.Columns.Add(new DataColumn("Rights", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("SelfURI", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("Summary", typeof(string)));
        //    CommentDataTable.Columns.Add(new DataColumn("UserCount", typeof(int)));


        //    foreach (var comment in Post.Comments)
        //    {
        //        CommentDataTable.Rows.Add(
        //            comment.CommentID,
        //            comment.PostID,
        //            comment.Content,
        //            comment.PublishedDate,
        //            comment.UpdatedDate,
        //            comment.Uri,
        //            comment.Title,
        //            comment.Src,
        //            comment.Type,
        //            comment.EditURI,
        //            comment.Feed,
        //            comment.Post,
        //            comment.Self,
        //            comment.StartIndex,
        //            comment.TotalResults,
        //            comment.IdURI,
        //            comment.LinkCount,
        //            comment.LinkReadOnly,
        //            comment.MediaURI,
        //            comment.ReadOnly,
        //            comment.Rights,
        //            comment.SelfURI,
        //            comment.Summary,
        //            comment.UserCount);

        //    }


        //    SqlParameter tvp = new SqlParameter("@Comments", CommentDataTable);
        //    tvp.SqlDbType = SqlDbType.Structured;
        //    Parameters.Add(tvp);

        //    tvp = new SqlParameter("@Users", UserDataTable);
        //    tvp.SqlDbType = SqlDbType.Structured;
        //    Parameters.Add(tvp);

        //    tvp = new SqlParameter("@Links", LinkDataTable);
        //    tvp.SqlDbType = SqlDbType.Structured;
        //    Parameters.Add(tvp);
        //    tvp = new SqlParameter("@Searchterms", SearchtermDataTable);
        //    tvp.SqlDbType = SqlDbType.Structured;
        //    Parameters.Add(tvp);

        //    obj = DBUtil.ExecuteScalar(query, Parameters, null, CommandType.StoredProcedure);
        //    if (obj != null)
        //        id = Convert.ToInt64(obj);
        //    else
        //        return false;

        //    return true;
        //}

       public static bool PersistBloggerFeed(BloggerPost Post, bool isSearch, bool GetComments)
       {
           long id = -1;
           List<SqlParameter> Parameters = new List<SqlParameter>();
           string query;
           object obj = null;

           query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Staging_Blogger_AddPost);
           Parameters.Add(new SqlParameter("@Staging_UpdateTime", Post.Staging_UpdateTime));
           Parameters.Add(new SqlParameter("@Staging_InsertTime", Post.Staging_InsertTime));
           Parameters.Add(new SqlParameter("@PostID", Post.PostID));
           Parameters.Add(new SqlParameter("@BlogID", Post.BlogID));
           Parameters.Add(new SqlParameter("@Uri", Post.Uri));
           Parameters.Add(new SqlParameter("@PublishedDate", Post.PublishedDate));
           Parameters.Add(new SqlParameter("@UpdatedDate", Post.UpdatedDate));
           Parameters.Add(new SqlParameter("@Content", Post.Content));
           Parameters.Add(new SqlParameter("@Title", Post.Title));
           Parameters.Add(new SqlParameter("@Src", Post.Src));
           Parameters.Add(new SqlParameter("@Type", Post.Type));
           Parameters.Add(new SqlParameter("@EditURI", Post.EditURI));
           Parameters.Add(new SqlParameter("@Feed", Post.Feed));
           Parameters.Add(new SqlParameter("@Post", Post.Post));
           Parameters.Add(new SqlParameter("@Self", Post.Self));
           Parameters.Add(new SqlParameter("@StartIndex", Post.StartIndex));
           Parameters.Add(new SqlParameter("@TotalResults", Post.TotalResults));
           Parameters.Add(new SqlParameter("@IdURI", Post.IdURI));
           Parameters.Add(new SqlParameter("@LinkCount", Post.LinkCount));
           Parameters.Add(new SqlParameter("@LinkReadOnly", Post.LinkReadOnly));
           Parameters.Add(new SqlParameter("@MediaURI", Post.MediaURI));
           Parameters.Add(new SqlParameter("@ReadOnly", Post.ReadOnly));
           Parameters.Add(new SqlParameter("@Rights", Post.Rights));
           Parameters.Add(new SqlParameter("@SelfURI", Post.SelfURI));
           Parameters.Add(new SqlParameter("@Summary", Post.Summary));
           Parameters.Add(new SqlParameter("@UserCount", Post.UserCount));
           //Parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
           Parameters.Add(new SqlParameter("@GetComments", GetComments));
           Parameters.Add(new SqlParameter("@isSearch", isSearch));
           Parameters.Add(new SqlParameter("@DocumentSource_id", Post.DocumentSource_ID));
           Parameters.Add(new SqlParameter("@Sentiment", Post.Sentiment));
           Parameters.Add(new SqlParameter("@LanguageCode", Post.LanguageCode));


            DataTable UserDataTable = new DataTable();
                UserDataTable.Columns.Add(new DataColumn("Name", typeof(string)));
                UserDataTable.Columns.Add(new DataColumn("Email", typeof(string)));
                UserDataTable.Columns.Add(new DataColumn("Language", typeof(string)));
                UserDataTable.Columns.Add(new DataColumn("URI", typeof(string)));
                        
           foreach (var user in Post.Users)
                {
                UserDataTable.Rows.Add(
                         user.Name,
                         user.Email,
                         user.Language,
                         user.URI);
                    
                }


           
            DataTable SearchtermDataTable = new DataTable();
            SearchtermDataTable.Columns.Add(new DataColumn("SearchTermId", typeof(string)));
                    
           foreach (var sid in Post.SearchTerms)
                {
                    SearchtermDataTable.Rows.Add(
                         sid.SearchTermsID);
                    
                }



           DataTable LinkDataTable = new DataTable();
           LinkDataTable.Columns.Add(new DataColumn("AbsoluteURI", typeof(string)));
           LinkDataTable.Columns.Add(new DataColumn("Rel", typeof(string)));
           LinkDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
           LinkDataTable.Columns.Add(new DataColumn("Type", typeof(string)));

           foreach (var link in Post.Links)
           {
               LinkDataTable.Rows.Add(
                    link.AbsoluteURI,
                    link.Rel,
                    link.Title,
                    link.Type);

           }


             DataTable CommentDataTable = new DataTable();
                CommentDataTable.Columns.Add(new DataColumn("CommentID", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("PostID", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Content", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("PublishedDate", typeof(DateTime)));
                CommentDataTable.Columns.Add(new DataColumn("UpdatedDate", typeof(DateTime)));
                CommentDataTable.Columns.Add(new DataColumn("Uri", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Src", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Type", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("EditURI", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Feed", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Post", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Self", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("StartIndex", typeof(int)));
                CommentDataTable.Columns.Add(new DataColumn("TotalResults", typeof(int)));
                CommentDataTable.Columns.Add(new DataColumn("IdURI", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("LinkCount", typeof(int)));
                CommentDataTable.Columns.Add(new DataColumn("LinkReadOnly", typeof(bool)));
                CommentDataTable.Columns.Add(new DataColumn("MediaURI", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("ReadOnly", typeof(bool)));
                CommentDataTable.Columns.Add(new DataColumn("Rights", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("SelfURI", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("Summary", typeof(string)));
                CommentDataTable.Columns.Add(new DataColumn("UserCount", typeof(int)));
                CommentDataTable.Columns.Add(new DataColumn("Sentiment", typeof(int)));
                CommentDataTable.Columns.Add(new DataColumn("LanguageCode", typeof(string)));


            foreach (var comment in Post.Comments)
                {
                    CommentDataTable.Rows.Add(
                        comment.CommentID,
	                    comment.PostID,
	                    comment.Content,
	                    comment.PublishedDate,
	                    comment.UpdatedDate,
	                    comment.Uri,
	                    comment.Title,
	                    comment.Src,
	                    comment.Type,
	                    comment.EditURI,
	                    comment.Feed,
	                    comment.Post,
	                    comment.Self,
	                    comment.StartIndex,
	                    comment.TotalResults,
	                    comment.IdURI,
	                    comment.LinkCount,
	                    comment.LinkReadOnly,
	                    comment.MediaURI,
	                    comment.ReadOnly,
	                    comment.Rights,
	                    comment.SelfURI,
	                    comment.Summary,
	                    comment.UserCount,
                        comment.Sentiment,
                        comment.LanguageCode
                        );
                    
                }


            SqlParameter tvp = new SqlParameter("@Comments", CommentDataTable);
                tvp.SqlDbType = SqlDbType.Structured;
                Parameters.Add(tvp);

                tvp = new SqlParameter("@Users", UserDataTable);
                tvp.SqlDbType = SqlDbType.Structured;
                Parameters.Add(tvp);

               tvp = new SqlParameter("@Links", LinkDataTable);
                tvp.SqlDbType = SqlDbType.Structured;
                Parameters.Add(tvp);
                tvp = new SqlParameter("@Searchterms", SearchtermDataTable);
                tvp.SqlDbType = SqlDbType.Structured;
                Parameters.Add(tvp);

                obj = DBUtil.ExecuteScalar(query, Parameters, null, CommandType.StoredProcedure);
                if (obj != null)
                    id = Convert.ToInt64(obj);
                else
                    return false;

                return true;
       }
    }
}
