using Newtonsoft.Json;
using System;

namespace Insight360.FeedlySharp.Models
{
  public class FeedlySearchFeed : FeedlyFeed
  {
    [JsonProperty("feedId")]
    public new string Id { get; set; }
  }
}
