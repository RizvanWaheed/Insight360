using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{


    public class YouTubeFeed
    {

        public YouTubeFeed()
        {
            Comments = new List<YouTubeCommentInfo>();
            User = new YouTubeUserInfo();
            Staging = new StagingInfo();
            Staging.DocumentSourceId = 3;
            Location = new Location();
        }

        public StagingInfo Staging { get; set; }
        public string VideoTitle { get; set; }
        public string VideoID { get; set; }
        public string YoutubeURL { get; set; }
        public long ViewCount { get; set; }
        public string Author { get; set; }
        public long CommentCount { get; set; }
        public string keywords { get; set; }
        public string Description { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Published { get; set; }
        public string Language { get; set; }
        public DateTime Staging_InsertTime { get; set; }
        public DateTime Staging_UpdateTime { get; set; }
        public string CommentsUpdated { get; set; }
        public double Rating { get; set; }
        public long NumRaters { get; set; }
        public string FavoriteCount { get; set; }
        public string Likes { get; set; }
        public string DisLikes { get; set; }
        public string Thumbnails { get; set; }
        public string Duration { get; set; }
        public string DurationText { get; set; }
        public int? Sentiment { get; set; }
        public List<YouTubeCommentInfo> Comments { get; set; }
        public YouTubeUserInfo User { get; set; }
        public Location Location { get; set; }
    }

    public class YouTubeUserInfo
    {

        public string Author { get; set; }
        public int Age { get; set; }
        public string Company { get; set; }
        public string Etag { get; set; }
        public string FeedUri { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Hobbies { get; set; }
        public string Hometown { get; set; }
        public string Language { get; set; }
        public string Location { get; set; }
        public string Movies { get; set; }
        public string Music { get; set; }
        public string Occupation { get; set; }
        public string Relationship { get; set; }
        public string School { get; set; }
        public string Username { get; set; }
        public string SubscriberCount { get; set; }
        public string ViewCount { get; set; }
        public string ChannelId { get; set; }
    }

    public class YouTubeCommentInfo
    {
        public string Staging_VideoId { get; set; }
        public string YouTubeCommentID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string Language { get; set; }
        public DateTime Published { get; set; }
        public DateTime Updated { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string LanguageCode { get; set; }
        public int? Sentiment { get; set; }
    }
}
