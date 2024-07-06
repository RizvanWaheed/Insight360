using System.Collections.Generic;

namespace Insight360.FeedlySharp.Models
{
  public class FeedlyStreamEntryIdsResponse
  {
    public string Continuation { get; set; }

    public List<string> Ids { get; set; }
  }
}
