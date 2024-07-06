using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook;
using System.Collections.Specialized;
using System.Web;

namespace Insight360.Sources.Facebook
{
    public class Engagement
    {
        public static bool CommentOnPost(string accessToken,string postId, string message)
        {
            FacebookClient app = new FacebookClient();
            app.AccessToken = accessToken;
            //https://graph.facebook.com/POST_ID/comments
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("message", message);
            parameters.Add("access_token", accessToken);
            app.Post(postId + "/comments",parameters);
            return true;
        }

        public static bool LikePost(string accessToken, string postId)
        {
            FacebookClient app = new FacebookClient();
            app.AccessToken = accessToken;
            //https://graph.facebook.com/POST_ID/likes
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("message", message);
            parameters.Add("access_token", accessToken);
            app.Post(postId + "/likes", parameters);
            return true;
        }

        
    }
}
