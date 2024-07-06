using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.Common.BO
{
    public class PostDaywiseEngagementBO
    {
        public int DocumentId { get; set; }

        public int Media_DocumentId { get; set; }

        public int DocumentSourceId { get; set; }

        public int SearchTermId { get; set; }

        public DateTime Date { get; set; }

        public int Hour { get; set; }

        public int Likes { get; set; }

        public int Comments { get; set; }

        public int Shares { get; set; }

        public int Views { get; set; }

        public int Retweets { get; set; }
    }
}
