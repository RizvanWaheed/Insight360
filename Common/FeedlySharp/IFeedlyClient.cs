using System;
using System.Threading;

namespace Insight360.FeedlySharp
{
  public interface IFeedlyClient
  {
    void Activate(string accessToken, string userId);
    System.Threading.Tasks.Task<bool> AddOrUpdateSubscription(string id, System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyCategory> categories = null, string optionalTitle = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> AddOrUpdateTopic(string topicId, Insight360.FeedlySharp.Models.Interest interest = Insight360.FeedlySharp.Models.Interest.Low, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task DeleteCategory(string id, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    void Dispose();
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlySearchResponse> FindEntries(string contentId, string searchQuery, DateTime? newerThan = null, string continuation = null, string fields = null, string embedded = null, string engagement = null, int? count = null, string locale = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlySearchFeed>> FindFeeds(string searchQuery, int? count = null, string locale = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    string GetAuthenticationUri(string scope = "subscriptions", string state = default(String));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyCategory>> GetCategories(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyEntry>> GetEntries(string[] ids, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyEntry> GetEntry(string id, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyFeed> GetFeed(string id, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyFeed>> GetFeeds(string[] ids, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyReadOperations> GetMarkersReadOperations(DateTime newerThan, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyCategoryUnreadCount>> GetMarkersUnreadCount(bool isAutoRefresh = false, DateTime? newerThan = null, string categoryId = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyEntry>> GetMixes(string contentId, int? count = null, bool unreadOnly = false, int? limitHours = null, DateTime? newerThan = null, bool backfill = true, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<string> GetOPML(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, string>> GetPreferences(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyUser> GetProfile(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyStreamEntriesResponse> GetStreamEntries(string id, ContentType type, int? count = null, FeedSorting sorting = FeedSorting.Newest, bool? unreadOnly = null, DateTime? newerThan = null, string continuation = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyStreamEntryIdsResponse> GetStreamEntryIds(string id, ContentType type, int? count = null, FeedSorting sorting = FeedSorting.Newest, bool? unreadOnly = null, DateTime? newerThan = null, string continuation = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlySubscription>> GetSubscriptions(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyTag>> GetTags(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.List<Insight360.FeedlySharp.Models.FeedlyTopic>> GetTopics(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> ImportOPML(string opml, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> KeepEntriesAsUnread(string[] entryIds, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> KeepEntryAsUnread(string entryId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkCategoriesAsRead(string[] categoryIds, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkCategoryAsRead(string categoryId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkEntriesAsRead(string[] entryIds, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkEntriesAsSaved(string[] entryIds, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkEntriesAsUnsaved(string[] entryIds, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkEntryAsRead(string entryId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkEntryAsSaved(string entryId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkEntryAsUnsaved(string entryId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkFeedAsRead(string feedId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> MarkFeedsAsRead(string[] feedIds, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    Insight360.FeedlySharp.Models.AuthenticationResponse ParseAuthenticationResponseUri(string uri);
    Insight360.FeedlySharp.Models.AuthenticationResponse ParseAuthenticationResponseUri(Uri uri);
    System.Threading.Tasks.Task<bool> RemoveSubscription(string id, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> RemoveTags(string[] entryIds, string[] tags, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> RemoveTags(string[] tags, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> RemoveTopic(string topicId, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task RenameCategory(string id, string label, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> RenameTag(string oldTag, string newTag, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.AccessTokenResponse> RequestAccessToken(string authenticationCode, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.AccessTokenResponse> RequestRefreshToken(string refreshToken, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task RevokeRefreshToken(string refreshToken, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, string>> UpdatePreferences(System.Collections.Generic.Dictionary<string, string> preferences, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<Insight360.FeedlySharp.Models.FeedlyUser> UpdateProfile(System.Collections.Generic.Dictionary<string, string> parameters, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> UpdateTags(string entryId, string[] tags, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    System.Threading.Tasks.Task<bool> UpdateTags(string[] entryIds, string[] tags, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
  }
}
