using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class OrganizationInfo
    {
        public OrganizationInfo()
        {
            SearchTerms = new List<SearchTermInfo>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastFetchId { get; set; }
        public string LastFetchId2 { get; set; }
        public DateTime LastFetchTime { get; set; }

        public List<SearchTermInfo> SearchTerms { get; private set; }
    }
}
