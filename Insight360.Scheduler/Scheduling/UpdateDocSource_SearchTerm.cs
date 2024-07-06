using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
using Insight360.Common;
using Insight360.DAL;
using System.Data;
using Insight360.Sources.Facebook;

namespace Insight360.Scheduling
{
    [DisallowConcurrentExecution]
    public class UpdateDocSource_SearchTerm : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //executes for Facebook. To make sure that execution happens, threshold of 15 minutes is kept
                if ((DateTime.Now.Hour == 0) && DateTime.Now.Minute < 15)  // DateTime.Now.Hour == 3 || 
                {
                    Util.Log.WriteOperationStarted(GetType(), "Execute", "Update_Config_DocumentSource_SearchTerm SP is executed");
                    DBUtil.ExecuteNonQuery("Update_Config_DocumentSource_SearchTerm", null, null, CommandType.StoredProcedure);
                }

                //Update comments for manually added Facebook Urls/ FB Posts mined and record in DB. Executesfor 6 hours in a day. 
                if ((DateTime.Now.Hour == 0 || DateTime.Now.Hour == 8 || DateTime.Now.Hour == 11 || DateTime.Now.Hour == 14 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 20) && DateTime.Now.Minute > 30)
                {
                    Util.Log.WriteOperationStarted(GetType(), "Execute", "Facebook Comments Job is executed");

                    var FBPosts = PostDaywiseEngagementDAL.GetFBPosts();
                    List<BO.FacebookCommentInfo> Comments = new List<Insight360.BO.FacebookCommentInfo>();
                    string accessToken = AppSettings.Facebook_AccessToken_User;

                    if (FBPosts.Rows.Count > 0)
                    {
                        foreach (DataRow post in FBPosts.Rows)
                        {
                            Comments = DataCollection.GetPostComments(post["WebSourceId"].ToString(), accessToken);

                            try
                            {
                                if (Comments.Count > 0)
                                    PostDaywiseEngagementDAL.PersistMediaUrl_FacebookComment(Convert.ToInt32(post["Id"]), Comments, Convert.ToBoolean(post["isMediaUrl"]));
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.Log.WriteException(GetType(), "Execute- Update_Config_DocumentSource_SearchTerm or Media Url- Facebook Comments", ex);
            }
        }
    }
}