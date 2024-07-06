using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;
using System.Xml.Linq;
using System.IO;

namespace Insight360.Common
{
    public static class AppSettings
    {
        public static int GoogleQuota = 0;


        static AppSettings()
        {
            //try
            //{
            //    WebMiner_Common = ConfigurationManager.AppSettings["WebMiner_Common"];
            //    WebMiner_Production = ConfigurationManager.AppSettings["WebMiner_Production"];
            //    Blogger_EnableSearchThread = Convert.ToBoolean(ConfigurationManager.AppSettings["Blogger_EnableSearchThread"]);
            //    GetComments = Convert.ToBoolean(ConfigurationManager.AppSettings["GetComments"]);
            //}
            //catch (Exception ex)
            //{
            //}

        }

        //Connection Strings
        public static string Insight360_ConnectionString { get; } = ConfigurationManager.AppSettings["Insight360_ConnectionString"];

        //Sentiment API
        public static string HelpersAPI_URL { get; } = ConfigurationManager.AppSettings["HelpersAPI_URL"];

        //Blogger Settings
        public static bool Blogger_EnableSearchThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["Blogger_EnableSearchThread"]);
        public static bool GetComments { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["GetComments"]);
        public static string GetAPIKey { get; } = ConfigurationManager.AppSettings["Blogger_APIKey"].ToString();

        // Data Processors Settings

        public static bool EnableProcessingThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableProcessingThread"]);
        public static bool EnableQueuedProcessing { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableQueuedProcessing"]);
        public static bool EnableSingleProcessingThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSingleProcessingThread"]);
        public static bool EnableSentimentThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSentimentThread"]);
        public static bool EnableSentiment { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSentiment"]);
        public static TimeSpan DataProcessor_WaitInterval_NothingFetched { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["DataProcessor_WaitInterval_NothingFetched"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static TimeSpan WaitInterval_DataFetched { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["WaitInterval_DataFetched"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static int DocumentPesistenceQueueLength { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["DocumentPesistenceQueueLength"]);
        public static int IndexPersistenceQueueLength { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["IndexPersistenceQueueLength"]);
        public static int MinimumOrganizationQueued { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["MinimumOrganizationQueued"]);
        public static int MinimumTotalQueued { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["MinimumTotalQueued"]);
        public static int TopCount_Sentiment { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["TopCount_Sentiment"]);

        // Facebook Settings
        public static bool Facebook_EnableSearchThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["Facebook_EnableSearchThread"]);
        public static int TopCount_User { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["TopCount_User"]);
        public static string Facebook_AccessToken_User { get; } = ConfigurationManager.AppSettings["Facebook_AccessToken_User"];
        public static string Facebook_ClientId { get; } = ConfigurationManager.AppSettings["Facebook_ClientId"];
        public static string Facebook_ClientSecret { get; } = ConfigurationManager.AppSettings["Facebook_ClientSecret"];

        public static int TopCount_DataRefresh { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["TopCount_DataRefresh"]);
        public static int UntilHoursCount { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["UntilHoursCount"]);
        public static bool GetCommentsAndLikes { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["GetCommentsAndLikes"]);
        public static TimeSpan WaitInterval_UserCollection { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["WaitInterval_UserCollection"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static TimeSpan WaitInterval_ApiCalls { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["WaitInterval_ApiCalls"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static TimeSpan Facebook_WaitInterval_NothingFetched { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["Facebook_WaitInterval_NothingFetched"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);

        // Twitter Settings
        public static bool Twitter_EnableSearchThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["Twitter_EnableSearchThread"]);
        public static int TopCount_StatusDetails { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["TopCount_StatusDetails"]);
        public static TimeSpan Twitter_WaitInterval_ApiCalls { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["Twitter_WaitInterval_ApiCalls"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static TimeSpan WaitInterval_DetailsCollection { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["WaitInterval_DetailsCollection"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static string TwitterDetails_ConsumerKey { get; } = ConfigurationManager.AppSettings["TwitterDetails_ConsumerKey"];
        public static string TwitterDetails_ConsumerSecret { get; } = ConfigurationManager.AppSettings["TwitterDetails_ConsumerSecret"];
        public static string TwitterDetails_Token { get; } = ConfigurationManager.AppSettings["TwitterDetails_Token"];
        public static string TwitterDetails_TokenSecret { get; } = ConfigurationManager.AppSettings["TwitterDetails_TokenSecret"];
        public static string TwitterSearch_ConsumerKey { get; } = ConfigurationManager.AppSettings["TwitterSearch_ConsumerKey"];
        public static string TwitterSearch_ConsumerSecret { get; } = ConfigurationManager.AppSettings["TwitterSearch_ConsumerSecret"];
        public static string TwitterSearch_Token { get; } = ConfigurationManager.AppSettings["TwitterSearch_Token"];
        public static string TwitterSearch_TokenSecret { get; } = ConfigurationManager.AppSettings["TwitterSearch_TokenSecret"];
        public static string TwitterMentions_ConsumerKey { get; } = ConfigurationManager.AppSettings["TwitterMentions_ConsumerKey"];
        public static string TwitterMentions_ConsumerSecret { get; } = ConfigurationManager.AppSettings["TwitterMentions_ConsumerSecret"];
        public static string TwitterMentions_Token { get; } = ConfigurationManager.AppSettings["TwitterMentions_Token"];
        public static string TwitterMentions_TokenSecret { get; } = ConfigurationManager.AppSettings["TwitterMentions_TokenSecret"];

        // Web Custom Search Settings
        public static string GoogleCustomSearch_SearchEngineId { get; } = ConfigurationManager.AppSettings["GoogleCustomSearch_SearchEngineId"];

        public static string GoogleCustomSearch_APIKey { get; } = ConfigurationManager.AppSettings["GoogleCustomSearch_APIKey"];
        public static int GoogleCustomSearch_MaxAPIResults { get; } = String.IsNullOrEmpty(ConfigurationManager.AppSettings["GoogleCustomSearch_MaxAPIResults"]) ? 10 : Convert.ToInt32(ConfigurationManager.AppSettings["GoogleCustomSearch_MaxAPIResults"]);
        public static bool DawnNews_GetComments { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["DawnNews_GetComments"]);
        
        // Youtube Settings
        public static bool Youtube_EnableSearchThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["Youtube_EnableSearchThread"]);
        public static bool Youtube_EnableDataRefreshThread { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["Youtube_EnableDataRefreshThread"]);
        public static bool Youtube_GetComments { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["Youtube_GetComments"]);
        public static int Youtube_TopCount_DataRefresh { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["Youtube_TopCount_DataRefresh"]);
        public static int Youtube_MaxData { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["Youtube_MaxData"]);
        public static int Youtube_PageSize { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["Youtube_PageSize"]);
        public static TimeSpan Youtube_WaitInterval_ApiCalls { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["Youtube_WaitInterval_ApiCalls"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static TimeSpan Youtube_WaitInterval_NothingFetched { get; } = TimeSpan.ParseExact(ConfigurationManager.AppSettings["Youtube_WaitInterval_NothingFetched"], "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
        public static string Youtube_APIKey { get; } = ConfigurationManager.AppSettings["Youtube_APIKey"];
        public static string Youtube_ProdKey { get; } = ConfigurationManager.AppSettings["Youtube_ProdKey"];
        public static string Youtube_AppName { get; } = ConfigurationManager.AppSettings["Youtube_AppName"];
        public static string Youtube_FetchTime { get; } = ConfigurationManager.AppSettings["Youtube_FetchTime"];
        public static string Youtube_UserName { get; } = ConfigurationManager.AppSettings["Youtube_UserName"];
        public static string Youtube_Password { get; } = ConfigurationManager.AppSettings["Youtube_Password"];

        // Scheduler Settings
        public static string DataMiningInterval { get; } = ConfigurationManager.AppSettings["DataMiningInterval"];
        public static string SchedulerURL { get; } = ConfigurationManager.AppSettings["Scheduler_URL"];

        // SMTP Settings
        public static string SmtpUserName { get; } = ConfigurationManager.AppSettings["SmtpUserName"];
        public static string SmtpPassword { get; } = ConfigurationManager.AppSettings["SmtpPassword"];
        public static string SmtpHost { get; } = ConfigurationManager.AppSettings["SmtpHost"];
        public static int SmtpPort { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
        public static string EmailAlertRecipients { get; } = ConfigurationManager.AppSettings["EmailAlertRecipients"];

        // Log4net Settings
        public static string LogFileSize { get; } = ConfigurationManager.AppSettings["LogFileSize"];

        private static string GetAppSettings(string key)
        {
            string settingConfig = "AppSettings.config";
            if (File.Exists(settingConfig))
            {
                var xdoc = XDocument.Load(settingConfig);
                var properties = xdoc.Element("appSettings").Elements("add");
                foreach (var property in properties)
                {
                    if (property.Attribute("key").Value == key)
                        return property.Attribute("value").Value;
                }
            }
            return "";
        }
    }
}

