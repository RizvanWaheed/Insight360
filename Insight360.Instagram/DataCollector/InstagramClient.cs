using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Net;
using System.IO;
using Insight360.Common;
using Insight360.Common.BO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Insight360.Util;
using Insight360.DAL;
namespace Insight360.Instagram
{
    // https://api.instagram.com/v1/tags/please/media/recent?access_token=1112909426.3eb2c38.d291dc7c5df74e74aa8c670589957dae
    //https://api.instagram.com/v1/users/ahsrana123/media/recent?access_token=27a782b868e7421e8c555f13d27b57c7
    //https://api.instagram.com/v1/users/self/media/recent/?access_token=1112909426.3eb2c38.d291dc7c5df74e74aa8c670589957dae
    //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.instagram.com/oauth/authorize/?client_id=3eb2c3825fd94801a46ffc204af14b9f&client_secret=a7db30e732aa450dbd12975540aec210&username=ahsrana123&password=ahsenrana123&redirect_uri=http://localhost:5911&response_type=code");
    class InstagramClient
    {
        ManualResetEvent stopEvent;
        private static int requestCounter = 0;
        public InstagramClient(ManualResetEvent stopEvent)
        {
            this.stopEvent = stopEvent;
        }

        public DateTime FetchPostsAndPersist(BO.SearchTermInfo searchTerm, BO.DocumentSourceInfo info, BO.Location location)
        {
            bool exit = false;
            exit = stopEvent.WaitOne(1);

            if (exit)
                return searchTerm.LastFetchTime;

            bool nextPageAvailable = true;
            Pagination pagination = null;
            string accessToken = ConfigDAL.GetInstagramToken(); //"1112909426.3eb2c38.d291dc7c5df74e74aa8c670589957dae";

            while (nextPageAvailable)
            {
                if (requestCounter >= 499)
                {
                    Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "Rate Limit exceeded 500/h Instagram. The Request Counter is " + requestCounter);
                    stopEvent.WaitOne(1000 * 60 * 60);
                    requestCounter = 0; // restarting the counter after wait.
                }
                List<InstagramPost> instagramPosts = new List<InstagramPost>();
               
                instagramPosts = InstagramWrapper.GetInstagramPosts(accessToken, searchTerm, ref pagination, info);

                foreach (var post in instagramPosts)
                {
                    Insight360.DAL.StagingDAL.PersistInstagramPost(post, searchTerm.Id, true, info.DocumentSourceId);
                }

                if (pagination != null && (!string.IsNullOrEmpty(pagination.min_tag_id) || !string.IsNullOrEmpty(pagination.next_max_id)))
                {
                    if (info.ProfileType == BO.ProfileTypes.InstagramUser) {
                        searchTerm.LastFetchId = pagination.next_max_id;
                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(searchTerm, info.DocumentSourceId);
                    }
                    else {
                        searchTerm.LastFetchId = pagination.min_tag_id;
                        DAL.ConfigDAL.UpdateDocumentSourceConfiguration(searchTerm, info.DocumentSourceId);
                    }
                    //Util.Log.WriteOperationStarted(typeof(TwitterSearchClient), "FetchAndPersist", /*organizationName + "-" + */config.SearchTerm + "-" + config.Type + "-" + maximumId);
                }

                nextPageAvailable = (pagination !=null && pagination.next_url != null);
                requestCounter++;
                Util.Log.WriteOperationStarted(this.GetType(), "SearchThread", "The Request Counter was " + requestCounter);
            }

            DAL.StagingDAL.LogMiner(1073741826, "Instagram", "Processing", "Row Id: " + info.RowId + ", KW: " + searchTerm.SearchTerm + ", Src: " + info.DisplayName);
            return searchTerm.LastFetchTime;
        }


        public DateTime FetchPostsAndPersistUserPosts(BO.SearchTermInfo searchTerm, BO.DocumentSourceInfo info, string userId)
        {
            bool exit = false;
            exit = stopEvent.WaitOne(1);

            if (exit)
                return searchTerm.LastFetchTime;

            bool nextPageAvailable = true;
            Pagination pagination = null;
            string accessToken = ConfigDAL.GetInstagramToken(); //"1112909426.3eb2c38.d291dc7c5df74e74aa8c670589957dae";

            while (nextPageAvailable)
            {
                if (requestCounter >= 499)
                {
                    stopEvent.WaitOne(1000 * 60 * 60);
                    requestCounter = 0; // restarting the counter after wait.
                }
                List<InstagramPost> instagramPosts = InstagramWrapper.GetInstagramPosts(accessToken, searchTerm, ref pagination, info);

                instagramPosts = InstagramWrapper.GetInstagramUserPosts(accessToken, searchTerm, ref pagination, userId);
                foreach (var post in instagramPosts)
                {
                    Insight360.DAL.StagingDAL.PersistInstagramPost(post, searchTerm.Id, true, info.DocumentSourceId);
                }

                if (pagination != null && !string.IsNullOrEmpty(pagination.min_tag_id))
                {
                    searchTerm.LastFetchId = pagination.min_tag_id;
                    DAL.ConfigDAL.UpdateDocumentSourceConfiguration(searchTerm, info.DocumentSourceId);

                    //Util.Log.WriteOperationStarted(typeof(TwitterSearchClient), "FetchAndPersist", /*organizationName + "-" + */config.SearchTerm + "-" + config.Type + "-" + maximumId);
                }
                requestCounter++;
                nextPageAvailable = (pagination != null && pagination.next_url != null);
            }

            return searchTerm.LastFetchTime;
        }
    }
}
