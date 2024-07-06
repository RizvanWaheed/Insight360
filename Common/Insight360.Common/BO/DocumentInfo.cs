using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insight360.Util;

namespace Insight360.BO
{
    public class DocumentInfo
    {
        public int DocumentSourceId { get; set; }

        public string ContentUri { get; set; }

        public string ContentText { get; set; }

        public string SearchTerm_Id { get; set; }

        public DateTime? ContentTime { get; set; }

        public string ContentId { get; set; }

        public string ContentUniqueId { get; set; }

        public string AuthorName { get; set; }

        public string AuthorUri { get; set; }

        public string AuthorImageUri { get; set; }

        public string AuthorId { get; set; }

        public string AuthorUniqueId { get; set; }

        public int? AuthorAge { get; set; }

        public string AuthorGender { get; set; }

        public string AuthorLocation { get; set; }
        public string AuthorTimeZone {get;set; }
        public string Type { get; set; }
        public long? Followers { get; set; }
        public long? Following { get; set; }

        public string ToAuthorName { get; set; }

        public string ToAuthorUri { get; set; }

        public string ToAuthorImageUri { get; set; }

        public string ToAuthorId { get; set; }

        public string ToAuthorUniqueId { get; set; }

        public int? ToAuthorAge { get; set; }

        public string ToAuthorGender { get; set; }

        public string ToAuthorLocation { get; set; }
        public string ToAuthorTimeZone { get; set; }
        public long? ToFollowers { get; set; }
        public long? ToFollowing { get; set; } 

        public int? SentimentScore { get; set; }

        public string OrganizationName { get; set; }

        public int DocumentSourceTypeId { get; set; }

        public string DocumentSourceViewName { get; set; }

        public long StagingId { get; set; }

        public bool Followup { get; set; }

        public string Language { get; set; }

        public int CommentsCount { get; set; }

        public int LikesCount { get; set; }

        public int ViewsCount { get; set; }

        public int SharesCount { get; set; }

        public int UniqueCommentersCount { get; set; }

        public string ParentContentId { get; set; }

        public long? ParentStagingId { get; set; }

        public int? ParentStagingDocumentSourceId { get; set; }

    }
}
