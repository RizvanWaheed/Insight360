using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Insight360.Scheduling.Quartz;
using System.Reflection;
using System.Diagnostics;

namespace Insight360.Scheduler
{
    public class Global : HttpApplication
    {
        readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            JobScheduler.Start();
        }

        void Application_End()
        {
            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

            if (runtime == null)
                return;

            string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

            string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

            if (!EventLog.SourceExists(".NET Runtime"))
            {
                EventLog.CreateEventSource(".NET Runtime", "Application");
            }

            EventLog log = new EventLog();
            log.Source = ".NET Runtime";
            log.WriteEntry(String.Format("\r\n\r\n_shutDownMessage={0}\r\n\r\n_shutDownStack={1}",
                                         shutDownMessage,
                                         shutDownStack),
                                         EventLogEntryType.Error);
            logger.Info(String.Format("\r\n\r\n_shutDownMessage={0}\r\n\r\n_shutDownStack={1}",
                                         shutDownMessage,
                                         shutDownStack));

        }
    }
}