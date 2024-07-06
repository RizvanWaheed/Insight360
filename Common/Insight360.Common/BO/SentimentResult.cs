using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.BO
{
    public class SentimentResult
    {
    }
    public class Result
    {
        public string confidence { get; set; }
        public string sentiment { get; set; }
    }

    public class ResultJson
    {
        public Result result { get; set; }
    }

    public class LanguageResult
    {
        public string language { get; set; }
        public string language_id { get; set; }
    }

    public class LanguageJson
    {
        public LanguageResult result { get; set; }
    }
}
