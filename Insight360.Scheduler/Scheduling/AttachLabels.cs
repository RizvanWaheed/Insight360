using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Net;
using Insight360.Common;
using Insight360.DAL;
using System.Data;

namespace Insight360.Scheduling
{
    [DisallowConcurrentExecution]
    public class AttachLabels : IJob
    {
        //readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Util.Log.WriteOperationStarted(GetType(), "Execute", "Store procedure AttachToFacebook was executed");
                DBUtil.ExecuteNonQuery("Config_Labels_AttachToFacebook", null, null, CommandType.StoredProcedure);
                Util.Log.WriteOperationStarted(GetType(), "Execute", "Store procedure AttachToTwitter was executed");
                DBUtil.ExecuteNonQuery("Config_Labels_AttachToTwitter", null, null, CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(GetType(), "Execute", ex);
            }
        }
    }
}