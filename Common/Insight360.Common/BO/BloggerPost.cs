using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
     public class BloggerPost
    {

         public BloggerPost()
        {
            Comments = new List<BloggerComment>();
            Users = new List<BloggerUser>();
            Links = new List<BloggerLink>();
            Staging = new StagingInfo();
            Staging.DocumentSourceId = 3;
        }
            public StagingInfo Staging { get; set; }
            public DateTime Staging_UpdateTime { get; set; }
	        public DateTime Staging_InsertTime { get; set; }
	        public string PostID  { get; set; }
	        public string BlogID { get; set; }
	        public string Uri { get; set; }
	        public DateTime PublishedDate { get; set; }
	        public DateTime UpdatedDate { get; set; }
	        public string Content { get; set; }
	        public string Title { get; set; }
	        public string Src { get; set; }
	        public string Type { get; set; }
	        public string EditURI { get; set; }
	        public string Feed { get; set; }
	        public string Post { get; set; }
	        public string Self { get; set; }
	        public int StartIndex { get; set; }
	        public int TotalResults { get; set; }
	        public string IdURI { get; set; }
	        public int LinkCount { get; set; }
	        public bool LinkReadOnly { get; set; }
	        public string MediaURI { get; set; }
	        public bool ReadOnly { get; set; }
	        public string Rights { get; set; }
	        public string SelfURI { get; set; }
	        public string Summary { get; set; }
	        public int UserCount { get; set; }
	        public int Staging_ProcessingState { get; set; }
	        public int DocumentSource_ID { get; set; }
            public int? Sentiment { get; set; }
            public string LanguageCode { get; set; }
            public List<BloggerComment> Comments { get; set; }
            public List<BloggerUser> Users { get; set; }
            public List<BloggerLink> Links { get; set; }
            public List<BloggerSearchTearm> SearchTerms { get; set; }
   
    }
     public class BloggerSearchTearm
     {
         public int SearchTermsID { get; set; }
     }
     public class BloggerUser
     {
        
	        public string Name { get; set; }
		    public string Email { get; set; } 
		    public string Language { get; set; }
		    public string URI { get; set; }

     }

     public class BloggerLink
     {
            public string AbsoluteURI{ get; set; }
	        public string Rel { get; set; }
	        public string Title { get; set; }
            public string Type  { get; set; }
     }

     public class BloggerComment
     {
            public string CommentID { get; set; }
	        public string PostID { get; set; }
	        public string Content { get; set; }
	        public DateTime PublishedDate { get; set; }
	        public DateTime UpdatedDate { get; set; }
	        public string Uri { get; set; }
	        public string Title { get; set; }
	        public string Src { get; set; }
	        public string Type { get; set; }
	        public string EditURI { get; set; }
            public string Feed { get; set; }
	        public string Post { get; set; }
	        public string Self { get; set; }
	        public int StartIndex { get; set; }
	        public int TotalResults { get; set; }
	        public string IdURI { get; set; }
	        public int LinkCount { get; set; }
	        public bool LinkReadOnly { get; set; }
	        public string MediaURI { get; set; }
	        public bool ReadOnly { get; set; }
	        public string Rights { get; set; }
            public string SelfURI { get; set; }
	        public string Summary { get; set; }
            public int UserCount { get; set; }
            public int? Sentiment { get; set; }
            public string LanguageCode { get; set; }
            public List<BloggerUser> CommentUsers { get; set; }
            public List<BloggerLink> CommentLinks { get; set; }

     }

}
