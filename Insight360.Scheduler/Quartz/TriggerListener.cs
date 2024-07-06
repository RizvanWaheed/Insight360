using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using Quartz;

namespace Insight360.Scheduling.Quartz
{
    public class TriggerListener : ITriggerListener
    {
        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            Util.Log.WriteOperationStarted(GetType(), "TriggerFired", $"The scheduler called {MethodBase.GetCurrentMethod().Name} for trigger {trigger.Key}");
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            Console.WriteLine("The scheduler called {0} for trigger {1}", MethodBase.GetCurrentMethod().Name, trigger.Key);

            var jobs = context.Scheduler.GetCurrentlyExecutingJobs().Where(j => j.Trigger.Key.Name == "WebProcessorTrigger" && j.Trigger.Key.Group == "WebProcessorGroup").ToList();
            if (jobs.Count > 1)
            {
                foreach (var job in jobs)
                {
                    Util.Log.WriteOperationStarted(GetType(), "VetoJobExecution", job.JobDetail.Key + " job Vetoed.");
                }
                return true;
            }
            var AttachLabeljob = context.Scheduler.GetCurrentlyExecutingJobs().Where(j => j.Trigger.Key.Name == "ProcessLabelsTrigger" && j.Trigger.Key.Group == "ProcessLabelsGroup").ToList();
            if (AttachLabeljob.Count > 1)
            {
                foreach (var job in AttachLabeljob)
                {
                    Util.Log.WriteOperationStarted(GetType(), "VetoJobExecution", job.JobDetail.Key + " job Vetoed.");
                }
                return true;
            }
            return false;
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            Util.Log.WriteOperationStarted(GetType(), "TriggerComplete", $"The scheduler called {MethodBase.GetCurrentMethod().Name} for trigger {trigger.Key}");
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            Util.Log.WriteOperationStarted(GetType(), "TriggerMisfired", $"The scheduler called {MethodBase.GetCurrentMethod().Name} for trigger {trigger.Key}");
        }

        public string Name
        {
            get { return "CurrentTimeTriggerListener"; }
        }
    }
}