using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class TwitterStatus
    {
        public TwitterStatus()
        {
            Staging = new StagingInfo();
            UserDetails = new TwitterUser();
        }

        public StagingInfo Staging { get; set; }
        //* from_user_id_str: "144110027"
        //* profile_image_url: http://a2.twimg.com/profile_images/1248223502/264840618_normal.jpg
        //* created_at: "Thu, 24 Feb 2011 13:39:58 +0000"
        //* from_user: "GinaNoPayne"
        //* id_str: "40767896330174464"
        //*
        //  -
        //  metadata: {
        //      o result_type: "recent"
        //  }
        //* to_user_id: null
        //* text: "Good morning twitter!!! :D"
        //* id: 40767896330174460
        //* from_user_id: 144110027
        //* geo: null
        //* iso_language_code: "en"
        //* to_user_id_str: null
        //* source: "&lt;a href=&quot;http://ubersocial.com&quot; rel=&quot;nofollow&quot;&gt;ÜberSocial&lt;/a&gt;"

        /// <summary>
        /// from_user_id: it is not valid user id because of a twitter bug
        /// </summary>
        public string FromUserId { get; set; }
        ///// <summary>
        ///// profile_image_url
        ///// </summary>
        //public string ProgfileImageUrl { get; set; }
        /// <summary>
        /// created_at
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// from_user
        /// </summary>
        public string FromUser { get; set; }
        /// <summary>
        /// to_user
        /// </summary>
        public string ToUser { get; set; }
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// to_user_id: it is not valid user id because of a twitter bug
        /// </summary>
        public string ToUserId { get; set; }
        /// <summary>
        /// text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// geo
        /// </summary>
        public string Geo { get; set; }
        /// <summary>
        /// iso_language_code
        /// </summary>
        public string IsoLanguageCode { get; set; }

        public string Media_Url { get; set; }

        public string Media_Url2 { get; set; }

        public string Media_Url3 { get; set; }

        public int Media_Cnt { get; set; }
        /// <summary>
        /// source
        /// </summary>
        public string Source { get; set; }
        //        <status>
        //<created_at>Thu Feb 24 22:00:52 +0000 2011</created_at>
        //<id>40893950000627712</id>
        //−
        //<text>
        //@sonaeBEmixed oh yea there's random places for $20 oil changes. ii see those everywhere lol && a 2007 corolla. yuh?
        //</text>
        //−
        //<source>
        //<a href="http://www.echofon.com/" rel="nofollow">Echofon</a>
        //</source>
        //<truncated>false</truncated>
        //<favorited>false</favorited>
        //<in_reply_to_status_id>40893586509799424</in_reply_to_status_id>
        //<in_reply_to_user_id>94251948</in_reply_to_user_id>
        //<in_reply_to_screen_name>sonaeBEmixed</in_reply_to_screen_name>
        //<retweet_count>0</retweet_count>
        //<retweeted>false</retweeted>
        //−
        //<user>
        //<id>53812964</id>
        //<name>Brittan Leiann </name>
        //<screen_name>BbMiixedBabii</screen_name>
        //<location>LA born.IE raised.OC resident</location>
        //−
        //<description>
        //find another like meeh, truth be told ii dont know any :-*

        //#TeamVirgo
        //#TeamMiixedChiqqs
        //</description>
        //−
        //<profile_image_url>
        //http://a3.twimg.com/profile_images/1260901760/BbMiixedBabii_normal.jpg
        //</profile_image_url>
        //<url/>
        //<protected>false</protected>
        //<followers_count>75</followers_count>
        //<profile_background_color>642D8B</profile_background_color>
        //<profile_text_color>3D1957</profile_text_color>
        //<profile_link_color>FF0000</profile_link_color>
        //<profile_sidebar_fill_color>7AC3EE</profile_sidebar_fill_color>
        //<profile_sidebar_border_color>65B0DA</profile_sidebar_border_color>
        //<friends_count>66</friends_count>
        //<created_at>Sun Jul 05 02:04:10 +0000 2009</created_at>
        //<favourites_count>2</favourites_count>
        //<utc_offset>-28800</utc_offset>
        //<time_zone>Pacific Time (US & Canada)</time_zone>
        //−
        //<profile_background_image_url>
        //http://a0.twimg.com/profile_background_images/179313577/Snapshot_20100925.jpg
        //</profile_background_image_url>
        //<profile_background_tile>true</profile_background_tile>
        //<profile_use_background_image>true</profile_use_background_image>
        //<notifications/>
        //<geo_enabled>false</geo_enabled>
        //<verified>false</verified>
        //<following/>
        //<statuses_count>1895</statuses_count>
        //<lang>en</lang>
        //<contributors_enabled>false</contributors_enabled>
        //<follow_request_sent/>
        //<listed_count>2</listed_count>
        //<show_all_inline_media>false</show_all_inline_media>
        //<is_translator>false</is_translator>
        //</user>
        //<geo/>
        //<coordinates/>
        //<place/>
        //<contributors/>
        //</status>

        /// <summary>
        /// in_reply_to_status_id
        /// </summary>
        public string InReplyToStatusId { get; set; }

        /// <summary>
        /// in_reply_to_user_id
        /// </summary>
        public string InReplyToUserId { get; set; }

        /// <summary>
        /// in_reply_to_screen_name
        /// </summary>
        public string InReplyToScreenName { get; set; }

        /// <summary>
        /// retweet_count
        /// </summary>
        public int RetweetCount { get; set; }
        public int LikesCount { get; set; }
        /// <summary>
        /// favorited
        /// </summary>
        public bool Favorited { get; set; }

        /// <summary>
        /// retweeted
        /// </summary>
        public bool Retweeted { get; set; }

        /// <summary>
        /// truncated
        /// </summary>
        public bool Truncated { get; set; }
        public TwitterUser UserDetails { get; set; }
        public bool SkippedDueToRateLimiting { get; set; }
        public int? Sentiment { get; set; }
        public string LanguageCode { get; set; }
        public List<int> FeedLabels { get; set; }
    }
}
