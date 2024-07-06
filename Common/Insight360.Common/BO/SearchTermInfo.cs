using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class SearchTermInfo
    {
        public enum Types
        {
            SearchTerm = 0,
            TwitterEngagementUser,
            TwitterUser,
            FacebookPage,
            FacebookEngagementUser,
            FacebookUser,
            YoutubeUser = 7
        }
        public int Id { get; set; }
        public string SearchTerm { get; set; }
        //public bool IsEnabled { get; set; }
        public Types Type { get; set; }
        public string LastFetchId { get; set; }
        public string LastFetchId2 { get; set; }
        public DateTime LastFetchTime { get; set; }
        public DateTime UntillFetchTime { get; set; }
        public int IsTermEnabled { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }
    }
}
