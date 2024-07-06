using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.Common.BO
{
    #region Posts data
    public class Pagination
    {
        public string next_max_tag_id { get; set; }

        public string deprecation_warning { get; set; }

        public string next_max_id { get; set; }

        public string next_min_id { get; set; }

        public string min_tag_id { get; set; }

        public string next_url { get; set; }
    }

    public class Meta
    {
        public int code { get; set; }
    }

    public class Location
    {
        public double latitude { get; set; }
        public string name { get; set; }
        public double longitude { get; set; }
        public double id { get; set; }
    }

    public class Comments
    {
        public int count { get; set; }
    }

    public class Likes
    {
        public int count { get; set; }
    }

    public class LowResolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class StandardResolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Images
    {
        public LowResolution low_resolution { get; set; }
        public Thumbnail thumbnail { get; set; }
        public StandardResolution standard_resolution { get; set; }
    }

    public class Position
    {
        public double y { get; set; }
        public double x { get; set; }
    }

    public class User
    {
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string id { get; set; }
        public string full_name { get; set; }
    }

    public class UsersInPhoto
    {
        public Position position { get; set; }
        public User user { get; set; }
    }

    public class From
    {
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string id { get; set; }
        public string full_name { get; set; }
        public string gender { get; set; }
    }

    public class Caption
    {
        public string created_time { get; set; }
        public string text { get; set; }
        public From from { get; set; }
        public string id { get; set; }

        public string language { get; set; }
        public int sentiment { get; set; }
    }

    public class User2
    {
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string id { get; set; }
        public string full_name { get; set; }
        public string gender { get; set; }
    }

    public class InstagramPost
    {
        public object attribution { get; set; }
        public List<string> tags { get; set; }
        public string alltags { get; set; }
        public string type { get; set; }
        public Location location { get; set; }
        public Comments comments { get; set; }
        public string filter { get; set; }
        public string created_time { get; set; }

        public DateTime created_time_obj { get; set; }
        public string link { get; set; }
        public Likes likes { get; set; }
        public Images images { get; set; }
        public List<UsersInPhoto> users_in_photo { get; set; }
        public Caption caption { get; set; }
        public bool user_has_liked { get; set; }
        public string id { get; set; }
        public User2 user { get; set; }

        public CommentRootObject commentsData { get; set; }
    }

    public class RootObject
    {
        public Pagination pagination { get; set; }
        public Meta meta { get; set; }
        public List<InstagramPost> data { get; set; }


    }

    #endregion

    #region Comments

    public class CommentMeta
    {
        public int code { get; set; }
    }

    public class CommentFrom
    {
        public string username { get; set; }
        public string profile_picture { get; set; }
        public string id { get; set; }
        public string full_name { get; set; }
    }

    public class CommentData
    {
        public string created_time { get; set; }
        public DateTime created_time_obj { get; set; }
        public string text { get; set; }
        public From from { get; set; }
        public string id { get; set; }
        public string language { get; set; }
        public int sentiment { get; set; }
    }

    public class CommentRootObject
    {
        public Meta meta { get; set; }
        public List<CommentData> data { get; set; }
    }
    #endregion

    #region User data
    public class UserMeta
    {
        public int code { get; set; }
    }

    public class UserCounts
    {
        public int media { get; set; }
        public int followed_by { get; set; }
        public int follows { get; set; }
    }

    public class UserData
    {
        public string username { get; set; }
        public string bio { get; set; }
        public string website { get; set; }
        public string profile_picture { get; set; }
        public string full_name { get; set; }
        public UserCounts counts { get; set; }
        public string id { get; set; }
    }

    public class UserRootObject
    {
        public Meta meta { get; set; }
        public UserData data { get; set; }
    }

    #endregion
}
