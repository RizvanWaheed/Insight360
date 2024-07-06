//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Google.YouTube;
//using Google.GData.YouTube;
//using Google.GData.Client;
//using System.Configuration;
//using System.IO;
//using Insight360.DAL;
//using Insight360.BO;
//using System.Data;
//using System.Collections;
//using Newtonsoft.Json;

//namespace Insight360.Sources.Youtube
//{
//    public class Operations
//    {

//        #region Configuration Settings




//        private static int pageSize
//        {
//            get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["PageSize"].ToString()); }
//        }

//        //private int orgID
//        //{
//        //    get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["OrgID"].ToString()); }
//        //}

//        private static string prodkey
//        {
//            get { return (System.Configuration.ConfigurationManager.AppSettings["prodkey"].ToString()); }
//        }

//        private static int maxData
//        {
//            get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["maxData"].ToString()); }
//        }

//        private static string YoutubeUserName
//        {
//            get { return (System.Configuration.ConfigurationManager.AppSettings["YoutubeUserName"].ToString()); }
//        }

//        private static string YoutubePassword
//        {
//            get { return (System.Configuration.ConfigurationManager.AppSettings["YoutubePassword"].ToString()); }
//        }

//        private static string FetchTime
//        {
//            get { return (System.Configuration.ConfigurationManager.AppSettings["FetchTime"].ToString()); }
//        }

//        #endregion

//        public void CollectData()
//        {
//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            DBUtil.ConnectionString = ConfigurationManager.ConnectionStrings["YouTubeDS"].ConnectionString;

//            List<DocumentSourceInfo> orgsKeys = ConfigDAL.FetchDocumentSourceConfiguration("Youtube");

//            foreach (DocumentSourceInfo orgKey in orgsKeys)
//            {

//                //foreach (OrganizationInfo org in orgKey.Organizations)
//                //{
//                    this.RefreshStatistics(request, org);
//                    foreach (SearchTermInfo stc in org.SearchTerms)
//                    {

//                        try
//                        {
//                            int Offset = 1;//int.Parse(stc.LastFetchId);
//                            DateTime maxDateCurrent;
//                            DateTime maxDate = maxDateCurrent = stc.LastFetchTime;
//                            while (true)
//                            {

//                                if (Offset > maxData)
//                                {
//                                    Log("Offset greater than MaxData");
//                                    stc.LastFetchTime = maxDateCurrent;
//                                    ConfigDAL.UpdateDocumentSourceConfiguration(stc, orgKey.DocumentSourceId, org.Id);
//                                    break;

//                                }

//                                //Get the Videos, Start from offset, and fetch a set of pageSize
//                                YouTubeQuery myQuery = null;
//                                Feed<Video> videoFeed = null;
//                                if (stc.Type.Equals(SearchTermInfo.Types.SearchTerm))
//                                {
//                                    myQuery = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
//                                    myQuery.Query = stc.SearchTerm;
//                                    myQuery.StartIndex = Offset;
//                                    myQuery.OrderBy = "published";

//                                    if (FetchTime.Equals(YouTubeQuery.UploadTime.Today))
//                                    {
//                                        myQuery.Time = YouTubeQuery.UploadTime.Today;
//                                    }
                                    
//                                    myQuery.NumberToRetrieve = pageSize;

//                                    Log("Starting Searching, Offset: " + Offset.ToString() + " maxDate: " + maxDate.ToString() + " key:" + myQuery.Query);

//                                    videoFeed = request.Get<Video>(myQuery);
//                                }
//                                else if (stc.Type.Equals(SearchTermInfo.Types.YoutubeUser))
//                                {
//                                    Uri uri = new Uri("http://gdata.youtube.com/feeds/api/users/" + stc.SearchTerm + "/uploads");
//                                    videoFeed = request.Get<Video>(uri);
//                                }



//                                //Now see if no record is fetched, Update the MaxDate and set Offset back to 1
//                                if (videoFeed.Entries == null || videoFeed.Entries.Count() < 1)
//                                {
//                                    Log("No Video Found");
//                                    stc.LastFetchTime = maxDateCurrent;
//                                    ConfigDAL.UpdateDocumentSourceConfiguration(stc, orgKey.DocumentSourceId, org.Id);
//                                    break;
//                                }
//                                else
//                                {
//                                    Log("Some Videos Found");
//                                    List<DateTime> dates = GetMinMaxDate(videoFeed.Entries);
//                                    Log(videoFeed.Entries.Count().ToString() + " videos Found, Now Inserting");
//                                    foreach (Video vdo in videoFeed.Entries)
//                                    {
//                                        Log("Inserting Video " + vdo.VideoId);
//                                        int newID = DbOperations.InsertVideo(vdo, org.Name);
//                                        //Now Insert the UserInfo
//                                        ProfileEntry profile = GetUserInfo(vdo.Author);
//                                        if (profile != null)
//                                        {
//                                            Log("Inserting UserInfo - Start " + vdo.Author);
//                                            DbOperations.InsertUserInfo(profile, vdo.Author);
//                                            Log("Inserting UserInfo - Done" + vdo.Author);
//                                        }

//                                        Log("Video Inserted, DB ID " + newID.ToString());
//                                        if (newID > 0)
//                                        {
//                                            List<Comment> coms = GetVideoComments(request, vdo);
//                                            Log(coms.Count.ToString() + "Comments found ");
//                                            foreach (Comment com in coms)
//                                            {
//                                                Log("Inserting Comment: " + com.Id + " VideoId: " + newID.ToString());
//                                                DbOperations.InsertVideoComment(newID, com);

//                                                Log("Comment Inserted, Comment: " + com.Id + " VideoId: " + newID.ToString());
//                                            }
//                                            //Now Update its CommentsUpdated field
//                                            Dictionary<string, string> uP = new Dictionary<string, string>();
//                                            uP.Add("@VideoID", newID.ToString());
//                                            uP.Add("@CommentsUpdated", DateTime.Now.ToString());
//                                            DbOperations.UpdateSP("ytUpdateCommentsUpdated", uP);
//                                        }

//                                    }

//                                    //Now see if it contains the overlapping eintries then reset MaxDate and Offset
//                                    if (dates[0] <= maxDate)
//                                    {
//                                        Log("Overlapping Entries found");
//                                        stc.LastFetchTime = dates[1] > maxDateCurrent ? dates[1] : maxDateCurrent;
//                                        Log("Updating Last Fetch Time to: " + stc.LastFetchTime.ToString());
//                                        ConfigDAL.UpdateDocumentSourceConfiguration(stc, orgKey.DocumentSourceId, org.Id);
//                                        break;
//                                    }
//                                    else //Just Update the Offset
//                                    {
//                                        Log("No Overlapping Entries found, updating offset to: " + (Offset + pageSize).ToString());
//                                        Offset += pageSize;
//                                        maxDateCurrent = dates[1] > maxDateCurrent ? dates[1] : maxDateCurrent;
//                                    }

//                                    //DbOperations.UpdateSP("ytUpdateOffsetAndMaxDate", updateParam);
//                                    Log("Offset and Maxdate updating Finished");

//                                    if (stc.Type.Equals(SearchTermInfo.Types.YoutubeUser))
//                                    {
//                                        break;
//                                    }
//                                }
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            Log("Exception Occured in CollectData: " + ex.Message);

//                        }
//                    }

                    

//                //}

//            }

//        }

//        public void FollowUsers()
//        {
//            IEnumerable<string> users = new string[] { "ash131dfs", "erosentertainment" };

//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey);

//            YouTubeRequest request = new YouTubeRequest(settings);


//            foreach (string user in users)
//            {
//                YouTubeQuery myQuery = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
//                myQuery.Author = user;
//                //myQuery.StartIndex = Offset;
//                myQuery.OrderBy = "published";
//                myQuery.NumberToRetrieve = pageSize;

//                Feed<Video> videoFeed = request.Get<Video>(myQuery);

//                //Now see if no record is fetched, Update the MaxDate and set Offset back to 1
//                if (videoFeed.Entries == null || videoFeed.Entries.Count() < 1)
//                {

//                }
//                else
//                {
//                    foreach (Video vdo in videoFeed.Entries)
//                    {

//                    }
//                }
//            }

//        }

//        public void RefreshStatistics(YouTubeRequest request, OrganizationInfo org)
//        {
//            DataTable table =  DbOperations.FetchVideosToUpdate();
//            YouTubeQuery myQuery = null;
//            Feed<Video> videoFeed = null;
//            Hashtable hash = new Hashtable();

//            StringBuilder sb = new StringBuilder();
//            int counter = 0;
//            foreach (DataRow row in table.Rows)
//            {
//                if (counter == 0)
//                {
//                    sb.Append(row["YoutubeID"].ToString());
//                }
//                else
//                {
//                    sb.Append("|" + row[0].ToString());
//                }

//                hash.Add(row["YoutubeID"].ToString(), row["CommentCount"].ToString());

//                counter++;
//            }           

//            string videos = sb.ToString();

//            if (string.IsNullOrEmpty(videos) == false)
//            {
//                myQuery = new YouTubeQuery("http://gdata.youtube.com/feeds/api/videos?q=" + videos + "&v=2");
//                videoFeed = request.Get<Video>(myQuery);

//                if (videoFeed.Entries == null || videoFeed.Entries.Count() < 1)
//                {
//                    Log("No Updates Found");
//                }
//                else
//                {
//                    Log("Some Videos Found");
//                    Log(videoFeed.Entries.Count().ToString() + " videos Found, Now Inserting");
//                    foreach (Video vdo in videoFeed.Entries)
//                    {
//                        Log("Inserting Video " + vdo.VideoId);
//                        int newID = DbOperations.InsertVideo(vdo, org.Name);

//                        ProfileEntry profile = GetUserInfo(vdo.Author);
//                        if (profile != null)
//                        {
//                            Log("Inserting UserInfo - Start " + vdo.Author);
//                            DbOperations.InsertUserInfo(profile, vdo.Author);
//                            Log("Inserting UserInfo - Done" + vdo.Author);
//                        }

//                        List<Comment> coms = GetVideoComments(request, vdo);
//                        Log(coms.Count.ToString() + "Comments found ");
//                        foreach (Comment com in coms)
//                        {
                            
//                            Log("Inserting Comment: " + com.Id + " VideoId: " + newID.ToString());
//                            DbOperations.InsertVideoComment(newID, com);

//                            Log("Comment Inserted, Comment: " + com.Id + " VideoId: " + newID.ToString());
//                        }

//                        //Now Update its CommentsUpdated field
//                        Dictionary<string, string> uP = new Dictionary<string, string>();
//                        uP.Add("@VideoID", newID.ToString());
//                        uP.Add("@CommentsUpdated", DateTime.Now.ToString());
//                        DbOperations.UpdateSP("ytUpdateCommentsUpdated", uP);
//                    }
//                }
//            }
//        }

//        public void AddExistingUsersInfo()
//        {
//            DataTable dtAuthors = DbOperations.GetExistingAuthors();
//            if (dtAuthors != null)
//            {
//                foreach (DataRow dRow in dtAuthors.Rows)
//                {
//                    ProfileEntry profile = GetUserInfo(dRow[0].ToString());
//                    if (profile != null)
//                    {
//                        DbOperations.InsertUserInfo(profile, dRow[0].ToString());
//                    }
//                }
//            }
//        }


//        public bool PostCommentOnVideo(Comment comment, Video video)
//        {
//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey, YoutubeUserName, YoutubePassword);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            try
//            {
//                Log("Adding comment on Video: " + video.VideoId);
//                request.AddComment(video, comment);
//                Log("Comment added on Video: " + video.VideoId);
//            }
//            catch (Exception ex)
//            {
//                Log("Exception Occured in PostCommentOnVideo: " + ex.Message);
//                return false;
//            }

//            return true;
//        }


//        public void GetUserMessages(string userName, string passWord)
//        {
//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey, YoutubeUserName, YoutubePassword);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            string feedUrl = "http://gdata.youtube.com/feeds/api/users/default/inbox/";

//            YouTubeQuery query = new YouTubeQuery(feedUrl);

//            //query.StartIndex = 1;

//            //query.NumberToRetrieve = 10;

//            MessageFeed messagesFeed = request.Service.GetMessages(query);

//            List<AtomEntry> lstMsgs = messagesFeed.Entries.ToList<AtomEntry>();


//        }

//        public bool PostCommentOnComment(Comment comment, Comment parentComment, Video video)
//        {


//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey, YoutubeUserName, YoutubePassword);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            try
//            {
//                Log("Adding comment on Video: " + video.VideoId + "  and Comment: " + parentComment.Id);
//                comment.ReplyTo(parentComment);
//                request.AddComment(video, comment);
//                Log("Comment added on Video: " + video.VideoId);
//            }
//            catch (Exception ex)
//            {
//                Log("Exception Occured in PostCommentOnVideo: " + ex.Message);
//                return false;
//            }

//            return true;
//        }

//        private ProfileEntry GetUserInfo(string author)
//        {
//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            YouTubeService service = new YouTubeService("My Test App", prodkey);

//            try
//            {
//                //Get the Videos, Start from offset, and fetch a set of pageSize
//                //YouTubeQuery myQuery = new YouTubeQuery(YouTubeQuery.DefaultVideoUri);
//                //myQuery.Query = "Hunza Valley";
//                //myQuery.StartIndex = Offset;
//                //myQuery.OrderBy = "published";
//                //myQuery.NumberToRetrieve = pageSize;
//                //myQuery.Author = "ash1312";
//                ProfileEntry profile = (ProfileEntry)service.Get("http://gdata.youtube.com/feeds/api/users/" + author);
//                return profile;
//                //Feed<Video> videoFeed = request.Get<Video>(myQuery);

//            }
//            catch (Exception ex)
//            {
//                Log("Exception Occured in GetUserInfo: " + ex.Message);
//                return null;
//            }

//        }

//        public static Comment GetCommentByID(string YouTubeCommentID)
//        {

//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey, YoutubeUserName, YoutubePassword);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            Log("Serching for comment with URL: " + YouTubeCommentID);
//            Feed<Comment> comm = request.Get<Comment>(new Uri(YouTubeCommentID));
//            if (comm.Entries != null && comm.Entries.Count() > 0)
//            {
//                Log("Comment Found!");
//                return comm.Entries.First<Comment>();
//            }
//            Log("Comment Not Found!");
//            return null;
//        }

//        private static Video GetVideoByURL(string VideoURL)
//        {
//            YouTubeRequestSettings settings = new YouTubeRequestSettings("My Test App", prodkey, YoutubeUserName, YoutubePassword);

//            YouTubeRequest request = new YouTubeRequest(settings);

//            Log("Serching for Video with URL: " + VideoURL);
//            Feed<Video> comm = request.Get<Video>(new Uri(VideoURL));
//            if (comm.Entries != null && comm.Entries.Count() > 0)
//            {
//                Log("Video Found!");
//                return comm.Entries.First<Video>();
//            }
//            Log("Video Not Found!");
//            return null;
//        }

//        private static List<Comment> GetVideoComments(YouTubeRequest request, Video vdo)
//        {
//            List<Comment> comments = new List<Comment>();
//            if (vdo.CommmentCount > 0)
//            {
//                Feed<Comment> coms = request.GetComments(vdo);
//                comments.AddRange(coms.Entries);
//                while (coms != null && coms.Entries != null)
//                {
//                    coms = request.Get<Comment>(coms, FeedRequestType.Next);
//                    if (coms != null && coms.Entries != null)
//                        comments.AddRange(coms.Entries);
//                }

//            }
//            return comments;
//        }

//        private static List<DateTime> GetMinMaxDate(IEnumerable<Video> Videos)
//        {
//            DateTime minDate = DateTime.Now;
//            DateTime maxDate = DateTime.Now.AddYears(-50);

//            foreach (Video vdo in Videos)
//            {
//                if (vdo.YouTubeEntry.Published < minDate)
//                    minDate = vdo.YouTubeEntry.Published;

//                if (vdo.YouTubeEntry.Published > maxDate)
//                    maxDate = vdo.YouTubeEntry.Published;
//            }

//            List<DateTime> dates = new List<DateTime>();
//            dates.Clear();
//            dates.Add(minDate);
//            dates.Add(maxDate);
//            return dates;
//        }

//        private static void Log(string message)
//        {
//            try
//            {
//                if (string.Equals(ConfigurationManager.AppSettings["EnableLogging"], "1"))
//                {
//                    //Get the File Name 
//                    string fileName = ConfigurationManager.AppSettings["FilePath"] + ConfigurationManager.AppSettings["FileName"] + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + ".txt";

//                    if (!File.Exists(fileName))
//                        File.Create(fileName);
//                    //File.WriteAllText(fileName, Environment.NewLine + message);

//                    StreamWriter wr = File.AppendText(fileName);

//                    wr.Write(Environment.NewLine + message + "  :: " + DateTime.Now.ToString());

//                    wr.Flush();

//                    wr.Close();
//                }
//            }
//            catch (Exception ex) { }
//        }


//    }
//}
