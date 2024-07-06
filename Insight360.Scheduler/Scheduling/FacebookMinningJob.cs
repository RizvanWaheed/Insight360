using Insight360.Util;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Insight360.Scheduling
{
    [DisallowConcurrentExecution]
    public class FacebookMinningJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //check if fb limit reached hold to call this scrapper for 20 minutes.
                if (System.IO.File.Exists("/listening/fblimit.txt"))
                {
                    var lines = System.IO.File.ReadAllLines("/listening/fblimit.txt");
                    var limitTime = lines.FirstOrDefault();
                    DateTime fbLimitTime;
                    if (!string.IsNullOrEmpty(limitTime) && DateTime.TryParseExact(limitTime, "dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out fbLimitTime))
                    {
                        if (fbLimitTime < DateTime.Now)
                        {
                            System.IO.File.Delete("/listening/fblimit.txt");
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        System.IO.File.Delete("/listening/fblimit.txt");
                    }
                }
                Facebook.DataCollector dc = new Facebook.DataCollector();
                Log.WriteOperationStarted(this.GetType(), "MineFacebook", "Service for Facebook Started.....");
                var res=dc.SearchThreadOptimize();
            }
            catch (Exception ex)
            {
                Log.WriteException(this.GetType(), "MineFacebook", ex);
            }
        }
    }
}