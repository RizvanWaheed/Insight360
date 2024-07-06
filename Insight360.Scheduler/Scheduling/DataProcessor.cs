using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Insight360.Common;
using System.Net;

namespace Insight360.Scheduling
{
    [DisallowConcurrentExecution]
    public class DataProcessor : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            WebClient http = new WebClient();
            Util.Log.WriteOperationStarted(GetType(), "Execute", AppSettings.SchedulerURL + " website called it self");
            string Result = http.DownloadString(AppSettings.SchedulerURL);
            DAL.DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
            new SchedulingInitiator().Process();

        }
    }
}