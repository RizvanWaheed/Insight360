using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.BO
{
    public class StagingInfo
    {
        public long StagingId { get; set; }
        public DateTime InsertTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int ProcessingState { get; set; }
        //public string OrgName { get; set; }
        public bool Followup { get; set; }
        public int DocumentSourceId { get; set; }
        public int SearchTermId { get; set; }
    }
}
