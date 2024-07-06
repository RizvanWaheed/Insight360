using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.Common.BO
{
    public class GoogleSearch
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }

        public string DisplayLink { get; set; }

        public string FormattedUrl { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        public int DocumentSourceId { get; set; }
        public int SearchTermId { get; set; }
        public string LanguageCode { get; set; }
        public int? Sentiment { get; set; }
    }
}
