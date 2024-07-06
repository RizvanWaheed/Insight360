using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Insight360.Common;

namespace Insight360.Scheduling.Quartz
{
    public class JobScheduler
    {
        //readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void Start()
        {
            Util.Log.WriteOperationStarted(typeof(JobScheduler), "Start", "*** Scheduling Started ***");
            Insight360.DAL.DBUtil.ConnectionString = AppSettings.Insight360_ConnectionString;
          //  new SchedulingInitiator().MineWeb();

            string dataMiningInterval = AppSettings.DataMiningInterval;
            if (!string.IsNullOrEmpty(dataMiningInterval))
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.Start();

                IJobDetail webJob = JobBuilder.Create<SchedulingInitiator>().WithIdentity("Web", "WebGroup").Build();

                ITrigger webTrigger = TriggerBuilder.Create()
                    .WithIdentity("WebTrigger", "WebGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(Convert.ToInt32(dataMiningInterval))
                        .RepeatForever()).Build();

                var webListener = new JobListener();
                scheduler.ListenerManager.AddJobListener(webListener, GroupMatcher<JobKey>.GroupEquals("WebGroup"));

                var webTriggerListener = new TriggerListener();
                scheduler.ListenerManager.AddTriggerListener(webTriggerListener);

                scheduler.ScheduleJob(webJob, webTrigger);

                IJobDetail webFBJob = JobBuilder.Create<FacebookMinningJob>().WithIdentity("WebFB", "WebFBGroup").Build();

                ITrigger webFBTrigger = TriggerBuilder.Create()
                        .WithIdentity("WebFBTrigger", "WebFBGroup")
                        .StartNow()
                        .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(5)
                        .RepeatForever()).Build();
                scheduler.ScheduleJob(webFBJob, webFBTrigger);


                IJobDetail job = JobBuilder.Create<DataProcessor>().WithIdentity("WebProcessor", "WebProcessorGroup").Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("WebProcessorTrigger", "WebProcessorGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(Convert.ToInt32(dataMiningInterval))
                        .RepeatForever()).UsingJobData("NextJobName", "EmailAlerts")
                    .Build();

                var currentTimeListener = new JobListener();
                scheduler.ListenerManager.AddJobListener(currentTimeListener, GroupMatcher<JobKey>.GroupEquals("WebProcessorGroup"));

                var currentTimeTriggerListener = new TriggerListener();
                scheduler.ListenerManager.AddTriggerListener(currentTimeTriggerListener);

                scheduler.ScheduleJob(job, trigger);

                //Email Job
                IJobDetail emailsJob = JobBuilder.Create<EmailAlerts>().WithIdentity("Emails", "EmailsGroup").Build();

                ITrigger emailsTrigger = TriggerBuilder.Create()
                    .WithIdentity("EmailsTrigger", "EmailsGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(Convert.ToInt32(dataMiningInterval) + 1)
                        .RepeatForever()).Build();

                var emailsJobListener = new JobListener();
                scheduler.ListenerManager.AddJobListener(emailsJobListener, GroupMatcher<JobKey>.GroupEquals("EmailsGroup"));

                var emailsTriggerListener = new TriggerListener();
                scheduler.ListenerManager.AddTriggerListener(emailsTriggerListener);

                scheduler.ScheduleJob(emailsJob, emailsTrigger);

                //Label Creation Job
                IJobDetail processLabelsJob = JobBuilder.Create<AttachLabels>().WithIdentity("ProcessLabels", "ProcessLabelsGroup").Build();

                ITrigger processLabelsTrigger = TriggerBuilder.Create()
                    .WithIdentity("ProcessLabelsTrigger", "ProcessLabelsGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(Convert.ToInt32(dataMiningInterval) + 1)
                        .RepeatForever()).Build();

                var processLabelsJobListener = new JobListener();
                scheduler.ListenerManager.AddJobListener(processLabelsJobListener, GroupMatcher<JobKey>.GroupEquals("ProcessLabelsGroup"));

                var processLabelsTriggerListener = new TriggerListener();
                scheduler.ListenerManager.AddTriggerListener(processLabelsTriggerListener);

                scheduler.ScheduleJob(processLabelsJob, processLabelsTrigger);

                //Post Daywise Engagement 
                IJobDetail daywiseEngagementJob = JobBuilder.Create<PostDaywiseEngagement>().WithIdentity("DaywiseEngagement", "DaywiseEngagementGroup").Build();

                ITrigger daywiseEngagementTrigger = TriggerBuilder.Create()
                    .WithIdentity("DaywiseEngagementTrigger", "DaywiseEngagementGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(Convert.ToInt32(dataMiningInterval) + 1)
                        .RepeatForever()).Build();

                var daywiseEngagementJobListener = new JobListener();
                scheduler.ListenerManager.AddJobListener(daywiseEngagementJobListener, GroupMatcher<JobKey>.GroupEquals("DaywiseEngagementGroup"));

                var daywiseEngagementTriggerListener = new TriggerListener();
                scheduler.ListenerManager.AddTriggerListener(daywiseEngagementTriggerListener);

                scheduler.ScheduleJob(daywiseEngagementJob, daywiseEngagementTrigger);

                //Update Document Source for Search Terms (Facebook)
                IJobDetail docSourceJob = JobBuilder.Create<UpdateDocSource_SearchTerm>().WithIdentity("DocSource", "DocSourceGroup").Build();

                ITrigger docSourceTrigger = TriggerBuilder.Create()
                    .WithIdentity("DocSourceTrigger", "DocSourceGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(Convert.ToInt32(dataMiningInterval) + 1)
                        .RepeatForever()).Build();

                var docSourceJobListener = new JobListener();
                scheduler.ListenerManager.AddJobListener(docSourceJobListener, GroupMatcher<JobKey>.GroupEquals("DocSourceGroup"));

                var docSourceTriggerListener = new TriggerListener();
                scheduler.ListenerManager.AddTriggerListener(docSourceTriggerListener);

                scheduler.ScheduleJob(docSourceJob, docSourceTrigger);

            }
        }
    }
}