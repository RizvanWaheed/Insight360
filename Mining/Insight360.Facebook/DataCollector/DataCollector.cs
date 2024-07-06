using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Globalization;
using Insight360.Common;
using Insight360.Util;
using System.Data;

namespace Insight360.Facebook
{
    public class DataCollector
    {

        //Nov 30, 2015 Ahsen Mehmood 
        public DataCollector()
        {
            //Dec 12,2015 Ahsen Mehmood: Getting the frequency from DB
            //var doc = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Facebook");
            //Frequency = doc[0].Frequency * 60000; // converting in milliseconds
        }
        private int Frequency;
        ManualResetEvent stopEvent = new ManualResetEvent(false);

        List<Thread> threads = new List<Thread>();

        public void Start()
        {
            /*if (AppSettings.Facebook_EnableSearchThread)
              threads.Add(new Thread(new ThreadStart(SearchThread)));

           foreach (Thread thread in threads)
           {
               thread.Start();
           }*/
        }

        public int SearchThread()
        {
            if (!AppSettings.Facebook_EnableSearchThread)
                return 1;
            //DateTime dt = DateTime.Now;
            Util.Log.WriteOperationStarted(this.GetType(), "Facebook SearchThread", "Thread Enter");
            FacebookClient searchClient = new FacebookClient();
            bool exit = false;
            // exit = stopEvent.WaitOne(1);
            bool hasFBLimitAvailable = true;
            while (!exit)
            {
                var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("Facebook");
                Frequency = frequencyInMinutes * 60000;
                int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(2);
                var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Facebook");
                DAL.StagingDAL.LogMiner(2, "Facebook", "Started", "Facebook Miner started with " + (docSources.Count - lastRowId) + " channels");
                for (int i = lastRowId + 1; i <= docSources.Count; i++)   //foreach (var source in docSources)
                {
                    try
                    {
                        if (!exit)
                        {
                            hasFBLimitAvailable = searchClient.FetchPostsAndPersist(docSources[i - 1]);
                            if (!hasFBLimitAvailable)
                            {
                                exit = true;
                                break;
                            }
                        }
                        else
                            break;
                    }

                    catch (Exception exp)
                    {
                        DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"FetchPostsAndPersist\" KW: " + docSources[i - 1].SearchTerms[0].SearchTerm + " exp: " + exp.ToString());
                        //Util.Log.WriteException(this.GetType(), "FetchPostsAndPersist " + source.DisplayName, ex);                      
                    }
                }

                DAL.ConfigDAL.UpdateLastFetchConfigId(2, 0);
                if (hasFBLimitAvailable)
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Completed", "Facebook Miner Cycle completed.");
                }
                else
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Limit Reached", "Facebook Miner Cycle Stopped Graph API Limit.");
                }
                exit = stopEvent.WaitOne(Frequency);
                if (docSources.Count == 0)
                    exit = stopEvent.WaitOne(5 * 60 * 1000);
            }
            return 1;
        }

        public int SearchThreadOptimize()
        {
            if (!AppSettings.Facebook_EnableSearchThread)
                return 1;
            Util.Log.WriteOperationStarted(this.GetType(), "Facebook SearchThread", "Thread Enter");
            FacebookClient searchClient = new FacebookClient();
            bool hasFBLimitAvailable = true;
            var frequencyInMinutes = DAL.ConfigDAL.GetFrequency("Facebook");
            Frequency = frequencyInMinutes * 60000;
            int lastRowId = DAL.ConfigDAL.GetLastFetchConfigId(2);
            var docSources = DAL.ConfigDAL.FetchDocumentSourceConfiguration("Facebook");
            DAL.StagingDAL.LogMiner(2, "Facebook", "Started", "Facebook Miner started with " + (docSources.Count - lastRowId) + " channels");
            var accessToken = Insight360.Sources.Facebook.DataCollection.GetToken();
            for (int i = lastRowId + 1; i <= docSources.Count; i++)
            {
                try
                {
                    hasFBLimitAvailable = searchClient.FetchPostsAndPersistOptimize(docSources[i - 1], accessToken);
                    if (!hasFBLimitAvailable)
                    {
                        DAL.StagingDAL.LogMiner(2, "Facebook", "Limit Reached ", "Facebook Miner Cycle Stopped Graph API Limit.");
                        Thread.Sleep(60000 * 30);//wait for next 30 minute and then resume from here.
                        //break;
                    }
                }
                catch (Exception exp)
                {
                    DAL.StagingDAL.LogMiner(2, "Facebook", "Error", "Facebook Miner Issue in \"FetchPostsAndPersist\" KW: " + docSources[i - 1].SearchTerms[0].SearchTerm + " exp: " + exp.ToString());
                }
            }
            DAL.ConfigDAL.UpdateLastFetchConfigId(2, 0);
            if (hasFBLimitAvailable)
            {
                DAL.StagingDAL.LogMiner(2, "Facebook", "Completed", "Facebook Miner Cycle completed.");
            }
            else
            {
                DAL.StagingDAL.LogMiner(2, "Facebook", "Limit Reached", "Facebook Miner Cycle Stopped Graph API Limit.");
            }
            return 1;
        }

        void UserThread()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "UserThread", "Thread Enter");
            try
            {
                FacebookClient searchClient = new FacebookClient(stopEvent);

                bool exit = false;
                exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    Util.Log.WriteOperationStarted(this.GetType(), "UserThread", "Initialize Facebook User Collection");
                    //   DAL.StagingDAL.InitializeFacebookUserCollection();

                    List<BO.FacebookUser> users = null;

                    while (!exit)
                    {
                        try
                        {
                            users = DAL.StagingDAL.FetchFacebookUsersToUpdate(AppSettings.TopCount_User);

                            if (users != null && users.Count > 0)
                            {

                                Util.Log.WriteOperationCompleted(this.GetType(), "UserThread", users.Count + " users fetched");
                                List<string> userIds = new List<string>();
                                foreach (var user in users)
                                {
                                    userIds.Add(user.Id);
                                }

                                users = Insight360.Sources.Facebook.DataCollection.GetUserDetails(
                                    AppSettings.Facebook_AccessToken_User,
                                    userIds);

                                foreach (var user in users)
                                {
                                    DAL.StagingDAL.UpdateFacebookUser(user);
                                }

                                Util.Log.WriteOperationCompleted(this.GetType(), "UserThread", users.Count + " users processed");

                                // DAL.StagingDAL.UpdateFacebookUserProcessingState(userIds, Insight360.Util.DataProcessingState.Processed);


                            }
                            else
                                break;
                        }
                        catch (Exception ex)
                        {
                            Util.Log.WriteException(this.GetType(), "UserThread", ex);
                        }

                        exit = stopEvent.WaitOne((int)AppSettings.WaitInterval_ApiCalls.TotalMilliseconds);
                    }

                    exit = stopEvent.WaitOne((int)AppSettings.WaitInterval_UserCollection.TotalMilliseconds);
                }

            }
            catch (Exception ex)
            {
                Util.Log.WriteException(this.GetType(), "UserThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(this.GetType(), "UserThread", "Thread Exit");
            }
        }

        void DataRefreshThread()
        {
            Util.Log.WriteOperationStarted(this.GetType(), "DataRefreshThread", "Thread Enter");
            try
            {
                FacebookClient searchClient = new FacebookClient(stopEvent);

                bool exit = false;
                exit = stopEvent.WaitOne(1);

                while (!exit)
                {
                    var posts = DAL.StagingDAL.FetchFacebookPostsToUpdate(AppSettings.TopCount_DataRefresh);
                    Util.Log.WriteOperationCompleted(this.GetType(), "DataRefreshThread", "Fetched " + posts.Count);
                    foreach (var post in posts)
                    {
                        var p = Insight360.Sources.Facebook.DataCollection.GetPost(AppSettings.Facebook_AccessToken_User, post.Id, AppSettings.GetCommentsAndLikes);
                        if (p != null)
                        {
                            //p.Staging.OrgName = post.Staging.OrgName;
                            p.Staging.StagingId = post.Staging.StagingId;
                            p.Staging.Followup = post.Staging.Followup;
                            p.Staging.DocumentSourceId = post.Staging.DocumentSourceId;
                            DAL.StagingDAL.PersistFacebookPost(p, AppSettings.GetCommentsAndLikes, false, -1);
                        }
                        Thread.Sleep((int)AppSettings.WaitInterval_ApiCalls.TotalMilliseconds);
                    }
                    Util.Log.WriteOperationCompleted(this.GetType(), "DataRefreshThread",
                        "Processed " + posts.Count);
                    exit = stopEvent.WaitOne(posts.Count > 0 ? 30000 : (int)AppSettings.Facebook_WaitInterval_NothingFetched.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(this.GetType(), "DataRefreshThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationStarted(this.GetType(), "DataRefreshThread", "Thread Exit");
            }
        }


        public void Stop()
        {
            stopEvent.Set();

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

        }
    }
}
