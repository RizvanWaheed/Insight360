using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.InfluenceCalculator.Klout
{
    public class KloutUser
    {
        public string twitter_id
        { get; set; }
        public string twitter_screen_name
        { get; set; }
        public string kscore
        { get; set; }
        public string slope
        { get; set; }
        public string description
        { get; set; }
        public string kscore_description
        { get; set; }
        public string network_score
        { get; set; }
        public string amplification_score
        { get; set; }
        public string true_reach
        { get; set; }
        public string kclass
        { get; set; }
        public string kclass_id
        { get; set; }
        public string kclass_description
        { get; set; }
        public string delta_1day
        { get; set; }
        public string delta_5day
        { get; set; }
    }
}
