using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.BO
{
    public class FeedlyItem
    {

        public FeedlyItem()
        {
            Feed = new FeedlyFeed();
            Links = new List<FeedlyItemLink>();
        }

        public long Staging_Id { get; set; }
        public string Staging_OrgName { get; set; }
        public int Staging_DocumentSource_Id { get; set; }
        public int Staging_SearchTerm_Id { get; set; }
        public DateTime Staging_InsertTime { get; set; }
        public DateTime Staging_UpdateTime { get; set; }
        public int Staging_ProcessingState { get; set; }
        public bool Staging_Refresh { get; set; }

        public string Id { get; set; }
        public string BaseUri { get; set; }
        public string FingerPrint { get; set; }
        public string OriginId { get; set; }
        public string Title { get; set; }
        public string Summary_Text { get; set; }
        public string Content_Text { get; set; }
        public string Keywords { get; set; }
        public string Author { get; set; }
        public bool? IsUnread { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? CrawledDate { get; set; }

        public int? Sentiment { get; set; }
        public string LanguageCode { get; set; }

        public FeedlyFeed Feed { get; set; }
        public List<FeedlyItemLink> Links { get; set; }
    }


    public class FeedlyFeed
    {
        public long Staging_Id { get; set; }
        public long Staging_Feedly_Item_Id { get; set; }
        public DateTime Staging_InsertTime { get; set; }
        public DateTime Staging_UpdateTime { get; set; }

        public string FeedId { get; set; }
        public string Title { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public long Subscirbers { get; set; }
        public string FacebookUserName { get; set; }
        public long? FacebookLikes { get; set; }
        public string TwitterScreenName { get; set; }
        public long? TwitterFollowers { get; set; }

        public decimal? Coverage { get; set; }
        public decimal? CoverageScore { get; set; }
        public decimal? EstimatedEngagement { get; set; }
        public decimal? Velocity { get; set; }
        public decimal? Score { get; set; }
        public string DeliciousTags { get; set; }
        public string Language { get; set; }
        public string Hint { get; set; }
        public string Scheme { get; set; }
        public string VisualUrl { get; set; }
        public string CoverUrl { get; set; }
        public string IconUrl { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class FeedlyItemLink
    {
        public long Staging_Id { get; set; }
        public long Staging_Feedly_Item_Id { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string MediaType { get; set; }
        public string RelationshipType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
