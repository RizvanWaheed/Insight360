using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Insight360.BO;
using System.Diagnostics;
using System.Data.SqlClient;
using Insight360.Common.BO;

namespace Insight360.DAL
{
    public static class ConfigDAL
    {
        public static List<string> FetchOrganizations()
        {
            string query = "select name from " + CommonDAL.GetCommonTableName(CommonDAL.Tables.Config_Organization)//CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Config_FetchOrganizationConfiguration);
                + " WHERE isenabled=1";
            List<string> list = new List<string>();

            DataTable table = DBUtil.FetchRecords(query, null, null, CommandType.Text);

            foreach (DataRow row in table.Rows)
            {
                if (!list.Contains(row["Name"].ToString()))
                {
                    list.Add(row["Name"].ToString());
                }
            }

            return list;
        }

        public static int GetFrequency(string DocumentSourceType)
        {
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetFrequency);
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@DocumentSourceType_Type", DocumentSourceType));
            var fre = DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);
            return Convert.ToInt32(fre);
        }

        public static List<BO.Location> GetLocations(int searchTermId)
        {
            List<BO.Location> list = new List<BO.Location>();
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetLocations);
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);
            foreach (DataRow row in table.Rows)
            {
                BO.Location loc = new BO.Location();
                loc.Id = Convert.ToInt32(row["Id"]);
                loc.SearchTerm_Id = Convert.ToInt32(row["SearchTerm_Id"]);
                loc.latitude = Convert.ToDecimal(row["latitude"]);
                loc.longitude = Convert.ToDecimal(row["longitude"]);
                loc.name = row["name"].ToString();
                loc.TwitterGranuality = row["TwitterGranuality"].ToString();
                loc.TwitterPlaceId = row["TwitterPlaceId"].ToString();
                list.Add(loc);
            }
            return list;
        }

        public static List<BO.Location> GetLocationsByDocumentSourceId(int id)
        {
            List<BO.Location> list = new List<BO.Location>();
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetLocations);

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@DocumentSourceId", id));

            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                BO.Location loc = new BO.Location();

                loc.Id = Convert.ToInt32(row["Id"]);
                loc.SearchTerm_Id = Convert.ToInt32(row["SearchTerm_Id"]);
                loc.latitude = Convert.ToDecimal(row["latitude"]);
                loc.longitude = Convert.ToDecimal(row["longitude"]);
                loc.name = row["name"].ToString();
                loc.TwitterGranuality = row["TwitterGranuality"].ToString();
                loc.TwitterPlaceId = row["TwitterPlaceId"].ToString();

                list.Add(loc);
            }

            return list;
        }

        public static List<string> GetFacebookPages()
        {
            List<string> list = new List<string>();

            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetFacebookPages);
            DataTable table = DBUtil.FetchRecords(query, null, null, CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                list.Add(row["PageId"].ToString());
            }

            return list;
        }

        public static List<KeyTerm> GetKeyTerms(int searchTermId)
        {
            List<KeyTerm> list = new List<KeyTerm>();
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetKeyTerms);
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);
            foreach (DataRow row in table.Rows)
            {
                KeyTerm term = new KeyTerm();
                term.Id = Convert.ToInt32(row["Id"]);
                term.Name = row["KeyTerm"].ToString();
                list.Add(term);
            }
            return list;
        }

        public static GoogleProximity GetGoogleProximity(int searchTermId)
        {
            GoogleProximity proximity = new GoogleProximity();
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetGoogleProximity);

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SearchTermId", searchTermId));
            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                proximity.id = Convert.ToInt32(row["id"]);
                proximity.SearchTermId = Convert.ToInt32(row["SearchTermId"]);
                proximity.CountryRestrict = row["CountryRestrict"].ToString();
                proximity.IsExactTerms = Convert.ToBoolean(row["IsExactTerms"]);
                proximity.IsExcludeTerms = Convert.ToBoolean(row["IsExcludeTerms"]);
                proximity.Geolocation = row["GeoLocation"].ToString();
                proximity.GoogleHost = row["GoogleHost"].ToString();
                proximity.orTerms = row["orTerms"].ToString();
                proximity.IsSiteSearch = Convert.ToBoolean(row["IsSiteSearch"]);

                if (proximity.IsExactTerms)
                {
                    query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetGoogleProximityExactTerms);

                    parameters.Clear();
                    parameters.Add(new SqlParameter("@proximityId", proximity.id));

                    DataTable exactTermsTable = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

                    foreach (DataRow termRow in exactTermsTable.Rows)
                    {
                        string term = termRow["term"].ToString();
                        proximity.ExactTerms.Add(term);
                    }
                }

                if (proximity.IsExcludeTerms)
                {
                    query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetGoogleProximityExcludeTerms);

                    parameters.Clear();
                    parameters.Add(new SqlParameter("@proximityId", proximity.id));

                    DataTable exactTermsTable = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

                    foreach (DataRow termRow in exactTermsTable.Rows)
                    {
                        string term = termRow["term"].ToString();
                        proximity.ExcludeTerms.Add(term);
                    }
                }

                if (proximity.IsSiteSearch)
                {
                    query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetGoogleProximityExcludeUrls);

                    parameters.Clear();
                    parameters.Add(new SqlParameter("@proximityId", proximity.id));

                    DataTable exactTermsTable = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

                    foreach (DataRow termRow in exactTermsTable.Rows)
                    {
                        string term = termRow["Url"].ToString();
                        proximity.SiteSearch.Add(term);
                    }
                }
            }

            return proximity;
        }

        public static List<DocumentSourceInfo> FetchDocumentSourceConfiguration(string DocumentSourceType)
        {
            List<FeedLabel> feedLabels = new List<FeedLabel>();
            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Config_GetFeedLabels);
            DataTable labelsTable = DBUtil.FetchRecords(query, null, null, CommandType.StoredProcedure);

            foreach (DataRow row in labelsTable.Rows)
            {
                feedLabels.Add(new FeedLabel() { LabelId = Convert.ToInt32(row["LabelId"]), LabelName = row["LabelName"].ToString() });
            }

            query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Config_FetchDocumentSourceConfiguration);

            List<DocumentSourceInfo> list = new List<DocumentSourceInfo>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@DocumentSourceType_Type", DocumentSourceType));

            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                DocumentSourceInfo config = new DocumentSourceInfo();
                config.RowId = Convert.ToInt32(row["Row_Id"]);
                config.DocumentSourceId = Convert.ToInt32(row["DocumentSource_Id"]);
                config.Url = row["DocumentSource_Url"].ToString();
                config.DocumentSourceTypeId = Convert.ToInt32(row["DocumentSourceType_Id"]);
                config.Frequency = Convert.ToInt32(row["Frequency"]);
                config.IsEnabled = Convert.ToInt32(row["IsEnabled"]);
                config.DisplayName = row["DisplayName"].ToString();
                config.ProfileType = (ProfileTypes)(int)row["FBProfileType"];
                config.Domain = row["Domain"].ToString();
                config.FeedLabels = feedLabels;
                config.PageId = row["PageId"].ToString();

                SearchTermInfo searchTerm = new SearchTermInfo();
                bool searchTermValid = !(row["SearchTerm_Term"] is DBNull);

                if (searchTermValid)
                {
                    searchTerm.Id = (int)row["SearchTerm_Id"];
                    searchTerm.LastFetchTime = (DateTime)row["SearchTerm_LastFetch_Time"];
                    searchTerm.UntillFetchTime = (DateTime)row["SearchTerm_UntillTime"];
                    searchTerm.LastFetchId = (string)row["SearchTerm_LastFetch_Id"];
                    searchTerm.LastFetchId2 = (string)row["SearchTerm_LastFetch_Id2"];
                    searchTerm.SearchTerm = (string)row["SearchTerm_Term"];
                    searchTerm.Type = (SearchTermInfo.Types)(int)row["SearchTerm_Type"];
                    searchTerm.IsTermEnabled = Convert.ToInt32(row["IsTermEnabled"]);
                    searchTerm.latitude = Convert.ToDouble(row["latitude"]);
                    searchTerm.longitude = Convert.ToDouble(row["longitude"]);
                }

                config.SearchTerms.Add(searchTerm);
                if (!string.IsNullOrEmpty(config.PageId) && DocumentSourceType.ToLower().Contains("facebook"))
                { list.Add(config); }
                else if (!DocumentSourceType.ToLower().Contains("facebook"))
                { list.Add(config); }

                //if (list.Count == 0)
                //{
                //    config.SearchTerms.Add(searchTerm);
                //    list.Add(config);
                //}
                //else
                //{
                //    DocumentSourceInfo lastConfig = list[list.Count - 1];
                //    if (lastConfig.DocumentSourceId == config.DocumentSourceId)
                //    {
                //        if (searchTermValid)
                //            lastConfig.SearchTerms.Add(searchTerm);
                //    }
                //    else
                //    {
                //        if (searchTermValid)
                //            config.SearchTerms.Add(searchTerm);

                //       // list.Add(config);
                //    }
                //    list.Add(config);
                //}
            }

            if (DocumentSourceType == "Facebook" && false)
            {
                query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.Config_FetchFacebookProfiles);
                DataTable profileTable = DBUtil.FetchRecords(query, null, null, CommandType.StoredProcedure);

                foreach (DataRow row in profileTable.Rows)
                {
                    DocumentSourceInfo profile = new DocumentSourceInfo();

                    profile.DocumentSourceId = 2;
                    profile.Url = row["FacebookUserId"].ToString();
                    profile.Domain = row["AccessToken"].ToString();
                    profile.DocumentSourceTypeId = 0;
                    //profile.Frequency = Convert.ToInt32(row["Frequency"]);
                    profile.DisplayName = row["ScreenName"].ToString();
                    profile.ProfileType = ProfileTypes.FacebookUser;

                    profile.FeedLabels = feedLabels;

                    list.Add(profile);
                }
            }

            return list;
        }

        public static List<ExcludedDocumentSourceInfo> FetchExcludedDocumentSourceConfiguration(int documentSourceTypeId)
        {
            List<ExcludedDocumentSourceInfo> list = new List<ExcludedDocumentSourceInfo>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@DocumentSourceType_Id", documentSourceTypeId));

            DataTable table = DBUtil.FetchRecords("Config_FetchExcludedDocumentSourceConfiguration", parameters, null, CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                ExcludedDocumentSourceInfo config = new ExcludedDocumentSourceInfo();
                config.DocumentSourceTypeId = Convert.ToInt32(row["DocumentSourceType_Id"]);
                config.DisplayName = row["DisplayName"].ToString();
                config.ProfileType = (ProfileTypes)(int)row["ProfileType"];
                config.Domain = row["Domain"].ToString();
                config.Url = row["Url"].ToString().Replace("@", "");
                list.Add(config);
            }

            return list;
        }

        public static List<string> GetFacebookPagesBySearchTermId(int searchTermId)
        {
            List<string> list = new List<string>();

            string query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.GetFacebookPagesBySearchTermId);
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SearchTermId", searchTermId));

            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                list.Add(row["PageId"].ToString());
            }

            return list;
        }

        public static void UpdateLastFetchConfigId(int documentSourceTypeId, int lastFetchId)
        {
            string query = "";
            query = "update Config_DocumentSourceType set LastFetchId = " + lastFetchId + ", LastFetchTime = '" + DateTime.Now + "' where id = " + documentSourceTypeId;
            var guid = Guid.NewGuid().ToString();
            DBUtil.ExecuteNonQuery(query.ToString(), null, null, CommandType.Text, guid);
            DBUtil.CloseConnection(guid);
        }
        public static int GetLastFetchConfigId(int documentSourceTypeId)
        {
            string query = "";
            query = "select LastFetchId from Config_DocumentSourceType where id = " + documentSourceTypeId;

            return Convert.ToInt32(DBUtil.ExecuteScalar(query, null, null, CommandType.Text));
        }

        public static void UpdateDocumentSourceConfiguration(SearchTermInfo updated, int documentSourceId)
        {
            StringBuilder query = new StringBuilder();
            query.Append("UPDATE ");
            query.Append(CommonDAL.GetCommonTableName(CommonDAL.Tables.Config_DocumentSource_SearchTerm));
            query.Append(" SET ");

            List<SqlParameter> parameters = new List<SqlParameter>();
            bool check = false;
            if (updated.LastFetchId != "0")
            {
                query.Append(" LastFetch_Id =@LastFetch_Id");
                parameters.Add(new SqlParameter("@LastFetch_Id", updated.LastFetchId));
                check = true;
            }
            //if (updated.LastFetchId2 != "0")
            //{
            if (check)
                query.Append(",");
            query.Append(" LastFetch_Id2 =@LastFetch_Id2");
            parameters.Add(new SqlParameter("@LastFetch_Id2", updated.LastFetchId2));
            check = true;
            //   }

            if (updated.LastFetchTime.Year > 1900 && updated.LastFetchTime < DateTime.MaxValue)
            {
                if (check)
                    query.Append(",");
                query.Append(" LastFetch_Time =@LastFetch_Time");
                parameters.Add(new SqlParameter("@LastFetch_Time", updated.LastFetchTime));
                check = true;
            }
            if (check)
            {
                //parameters.Add(new SqlParameter("@Organization_Id", organizationId));
                parameters.Add(new SqlParameter("@DocumentSource_Id", documentSourceId));
                parameters.Add(new SqlParameter("@SearchTerm_Id", updated.Id));
                query.Append(" WHERE DocumentSource_Id = @DocumentSource_Id and SearchTerm_Id=@SearchTerm_Id");
                var guid = Guid.NewGuid().ToString();
                DBUtil.ExecuteNonQuery(query.ToString(), parameters, null, CommandType.Text, guid);
                DBUtil.CloseConnection(guid);
            }
        }

        //public static void UpdateDocumentSourceConfiguration(OrganizationInfo updated, int documentSourceId)
        //{
        //    StringBuilder query = new StringBuilder();
        //    query.Append("UPDATE ");
        //    query.Append(CommonDAL.GetCommonTableName(CommonDAL.Tables.Config_DocumentSource_Organization));
        //    query.Append(" SET ");

        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    bool check = false;
        //    if (updated.LastFetchId != "0")
        //    {
        //        query.Append(" LastFetch_Id =@LastFetch_Id");
        //        parameters.Add(new SqlParameter("@LastFetch_Id", updated.LastFetchId));
        //        check = true;
        //    }
        //    if (updated.LastFetchTime.Year > 1900 && updated.LastFetchTime < DateTime.MaxValue)
        //    {
        //        if (check)
        //            query.Append(",");
        //        query.Append(" LastFetch_Time =@LastFetch_Time");
        //        parameters.Add(new SqlParameter("@LastFetch_Time", updated.LastFetchTime));
        //        check = true;
        //    }
        //    if (check)
        //    {
        //        parameters.Add(new SqlParameter("@Organization_Id", updated.Id));
        //        parameters.Add(new SqlParameter("@DocumentSource_Id", documentSourceId));
        //        query.Append(" WHERE Organization_Id =@Organization_Id and DocumentSource_Id = @DocumentSource_Id");
        //        var guid = Guid.NewGuid( ).ToString( );
        //        DBUtil.ExecuteNonQuery( query.ToString( ) , parameters , null , CommandType.Text , guid );
        //        DBUtil.CloseConnection( guid );
        //    }
        //}

        //public static void UpdateDocumentSourceConfiguration(DocumentSourceInfo updated)
        //{
        //    StringBuilder query = new StringBuilder();
        //    query.Append("UPDATE ");
        //    query.Append(CommonDAL.GetCommonTableName(CommonDAL.Tables.Config_DocumentSources));
        //    query.Append(" SET ");

        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    //bool check = false;
        //    //if (updated.LastFetchId != "0")
        //    //{
        //    //    query.Append(" LastFetch_Id =@LastFetch_Id");
        //    //    parameters.Add(new SqlParameter("@LastFetch_Id", updated.LastFetchId));
        //    //    check = true;
        //    //}
        //    //if (updated.LastFetchTime.Year > 1900 && updated.LastFetchTime < DateTime.MaxValue)
        //    //{
        //    //    if (check)
        //    //        query.Append(",");
        //    //    query.Append(" LastFetch_Time =@LastFetch_Time");
        //    //    parameters.Add(new SqlParameter("@LastFetch_Time", updated.LastFetchTime));
        //    //    check = true;
        //    //}
        //    //if (check)
        //    {
        //        parameters.Add(new SqlParameter("@DocumentSource_Id", updated.DocumentSourceId));
        //        query.Append(" WHERE Id = @DocumentSource_Id");
        //        var guid = Guid.NewGuid( ).ToString( );
        //        DBUtil.ExecuteNonQuery( query.ToString( ) , parameters , null , CommandType.Text , guid );
        //        DBUtil.CloseConnection(guid);
        //    }
        //}



        public static List<DocumentSourceTypeIndexingInfo> FetchDocumentSourceType(int? documentSourceTypeId)
        {
            string query = "SELECT dsti.DocumentSourceType_Id,dsti.ViewName,dst.Type,dsti.IsEnabled,dsti.FindSearchTerms,dsti.TopCount,dsti.WhereCondition,dsti.OrderBy FROM "
                + CommonDAL.GetCommonTableName(CommonDAL.Tables.Config_DocumentSourceType)
                + " dst inner join "
                + CommonDAL.GetCommonTableName(CommonDAL.Tables.Config_DocumentSourceType_Indexing)
                + " dsti on dsti.DocumentSourceType_Id=dst.Id"
                ;

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (documentSourceTypeId != null)
            {
                query += " WHERE dst.Id = @Id";
                parameters.Add(new SqlParameter("@Id", documentSourceTypeId));
            }
            List<DocumentSourceTypeIndexingInfo> list = new List<DocumentSourceTypeIndexingInfo>();

            DataTable table = DBUtil.FetchRecords(query, parameters, null, CommandType.Text);

            foreach (DataRow row in table.Rows)
            {
                DocumentSourceTypeIndexingInfo config = new DocumentSourceTypeIndexingInfo()
                {
                    DocumentSourceTypeId = (int)row["DocumentSourceType_Id"],
                    Type = row["Type"].ToString(),
                    ViewName = row["ViewName"].ToString(),
                    //config.DisplayName = row["DisplayName"].ToString();
                    //config.ImageUrl = row["ImageUrl"].ToString();
                    //config.QueryFetch = row["Query"].ToString();
                    //config.TableName = row["TableName"].ToString();
                    FindSearchTerms = Convert.ToBoolean(row["FindSearchTerms"]),
                    Enabled = Convert.ToBoolean(row["IsEnabled"]),
                    //config.IndexingEnabled = Convert.ToBoolean(row["IndexingEnabled"]);
                    TopCount = (int)row["TopCount"],
                    WhereCondition = row["WhereCondition"].ToString(),
                    OrderBy = row["OrderBy"].ToString(),

                };
                list.Add(config);
            }
            return list;
        }

        public static string GetInstagramToken()
        {
            DataTable table = DBUtil.FetchRecords("GetInstagramToken", null, null, CommandType.StoredProcedure);
            string code = "not found";
            if (table.Rows.Count > 0)
            {
                DataRow row = table.Rows[0];
                code = row["Token"].ToString();
            }
            return code;
        }
        public static List<string> GetInstagramUsers()
        {
            DataTable table = DBUtil.FetchRecords("GetInstgramUsers", null, null, CommandType.StoredProcedure);
            string code = "not found";
            List<string> users = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                users.Add(row["InstaId"].ToString());
            }

            return users;
        }

        public static int GetSearchTermIdForInstagramUser()
        {
            string query = "SELECT Id FROM [WebMiner_Common].[dbo].[Config_SearchTerm] WHERE Term = 'InstaUserPosts'";
            DataTable table = DBUtil.FetchRecords(query, null, null, CommandType.Text);

            DataRow row = table.Rows[0];
            int id = (int)row["Id"];
            return id;
        }

        public static bool isFBChannelExcluded(string channelId)
        {
            string query = "select id from Config_FacebookExcludeUrl where isEnabled = 1 and pageid = '" + channelId + "'";
            int cnt = Convert.ToInt32(DBUtil.ExecuteScalar(query, null, null, CommandType.Text));

            if (cnt > 0)
                return true;
            else
                return false;
        }


    }
}
