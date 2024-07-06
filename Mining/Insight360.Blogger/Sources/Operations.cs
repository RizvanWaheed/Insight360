//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data.SqlClient;
//using System.Collections;
//using Insight360.DAL;
//using Insight360.BO;
//using Google.GData.Blogger;
//using System.Data;
//using Google.GData.Client;
//using System.Text.RegularExpressions;
//using System.Diagnostics;

namespace Insight360.Blogger
{
	public class Operations
	{

		//        #region Config Settings

		//        private ArrayList entryList;
		//        private string feedUri;
		//        private string feedContent;

		//        public string BloggerURI = "http://sleeptalkinman.blogspot.com/feeds/posts/default/";
		//        public string BloggerURI = "http://mufarooqi.blogspot.com/feeds/comments/default/";

		//        public string UserName = "mufarooqi";
		//        public string Password = "p@kist@n;";
		//        public string connectionString = "Server=localhost;Integrated security=SSPI;database=BlogData";
		//        public string table_Blog = "[dbo].[Blog]";
		//        public string table_Comment = "[dbo].[Comment]";
		//        public string table_Post = "[dbo].[Post]";
		//        public string table_Users = "[dbo].[Users]";
		//        public string table_Link = "[dbo].[Link]";
		//        public string table_Post_User_Rel = "[dbo].[Post_User_Rel]";
		//        public string table_Post_Link_Rel = "[dbo].[Post_Link_Rel]";
		//        public string table_Comment_User_Rel = "[dbo].[Comment_User_Rel]";
		//        public string table_Comment_Link_Rel = "[dbo].[Comment_Link_Rel]";
		//        public BloggerService _service;

		//        public SqlConnection destinationDB = new SqlConnection();
		//        public DateTime maxdate;
		//        public DateTime maxUpdateDate;

		//        public BloggerService service
		//        {
		//            get
		//            {

		//                if (_service == null)
		//                    _service = new BloggerService("BloggerSampleApp.NET");

		//                if (UserName != null && UserName.Length > 0)
		//                {
		//                    _service.Credentials = new GDataCredentials(UserName, Password);
		//                }
		//                return _service;

		//            }

		//        }

		//        #endregion


		//        public void StartAPI()
		//        {
		//            this.entryList = new ArrayList();
		//            this.feedUri = null;
		//            this.feedContent = null;
		//            SqlConnection destinationDB = new SqlConnection(connectionString);
		//            destinationDB.ConnectionString = connectionString;
		//            destinationDB.Open();


		//            SqlCommand myCommand = new SqlCommand("select * from "+table_Blog+" where Active = 1", destinationDB);
		//            DBUtil.ConnectionString = connectionString;

		//            List<OrganizationConfiguration> orgConfig = ConfigDAL.FetchOrganizationConfiguration();
		//            List<DocumentSourceInfo> docSourceInfo = ConfigDAL.FetchDocumentSourceConfiguration("Blogger");
		//            try
		//            {
		//                SqlDataReader sdr = myCommand.ExecuteReader();
		//                foreach (DocumentSourceInfo doc_info in docSourceInfo)
		//                {
		//                    this.feedUri = sdr["BlogUrl"].ToString();

		//                    this.feedUri = doc_info.Url.TrimEnd('/') + "/feeds/posts/default/";
		//                    to-remove
		//                    this.feedUri = "https://www.blogger.com/feeds/16814760778503948497/blogs";
		//                    BloggerURI = this.feedUri;

		//                    int isBlogUrlValid = this.feedUri.ToLower().IndexOf("blogspot.com");
		//                    if (isBlogUrlValid == -1)
		//                    {
		//                        continue;
		//                    }


		//                    maxUpdateDate = DateTime.SpecifyKind(doc_info.LastFetchTime, DateTimeKind.Utc);
		//                    DateTime temp_dt = maxUpdateDate;//.ToUniversalTime();
		//                    temp_dt = maxUpdateDate.ToLocalTime();
		//                    -to remove
		//                    maxUpdateDate = new DateTime(2011, 2, 1, 0, 0, 0);

		//                    try
		//                    {
		//                        GetPosts();

		//                        GetComments();
		//                    }
		//                    catch (Exception ex)
		//                    {



		//                        throw ex;
		//                    }
		//                    finally
		//                    {
		//                        to-uncomment
		//                        maxUpdateDate = getMaxUpdateDate("post");
		//                        DateTime dt = getMaxUpdateDate("comment");
		//                        maxUpdateDate = maxUpdateDate > dt ? maxUpdateDate : dt;

		//                        doc_info.LastFetchTime = maxUpdateDate;
		//                        ConfigDAL.UpdateDocumentSourceConfiguration(doc_info);
		//                    }
		//                }

		//            }
		//            catch (System.Exception ex)
		//            {
		//                throw ex;
		//                MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
		//            }
		//            finally
		//            {
		//                if (destinationDB.State == ConnectionState.Open)
		//                {
		//                    destinationDB.Close();
		//                }
		//            }

		//        }


		//        public DateTime getMaxUpdateDate(string feed_for)
		//        {
		//            SqlConnection tempConn = new SqlConnection();
		//            tempConn.ConnectionString = this.connectionString;
		//            tempConn.Open();
		//            DateTime dt = new DateTime(1999, 1, 1);
		//            DateTime dt = new DateTime();
		//            string cmdtext = "select max(p.updateddate) from " + table_Post + " p;";

		//            if (feed_for == "comment")
		//            {
		//                cmdtext = "select max(c.updateddate) from " + table_Comment + " c;";
		//            }

		//            try
		//            {
		//                SqlCommand getMaxPublishedDate = new SqlCommand(cmdtext, tempConn);
		//                dt = (DateTime)getMaxPublishedDate.ExecuteScalar();
		//                dt = dt.AddSeconds(1);
		//            }
		//            catch (System.Exception ex)
		//            {
		//                throw ex;
		//                MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
		//            }
		//            finally
		//            {
		//                if (tempConn.State == ConnectionState.Open)
		//                {
		//                    tempConn.Close();
		//                }
		//            }

		//            return dt;



		//        }


		//        public void GetPosts()
		//        {

		//            string bloggerURI = this.BloggerURI;
		//            string userName = this.UserName;
		//            string passWord = this.Password;

		//            BloggerQuery query = new BloggerQuery();


		//            maxUpdateDate = DateTime.SpecifyKind(maxUpdateDate, DateTimeKind.Utc);

		//            query.Uri = new Uri(bloggerURI);
		//            query.OrderBy = BloggerQuery.OrderByUpdated;
		//            query.ModifiedSince = maxUpdateDate;
		//            query.StartDate = maxUpdateDate;

		//            /*
		//            string query_uri = query.Uri.ToString();
		//            string sub_query_uri = query_uri.Substring(query_uri.IndexOf("updated-min="));
		//            sub_query_uri = sub_query_uri.Substring(0, sub_query_uri.IndexOf('&'));
		//            string to_replace_query = sub_query_uri.Substring(0, 31);
		//            query_uri = query_uri.Replace(sub_query_uri, to_replace_query);

		//            query.Uri = new Uri(query_uri);
		//            */
		//            BloggerFeed bloggerFeed = service.Query(query);

		//            if (bloggerFeed.Entries.Count == 0)
		//            {
		//                bloggerURI = bloggerURI.Replace("/default/", "/summary/");
		//                query.Uri = new Uri(bloggerURI);
		//                query.OrderBy = BloggerQuery.OrderByUpdated;
		//                query.ModifiedSince = maxUpdateDate;
		//                query.StartDate = maxUpdateDate;
		//                bloggerFeed = service.Query(query);
		//            }

		//            string blogid = bloggerFeed.Id.Uri.Content.Substring(Convert.ToString(bloggerFeed.Id.Uri.Content).LastIndexOf("blog-") + 5);
		//            string blogurl = bloggerFeed.Id.AbsoluteUri;


		//            ArrayList FeedChooser = new ArrayList();

		//            while (bloggerFeed != null && bloggerFeed.Entries.Count > 0)
		//            {
		//                foreach (BloggerEntry entry in bloggerFeed.Entries)
		//                {
		//                    int iIndex = FeedChooser.Add(new ListEntry(entry));
		//                }
		//                 do the chunking...
		//                if (bloggerFeed.NextChunk != null)
		//                {

		//                    string query_uri = bloggerFeed.NextChunk;
		//                    string sub_query_uri = query_uri.Substring(query_uri.IndexOf("updated-min="));
		//                    sub_query_uri = sub_query_uri.Substring(0, sub_query_uri.IndexOf('&'));
		//                    string to_replace_query = sub_query_uri.Substring(0, 31);
		//                    query_uri = query_uri.Replace(sub_query_uri, to_replace_query);

		//                    query.Uri = new Uri(query_uri);
		//                    bloggerFeed = service.Query(query);
		//                }
		//                else
		//                {
		//                    bloggerFeed = null;
		//                }
		//            }

		//            if (destinationDB.State != ConnectionState.Open)
		//            {
		//                destinationDB.ConnectionString = connectionString;
		//                destinationDB.Open();
		//            }





		//            try
		//            {


		//                foreach (ListEntry alist in FeedChooser)
		//                {
		//                    AtomId tempID = alist.Entry.Id;

		//                    string blog_post_id = tempID.Uri.Content.Substring(Convert.ToString(tempID.Uri.Content).LastIndexOf("blog-") + 5);
		//                    string postid = blog_post_id.Substring(Convert.ToString(blog_post_id).LastIndexOf("post-") + 5);

		//                    DateTime publish_date = alist.Entry.Published;
		//                    DateTime update_date = alist.Entry.Updated;
		//                    string post_content = @StripHTML(alist.Entry.Content.Content);
		//                    alist.Postid = postid;

		//                    string altURI = alist.Entry.AlternateUri.Content == null ? "" : alist.Entry.AlternateUri.Content;
		//                    string src = alist.Entry.Content.Src == null ? "" : (alist.Entry.Content.Src.Content == null ? "" : alist.Entry.Content.Src.Content);
		//                    string type = alist.Entry.Content.Type == null ? "" : alist.Entry.Content.Type;
		//                    string EditURI = alist.Entry.EditUri == null ? "" : (alist.Entry.EditUri.Content == null ? "" : alist.Entry.EditUri.Content);
		//                    string Feed = alist.Entry.Feed.Feed == null ? "" : alist.Entry.Feed.Feed;
		//                    string Post = alist.Entry.Feed.Post == null ? "" : alist.Entry.Feed.Post;
		//                    string Self = alist.Entry.Feed.Self == null ? "" : alist.Entry.Feed.Self;
		//                    int StartIndex = alist.Entry.Feed.StartIndex == null ? 0 : alist.Entry.Feed.StartIndex;
		//                    int TotalResults = alist.Entry.Feed.TotalResults == null ? 0 : alist.Entry.Feed.TotalResults;
		//                    string IdUri = alist.Entry.Id.Uri == null ? "" : (alist.Entry.Id.Uri.Content == null ? "" : alist.Entry.Id.Uri.Content);
		//                    int LinkCount = alist.Entry.Links.Count == null ? 0 : alist.Entry.Links.Count;
		//                    bool LinkReadOnly = alist.Entry.Links.IsReadOnly == null ? true : alist.Entry.Links.IsReadOnly;
		//                    string MediaURI = alist.Entry.MediaUri == null ? "" : (alist.Entry.MediaUri.Content == null ? "" : alist.Entry.MediaUri.Content);
		//                    bool ReadOnly = alist.Entry.ReadOnly == null ? true : alist.Entry.ReadOnly;
		//                    string Rights = alist.Entry.Rights.Text == null ? "" : alist.Entry.Rights.Text;
		//                    string SelfURI = alist.Entry.SelfUri == null ? "" : (alist.Entry.SelfUri.Content == null ? "" : alist.Entry.SelfUri.Content);
		//                    string Summary = alist.Entry.Summary.Text == null ? "" : alist.Entry.Summary.Text;
		//                    int UserCount = alist.Entry.Authors.Count == null ? 0 : alist.Entry.Authors.Count;

		//                    string str = "INSERT INTO " + table_Post + " " +
		//                                  " ([Staging_OrgName] " +
		//                                  " ,[Staging_ProcessState] " +
		//                                  " ,[Staging_UpdateTime] " +
		//                                  " ,[Staging_InsertTime] " +
		//                                  " ,[PostID] " +
		//                                  " ,[BlogID] " +
		//                                  " ,[Uri] " +
		//                                  " ,[PublishedDate] " +
		//                                  " ,[UpdatedDate] " +
		//                                  " ,[Content] " +
		//                                  " ,[Title] " +
		//                                  " ,[Src] " +
		//                                  " ,[Type] " +
		//                                  " ,[EditURI] " +
		//                                  " ,[Feed] " +
		//                                  " ,[Post] " +
		//                                  " ,[Self] " +
		//                                  " ,[StartIndex] " +
		//                                  " ,[TotalResults] " +
		//                                  " ,[IdURI] " +
		//                                  " ,[LinkCount] " +
		//                                  " ,[LinkReadOnly] " +
		//                                  " ,[MediaURI] " +
		//                                  " ,[ReadOnly] " +
		//                                  " ,[Rights] " +
		//                                  " ,[SelfURI] " +
		//                                  " ,[Summary] " +
		//                                  " ,[UserCount]) " +

		//                                  "  VALUES ('" +
		//                                  "Telenor" + "'," +
		//                                  0 + ",'" +
		//                                  DateTime.Now + "','" +
		//                                  DateTime.Now + "','" +

		//                                  postid + "','" +
		//                                  blogid + "','" +
		//                                  Convert.ToString(altURI) + "','" +
		//                                  publish_date + "','" +
		//                                  update_date + "','" +
		//                                  post_content.Replace("'", "''") + "','" +
		//                                  Convert.ToString(alist.Entry.Title.Text.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(src.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(type.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(EditURI.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(Feed.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(Post.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(Self.Replace("'", "''")) + "'," +
		//                                  Convert.ToString(StartIndex) + "," +
		//                                  Convert.ToString(TotalResults) + ",'" +
		//                                  Convert.ToString(IdUri.Replace("'", "''")) + "'," +
		//                                  Convert.ToString(LinkCount) + "," +
		//                                  Convert.ToString(LinkReadOnly == true ? 1 : 0) + ",'" +
		//                                  Convert.ToString(MediaURI.Replace("'", "''")) + "'," +
		//                                  Convert.ToString(ReadOnly == true ? 1 : 0) + ",'" +
		//                                  Convert.ToString(Rights.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(SelfURI.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(Summary.Replace("'", "''")) + "'," +
		//                                  Convert.ToString(UserCount) + "" +


		//                                  ")";


		//                    SqlCommand myCommand = new SqlCommand(str, destinationDB);

		//                    try
		//                    {
		//                        myCommand.ExecuteNonQuery();
		//                    }
		//                    catch (Exception ex)
		//                    {
		//                        throw ex;
		//                    }
		//                    long userid = 0;
		//                    int counter = 0;
		//                    for (counter = 0; counter < UserCount; counter++)
		//                    {
		//                        string username = alist.Entry.Authors[counter].Name == null ? "" : alist.Entry.Authors[counter].Name;
		//                        string useremail = alist.Entry.Authors[counter].Email == null ? "" : alist.Entry.Authors[counter].Email;
		//                        string userlang = alist.Entry.Authors[counter].Language == null ? "" : alist.Entry.Authors[counter].Language;
		//                        string useruri = alist.Entry.Authors[counter].Uri == null ? "" : (alist.Entry.Authors[counter].Uri.Content == null ? "" : alist.Entry.Authors[counter].Uri.Content);

		//                        str = "select coalesce(max(UserId),0) from " + table_Users + " where Name = '" + username.Replace("'", "''") + "' and Email = '" + useremail.Replace("'", "''") + "' and Language = '"
		//                                + userlang.Replace("'", "''") + "' and URI = '" + useruri.Replace("'", "''") + "'";


		//                        myCommand = new SqlCommand(str, destinationDB);


		//                        userid = Convert.ToInt64(myCommand.ExecuteScalar());
		//                        if (userid == 0)
		//                        {
		//                            str = "INSERT INTO " + table_Users + "" +
		//                                  " ([Name] " +
		//                                  " ,[Email] " +
		//                                  " ,[Language] " +
		//                                  " ,[URI]) " +
		//                                  " VALUES " +
		//                                  " ( '" +
		//                                  Convert.ToString(username.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(useremail.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(userlang.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(useruri.Replace("'", "''")) +
		//                                  " ') ";
		//                            myCommand = new SqlCommand(str, destinationDB);

		//                            try
		//                            {

		//                                myCommand.ExecuteNonQuery();
		//                            }
		//                            catch (Exception ex)
		//                            {

		//                            }
		//                        }

		//                        str = "select coalesce(max(UserId),0) from " + table_Users + " where Name = '" + username.Replace("'", "''") + "' and Email = '" + useremail.Replace("'", "''") + "' and Language = '"
		//                                + userlang.Replace("'", "''") + "' and URI = '" + useruri.Replace("'", "''") + "'";


		//                        myCommand = new SqlCommand(str, destinationDB);


		//                        userid = Convert.ToInt64(myCommand.ExecuteScalar());

		//                        str = "INSERT INTO " + table_Post_User_Rel + "" +
		//                                  " ([UserId] " +
		//                                  " ,[PostId]) " +

		//                                  " VALUES " +
		//                                  " ( " +
		//                                  Convert.ToString(userid) + ",'" +
		//                                  Convert.ToString(postid.Replace("'", "''")) +

		//                                  " ') ";
		//                        myCommand = new SqlCommand(str, destinationDB);

		//                        try
		//                        {

		//                            myCommand.ExecuteNonQuery();
		//                        }
		//                        catch
		//                        {
		//                        }

		//                    }

		//                    /////////////////////////////////////////////////////// Link ///////////////////////////////////////////////////////////////

		//                    long linkid = 0;
		//                    for (counter = 0; counter < LinkCount; counter++)
		//                    {
		//                        string linkuri = alist.Entry.Links[counter].AbsoluteUri == null ? "" : alist.Entry.Links[counter].AbsoluteUri;
		//                        string linkrel = alist.Entry.Links[counter].Rel == null ? "" : alist.Entry.Links[counter].Rel;
		//                        string linktitle = alist.Entry.Links[counter].Title == null ? "" : alist.Entry.Links[counter].Title;
		//                        string linktype = alist.Entry.Links[counter].Type == null ? "" : alist.Entry.Links[counter].Type;

		//                        str = "select coalesce(max(LinkId),0) from " + table_Link + " where AbsoluteURI = '" + linkuri.Replace("'", "''") + "' and Rel = '" + linkrel.Replace("'", "''") + "' and Title = '"
		//                                + linktitle.Replace("'", "''") + "' and Type = '" + linktype.Replace("'", "''") + "'";


		//                        myCommand = new SqlCommand(str, destinationDB);


		//                        linkid = Convert.ToInt64(myCommand.ExecuteScalar());
		//                        if (linkid == 0)
		//                        {
		//                            str = "INSERT INTO " + table_Link + "" +
		//                                  " ([AbsoluteURI] " +
		//                                  " ,[Rel] " +
		//                                  " ,[Title] " +
		//                                  " ,[Type]) " +
		//                                  " VALUES " +
		//                                  " ( '" +
		//                                  Convert.ToString(linkuri.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(linkrel.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(linktitle.Replace("'", "''")) + "','" +
		//                                  Convert.ToString(linktype.Replace("'", "''")) +
		//                                  " ') ";
		//                            myCommand = new SqlCommand(str, destinationDB);

		//                            try
		//                            {

		//                                myCommand.ExecuteNonQuery();
		//                            }
		//                            catch
		//                            {
		//                            }
		//                        }

		//                        str = "select coalesce(max(LinkId),0) from " + table_Link + " where AbsoluteURI = '" + linkuri.Replace("'", "''") + "' and Rel = '" + linkrel.Replace("'", "''") + "' and Title = '"
		//                               + linktitle.Replace("'", "''") + "' and Type = '" + linktype.Replace("'", "''") + "'";


		//                        myCommand = new SqlCommand(str, destinationDB);


		//                        linkid = Convert.ToInt64(myCommand.ExecuteScalar());

		//                        str = "INSERT INTO " + table_Post_Link_Rel + "" +
		//                                  " ([LinkId] " +
		//                                  " ,[PostId]) " +

		//                                  " VALUES " +
		//                                  " ( " +
		//                                  Convert.ToString(linkid) + ",'" +
		//                                  Convert.ToString(postid.Replace("'", "''")) +

		//                                  " ') ";
		//                        myCommand = new SqlCommand(str, destinationDB);

		//                        try
		//                        {

		//                            myCommand.ExecuteNonQuery();
		//                        }
		//                        catch
		//                        {
		//                        }

		//                    }


		//                }
		//            }
		//            catch (System.Exception ex)
		//            {
		//                throw ex;
		//                MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
		//            }
		//            finally
		//            {
		//                if (destinationDB.State == ConnectionState.Open)
		//                {
		//                    destinationDB.Close();
		//                }

		//            }




		//        }

		//        private static string StripHTML(string HTMLText)
		//        {
		//            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase); return reg.Replace(HTMLText, "");
		//        }

		//        private void GetComments()
		//        {
		//            string userName = this.UserName;
		//            string passWord = this.Password;

		//            if (this.feedUri != null)
		//            {
		//                BloggerQuery query = new BloggerQuery();



		//                string bloggerURIforComments = this.BloggerURI.ToString().Replace("/posts/", "/comments/");
		//                query.Uri = new Uri(bloggerURIforComments);

		//                query.OrderBy = BloggerQuery.OrderByUpdated;
		//                query.ModifiedSince = maxUpdateDate;
		//                query.StartDate = maxUpdateDate;

		//                BloggerFeed bloggerFeed = service.Query(query);

		//                ArrayList FeedChooser = new ArrayList();
		//                while (bloggerFeed != null && bloggerFeed.Entries.Count > 0)
		//                {

		//                    foreach (BloggerEntry entry in bloggerFeed.Entries)
		//                    {
		//                        FeedChooser.Add(new ListEntry(entry));
		//                    }

		//                     do the chunking...
		//                    if (bloggerFeed.NextChunk != null)
		//                    {
		//                        query.Uri = new Uri(bloggerFeed.NextChunk);
		//                        bloggerFeed = service.Query(query);
		//                        string query_uri = bloggerFeed.NextChunk;
		//                        string sub_query_uri = query_uri.Substring(query_uri.IndexOf("updated-min="));
		//                        sub_query_uri = sub_query_uri.Substring(0, sub_query_uri.IndexOf('&'));
		//                        string to_replace_query = sub_query_uri.Substring(0, 31);
		//                        query_uri = query_uri.Replace(sub_query_uri, to_replace_query);
		//                        query.Uri = new Uri(bloggerFeed.NextChunk);
		//                        query.Uri = new Uri(query_uri);
		//                        bloggerFeed = service.Query(query);
		//                    }
		//                    else
		//                    {
		//                        bloggerFeed = null;
		//                    }
		//                }





		//                if (destinationDB.State != ConnectionState.Open)
		//                {
		//                    destinationDB.ConnectionString = connectionString;
		//                    destinationDB.Open();
		//                }

		//                string post_content = "";
		//                long userid;// = "";
		//                try
		//                {

		//                    foreach (ListEntry alist in FeedChooser)
		//                    {
		//                        AtomId tempID = alist.Entry.Id;
		//                        string comment_feed_url = alist.Entry.Feed.Feed;

		//                        string blog_post_id = tempID.Uri.Content.Substring(Convert.ToString(tempID.Uri.Content).LastIndexOf("blog-") + 5);
		//                        string commentid = blog_post_id.Substring(Convert.ToString(blog_post_id).LastIndexOf("post-") + 5);
		//                        DateTime publish_date = alist.Entry.Published;
		//                        DateTime update_date = alist.Entry.Updated;


		//                        string self_uri = alist.Entry.SelfUri.Content == null ? "" : alist.Entry.SelfUri.Content;
		//                        self_uri = self_uri.Substring(self_uri.IndexOf("/feeds/") + 7);
		//                        self_uri = self_uri.Substring(self_uri.IndexOf("/") + 1);
		//                        self_uri = self_uri.Substring(0, self_uri.IndexOf("/comments/"));
		//                        string postid = self_uri;

		//                        post_content = @StripHTML(alist.Entry.Content.Content);
		//                        userid = alist.Entry.Authors[0].Name == null ? "" : alist.Entry.Authors[0].Name;
		//                        string useremail = alist.Entry.Authors[0].Email == null ? "" : alist.Entry.Authors[0].Email;
		//                        string userlang = alist.Entry.Authors[0].Language == null ? "" : alist.Entry.Authors[0].Language;

		//                        string altURI = alist.Entry.AlternateUri.Content == null ? "" : alist.Entry.AlternateUri.Content;
		//                        string src = alist.Entry.Content.Src == null ? "" : (alist.Entry.Content.Src.Content == null ? "" : alist.Entry.Content.Src.Content);
		//                        string type = alist.Entry.Content.Type == null ? "" : alist.Entry.Content.Type;
		//                        string EditURI = alist.Entry.EditUri == null ? "" : (alist.Entry.EditUri.Content == null ? "" : alist.Entry.EditUri.Content);
		//                        string Feed = alist.Entry.Feed.Feed == null ? "" : alist.Entry.Feed.Feed;
		//                        string Post = alist.Entry.Feed.Post == null ? "" : alist.Entry.Feed.Post;
		//                        string Self = alist.Entry.Feed.Self == null ? "" : alist.Entry.Feed.Self;
		//                        int StartIndex = alist.Entry.Feed.StartIndex == null ? 0 : alist.Entry.Feed.StartIndex;
		//                        int TotalResults = alist.Entry.Feed.TotalResults == null ? 0 : alist.Entry.Feed.TotalResults;
		//                        string IdUri = alist.Entry.Id.Uri == null ? "" : (alist.Entry.Id.Uri.Content == null ? "" : alist.Entry.Id.Uri.Content);
		//                        int LinkCount = alist.Entry.Links.Count == null ? 0 : alist.Entry.Links.Count;
		//                        bool LinkReadOnly = alist.Entry.Links.IsReadOnly == null ? true : alist.Entry.Links.IsReadOnly;
		//                        string MediaURI = alist.Entry.MediaUri == null ? "" : (alist.Entry.MediaUri.Content == null ? "" : alist.Entry.MediaUri.Content);
		//                        bool ReadOnly = alist.Entry.ReadOnly == null ? true : alist.Entry.ReadOnly;
		//                        string Rights = alist.Entry.Rights.Text == null ? "" : alist.Entry.Rights.Text;
		//                        string SelfURI = alist.Entry.SelfUri == null ? "" : (alist.Entry.SelfUri.Content == null ? "" : alist.Entry.SelfUri.Content);
		//                        string Summary = alist.Entry.Summary.Text == null ? "" : alist.Entry.Summary.Text;
		//                        int UserCount = alist.Entry.Authors.Count == null ? 0 : alist.Entry.Authors.Count;



		//                        string str = "INSERT INTO " + table_Comment + "" +
		//                                      " ([CommentID] " +
		//                                      " ,[PostID] " +
		//                                     " ,[Uri] " +
		//                                      " ,[PublishedDate] " +
		//                                      " ,[UpdatedDate] " +
		//                                      " ,[Content] " +
		//                                      " ,[Title] " +
		//                                      " ,[Src] " +
		//                                      " ,[Type] " +
		//                                      " ,[EditURI] " +
		//                                      " ,[Feed] " +
		//                                      " ,[Post] " +
		//                                      " ,[Self] " +
		//                                      " ,[StartIndex] " +
		//                                      " ,[TotalResults] " +
		//                                      " ,[IdURI] " +
		//                                      " ,[LinkCount] " +
		//                                      " ,[LinkReadOnly] " +
		//                                      " ,[MediaURI] " +
		//                                      " ,[ReadOnly] " +
		//                                      " ,[Rights] " +
		//                                      " ,[SelfURI] " +
		//                                      " ,[Summary] " +
		//                                      " ,[UserCount]) " +


		//                                      "  VALUES ('" +
		//                                      commentid + "','" +
		//                                      postid + "','" +
		//                                      Convert.ToString(altURI) + "','" +
		//                                      publish_date + "','" +
		//                                      update_date + "','" +
		//                                      post_content.Replace("'", "''") + "','" +
		//                                      Convert.ToString(alist.Entry.Title.Text.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(src.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(type.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(EditURI.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(Feed.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(Post.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(Self.Replace("'", "''")) + "'," +
		//                                      Convert.ToString(StartIndex) + "," +
		//                                      Convert.ToString(TotalResults) + ",'" +
		//                                      Convert.ToString(IdUri.Replace("'", "''")) + "'," +
		//                                      Convert.ToString(LinkCount) + "," +
		//                                      Convert.ToString(LinkReadOnly == true ? 1 : 0) + ",'" +
		//                                      Convert.ToString(MediaURI.Replace("'", "''")) + "'," +
		//                                      Convert.ToString(ReadOnly == true ? 1 : 0) + ",'" +
		//                                      Convert.ToString(Rights.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(SelfURI.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(Summary.Replace("'", "''")) + "'," +
		//                                      Convert.ToString(UserCount) + "" +

		//                                      ")";


		//                        SqlCommand myCommand = new SqlCommand(str, destinationDB);

		//                        try
		//                        {
		//                            myCommand.ExecuteNonQuery();
		//                        }
		//                        catch
		//                        {
		//                        }

		//                        /////////////////////////////////////////////////////// User ///////////////////////////////////////////////////////////////

		//                        int counter = 0;
		//                        for (counter = 0; counter < UserCount; counter++)
		//                        {
		//                            string username = alist.Entry.Authors[counter].Name == null ? "" : alist.Entry.Authors[counter].Name;
		//                            string useremail = alist.Entry.Authors[counter].Email == null ? "" : alist.Entry.Authors[counter].Email;
		//                            string userlang = alist.Entry.Authors[counter].Language == null ? "" : alist.Entry.Authors[counter].Language;
		//                            string useruri = alist.Entry.Authors[counter].Uri == null ? "" : (alist.Entry.Authors[counter].Uri.Content == null ? "" : alist.Entry.Authors[counter].Uri.Content);

		//                            str = "select coalesce(max(UserId),0) from " + table_Users + " where Name = '" + username.Replace("'", "''") + "' and Email = '" + useremail.Replace("'", "''") + "' and Language = '"
		//                                    + userlang.Replace("'", "''") + "' and URI = '" + useruri.Replace("'", "''") + "'";


		//                            myCommand = new SqlCommand(str, destinationDB);


		//                            userid = Convert.ToInt64(myCommand.ExecuteScalar());
		//                            if (userid == 0)
		//                            {
		//                                str = "INSERT INTO " + table_Users + "" +
		//                                      " ([Name] " +
		//                                      " ,[Email] " +
		//                                      " ,[Language] " +
		//                                      " ,[URI]) " +
		//                                      " VALUES " +
		//                                      " ( '" +
		//                                      Convert.ToString(username.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(useremail.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(userlang.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(useruri.Replace("'", "''")) +
		//                                      " ') ";
		//                                myCommand = new SqlCommand(str, destinationDB);

		//                                try
		//                                {

		//                                    myCommand.ExecuteNonQuery();
		//                                }
		//                                catch
		//                                {
		//                                }
		//                            }

		//                            str = "select coalesce(max(UserId),0) from " + table_Users + " where Name = '" + username.Replace("'", "''") + "' and Email = '" + useremail.Replace("'", "''") + "' and Language = '"
		//                                    + userlang.Replace("'", "''") + "' and URI = '" + useruri.Replace("'", "''") + "'";


		//                            myCommand = new SqlCommand(str, destinationDB);


		//                            userid = Convert.ToInt64(myCommand.ExecuteScalar());

		//                            str = "INSERT INTO " + table_Comment_User_Rel + "" +
		//                                      " ([UserId] " +
		//                                      " ,[CommentId]) " +

		//                                      " VALUES " +
		//                                      " ( " +
		//                                      Convert.ToString(userid) + ",'" +
		//                                      Convert.ToString(commentid.Replace("'", "''")) +

		//                                      " ') ";
		//                            myCommand = new SqlCommand(str, destinationDB);

		//                            try
		//                            {

		//                                myCommand.ExecuteNonQuery();
		//                            }
		//                            catch
		//                            {
		//                            }

		//                        }




		//                        /////////////////////////////////////////////////////// Link ///////////////////////////////////////////////////////////////

		//                        long linkid = 0;
		//                        for (counter = 0; counter < LinkCount; counter++)
		//                        {
		//                            string linkuri = alist.Entry.Links[counter].AbsoluteUri == null ? "" : alist.Entry.Links[counter].AbsoluteUri;
		//                            string linkrel = alist.Entry.Links[counter].Rel == null ? "" : alist.Entry.Links[counter].Rel;
		//                            string linktitle = alist.Entry.Links[counter].Title == null ? "" : alist.Entry.Links[counter].Title;
		//                            string linktype = alist.Entry.Links[counter].Type == null ? "" : alist.Entry.Links[counter].Type;

		//                            str = "select coalesce(max(LinkId),0) from " + table_Link + " where AbsoluteURI = '" + linkuri.Replace("'", "''") + "' and Rel = '" + linkrel.Replace("'", "''") + "' and Title = '"
		//                                    + linktitle.Replace("'", "''") + "' and Type = '" + linktype.Replace("'", "''") + "'";


		//                            myCommand = new SqlCommand(str, destinationDB);


		//                            linkid = Convert.ToInt64(myCommand.ExecuteScalar());
		//                            if (linkid == 0)
		//                            {
		//                                str = "INSERT INTO " + table_Link + "" +
		//                                      " ([AbsoluteURI] " +
		//                                      " ,[Rel] " +
		//                                      " ,[Title] " +
		//                                      " ,[Type]) " +
		//                                      " VALUES " +
		//                                      " ( '" +
		//                                      Convert.ToString(linkuri.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(linkrel.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(linktitle.Replace("'", "''")) + "','" +
		//                                      Convert.ToString(linktype.Replace("'", "''")) +
		//                                      " ') ";
		//                                myCommand = new SqlCommand(str, destinationDB);

		//                                try
		//                                {

		//                                    myCommand.ExecuteNonQuery();
		//                                }
		//                                catch
		//                                {
		//                                }
		//                            }

		//                            str = "select coalesce(max(LinkId),0) from " + table_Link + " where AbsoluteURI = '" + linkuri.Replace("'", "''") + "' and Rel = '" + linkrel.Replace("'", "''") + "' and Title = '"
		//                                   + linktitle.Replace("'", "''") + "' and Type = '" + linktype.Replace("'", "''") + "'";


		//                            myCommand = new SqlCommand(str, destinationDB);


		//                            linkid = Convert.ToInt64(myCommand.ExecuteScalar());

		//                            str = "INSERT INTO " + table_Comment_Link_Rel + "" +
		//                                      " ([LinkId] " +
		//                                      " ,[CommentId]) " +

		//                                      " VALUES " +
		//                                      " ( " +
		//                                      Convert.ToString(linkid) + ",'" +
		//                                      Convert.ToString(commentid.Replace("'", "''")) +

		//                                      " ') ";
		//                            myCommand = new SqlCommand(str, destinationDB);

		//                            try
		//                            {

		//                                myCommand.ExecuteNonQuery();
		//                            }
		//                            catch
		//                            {
		//                            }

		//                        }



		//                    }
		//                }
		//                catch (System.Exception ex)
		//                {
		//                    throw ex;
		//                    MessageBox.Show(ex.ToString(), "MyProgram", MessageBoxButtons.OK, MessageBoxIcon.Information);
		//                }
		//                finally
		//                {
		//                    if (destinationDB.State == ConnectionState.Open)
		//                    {
		//                        destinationDB.Close();
		//                    }
		//                }






		//            }


		//        }

		public static bool PostNewComment( string postID , string commentText , string authorName )
		{
			throw new System.NotImplementedException( );
			//           // Get Blog ID first

			//            string blogid = DBOperations.GetBlogID(postID);

			//            string strCommentPostUri = "http://www.blogger.com/feeds/" + blogid + "/" + postID + "/comments/default";

			//            Google.GData.Client.Service service = new Service();

			//            Uri commentPostUri = new Uri(strCommentPostUri);

			//            AtomEntry postedComment = null;
			//            if (commentPostUri != null)
			//            {
			//                 //Add a comment.
			//                try
			//                {
			//                    AtomEntry comment = new AtomEntry();
			//                    comment.Title.Text = commentText;
			//                    comment.Content.Content = commentText;
			//                    comment.Authors.Add(new AtomPerson());
			//                    comment.Authors[0].Name = authorName;
			//                    postedComment = service.Insert(commentPostUri, comment);
			//                }
			//                catch (Exception ex)
			//                {
			//                    return false;
			//                }

			//            }
			//            return true;
		}





		//    }



		//     <summary>
		//     little helper class to put an atomentry into a combobox
		//     </summary>
		//    public class ListEntry
		//    {
		//        private BloggerEntry entry;
		//        private string postid;

		//        public BloggerEntry Entry
		//        {
		//            get
		//            {
		//                return this.entry;
		//            }
		//            set
		//            {
		//                this.entry = value;
		//            }
		//        }

		//        public string Postid
		//        {
		//            get
		//            {
		//                return this.postid;
		//            }
		//            set
		//            {
		//                this.postid = value;
		//            }
		//        }

		//        public ListEntry(BloggerEntry entry)
		//        {
		//            this.entry = entry;
		//        }

		//        public override string ToString()
		//        {
		//            return this.entry != null ? this.entry.Title.Text : "No Entry";
		//        }
	}

}
