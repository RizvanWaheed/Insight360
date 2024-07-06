using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections;

using Insight360.BO;
using Google.GData.Blogger;
using System.Data;
using Google.GData.Client;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Insight360.Util;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace Insight360.Blogger
{
    public class DataCollection
    {
        internal static BO.BloggerPost CreatePost(Google.Apis.Blogger.v3.Data.Post Post, string blogid, int documentSourceID, int totalResults)
        {
            BO.BloggerPost PostInfo = null;
            if (Post.Id != null)
            {
                try
                {
                    PostInfo = new Insight360.BO.BloggerPost()
                    {
                        Staging_UpdateTime = DateTime.Now,
                        Staging_InsertTime = DateTime.Now,
                        PostID = Post.Id,
                        BlogID = blogid,
                        Uri = Post.Url,
                        PublishedDate = Post.Published.Value,
                        UpdatedDate = Post.Updated.Value,
                        Content = Post.Content == null ? "" : StripHTML(Post.Content),
                        Title = Post.Title,
                        Src = "", //Post.Content.Src == null ? "" : (Post.Content.Src.Content == null ? "" : Post.Content.Src.Content),
                        Type = Post.Kind, // Post.Content.Type,
                        EditURI = "", //Post.EditUri == null ? "" : (Post.EditUri.Content == null ? "" : Post.EditUri.Content),
                        Feed = "", // Post.Feed.Feed == null ? "" : Post.Feed.Feed,
                        Post = "", //Post.Feed.Post == null ? "" : Post.Feed.Post,
                        Self = "", //Post.Feed.Self == null ? "" : Post.Feed.Self,
                        StartIndex = 0, //Post.Feed == null ? 0 : Post.Feed.StartIndex,
                        TotalResults = totalResults, //Post.Feed == null ? 0 : Post.Feed.TotalResults,
                        IdURI = "", //Post.Id.Uri == null ? "" : (Post.Id.Uri.Content == null ? "" : Post.Id.Uri.Content),
                        LinkCount = 0, //Post.Links == null ? 0 : Post.Links.Count,
                        LinkReadOnly = true, //Post.Links == null ? true : Post.Links.IsReadOnly,
                        MediaURI = "", //Post.MediaUri == null ? "" : (Post.MediaUri.Content == null ? "" : Post.MediaUri.Content),
                        ReadOnly = true, //Post.ReadOnly,
                        Rights = "", //Post.Rights.Text == null ? "" : Post.Rights.Text,
                        SelfURI = Post.SelfLink, //Post.SelfUri == null ? "" : (Post.SelfUri.Content == null ? "" : Post.SelfUri.Content),
                        Summary = "", //Post.Summary.Text == null ? "" : Post.Summary.Text,
                        UserCount = 1, //Post.Authors == null ? 0 : Post.Authors.Count,
                        DocumentSource_ID = documentSourceID,
                        Sentiment = Helpers.SentimentScore(StripHTML(Post.Content)),
                        LanguageCode = Helpers.DetectLanguage(StripHTML(Post.Content))
                    };

                    return PostInfo;
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"CreatePost\"");
                    Util.Log.WriteException(typeof(DataCollection), "CreatePost B1001", ex);
                }
            }

            PostInfo = new Insight360.BO.BloggerPost();
            return PostInfo;
        }

        internal static BO.BloggerUser CreateUser(Google.Apis.Blogger.v3.Data.Post Post)
        {
            BO.BloggerUser UserInfo = null;
            if (Post.Author != null)
            {
                try
                {
                    UserInfo = new Insight360.BO.BloggerUser()
                    {
                        Name = Post.Author.DisplayName,
                        Email = "", //Post.Authors[counter].Email == null ? "" : Post.Authors[counter].Email,
                        Language = "", //Post.Authors[counter].Language == null ? "" : Post.Authors[counter].Language,
                        URI = Post.Author.Url
                    };

                    return UserInfo;
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"CreateUser\"");
                    Util.Log.WriteException(typeof(DataCollection), "CreateUser B1002", ex);
                }
            }

            UserInfo = new Insight360.BO.BloggerUser();
            return UserInfo;
        }

        internal static BO.BloggerLink CreateLink(Google.Apis.Blogger.v3.Data.Post Post)
        {
            BO.BloggerLink LinkInfo = new Insight360.BO.BloggerLink()
            {
                AbsoluteURI = Post.Url,
                Rel = "",
                Title = Post.Title,
                Type = Post.Kind
            };

            return LinkInfo;
        }

        internal static BO.BloggerComment CreateComment(Google.Apis.Blogger.v3.Data.Comment Comment, Google.Apis.Blogger.v3.Data.Post Post, int totalResults)
        {
            BO.BloggerComment CommentInfo = null;
            if (Comment != null)
            {
                try
                {
                    CommentInfo = new Insight360.BO.BloggerComment()
                    {
                        CommentID = Comment.Id,
                        PostID = Comment.Post.Id,
                        PublishedDate = Comment.Published.Value,
                        UpdatedDate = Comment.Updated.Value,
                        Uri = Comment.SelfLink,
                        Title = Post.Title,
                        Src = "",
                        Type = Post.Kind,
                        EditURI = "",
                        Feed = "",
                        Post = "",
                        Self = Post.SelfLink,
                        StartIndex = 0,
                        TotalResults = totalResults,
                        IdURI = "",
                        LinkCount = 0,
                        LinkReadOnly = true,
                        MediaURI = "",
                        ReadOnly = true,
                        Rights = "",
                        SelfURI = Post.SelfLink,
                        Summary = "",
                        UserCount = 1,
                        Content = Comment.Content == null ? "" : StripHTML(Comment.Content),
                        Sentiment = Helpers.SentimentScore(StripHTML(Comment.Content == null ? "" : StripHTML(Comment.Content))),
                        LanguageCode = Helpers.DetectLanguage(StripHTML(Comment.Content == null ? "" : StripHTML(Comment.Content)))
                    };

                    return CommentInfo;
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"CreateComment\"");
                    Util.Log.WriteException(typeof(DataCollection), "CreateComment B1003", ex);
                }
            }

            CommentInfo = new Insight360.BO.BloggerComment();
            return CommentInfo;
        }

        private static string StripHTML(string HTMLText)
        {
            if (HTMLText == null || HTMLText == "")
                return "";
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase); return reg.Replace(HTMLText, "");
        }

        public static List<int> FindSearchTerms(string text, List<BO.SearchTermInfo> searchTerms, DateTime itemTime)
        {
            List<int> foundSearchTermIDs = new List<int>();
            text = text.ToLower();

            foreach (var searchTerm in searchTerms)
            {
                if (/*itemTime > searchTerm.LastFetchTime &&*/ text.ToLower().Contains(searchTerm.SearchTerm.ToLower())) //contains removed due to Lucene.net
                {
                    searchTerm.LastFetchTime = itemTime;
                    foundSearchTermIDs.Add(searchTerm.Id);
                }
            }

            return foundSearchTermIDs;
        }

        public static List<BloggerPost> GetPosts(BO.DocumentSourceInfo config, bool getComments, string apiKey)
        {
            List<Google.Apis.Blogger.v3.Data.Post> totalPosts = new List<Google.Apis.Blogger.v3.Data.Post>();

            Google.Apis.Blogger.v3.Data.PostList postListResult = null;

            Google.Apis.Services.BaseClientService.Initializer service = new Google.Apis.Services.BaseClientService.Initializer();
            service.ApiKey = apiKey; // "AIzaSyAdZZjpl7Akdwq3JP58pFFuGkkbCna-sM4";
            Google.Apis.Blogger.v3.BloggerService bloggerService = new Google.Apis.Blogger.v3.BloggerService(service);

            #region GetBLOG

            DAL.StagingDAL.LogMiner(32, "Blogger", "Processing", "Processing started for Blog: " + config.DisplayName);
            Google.Apis.Blogger.v3.BlogsResource.GetByUrlRequest blogQuery = bloggerService.Blogs.GetByUrl(config.Url); //("http://lailashyggehave.blogspot.com/");
            Google.Apis.Blogger.v3.Data.Blog blog = blogQuery.Execute();
            
            #endregion

            #region GetPOSTS
            try
            {
                foreach (var searchTerm in config.SearchTerms)
                {
                    //List<BO.DocumentSourceInfo> sources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Blogger");
                    //BO.DocumentSourceInfo source = sources.Where(x => x.DocumentSourceId == config.DocumentSourceId && x.SearchTerms != null && x.SearchTerms.Where(y => y.Id == searchTerm.Id && y.IsTermEnabled == 1) != null).First();

                    //if (searchTerm.IsTermEnabled == 1)  //commented on  Dec 08, 2016 by Tooba on requirement placed by Azfer
                    {
                        Google.Apis.Blogger.v3.PostsResource.SearchRequest bloggerSearchQuery = bloggerService.Posts.Search(blog.Id, searchTerm.SearchTerm); // search
                        Google.Apis.Blogger.v3.Data.PostList searchPostResult = bloggerSearchQuery.Execute(); // search

                        Google.Apis.Blogger.v3.PostsResource.ListRequest query = bloggerService.Posts.List(blog.Id); //bloggerService.Posts.Search(blog.Id, searchTerm.SearchTerm);
                        query.StartDate = searchTerm.LastFetchTime;
                        query.EndDate = DateTime.Now;
                        query.MaxResults = 500;

                        Google.Apis.Blogger.v3.Data.PostList tempPostResult = query.Execute();
                        postListResult = tempPostResult;

                        while (tempPostResult.NextPageToken != null)
                        {
                            query = bloggerService.Posts.List(blog.Id);
                            query.PageToken = tempPostResult.NextPageToken;

                            tempPostResult = query.Execute();

                            postListResult.Items = postListResult.Items.Concat(tempPostResult.Items).ToList();
                        }

                        if (postListResult.Items != null && postListResult.Items.Count > 0)
                        {
                            postListResult.Items = postListResult.Items.Where(x => StripHTML(x.Content).ToLower().Contains(searchTerm.SearchTerm.ToLower()) || x.Title.ToLower().Contains(searchTerm.SearchTerm.ToLower())).ToList();

                            totalPosts = totalPosts.Union(postListResult.Items).ToList();
                        }

                        DAL.StagingDAL.LogMiner(32, "Blogger", "Processing", "Search for Keyword: " + searchTerm.SearchTerm + " brought " + postListResult.Items.Count + " posts");
                        
                    }
                }
            }
            catch (Exception ex)
            {
                DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"GetPosts\"");
                Util.Log.WriteException(typeof(DataCollection), "B1004 Fetching Blogger Data Loop ", ex);
            }

            if (totalPosts.Count == 0)
                return null;            
            #endregion

            #region GetPOSTSCOMMENTS
            if (getComments)
            {
                foreach (Google.Apis.Blogger.v3.Data.Post post in totalPosts)
                {
                    post.Replies.Items = GetPostComments(bloggerService, blog.Id, post.Id);
                }
            }
            #endregion

            List<BloggerPost> bloggerPosts = new List<BloggerPost>();

            foreach (var p in totalPosts)
            {
                try
                {
                    List<int> foundSearchTermIDs = new List<int>();
                    string item = p.Content.ToString() + "," + p.Title.ToString();

                    foundSearchTermIDs = FindSearchTerms(item, config.SearchTerms, p.Published.Value);
                    if (foundSearchTermIDs.Count > 0)
                    {
                        List<BloggerUser> Users = new List<BloggerUser>();
                        List<BloggerLink> Links = new List<BloggerLink>();
                        List<BloggerSearchTearm> SearchTerms = new List<BloggerSearchTearm>();
                        List<BloggerComment> Comments = new List<BloggerComment>();

                        BloggerPost bloggerPost = CreatePost(p, p.Blog.Id, config.DocumentSourceId, totalPosts.Count);

                        foreach (var id in foundSearchTermIDs)
                        {
                            // adi change lucene.net
                            List<Common.BO.KeyTerm> terms = DAL.ConfigDAL.GetKeyTerms(id);
                            string searchQuery = "Text:";
                            int termCount = 0;
                            foreach (var term in terms)
                            {
                                if (termCount > 0)
                                    searchQuery += " OR " + "(" + term.Name + ")";
                                else
                                    searchQuery += "(" + term.Name + ")";
                                termCount++;
                            }
                            try
                            {
                                if (termCount > 0)
                                    bloggerPost = search(searchQuery, bloggerPost);
                            }
                            catch (Exception ex)
                            {
                               Util.Log.WriteException(typeof(DataCollection), "B1101 LUCENE.NET ", ex);
                            }
                            // adi change lucene.net end

                            if (bloggerPost != null)
                            {
                                var sid = CreateSearchterm(id);
                                SearchTerms.Add(sid);
                            }
                        }

                        if (SearchTerms.Count > 0)
                        {
                            bloggerPost.SearchTerms = SearchTerms;
                            bloggerPosts.Add(bloggerPost);

                            var User = CreateUser(p);
                            Users.Add(User);

                            var link = CreateLink(p);
                            Links.Add(link);

                            if (getComments)
                            {
                                foreach (var c in p.Replies.Items)
                                {
                                    Comments.Add(CreateComment(c, p, p.Replies.Items.Count));
                                }
                            }

                            bloggerPost.Comments = Comments;
                            bloggerPost.Users = Users;
                            bloggerPost.Links = Links;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DAL.StagingDAL.LogMiner(32, "Blogger", "Error", "Blogger Miner Issue in \"CreatePost\"");
                    Util.Log.WriteException(typeof(DataCollection), "B1005 CreatePost Loop", ex);
                }
            }

            return bloggerPosts.ToList();
        }

        public static List<Google.Apis.Blogger.v3.Data.Comment> GetPostComments(Google.Apis.Blogger.v3.BloggerService bloggerService, string blogId, string postId)
        {
            List<Google.Apis.Blogger.v3.Data.Comment> commentsReturn = new List<Google.Apis.Blogger.v3.Data.Comment>();

            Google.Apis.Blogger.v3.Data.CommentList commentListResult = new Google.Apis.Blogger.v3.Data.CommentList();
            Google.Apis.Blogger.v3.CommentsResource.ListRequest commentsQuery = bloggerService.Comments.List(blogId, postId);
            commentsQuery.MaxResults = 500;
            Google.Apis.Blogger.v3.Data.CommentList comments = commentsQuery.Execute();
            commentListResult = comments;

            while (comments.NextPageToken != null)
            {
                commentsQuery = bloggerService.Comments.List(blogId, postId);
                commentsQuery.MaxResults = 500;
                commentsQuery.PageToken = comments.NextPageToken;

                comments = commentsQuery.Execute();

                if(comments.Items!=null)
                    commentListResult.Items = commentListResult.Items.Concat(comments.Items).ToList();
            }

            if (commentListResult.Items != null && commentListResult.Items.Count > 0)
                return commentListResult.Items.ToList();
            else
                return new List<Google.Apis.Blogger.v3.Data.Comment>();
        }


        //----------------------------------------OLD----------------------------------------//


        internal static BO.BloggerPost CreatePost(AtomEntry Post, string blogid, int documentSourceID)
        {
            if (Post.Id != null)
            {
                AtomId tempID = Post.Id;
                string blog_post_id = tempID.Uri.Content.Substring(Convert.ToString(tempID.Uri.Content).LastIndexOf("blog-") + 5);
                string postid = blog_post_id.Substring(Convert.ToString(blog_post_id).LastIndexOf("post-") + 5);
                BO.BloggerPost PostInfo = new Insight360.BO.BloggerPost()
                {

                    Staging_UpdateTime = DateTime.Now,
                    Staging_InsertTime = DateTime.Now,
                    PostID = postid,
                    BlogID = blogid,
                    Uri = Post.AlternateUri.ToString(),
                    PublishedDate = Post.Published.ToUniversalTime(),
                    UpdatedDate = Post.Updated,
                    Content = @StripHTML(Post.Content.Content),
                    Title = Post.Title.Text.ToString(),
                    Src = Post.Content.Src == null ? "" : (Post.Content.Src.Content == null ? "" : Post.Content.Src.Content),
                    Type = Post.Content.Type,
                    EditURI = Post.EditUri == null ? "" : (Post.EditUri.Content == null ? "" : Post.EditUri.Content),
                    Feed = Post.Feed.Feed == null ? "" : Post.Feed.Feed,
                    Post = Post.Feed.Post == null ? "" : Post.Feed.Post,
                    Self = Post.Feed.Self == null ? "" : Post.Feed.Self,
                    StartIndex = Post.Feed == null ? 0 : Post.Feed.StartIndex,
                    TotalResults = Post.Feed == null ? 0 : Post.Feed.TotalResults,
                    IdURI = Post.Id.Uri == null ? "" : (Post.Id.Uri.Content == null ? "" : Post.Id.Uri.Content),
                    LinkCount = Post.Links == null ? 0 : Post.Links.Count,
                    LinkReadOnly = Post.Links == null ? true : Post.Links.IsReadOnly,
                    MediaURI = Post.MediaUri == null ? "" : (Post.MediaUri.Content == null ? "" : Post.MediaUri.Content),
                    ReadOnly = Post.ReadOnly,
                    Rights = Post.Rights.Text == null ? "" : Post.Rights.Text,
                    SelfURI = Post.SelfUri == null ? "" : (Post.SelfUri.Content == null ? "" : Post.SelfUri.Content),
                    Summary = Post.Summary.Text == null ? "" : Post.Summary.Text,
                    UserCount = Post.Authors == null ? 0 : Post.Authors.Count,
                    DocumentSource_ID = documentSourceID,
                };

                return PostInfo;
            }
            else
            {
                BloggerPost PostInfo = new Insight360.BO.BloggerPost();
                return PostInfo;
            }


        }
        internal static BO.BloggerUser CreateUser(AtomEntry Post, int counter)
        {
            if (Post.Authors.Count > 0)
            {
                ;
                BO.BloggerUser UserInfo = new Insight360.BO.BloggerUser()
                {

                    Name = Post.Authors[counter].Name == null ? "" : Post.Authors[counter].Name,
                    Email = Post.Authors[counter].Email == null ? "" : Post.Authors[counter].Email,
                    Language = Post.Authors[counter].Language == null ? "" : Post.Authors[counter].Language,
                    URI = Post.Authors[counter].Uri == null ? "" : (Post.Authors[counter].Uri.Content == null ? "" : Post.Authors[counter].Uri.Content)
                };

                return UserInfo;
            }
            else
            {
                BloggerUser UserInfo = new Insight360.BO.BloggerUser();
                return UserInfo;
            }


        }
        internal static BO.BloggerLink CreateLink(AtomEntry Post, int counter)
        {
            if (Post.Links.Count > 0)
            {

                BO.BloggerLink LinkInfo = new Insight360.BO.BloggerLink()
                {
                    AbsoluteURI = Post.Links[counter].AbsoluteUri == null ? "" : Post.Links[counter].AbsoluteUri,
                    Rel = Post.Links[counter].Rel == null ? "" : Post.Links[counter].Rel,
                    Title = Post.Links[counter].Title == null ? "" : Post.Links[counter].Title,
                    Type = Post.Links[counter].Type == null ? "" : Post.Links[counter].Type
                };

                return LinkInfo;
            }
            else
            {
                BloggerLink LinkInfo = new Insight360.BO.BloggerLink();
                return LinkInfo;
            }


        }
        internal static BO.BloggerComment CreateComment(AtomEntry Post)
        {
            if (Post.Links.Count > 0)
            {
                AtomId tempID = Post.Id;
                string comment_feed_url = Post.Feed.Feed;

                string blog_post_id = tempID.Uri.Content.Substring(Convert.ToString(tempID.Uri.Content).LastIndexOf("blog-") + 5);
                string commentid = blog_post_id.Substring(Convert.ToString(blog_post_id).LastIndexOf("post-") + 5);
                string self_uri = Post.SelfUri.Content == null ? "" : Post.SelfUri.Content;
                self_uri = self_uri.Substring(self_uri.IndexOf("/feeds/") + 7);
                self_uri = self_uri.Substring(self_uri.IndexOf("/") + 1);
                self_uri = self_uri.Substring(0, self_uri.IndexOf("/comments/"));
                string postid = self_uri;

                BO.BloggerComment CommentInfo = new Insight360.BO.BloggerComment()
                {
                    CommentID = commentid,
                    PostID = postid,
                    PublishedDate = Post.Published,
                    UpdatedDate = Post.Updated,
                    Uri = Post.AlternateUri.Content == null ? "" : Post.AlternateUri.Content,
                    Title = Post.Title.Text,
                    Src = Post.Content.Src == null ? "" : (Post.Content.Src.Content == null ? "" : Post.Content.Src.Content),
                    Type = Post.Content.Type == null ? "" : Post.Content.Type,
                    EditURI = Post.EditUri == null ? "" : (Post.EditUri.Content == null ? "" : Post.EditUri.Content),
                    Feed = Post.Feed.Feed == null ? "" : Post.Feed.Feed,
                    Post = Post.Feed.Post == null ? "" : Post.Feed.Post,
                    Self = Post.Feed.Self == null ? "" : Post.Feed.Self,
                    StartIndex = Post.Feed == null ? 0 : Post.Feed.StartIndex,
                    TotalResults = Post.Feed == null ? 0 : Post.Feed.TotalResults,
                    IdURI = Post.Id.Uri == null ? "" : (Post.Id.Uri.Content == null ? "" : Post.Id.Uri.Content),
                    LinkCount = Post.Links == null ? 0 : Post.Links.Count,
                    LinkReadOnly = Post.Links == null ? true : Post.Links.IsReadOnly,
                    MediaURI = Post.MediaUri == null ? "" : (Post.MediaUri.Content == null ? "" : Post.MediaUri.Content),
                    ReadOnly = Post.ReadOnly,
                    Rights = Post.Rights.Text == null ? "" : Post.Rights.Text,
                    SelfURI = Post.SelfUri == null ? "" : (Post.SelfUri.Content == null ? "" : Post.SelfUri.Content),
                    Summary = Post.Summary.Text == null ? "" : Post.Summary.Text,
                    UserCount = Post.Authors == null ? 0 : Post.Authors.Count
                };

                if (Post.Content.Content != null)
                    CommentInfo.Content = @StripHTML(Post.Content.Content);
                else
                    CommentInfo.Content = "";
                return CommentInfo;
            }
            else
            {
                BO.BloggerComment CommentInfo = new Insight360.BO.BloggerComment();
                return CommentInfo;
            }


        }
        internal static BO.BloggerSearchTearm CreateSearchterm(int id)
        {
            BO.BloggerSearchTearm sid = new Insight360.BO.BloggerSearchTearm()
            {
                SearchTermsID = id
            };
            return sid;
        }

        public static List<BloggerPost> GetPosts(BO.DocumentSourceInfo config, bool getComments, string username, string password, string appName, DateTime maxUpdateDate)
        {
            List<BloggerPost> posts = new List<BloggerPost>();
            try
            {
                List<AtomEntry> entries = new List<AtomEntry>();
                Service service = new Service("blogger", appName);
                service.RequestFactory.UseSSL = false;
                service.Credentials = new GDataCredentials(username, password);
                GDataGAuthRequestFactory factory = (GDataGAuthRequestFactory)service.RequestFactory;
                factory.AccountType = "GOOGLE";



                string BloggerURI = config.Url.TrimEnd('/') + "/feeds/posts/default/";
                //string BloggerURI = "http://www.boycottnestle.blogspot.com/feeds/posts/default";



                FeedQuery query = new FeedQuery();
                query.Uri = new Uri(BloggerURI);
                //query.OrderBy = BloggerQuery.OrderByUpdated;
                query.ModifiedSince = maxUpdateDate;

                //query.ModifiedSince = maxUpdateDate;
                query.StartDate = maxUpdateDate;

                //var a = service.Query(new Uri(BloggerURI));
                AtomFeed bloggerFeed = service.Query(query);

                if (bloggerFeed.Entries.Count == 0)
                {
                    BloggerURI = BloggerURI.Replace("/default/", "/summary/");
                    query.Uri = new Uri(BloggerURI);
                    //query.OrderBy = BloggerQuery.OrderByUpdated;
                    //query.ModifiedSince = maxUpdateDate;
                    //query.StartDate = maxUpdateDate;
                    Util.Log.WriteOperationStarted(typeof(DataCollection), "GetPosts", "Starting Searching, Url: " + BloggerURI.ToString() + "key:" + query.ToString());
                    bloggerFeed = service.Query(query);
                }

                string blogid = bloggerFeed.Id.Uri.Content.Substring(Convert.ToString(bloggerFeed.Id.Uri.Content).LastIndexOf("blog-") + 5);
                string blogurl = bloggerFeed.Id.AbsoluteUri;





                while (bloggerFeed != null && bloggerFeed.Entries.Count > 0)
                {

                    Util.Log.WriteOperationStarted(typeof(DataCollection), "GetPosts", "Post Found: " + bloggerFeed.Entries.Count.ToString() + " key:" + query.ToString());
                    int i = 1;
                    DateTime updatedDate = bloggerFeed.Entries[0].Updated;
                    foreach (AtomEntry entry in bloggerFeed.Entries)
                    {
                        if (entry.Updated < updatedDate)
                            updatedDate = entry.Updated;
                        entries.Add(entry);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(i++);
                        sb.Append("/");
                        sb.Append(bloggerFeed.Entries);
                        sb.Append(" completed");
                        Util.Log.WriteOperationCompleted(typeof(DataCollection), "GetPosts", sb.ToString());

                    }
                    query.StartIndex = entries.Count + 1;
                    bloggerFeed = service.Query(query);
                    // do the chunking...
                    //if (bloggerFeed.NextChunk != null)
                    //{

                    //    //string query_uri = bloggerFeed.NextChunk;
                    //    //string sub_query_uri = query_uri.Substring(query_uri.IndexOf("updated-min="));
                    //    //sub_query_uri = sub_query_uri.Substring(0, sub_query_uri.IndexOf('&'));
                    //    //string to_replace_query = sub_query_uri.Substring(0, 31);
                    //    //query_uri = query_uri.Replace(sub_query_uri, to_replace_query);

                    //    //query.Uri = new Uri(query_uri);
                    //    BloggerURI = config.Url.TrimEnd('/') + "/feeds/posts/default?&updated-max=" + updatedDate + "&orderby=updated";

                    //    query = new FeedQuery();
                    //    query.Uri = new Uri(BloggerURI);
                    //    //query.OrderBy = BloggerQuery.OrderByUpdated;
                    //    query.ModifiedSince = maxUpdateDate;

                    //    //query.ModifiedSince = maxUpdateDate;
                    //    query.StartDate = maxUpdateDate;

                    //    bloggerFeed = service.Query(query);
                    //}
                    //else
                    //{
                    //    bloggerFeed = null;
                    //    Util.Log.WriteOperationStarted(typeof(DataCollection), "Search Result", "No Post Found: " + bloggerFeed.Entries.Count().ToString() + " key:" + query.Query);
                    //    break;
                    //}
                }


                foreach (AtomEntry ae in entries)
                {
                    //if (alist.Updated > temp_dt)
                    //    maxUpdateDate = alist.Updated;
                    List<int> foundSearchTermIDs = new List<int>();
                    /*foreach (var org in config.Organizations)
					{*/
                    string item = ae.Content.Content.ToString() + "," + ae.Title.Text.ToString();

                    foundSearchTermIDs = FindSearchTerms(item, config.SearchTerms, ae.Published);
                    if (foundSearchTermIDs.Count > 0)
                    {
                        int UserCount = ae.Authors.Count == null ? 0 : ae.Authors.Count;
                        int LinkCount = ae.Links.Count == null ? 0 : ae.Links.Count;
                        List<BloggerUser> Users = new List<BloggerUser>();
                        List<BloggerLink> Links = new List<BloggerLink>();
                        List<BloggerSearchTearm> SearchTerms = new List<BloggerSearchTearm>();

                        foreach (var id in foundSearchTermIDs)
                        {
                            var sid = CreateSearchterm(id);
                            SearchTerms.Add(sid);
                        }

                        var post = CreatePost(ae, blogid, config.DocumentSourceId);
                        post.SearchTerms = SearchTerms;
                        posts.Add(post);
                        for (int counter = 0; counter < UserCount; counter++)
                        {
                            var User = CreateUser(ae, counter);
                            Users.Add(User);
                        }

                        post.Users = Users;
                        for (int counter = 0; counter < LinkCount; counter++)
                        {
                            var link = CreateLink(ae, counter);
                            Links.Add(link);
                        }
                        post.Links = Links;

                        if (getComments)
                        {
                            post.Comments = GetBlogComments(BloggerURI, username, password, appName, maxUpdateDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DataCollection), "B1006 GetPosts", ex);

            }

            return posts;
        }

        public static List<BloggerComment> GetBlogComments(string bloggerURI, string userName, string password, string appName, DateTime maxUpdateDate)
        {
            List<BloggerComment> comments = new List<BloggerComment>();
            List<AtomEntry> FeedChooser = new List<AtomEntry>();
            Service service = new Service("blogger", appName);
            service.RequestFactory.UseSSL = false;
            service.Credentials = new GDataCredentials(userName, password);
            GDataGAuthRequestFactory factory = (GDataGAuthRequestFactory)service.RequestFactory;
            factory.AccountType = "GOOGLE";




            if (bloggerURI != null)
            {

                FeedQuery query = new FeedQuery();
                string bloggerURIforComments = bloggerURI.ToString().Replace("/posts/", "/comments/");
                query.Uri = new Uri(bloggerURIforComments);

                //query.OrderBy = BloggerQuery.OrderByUpdated;
                query.ModifiedSince = maxUpdateDate;

                //query.ModifiedSince = maxUpdateDate;
                query.StartDate = maxUpdateDate;

                AtomFeed bloggerFeed = service.Query(query);

                while (bloggerFeed != null && bloggerFeed.Entries.Count > 0)
                {
                    DateTime updatedDate = bloggerFeed.Entries[0].Updated;
                    foreach (AtomEntry entry in bloggerFeed.Entries)
                    {
                        if (entry.Updated < updatedDate)
                            updatedDate = entry.Updated;
                        FeedChooser.Add(entry);
                    }

                    // do the chunking...
                    if (bloggerFeed.NextChunk != null)
                    {
                        bloggerURI = bloggerURI.TrimEnd('/') + "/feeds/comments/default?&updated-max=" + updatedDate + "&orderby=updated";

                        query = new FeedQuery();
                        query.Uri = new Uri(bloggerURI);
                        //query.OrderBy = BloggerQuery.OrderByUpdated;
                        query.ModifiedSince = maxUpdateDate;

                        //query.ModifiedSince = maxUpdateDate;
                        query.StartDate = maxUpdateDate;

                        bloggerFeed = service.Query(query);
                    }
                    else
                    {
                        bloggerFeed = null;
                    }
                }
                foreach (AtomEntry alist in FeedChooser)
                {

                    var comment = CreateComment(alist);
                    comments.Add(comment);

                    int UserCount = alist.Authors.Count == null ? 0 : alist.Authors.Count;
                    int LinkCount = alist.Links.Count == null ? 0 : alist.Links.Count;
                    List<BloggerUser> Users = new List<BloggerUser>();
                    List<BloggerLink> Links = new List<BloggerLink>();
                    for (int counter = 0; counter < UserCount; counter++)
                    {
                        var User = CreateUser(alist, counter);
                        Users.Add(User);
                    }

                    comment.CommentUsers = Users;
                    for (int counter = 0; counter < LinkCount; counter++)
                    {
                        var link = CreateLink(alist, counter);
                        Links.Add(link);
                    }
                    comment.CommentLinks = Links;


                }

            }
            return comments;

        }


        #region Search Methods

        private static Directory createIndex(BO.BloggerPost post)
        {
            var directory = new RAMDirectory();

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(1000)))
            { // the writer and analyzer will popuplate the directory with documents

                var document = new Lucene.Net.Documents.Document();

                document.Add(new Lucene.Net.Documents.Field("Text", post.Content + " " + post.Title, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                document.Add(new Lucene.Net.Documents.Field("Id", post.PostID.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

                writer.AddDocument(document);

                writer.Optimize();
                writer.Flush(true, true, true);
            }

            return directory;
        }

        private static BO.BloggerPost search(string textSearch, BO.BloggerPost post)
        {
            BO.BloggerPost filteredPosts = null;

            Directory Index = createIndex(post);

            using (var reader = IndexReader.Open(Index, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Text", analyzer);

                    //queryParser.AllowLeadingWildcard = true;

                    var query = queryParser.Parse(textSearch);

                    var collector = TopScoreDocCollector.Create(1000, true);

                    searcher.Search(query, collector);

                    var match = collector.TopDocs().ScoreDocs;

                    if(match!=null)
                    {
                        var id = match[0].Doc;
                        var doc = searcher.Doc(id);

                        filteredPosts = (post.PostID.ToString() == doc.GetField("Id").StringValue ? post : null);
                    }
                }
            }

            return filteredPosts;
        }

        #endregion

    }

    public class ListEntry
    {
        private AtomEntry entry;
        private string postid;

        public AtomEntry Entry
        {
            get
            {
                return this.entry;
            }
            set
            {
                this.entry = value;
            }
        }

        public string Postid
        {
            get
            {
                return this.postid;
            }
            set
            {
                this.postid = value;
            }
        }

        public ListEntry(AtomEntry entry)
        {
            this.entry = entry;
        }

        public override string ToString()
        {
            return this.entry != null ? this.entry.Title.Text : "No Entry";
        }
    }

}


