using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insight360.BO;
using Insight360.Util;

namespace Insight360.DAL
{
    public static class CommonDAL
    {

        
        public enum StoredProcedures
        {
            AddDocument,
            AddIndex,
            QueueTweet,
            Staging_Twitter_AddStatus,
            Config_AssignDocumentSource,
            Config_FetchDocumentSourceConfiguration,
            Config_FetchOrganizationConfiguration,
            GetTermCountsByDocumentSourceType,
            GetTermCountsByDay,
            GetGraphDataFromCache,
            Staging_Facebook_InitializeUserCollection,
            Staging_Twitter_InitializeDetailsCollection,
            Staging_Facebook_AddPost,
            Staging_Facebook_AddComment,
            Staging_Facebook_MediaUrl_AddComment,
            AddMentionSentimentCategory_Report,
            GetSocialMediaCount,
            GetSocialMediaPosts,
            GetWordCloudChart,
            GetProximityChart, 
            GetSocialMediaSentimentPercentages,
            GetSocialMediaTermCounts,
            VolumeOverTime,
            Staging_Youtube_AddVideo,
            Staging_Blogger_AddPost,
            Web_Processor,
            WebMiner_ProcessDashboard,
            GetFrequency,
            StagingSkyTraxWeb_Insert,
            GetLocations,
            Staging_Instagram_AddPost,
            Staging_Instagram_AddComment,
            GetKeyTerms,
            //GetLocations,
            GetLocationsByDocumentSourceId,
            GetFacebookPages,
            GetFacebookPagesBySearchTermId,
            Staging_WebGoogle_Insert,
            GetGoogleProximity,
            GetGoogleProximityExcludeTerms,
            GetGoogleProximityExactTerms,
            GetGoogleProximityExcludeUrls,
            Staging_Feedly_AddItem,
            GetInstagramToken,
            Config_FetchFacebookProfiles,
            Config_GetFeedLabels
        }
        public enum Tables
        {   DimDate,
            DimBrands,
            DimCategories,
            DimDocument,
            DimDocument_Update,
            DimDocumentSource,
            DimGeography,
            DimLanguage,
            DimMorphology,
            DimOrganization,
            DimSentiment,
            DimTerm,
            DimTime,
            Document,
            ETL_Errors,
            ETL_Max,
            FactDocument,
            FactTerm,
            DimAuthor,
            DimAuthor_Update, 
                  
            Config_Organization,
            Config_DocumentSource,
            Config_DocumentSource_Organization,
            Config_DocumentSource_SearchTerm,
            Config_Organization_SearchTerm,
            Config_DocumentSourceType,
            Config_DocumentSourceType_Indexing,
            Index_Term,
            Documents,
            Authors,
            Terms,
            Morphology,
            SpecialWords,
            Languages,

            Staging_Twitter_Statuses, 
            Staging_Twitter_Users,
            Staging_SurveyComments ,
            Staging_Web ,
            Staging_Facebook_Posts,
            Staging_Facebook_Post_Comments,
            Staging_Facebook_Post_Comment_Likes,
            Staging_Facebook_Post_Likes,
            Staging_Facebook_Users,
            Staging_RSS_Items,
            Staging_RSS_Persons,
            Staging_RSS_Categories,
            Staging_RSS_Links,
            Staging_RSS_Items_SearchTerms,

            Graphs_Cache,
            Graphs_Exclusion,
            Brands,

            Categories,
            SentimentCategories,
            TypeCategories, 
            ReportSentiment,
            ReportCategory,
            ReportMention,
            TopUserByPost,
            Report_KeywordFrequency,
            Report_BrandSentimentFrequency,
            MentionSentimentReports,

            Staging_YouTube_Videos
        }

        static string GetOrgDBName(string orgName)
        {
            return "Insight360_" + orgName;
        }
        
        public static string GetDataMartDBName(string orgName)
        {
            return " DataMart_SocialMedia_" + orgName;
        }
        public static string GetDataMartTableName(string orgName, Tables table)
        {
            return string.Format("{0}.dbo.{1}", GetDataMartDBName(orgName), table.ToString());
        }
        public static string GetOrgStoredProcedureName(string orgName, StoredProcedures sp)
        {
            return string.Format("{0}.dbo.{1}", GetOrgDBName(orgName), sp.ToString());
        }

        public static string GetCommonStoredProcedureName(StoredProcedures sp)
        {
            return "dbo." + sp.ToString();
        }

        public static string GetCommonTableName(Tables table)
        {
            return "dbo." + table.ToString();
        }

        public static string GetOrgTableName(string orgName, Tables table)
        {
            return string.Format("{0}.dbo.{1}", GetOrgDBName(orgName), table.ToString());
        }

        internal static void UpdateProcessingState(string tableOrViewName, string stateColumnName, string updatedTimeColumnName, string condition, Util.DataProcessingState state)
        {
            string query = string.Format(
                            "update {0} set {3} = {1},{4} = GetDate() " +
                            "where {2}",
                            tableOrViewName,
                            (int)state,
                            condition,
                            stateColumnName,
                            updatedTimeColumnName);

			var guid = Guid.NewGuid( ).ToString( );
			DBUtil.ExecuteNonQuery( query , null , null , System.Data.CommandType.Text , guid );
			DBUtil.CloseConnection( guid );
        }
        internal static void UpdateSkyTraxProcessingState(string tableOrViewName, String organizationName, string condition)
        {
 
        }

        internal static void UpdateStagingUpdateTime(Tables table, List<long> ids)
        {
            if (ids.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" Staging_Id in (");

                for (int i = 0; i < ids.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append(ids[i]);
                }
                sb.Append(")");

                Log.WriteOperationCompleted(typeof(CommonDAL), "UpdateStagingUpdateTime", "Updating Ids in query: " + sb);

                

                string query = string.Format(
                            "update {0} set {2} = GetDate() " +
                            "where {1}",
                            GetCommonTableName(table),
                            sb.ToString(),
                            "Staging_UpdateTime");

                var guid = Guid.NewGuid().ToString();
                DBUtil.ExecuteNonQuery(query, null, null, System.Data.CommandType.Text, guid);
                DBUtil.CloseConnection(guid);
            }
        }
    }
}
