using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class FacebookUserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class FacebookPageLocation
    {
        public string City { get; set; }
        public string Country { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
    public class FacebookLikeInfo
    {
        public long StagingId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class FacebookCommentInfo
    {
        public FacebookCommentInfo()
        {
            From = new FacebookUserInfo();
            FeedLabels = new List<int>();
        }

        public long StagingId { get; set; }
        public string Id { get; set; }
        public FacebookUserInfo From { get; set; }
        public FacebookUserInfo To { get; set; }
        public string Message { get; set; }
        public DateTime CreatedTime { get; set; }
        public int? Sentiment { get; set; }
        public string LanguageCode { get; set; }

        public List<FacebookLikeInfo> Likes { get; set; }
        public List<int> FeedLabels { get; set; }
    }


    public class FacebookPost
    {
        public FacebookPost()
        {
            Likes = new List<FacebookLikeInfo>();
            Comments = new List<FacebookCommentInfo>();
            From = new FacebookUserInfo();
            Staging = new StagingInfo();
            Targeting = new TargetingInfo();
            Staging.DocumentSourceId = 2;
            user = new FacebookUser();
            toUser = new FacebookUser();
            PageLocation = new Location();
        }

        public Location PageLocation { get; set; }
        public StagingInfo Staging { get; set; }
        //public int LikesCount { get; set; }
        public string Id { get; set; }
        public FacebookUserInfo From { get; set; }
        public FacebookUserInfo To { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Attribution { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public class TargetingInfo
        {
            public string Country { get; set; }
            public string City { get; set; }
            public string Region { get; set; }
            public string Locale { get; set; }
        }
        public TargetingInfo Targeting { get; set; }
        public List<FacebookLikeInfo> Likes { get; set; }
        public List<FacebookCommentInfo> Comments { get; set; }
        public FacebookUser user { get; set; }
        public FacebookUser toUser { get; set; }


        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }

        public int? Sentiment { get; set; }
        public string LanguageCode { get; set; }
    }


    public class FacebookUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Locale { get; set; }
        public string Link { get; set; }
        public string ThirdPartyId { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string Picture { get; set; }
        public string TimeZone { get; set; }
        public string FollowersCount { get; set; }
        public string FollowingCount { get; set; }
        public string Location { get; set; }
        //public FacebookUser()
        //{
        //    Location = "";
        //}
    }

}
