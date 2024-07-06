using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class DocumentSourceTypeIndexingInfo
    {
        public int DocumentSourceTypeId { get; set; }
        public string Type { get; set; }
        //public string DisplayName { get; set; }
        //public string ImageUrl { get; set; }        
        //public string TableName { get; set; }
        public string ViewName { get; set; }
        public bool FindSearchTerms { get; set; }
        public int TopCount { get; set; }
        public string WhereCondition { get; set; }
        public string OrderBy { get; set; }
        //public bool IndexingEnabled { get; set; }
        public bool Enabled { get; set; }
    }
}
