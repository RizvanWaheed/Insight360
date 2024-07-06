using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp.Twitter.Fluent;
using System.Diagnostics;
using TweetSharp.Twitter.Model;
using TweetSharp.Twitter.Extensions;
using TweetSharp.Extensions;
using System.Threading;

namespace Insight360.Twitter.DataCollector
{
    class TwitterStreamingClient
    {
        static int instanceCount = 0;
        string id;
        public TwitterStreamingClient()
        {
            id = "Connection " + ++instanceCount + "-- ";            
        }

        ITwitterLeafNode twitterCallbackRequest;

        public IAsyncResult StreamingStatus { get; private set; }


        ManualResetEvent reconnectEvent;

        
        public bool StartStreaming(List<string> keywords, ManualResetEvent reconnectEvent)
        {
            this.reconnectEvent = reconnectEvent;
            try
            {
                twitterCallbackRequest = FluentTwitter
                    .CreateRequest()
                    .AuthenticateWith(
                       Properties.Settings.Default.TwitterConsumerKey,
                       Properties.Settings.Default.TwitterConsumerSecret,
                       Properties.Settings.Default.TwitterToken,
                       Properties.Settings.Default.TwitterTokenSecret
                       )
                       
                    //.Configuration.TimeoutAfter(TimeSpan.FromMinutes(2))
                    //.Configuration.UseAutomaticRetries(RetryOn.ConnectionClosed, 2)
                    
                    .Stream()
                    .FromFilter()
                    .For(Properties.Settings.Default.ReconnectInterval)                    
                    //.Take(1)
                    //.WithBacklog(150000)
                    .Tracking(keywords.ToArray())
                    .CallbackTo(new TwitterWebCallback(TwitterStreamingCallback));

                StreamingStatus = twitterCallbackRequest.BeginRequest();

                Util.Log.WriteOperationStarted(id + " started");
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(ex);
                return false;
            }
            return true;
        }

        public static string GetTwitterResultString(TwitterResult result)
        {
            StringBuilder sb = new StringBuilder();
            if (result != null)
            {                
                sb.Append("IsNetworkError:");
                sb.Append(result.IsNetworkError.ToString());
                sb.Append(" , ");
                sb.Append("IsTwitterError:");
                sb.Append(result.IsTwitterError.ToString());
                sb.Append(" , ");
                sb.Append("ConnectionClosed:");
                sb.Append(result.ConnectionClosed.ToString());
                sb.Append(" , ");
                sb.Append("ExceptionStatus:");
                sb.Append(result.ExceptionStatus.ToString());
                sb.Append(" , ");
                sb.Append("Exception:");
                sb.Append(result.Exception != null ? result.Exception.ToString() : "null");
                sb.Append(" , ");
                sb.Append("StreamedResponses:");
                sb.Append(result.StreamedResponses != null ? "present" : "null");
                sb.Append(" , ");
                sb.Append("ResponseHttpStatusCode:");
                sb.Append(result.ResponseHttpStatusCode.ToString());
                sb.Append(" , ");
                sb.Append("RequestDate:");
                sb.Append(result.RequestDate!=null?result.RequestDate.ToString():"null");
                sb.Append(" , ");
                sb.Append("ResponseDate:");
                sb.Append(result.ResponseDate != null ? result.ResponseDate.ToString() : "null");

            }

            return sb.ToString();
        }

        public void TwitterStreamingCallback(object sender, TwitterResult result, object userState)
        {
            Util.Log.WriteOperationStarted(id + GetTwitterResultString(result));

            if (result.IsTwitterError ||
                result.IsNetworkError ||
                result.ConnectionClosed ||
                result.ExceptionStatus != System.Net.WebExceptionStatus.Success)
                reconnectEvent.Set();

            try
            {
                if (!result.IsNetworkError &&
                    !result.IsTwitterError &&
                    result.ResponseHttpStatusCode <= 200
                    )
                {
                    var res = result.AsStatuses();
                    if (res != null)
                    {
                        int count = 0;
                        foreach (TwitterStatus ts in res)
                        {
                            DAL.StagingDAL.PersistTweet(
                                new BO.DocumentInfo
                                {
                                    ContentId = ts.Id.ToString(),
                                    ContentText = ts.Text,
                                    ContentHtml = ts.TextHtml,
                                    ContentTime = ts.CreatedDate.ToLocalTime(),
                                    AuthorName = ts.User.ScreenName,
                                    AuthorId = ts.User.Id.ToString()
                                });
                            count++;
                        }
                        Util.Log.WriteOperationCompleted(id + count + " tweets queued");
                    }

                }
                else
                    Util.Log.WriteException(result.Exception);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(ex);
            }
        }

        public void StopStreaming()
        {
            twitterCallbackRequest.Root.Cancel();
            twitterCallbackRequest = null;
        }
    }
}
