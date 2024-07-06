using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.Common.BO
{
    public class GoogleProximity
    {
        public GoogleProximity()
        {
            ExactTerms = new List<string>();
            ExcludeTerms = new List<string>();
            SiteSearch = new List<string>();
        }

        public int id { get; set; }
        public int SearchTermId { get; set; }
        public string CountryRestrict { get; set; }
        public bool IsExactTerms { get; set; }
        public List<string> ExactTerms { get; set; }

        public string ExactTermString { get { return string.Join(" ", ExactTerms); } }

        public bool IsExcludeTerms { get; set; }
        public List<string> ExcludeTerms { get; set; }

        public string ExcludeTermString { get { return string.Join(" ", ExcludeTerms); } }
        public string Geolocation { get; set; }
        public string GoogleHost { get; set; }
        public string orTerms { get; set; }
        public bool IsSiteSearch { get; set; }
        public string SiteUrls { get { return string.Join(" ", SiteSearch); } }

        public List<string> SiteSearch { get; set; }
    }
}
