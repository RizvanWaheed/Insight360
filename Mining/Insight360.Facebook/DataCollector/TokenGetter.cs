using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook;
using System.Configuration;
namespace Token
{
    public static class TokenGetter
    {

        public static string currentAccessToken { get; set; }
        public static bool FirstToken { get; set; }
        public static string GetToken()
        {

            var fb = new FacebookClient();
            var result = fb.Get("oauth/access_token", new
            {
                client_id = "749622071786940",
                client_secret = "5058c117fcb3ba8695ec79aa1384f687",
                grant_type = "client_credentials"
            }) as JsonObject;
            return result["access_token"].ToString();

            //FacebookClient fbClient = new FacebookClient();
            //Dictionary<string, object> fbParams = new Dictionary<string, object>();
            //fbParams["client_id"] = "749622071786940";
            //fbParams["grant_type"] = "fb_exchange_token";
            //fbParams["client_secret"] = "5058c117fcb3ba8695ec79aa1384f687";
            //if (string.IsNullOrEmpty(currentAccessToken))
            //{
            //    currentAccessToken = System.Configuration.ConfigurationSettings.AppSettings["Facebook_AccessToken_User"];
            //}
            //fbParams["fb_exchange_token"] = currentAccessToken;
            //JsonObject publishedResponse = fbClient.Get("/oauth/access_token", fbParams) as JsonObject;
            //currentAccessToken = publishedResponse["access_token"].ToString();

        }

    }
}
