using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
using Insight360.Common;
using Insight360.DAL;
using System.Data;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace Insight360.Scheduling
{
    [DisallowConcurrentExecution]
    public class EmailAlerts : IJob
    {
        //readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //static bool first_period, second_period, third_period, final_period;

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                string emailSubject = "";

                Util.Log.WriteOperationStarted(GetType(), "Execute", "Email Alerts Called");
                DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
                var DsPosts = EmailAlertsDAL.GetPostsForEmailAlerts();
                bool isSent = false;

                //Periodic Emails (Specific Keywords)
                var Email_Intervals = DsPosts.Tables[0];
                if (Email_Intervals.Rows.Count > 0)
                {
                    bool Int_01, Int_02, Int_03, Int_04, Int_05;
                    int interval = 0, current_hour = DateTime.Now.Hour;

                    if (current_hour >= 9 && current_hour < 12)
                    {
                        interval = 1;
                        emailSubject = "E: Social Media Alerts - " + DateTime.Now.Date.AddDays(-1).ToString("MMM dd, yyyy") + " - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 09:00 P.M. - 09:00 A.M.";//"Email Alerts (KE Related) - " + DateTime.Now.Date.AddDays(-1).ToString("MMM dd, yyyy") + " - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 09:00 P.M. - 09:00 A.M.";
                    }
                    else if (current_hour >= 12 && current_hour < 15)
                    {
                        interval = 2;
                        emailSubject = "E: Social Media Alerts - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 09:00 A.M. - 12:00 P.M."; //"Email Alerts (KE Related) - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 09:00 A.M. - 12:00 P.M.";
                    }
                    else if (current_hour >= 15 && current_hour < 21)
                    {
                        interval = 3;
                        emailSubject = "E: Social Media Alerts - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 12:00 P.M. - 03:00 P.M."; //"Email Alerts (KE Related) - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 12:00 P.M. - 03:00 P.M.";
                    }
                    else if (current_hour >= 21 || current_hour < 9)
                    {
                        interval = 4;
                        emailSubject = "E: Social Media Alerts - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 03:00 P.M. - 09:00 P.M."; //"Email Alerts (KE Related) - " + DateTime.Now.Date.ToString("MMM dd, yyyy") + ": 03:00 P.M. - 09:00 P.M.";
                    }

                    Int_01 = Convert.ToBoolean(Email_Intervals.Rows[0]["isSet"]);
                    Int_02 = Convert.ToBoolean(Email_Intervals.Rows[1]["isSet"]);
                    Int_03 = Convert.ToBoolean(Email_Intervals.Rows[2]["isSet"]);
                    Int_04 = Convert.ToBoolean(Email_Intervals.Rows[3]["isSet"]);

                    if ((interval == 1 && (!Int_01)) || (interval == 2 && (!Int_02)) || (interval == 3 && (!Int_03)) || (interval == 4 && (!Int_04)))
                    {
                        var SpecificKW_Posts = DsPosts.Tables[1];
                        if (SpecificKW_Posts.Rows.Count > 0)
                        {
                            string emailBody = GetEmailBody(SpecificKW_Posts);
                            if (DsPosts.Tables[6].Rows.Count > 0)
                            {
                                EmailHelper emailHelper = new EmailHelper(AppSettings.SmtpHost, AppSettings.SmtpPort, AppSettings.SmtpUserName, AppSettings.SmtpPassword);
                                isSent = emailHelper.SendEmail(AppSettings.SmtpUserName, DsPosts.Tables[6], emailSubject, emailBody);

                                if (isSent)
                                {
                                    EmailAlertsDAL.UpdatePostsSentForEmailAlerts(SpecificKW_Posts, "Periodic", interval);
                                    Util.Log.WriteOperationStarted(GetType(), "Execute", "Periodic Email Sent for KE Related Keywords --- Posts Counts " + SpecificKW_Posts.Rows.Count);
                                }
                            }
                        }
                    }

                    //Periodic Email (General Keywords, only once in a day)
                    Int_05 = Convert.ToBoolean(Email_Intervals.Rows[4]["isSet"]); interval = 5;
                    emailSubject = "E: Other Disco/General News – Social Media - " + DateTime.Now.Date.ToString("MMM dd, yyyy"); //"Email Alerts (General News) - " + DateTime.Now.Date.ToString("MMM dd, yyyy");

                    if (current_hour == 9 && (!Int_05))
                    {
                        var GeneralKW_Posts = DsPosts.Tables[4];
                        if (GeneralKW_Posts.Rows.Count > 0)
                        {
                            string emailBody = GetEmailBody(GeneralKW_Posts);
                            if (DsPosts.Tables[6].Rows.Count > 0)
                            {
                                EmailHelper emailHelper = new EmailHelper(AppSettings.SmtpHost, AppSettings.SmtpPort, AppSettings.SmtpUserName, AppSettings.SmtpPassword);
                                isSent = emailHelper.SendEmail(AppSettings.SmtpUserName, DsPosts.Tables[6], emailSubject, emailBody);

                                if (isSent)
                                {
                                    EmailAlertsDAL.UpdatePostsSentForEmailAlerts(GeneralKW_Posts, "Periodic", interval);
                                    Util.Log.WriteOperationStarted(GetType(), "Execute", "Periodic Email Sent for General Keywords --- Posts Counts " + GeneralKW_Posts.Rows.Count);
                                }
                            }
                        }
                    }
                }

                //Pages/Profile Emails
                var Email_Profiles = DsPosts.Tables[2];
                if (Email_Profiles.Rows.Count > 0)
                {
                    //emailSubject = "Email Alerts - Preset Pages/Profiles";
                    //string emailBody = GetEmailBody(Email_Profiles);

                    if (DsPosts.Tables[6].Rows.Count > 0)
                    {
                        int Author_Id = 0; // Convert.ToInt32(DsPosts.Tables[6].Rows[0]["AuthorId"]);
                        string Author_Name = "", emailBody = "";// DsPosts.Tables[6].Rows[0]["Name"].ToString();

                        DataTable dt = Email_Profiles.Clone();
                        EmailHelper emailHelper = new EmailHelper(AppSettings.SmtpHost, AppSettings.SmtpPort, AppSettings.SmtpUserName, AppSettings.SmtpPassword);

                        for (int i = 0; i < Email_Profiles.Rows.Count; i++)
                        {
                            DataRow row = Email_Profiles.Rows[i];

                            int Post_AuthorId = Convert.ToInt32(row["AuthorId"]);
                            string Post_AuthorName = row["Name"].ToString();

                            if (Post_AuthorId != Author_Id && Author_Id != 0)
                            {
                                emailBody = GetEmailBody(dt);
                                isSent = emailHelper.SendEmail(AppSettings.SmtpUserName, DsPosts.Tables[6], emailSubject, emailBody);
                                dt.Clear();
                            }

                            dt.ImportRow(row);

                            Author_Id = Post_AuthorId;
                            Author_Name = Post_AuthorName;
                            emailSubject = "E: " + Author_Name + " Posted";
                        }

                        emailBody = GetEmailBody(dt);
                        isSent = emailHelper.SendEmail(AppSettings.SmtpUserName, DsPosts.Tables[6], emailSubject, emailBody);

                        if (isSent)
                        {
                            EmailAlertsDAL.UpdatePostsSentForEmailAlerts(Email_Profiles, "Profile");
                            Util.Log.WriteOperationStarted(GetType(), "Execute", "Profile Email Sent --- Posts Counts " + Email_Profiles.Rows.Count);
                        }
                    }
                }

                //Email when comment count on a specific post exceeds over a specific value
                var Email_Comments = DsPosts.Tables[3];
                if (Email_Comments.Rows.Count > 0)
                {
                    emailSubject = "E: Post Sentiment Spikes"; //"Email Alerts - Posts with Positive comments over 25 or Negative comments over 50";
                    string emailBody = GetEmailBody(Email_Comments);

                    if (DsPosts.Tables[6].Rows.Count > 0)
                    {
                        EmailHelper emailHelper = new EmailHelper(AppSettings.SmtpHost, AppSettings.SmtpPort, AppSettings.SmtpUserName, AppSettings.SmtpPassword);
                        isSent = emailHelper.SendEmail(AppSettings.SmtpUserName, DsPosts.Tables[6], emailSubject, emailBody);

                        if (isSent)
                        {
                            EmailAlertsDAL.UpdatePostsSentForEmailAlerts(Email_Comments, "Comment");
                            Util.Log.WriteOperationStarted(GetType(), "Execute", "Comments Email Sent --- Posts Counts " + Email_Comments.Rows.Count);
                        }
                    }
                }

                //Engagement Trend Graph Emails (will go only once for each Tracked (Manually added / Mined Data) record)
                var Email_Trends = DsPosts.Tables[5];
                if (Email_Trends.Rows.Count > 0)
                {
                    emailSubject = "E: Engagement Trend Graph - Reactions / Shares / Comments"; //"Email Alerts - Posts with Positive comments over 25 or Negative comments over 50";

                    if (DsPosts.Tables[6].Rows.Count > 0)
                    {
                        for (int i = 0; i < Email_Trends.Rows.Count; i++)
                        {
                            List<Attachment> attachments = new List<Attachment>();

                            string daywise_base64 = Email_Trends.Rows[i]["Graph_Daywise"].ToString().Split(new char[] { ',' })[1];
                            Attachment attachment1 = new Attachment(new MemoryStream(Convert.FromBase64String(daywise_base64)), "DaywiseGraph_" + Convert.ToInt32(Email_Trends.Rows[i]["Id"]) + ".png");
                            attachment1.ContentDisposition.Inline = true;
                            attachment1.ContentDisposition.DispositionType = "Inline";
                            attachment1.ContentType.MediaType = "image/png";
                            attachment1.ContentId = "image1.png";
                            attachments.Add(attachment1);

                            string hourly_base64 = Email_Trends.Rows[i]["Graph_Hourly"].ToString().Split(new char[] { ',' })[1];
                            Attachment attachment2 = new Attachment(new MemoryStream(Convert.FromBase64String(hourly_base64)), "Hourly_Graph_" + Convert.ToInt32(Email_Trends.Rows[i]["Id"]) + ".png");
                            attachment2.ContentDisposition.Inline = true;
                            attachment2.ContentDisposition.DispositionType = "Inline";
                            attachment2.ContentType.MediaType = "image/png";
                            attachment2.ContentId = "image2.png";
                            attachments.Add(attachment2);

                            string sentiment_base64 = Email_Trends.Rows[i]["Graph_Sentiment"].ToString().Split(new char[] { ',' })[1];
                            Attachment attachment3 = new Attachment(new MemoryStream(Convert.FromBase64String(sentiment_base64)), "Sentiment_Graph_" + Convert.ToInt32(Email_Trends.Rows[i]["Id"]) + ".png");
                            attachment3.ContentDisposition.Inline = true;
                            attachment3.ContentDisposition.DispositionType = "Inline";
                            attachment3.ContentType.MediaType = "image/png";
                            attachment3.ContentId = "image3.png";
                            attachments.Add(attachment3);

                            string emailBody = GetTrendEmailBody(Email_Trends.Rows[i], attachment1.ContentId, attachment2.ContentId, attachment3.ContentId);

                            EmailHelper emailHelper = new EmailHelper(AppSettings.SmtpHost, AppSettings.SmtpPort, AppSettings.SmtpUserName, AppSettings.SmtpPassword, attachments);
                            isSent = emailHelper.SendEmail(AppSettings.SmtpUserName, DsPosts.Tables[6], emailSubject, emailBody, attachments);
                        }

                        if (isSent)
                        {
                            EmailAlertsDAL.UpdatePostsSentForEmailAlerts(Email_Trends, "Trend");
                            Util.Log.WriteOperationStarted(GetType(), "Execute", "Engagement Trend Email Sent --- Posts Counts " + Email_Trends.Rows.Count);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.Log.WriteException(GetType(), "Execute", ex);
            }
        }

        protected string GetEmailBody(DataTable dtPosts)
        {
            StringBuilder HeaderTemplate = new StringBuilder(File.ReadAllText(HttpRuntime.AppDomainAppPath + "/Views/EmailTemplates/HeaderEmailAlertsTemplate.html"));
            StringBuilder BodyTemplate = new StringBuilder(File.ReadAllText(HttpRuntime.AppDomainAppPath + "/Views/EmailTemplates/BodyEmailAlertsTemplate.html"));
            StringBuilder emailBody = new StringBuilder();
            StringBuilder processEmailBody = new StringBuilder();

            int count = 1, Post_AuthorId = 0, AuthorId = 0, Post_MediaCnt = 0, MediaCnt = 0, DocumentSourceType_Id = 0;
            string Text = "", Post_Text = ""; StringBuilder Post_Link = new StringBuilder();

            foreach (DataRow row in dtPosts.Rows)
            {
                Post_Text = row["Text"].ToString().Split(new string[] { "http" }, StringSplitOptions.None)[0].Trim();
                Post_AuthorId = Convert.ToInt32(row["AuthorId"]);
                Post_MediaCnt = Convert.ToInt32(row["MediaCnt"]);

                if (Post_Text.ToLower() != Text.ToLower() || Post_AuthorId != AuthorId || MediaCnt > 0)
                {
                    if ((Text != "" || DocumentSourceType_Id == 99) && AuthorId != 0)
                    {
                        processEmailBody.Replace("#PostLink", Post_Link.ToString());
                        emailBody.Append(processEmailBody);
                    }

                    processEmailBody = new StringBuilder(BodyTemplate.ToString());
                    Post_Link = new StringBuilder();
                    string strMediaUrl = "", strMediaInfo = "";

                    string Term = row["Term"].ToString().Trim();
                    processEmailBody.Replace("#Icon", row["Icon"].ToString().Trim()); //showing social meida icon is not working
                    processEmailBody.Replace("#AuthorProfileImage", row["ImageUri"].ToString().Trim());
                    processEmailBody.Replace("#AuthorName", row["Name"].ToString().Trim());
                    processEmailBody.Replace("#AuthorNameInSmall", row["Name"].ToString().Trim());
                    processEmailBody.Replace("#PostedOn", Convert.ToDateTime(row["Time"]).ToString().Trim());
                    processEmailBody.Replace("#AuthorFollowers", row["Followers"].ToString().Trim());
                    processEmailBody.Replace("#AuthorFollowing", row["Following"].ToString().Trim());
                    processEmailBody.Replace("#AuthorProfileUriText", !string.IsNullOrEmpty(row["AuthorLink"].ToString()) ? row["AuthorLink"].ToString().Trim() : "N/A");
                    processEmailBody.Replace("#AuthorProfileUri", !string.IsNullOrEmpty(row["AuthorLink"].ToString()) ? row["AuthorLink"].ToString().Trim() : "#");
                    processEmailBody.Replace("#PostText", Regex.Replace(Post_Text, Term, "<b style='color:#3883a7'>" + Term + "</b>", RegexOptions.IgnoreCase));  //ConvertUrlsToLinks(row["Text"].ToString().Trim().Replace(Term, "<b style='color:#3883a7'>" + Term + "</b>"))

                    if (Post_MediaCnt > 0)
                    {
                        strMediaUrl = "<tr><td colspan='2' style = 'padding-bottom:10px;'><table cellpadding='0' cellspacing='0'><tr><td><img src = '" + row["MediaUrl"].ToString().Trim() + "' height = '280' width = '265' style = 'border: solid 2px #dadada !important;'/></td>";

                        if (Post_MediaCnt == 2)
                            strMediaUrl += "<td><img src = '" + row["MediaUrl2"].ToString().Trim() + "' height = '280' width = '265' style = 'border: solid 2px #dadada !important;'/></td></tr></table></td></tr>";
                        else if (Post_MediaCnt > 2)
                        {
                            strMediaUrl += "<td><table cellpadding='0' cellspacing='0'><tr><td style ='padding-left:2px;'><img src = '" + row["MediaUrl2"].ToString().Trim() + "' height = '136' width = '265' style = 'border: solid 2px #dadada !important;'/></td></tr>";
                            strMediaUrl += "<tr><td style ='padding-left:2px;'><img src = '" + row["MediaUrl3"].ToString().Trim() + "' height = '136' width = '265' style = 'border: solid 2px #dadada !important;'/></td></tr></table></td></tr></table></td></tr>";
                        }
                        else
                            strMediaUrl += "</tr></table></td></tr>";

                        if (Post_MediaCnt > 3)
                            strMediaInfo = "<tr><td colspan = '2' style = 'padding-bottom:5px; font-size:12px;'>View <b style='color:#3883a7; font-size:14px;'>" + (Convert.ToInt32(row["MediaCnt"]) - 3).ToString() + "+</b> more images / videos by clicking on the link below.</td></tr>";
                    }
                    processEmailBody.Replace("#MediaUrl", strMediaUrl);
                    processEmailBody.Replace("#MediaInfo", strMediaInfo);

                    //  processEmailBody.Replace("#PostLink", !string.IsNullOrEmpty(row["PostLink"].ToString()) ? row["PostLink"].ToString().Trim() : "#");
                    //processEmailBody.Replace("#CommentStats", row["type"].ToString() == "Comment" ? " | <span style='color:#15F015'> " + row["pos_comments"].ToString() + "</span> Comment(s) | <span style='color:#F40000'>" + row["neg_comments"].ToString() + "</span> Comment(s) " + (row["hidden_comments"].ToString() == "0" ? "" : "[ including <span style='color:#F40000'>" + row["hidden_comments"].ToString() + "</span> Hidden Comments ]") + " | <span style='color:#999'>" + row["neu_comments"].ToString() + "</span> Comment(s)" : "");
                    processEmailBody.Replace("#CommentStats", (row["type"].ToString() == "Comment" || (row["type"].ToString() == "Periodic" && row["DocumentSourceType_Id"].ToString() != "1")) ? " | <span style='color:#15F015'> " + row["pos_comments"].ToString() + "</span> Comment(s) | <span style='color:#F40000'>" + row["neg_comments"].ToString() + "</span> Comment(s) " + " | <span style='color:#999'>" + row["neu_comments"].ToString() + "</span> Comment(s)" : "");
                    Post_Link.Append("<span><a href=" + row["PostLink"].ToString().Trim() + " style ='color:#369'>" + row["PostLink"].ToString().Trim() + "</a><span>");

                    Text = Post_Text;
                    AuthorId = Convert.ToInt32(row["AuthorId"]);
                    DocumentSourceType_Id = Convert.ToInt32(row["DocumentSourceType_Id"]);
                    MediaCnt = Post_MediaCnt;
                }
                else
                {
                    Post_Link.Append("<br/><span><a href=" + row["PostLink"].ToString().Trim() + " style ='color:#369'>" + row["PostLink"].ToString().Trim() + "</a><span>");
                }
                count++;

                if (count > dtPosts.Rows.Count)
                {
                    processEmailBody.Replace("#PostLink", Post_Link.ToString());
                    emailBody.Append(processEmailBody);
                }
            }

            HeaderTemplate.Replace("#EmailAlertsBody", emailBody.ToString());
            return HeaderTemplate.ToString();
        }

        protected string GetTrendEmailBody(DataRow row, string cid1, string cid2, string cid3)
        {
            StringBuilder HeaderTemplate = new StringBuilder(File.ReadAllText(HttpRuntime.AppDomainAppPath + "/Views/EmailTemplates/HeaderEmailAlertsTemplate.html"));
            StringBuilder BodyTemplate = new StringBuilder(File.ReadAllText(HttpRuntime.AppDomainAppPath + "/Views/EmailTemplates/TrendEmailAlertsTemplate.html"));
            StringBuilder emailBody = new StringBuilder();
            StringBuilder processEmailBody = new StringBuilder(BodyTemplate.ToString());
            string strMediaUrl = "";

            string Term = row["Term"].ToString().Trim();
            processEmailBody.Replace("#Icon", row["Icon"].ToString().Trim()); //showing social meida icon is not working
            processEmailBody.Replace("#AuthorProfileImage", row["ImageUri"].ToString().Trim());
            processEmailBody.Replace("#AuthorName", row["Name"].ToString().Trim());
            processEmailBody.Replace("#AuthorNameInSmall", row["Name"].ToString().Trim());
            processEmailBody.Replace("#PostedOn", Convert.ToDateTime(row["Time"]).ToString().Trim());
            processEmailBody.Replace("#AuthorFollowers", row["Followers"].ToString().Trim());
            processEmailBody.Replace("#AuthorFollowing", row["Following"].ToString().Trim());
            processEmailBody.Replace("#AuthorProfileUriText", !string.IsNullOrEmpty(row["AuthorLink"].ToString()) ? row["AuthorLink"].ToString().Trim() : "N/A");
            processEmailBody.Replace("#AuthorProfileUri", !string.IsNullOrEmpty(row["AuthorLink"].ToString()) ? row["AuthorLink"].ToString().Trim() : "#");
            processEmailBody.Replace("#PostText", Regex.Replace(row["Text"].ToString().Trim(), Term, "<b style='color:#3883a7'>" + Term + "</b>", RegexOptions.IgnoreCase));  //ConvertUrlsToLinks(row["Text"].ToString().Trim().Replace(Term, "<b style='color:#3883a7'>" + Term + "</b>"))
            processEmailBody.Replace("#PostLink", !string.IsNullOrEmpty(row["PostLink"].ToString()) ? row["PostLink"].ToString().Trim() : "#");
            processEmailBody.Replace("#GraphDaywise", "cid:" + cid1);
            processEmailBody.Replace("#GraphHourly", "cid:" + cid2);
            processEmailBody.Replace("#GraphSentiment", "cid:" + cid3);

            if (row["MediaUrl"].ToString() != "")
            {
                strMediaUrl = "<tr><td colspan='2' style = 'padding-bottom:10px;'><table cellpadding='0' cellspacing='0'><tr><td><img src = '" + row["MediaUrl"].ToString().Trim() + "' height = '270' width = '255' style = 'border: solid 2px #dadada !important;'/></td>";
                strMediaUrl += "</tr></table></td></tr>";
            }
            processEmailBody.Replace("#MediaUrl", strMediaUrl);

            emailBody.Append(processEmailBody);
            //count++;
            //}

            HeaderTemplate.Replace("#EmailAlertsBody", emailBody.ToString());
            return HeaderTemplate.ToString();
        }

        private string ConvertUrlsToLinks(string text)
        {
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(text, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
        }
    }
}