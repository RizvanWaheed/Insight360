using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Insight360.Util
{
    public class CommonFilters
    {

        public static string FilterText(string text)
        {
            text = StripUrl(text);
            text = HttpUtility.HtmlDecode(text);
            text = StripHTML(text);
            //text = StripNonWords(text);
            return text;
        }

        private static string StripUrl(string text)
        {
            const string HTML_TAG_PATTERN = @"((http|https)://)*([\w-]+\.)+[\w-]+(/[\w- ./?%&=#]*)?";
            return System.Text.RegularExpressions.Regex.Replace(text, HTML_TAG_PATTERN, string.Empty, RegexOptions.IgnoreCase);
        }

        private static string StripHTML(string text)
        {
            //const string HTML_TAG_PATTERN = @"(?></?\w+)(?>(?:[^>'""]+|'[^']*'|""[^""]*"")*)>";
            ////return System.Text.RegularExpressions.Regex.Replace(inputString, HTML_TAG_PATTERN, string.Empty);
            //text = System.Text.RegularExpressions.Regex.Replace(text, HTML_TAG_PATTERN, " ");
            //text = Regex.Replace(text, @"<[/]?(font|span|xml|del|ins|[ovwxp]:\w+)[^>]*?>", "", RegexOptions.IgnoreCase);
            //// then run another pass over the html (twice), removing unwanted attributes 
            //text = Regex.Replace(text, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);
            //text = Regex.Replace(text, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);

            return System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);
        }

        private static string StripNonWords(string text)
        {
            string pattern = @"[^A-Za-z0-9\s.@?;,!]";
            Regex rgx = new Regex(pattern);
            string outputStr = rgx.Replace(text, " ");
            return outputStr;
        }
    }
}
