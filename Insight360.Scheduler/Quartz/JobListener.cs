using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Quartz;
using Insight360.Common;

namespace Insight360.Scheduling.Quartz
{
    public class JobListener : IJobListener
    {
        //readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string Name
        {
            get { return "CurrentTimeListener"; }
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            Util.Log.WriteOperationStarted(GetType(), "JobToBeExecuted", $"Job {context.JobDetail.Key.Name} in group {context.JobDetail.Key.Group} is about to be executed");
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            Util.Log.WriteOperationStarted(GetType(), "JobExecutionVetoed", $"Job {context.JobDetail.Key.Name} in group {context.JobDetail.Key.Group} is Vetoed");
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Util.Log.WriteOperationStarted(GetType(), "JobWasExecuted", $"Job {context.JobDetail.Key.Name} in group {context.JobDetail.Key.Group} was executed");

            // only run second job if first job was executed successfully
            if (jobException == null)
            {
                // fetching name of the job to be executed sequentially
                string nextJobName = Convert.ToString(context.MergedJobDataMap.GetString("NextJobName"));
                if (!string.IsNullOrEmpty(nextJobName))
                {
                    Util.Log.WriteOperationStarted(GetType(), "JobWasExecuted", "----------------- Next job to be executed :" + nextJobName + " -----------------------");
                    IJobDetail job = null;
                    ITrigger trigger = null;
                    // define a job and tie it to our JobTwo class
                    if (nextJobName == "EmailAlerts") // similarly we can write/handle cases for other jobs as well
                    {
                        job = JobBuilder.Create<EmailAlerts>().WithIdentity("EmailAlerts", "EmailAlertsGroup").Build();

                        trigger = TriggerBuilder.Create()
                            .WithIdentity("EmailAlertsTrigger", "EmailAlertsGroup")
                            .StartNow()
                            .Build();
                    }

                    //if (nextJobName == "JobThree") // similarly we can write/handle cases for other jobs as well
                    //{
                    //    job = JobBuilder.Create<JobThree>()
                    //            .WithIdentity("Job3", "Group1")
                    //            .Build();
                    //    // create a trigger to run the job now 
                    //    trigger = TriggerBuilder.Create()
                    //        .WithIdentity("Trigger3", "Group1")
                    //        .StartNow()
                    //        .Build();
                    //}

                    // finally, schedule the job
                    if (job != null && trigger != null)
                        context.Scheduler.ScheduleJob(job, trigger);
                }
                else
                {
                    Util.Log.WriteOperationStarted(GetType(), "JobWasExecuted", "No job to be executed sequentially");
                }
            }
            else
            {
                Util.Log.WriteOperationCompleted(GetType(), "JobWasExecuted", $"An exception occured while executing job: {context.JobDetail.Key.Name} in group {context.JobDetail.Key.Group} with following details : {jobException.Message}");
            }
        }
    }
}