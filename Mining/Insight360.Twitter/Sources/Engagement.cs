using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Insight360.Sources.Twitter
{
    public class Engagement
    {
        public static bool PostStatus(BO.TwitterAuthentication authentication,
            string text)
        {
            DataCollection dc = new DataCollection();

            //var twitterResult = FluentTwitter.CreateRequest()
            //    //.AuthenticateAs("TRG_TS", "TRGTECH")
            //       .AuthenticateWith(
            //               authentication.ConsumerKey,
            //               authentication.ConsumerSecret,
            //               authentication.Token,
            //               authentication.TokenSecret
            //               )
            //       .Statuses().Update(text);

            return true;// !twitterResult. && !twitterResult.IsTwitterError;
        }

        public static bool PostReply(BO.TwitterAuthentication authentication,
            long statusId, string text)
        {

            ///// Nabeel

            //var twitterResult = FluentTwitter.CreateRequest()
            //       //.AuthenticateAs("TRG_TS", "TRGTECH")
            //       .AuthenticateWith(
            //               authentication.ConsumerKey,
            //               authentication.ConsumerSecret,
            //               authentication.Token,
            //               authentication.TokenSecret
            //               )
            //       .Statuses()
            //       .Update(text)
            //       .InReplyToStatus(statusId);//                   .Request();

            return true;// !twitterResult.IsNetworkError && !twitterResult.IsTwitterError;
        }

        public static bool Retweet(BO.TwitterAuthentication authentication,
            long statusId)
        {

            ///// Nabeel

            //var twitterResult = FluentTwitter.CreateRequest()
            //       //.AuthenticateAs("TRG_TS", "TRGTECH")
            //       .AuthenticateWith(
            //               authentication.ConsumerKey,
            //               authentication.ConsumerSecret,
            //               authentication.Token,
            //               authentication.TokenSecret
            //               )
            //       .Statuses()
            //       .Retweet(statusId);//                   .Request();

            return true;// !twitterResult.IsNetworkError && !twitterResult.IsTwitterError;
        }

    }
}
