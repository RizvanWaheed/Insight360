using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class TwitterUser
    {
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
        /// id
        /// </summary>
        public string   Id { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// screen_name
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// profile_image_url
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// protected
        /// </summary>
        public bool? Protected { get; set; }

        /// <summary>
        /// followers_count
        /// </summary>
        public int FollowersCount { get; set; }

        /// <summary>
        /// friends_Count
        /// </summary>
        public int FriendsCount { get; set; }

        /// <summary>
        /// created_at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// favourites_count
        /// </summary>
        public int FavouritesCount { get; set; }

        /// <summary>
        /// utc_offset
        /// </summary>
        public string UtcOffset { get; set; }

        /// <summary>
        /// time_zone
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// geo_enabled
        /// </summary>
        public bool? GeoEnabled { get; set; }

        /// <summary>
        /// statuses_count
        /// </summary>
        public int StatusesCount { get; set; }

        /// <summary>
        /// lang
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// contributors_enabled
        /// </summary>
        public bool? ContributorsEnabled { get; set; }

        /// <summary>
        /// listed_count
        /// </summary>
        public int ListedCount { get; set; }

        /// <summary>
        /// following
        /// </summary>
        public bool? Following { get; set; }

        /// <summary>
        /// notifications
        /// </summary>
        public bool? Notifications { get; set; }

        /// <summary>
        /// verified
        /// </summary>
        public bool? IsVerified { get; set; }

        ///// <summary>
        ///// translator
        ///// </summary>
        //public bool? IsTranslator { get; set; }

        ///// <summary>
        ///// follow_request_sent
        ///// </summary>
        ///// 
        //public bool? FollowRequestSent { get; set; }

        /// <summary>
        /// profile_background_color
        /// </summary>
        public string ProfileBgColor { get; set; }

        /// <summary>
        /// profile_background_image_url
        /// </summary>
        public string ProfileBgImageUrl { get; set; }

        /// <summary>
        /// profile_link_color
        /// </summary>
        public string ProfileLinkColor { get; set; }

        /// <summary>
        /// profile_background_tile
        /// </summary>
        public bool? ProfileBgTiled { get; set; }

        ///// <summary>
        ///// profile_use_background_image
        ///// </summary>
        //public bool? ProfileUseBgImg { get; set; }

        /// <summary>
        /// profile_text_color
        /// </summary>
        public string ProfileTxtColor { get; set; }
        public string Gender { get; set; }

    }
}
