using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Insight360.BO;
using System.Data.SqlClient;
using System.Configuration;

namespace Insight360.DAL
{
    public static class IndexDAL
    {
        public static Hashtable getSpecialWordsHash()
        {
            Hashtable specialWords = new Hashtable();


            string query = "SELECT id,text,partOfSpeech FROM " + CommonDAL.GetCommonTableName(CommonDAL.Tables.SpecialWords);

            DataTable specialWordsTable = DBUtil.FetchRecords(query, null, null, CommandType.Text);

            if (specialWordsTable != null)
            {
                foreach (DataRow row in specialWordsTable.Rows)
                {
                    SpecialWord word = new SpecialWord();
                    word.Id = Int64.Parse(row["id"].ToString());
                    word.Text = row["text"].ToString().ToLower();
                    word.PartOfSpeech = row["partOfSpeech"].ToString();

                    specialWords.Add(word.Text, word);
                }
            }

            return specialWords;
        }



        public static long PersistDocument(DocumentInfo document, bool webIndex, Dictionary<string, DataTable> delayedPersistenceTable, string guid)
        {
            long documentID = -1;
            string query = CommonDAL.GetOrgStoredProcedureName(document.OrganizationName,
                    CommonDAL.StoredProcedures.AddDocument);
            if (webIndex)
                query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.AddDocument);

            document.ContentUniqueId = document.ContentId;
            if (document.ContentId.Length > 300)
                document.ContentUniqueId = Util.Helpers.CalculateSha256Hash(document.ContentId);
            document.AuthorUniqueId = document.AuthorId;
            if (!string.IsNullOrEmpty(document.AuthorId) && document.AuthorId.Length > 300)
                document.AuthorUniqueId = Util.Helpers.CalculateSha256Hash(document.AuthorId);
            string parentContentUniqueId = document.ParentContentId;
            if (!string.IsNullOrEmpty(document.ParentContentId) && document.ParentContentId.Length > 300)
                parentContentUniqueId = Util.Helpers.CalculateSha256Hash(document.ParentContentId);
            List<SqlParameter> parameters = new List<SqlParameter>();
            Util.Log.WriteOperationStarted(typeof(IndexDAL), "Before  Parameters Added", " \n");
            parameters.Add(new SqlParameter("@DocumentSourceType_Id", document.DocumentSourceTypeId));
            parameters.Add(new SqlParameter("@DocumentSource_Id", document.DocumentSourceId));
            parameters.Add(new SqlParameter("@Content_Uri", document.ContentUri));
            parameters.Add(new SqlParameter("@Content_Text", document.ContentText));
            parameters.Add(new SqlParameter("@Content_Time", document.ContentTime));
            parameters.Add(new SqlParameter("@Content_UniqueId", document.ContentUniqueId));
            parameters.Add(new SqlParameter("@Content_Id", document.ContentId));
            parameters.Add(new SqlParameter("@SearchTerm_Id", document.SearchTerm_Id));
            parameters.Add(new SqlParameter("@Author_Name", document.AuthorName));
            parameters.Add(new SqlParameter("@Author_Uri", document.AuthorUri));
            parameters.Add(new SqlParameter("@Author_Uri_Image", document.AuthorImageUri));
            parameters.Add(new SqlParameter("@Author_UniqueId", document.AuthorUniqueId));
            parameters.Add(new SqlParameter("@Author_Id", document.AuthorId));
            parameters.Add(new SqlParameter("@Sentiment_Score", document.SentimentScore));
            parameters.Add(new SqlParameter("@Staging_Id", document.StagingId));
            parameters.Add(new SqlParameter("@Followup", document.Followup));
            parameters.Add(new SqlParameter("@Language", document.Language));
            parameters.Add(new SqlParameter("@CommentsCount", document.CommentsCount));
            parameters.Add(new SqlParameter("@LikesCount", document.LikesCount));
            parameters.Add(new SqlParameter("@UniqueCommentersCount", document.UniqueCommentersCount));
            parameters.Add(new SqlParameter("@Parent_Staging_Id", document.ParentStagingId));
            parameters.Add(new SqlParameter("@Parent_Content_Id", document.ParentContentId));
            parameters.Add(new SqlParameter("@Parent_Content_UniqueId", parentContentUniqueId));
            parameters.Add(new SqlParameter("@Parent_DocumentSource_Id", document.ParentStagingDocumentSourceId));
            parameters.Add(new SqlParameter("@ViewsCount", document.ViewsCount));
            parameters.Add(new SqlParameter("@SharesCount", document.SharesCount));
            parameters.Add(new SqlParameter("@Author_Age", document.AuthorAge));
            parameters.Add(new SqlParameter("@Author_Gender", document.AuthorGender));
            parameters.Add(new SqlParameter("@Author_Location", document.AuthorLocation));
            parameters.Add(new SqlParameter("@Timezone", document.AuthorTimeZone));
            parameters.Add(new SqlParameter("@Followers", document.Followers));
            parameters.Add(new SqlParameter("@Following", document.Following));
            parameters.Add(new SqlParameter("@Type", document.Type));
            parameters.Add(new SqlParameter("@To_Author_Name", document.ToAuthorName));
            parameters.Add(new SqlParameter("@To_Author_Uri", document.ToAuthorUri));
            parameters.Add(new SqlParameter("@To_Author_Uri_Image", document.ToAuthorImageUri));
            parameters.Add(new SqlParameter("@To_Author_UniqueId", document.ToAuthorUniqueId));
            parameters.Add(new SqlParameter("@To_Author_Id", document.ToAuthorId));
            parameters.Add(new SqlParameter("@To_Author_Age", document.ToAuthorAge));
            parameters.Add(new SqlParameter("@To_Author_Gender", document.ToAuthorGender));
            parameters.Add(new SqlParameter("@To_Author_Location", document.ToAuthorLocation));
            parameters.Add(new SqlParameter("@To_Timezone", document.ToAuthorTimeZone));
            parameters.Add(new SqlParameter("@To_Followers", document.ToFollowers));
            parameters.Add(new SqlParameter("@To_Following", document.ToFollowing));

            Util.Log.WriteOperationStarted(typeof(IndexDAL), "After Parameters Added"," \n");

            try
            {
                if (delayedPersistenceTable == null)
                {
                    Util.Log.WriteOperationStarted(typeof(IndexDAL), " Before Executing Scaler", " \n");
                    documentID = (long)DBUtil.ExecuteScalar(query, parameters, null, CommandType.StoredProcedure);
                    Util.Log.WriteOperationStarted(typeof(IndexDAL), " After Executing Scaler", " \n");
                }
                else
                {
                    Util.Log.WriteOperationStarted(typeof(IndexDAL), " Before Creating Row", " \n");
                    var row = delayedPersistenceTable[document.OrganizationName].NewRow();
                    row[0] = document.DocumentSourceTypeId;
                    row[1] = document.DocumentSourceId;
                    row[2] = document.StagingId;
                    row[3] = document.ContentId;
                    row[4] = document.ContentUniqueId;
                    row[5] = document.ContentUri;
                    row[6] = document.ContentText;
                    row[7] = document.Followup;
                    row[8] = document.ContentTime;
                    row[9] = document.AuthorUniqueId;
                    row[10] = document.AuthorId;
                    row[11] = document.AuthorName;
                    row[12] = document.AuthorUri;
                    row[13] = document.AuthorImageUri;
                    row[14] = document.SentimentScore == null ? (object)DBNull.Value : document.SentimentScore;
                    row[15] = document.Language;
                    row[16] = document.CommentsCount;
                    row[17] = document.LikesCount;
                    row[18] = document.UniqueCommentersCount;
                    row[19] = document.ParentStagingId == null ? (object)DBNull.Value : document.ParentStagingId;
                    row[20] = document.ParentContentId == null ? (object)DBNull.Value : document.ParentContentId;
                    row[21] = parentContentUniqueId;
                    row[22] = document.ParentStagingDocumentSourceId == null ? (object)DBNull.Value : document.ParentStagingDocumentSourceId;
                    row[23] = guid;
                    row[24] = document.ViewsCount;
                    row[25] = document.SharesCount;
                    row[26] = document.AuthorAge == null ? (object)DBNull.Value : document.AuthorAge;
                    row[27] = document.AuthorGender == null ? (object)DBNull.Value : document.AuthorGender;
                    row[28] = document.AuthorLocation == null ? (object)DBNull.Value : document.AuthorLocation;
                    row[29] = document.AuthorTimeZone == null ? (object)DBNull.Value : document.AuthorTimeZone;
                    row[30] = document.Followers == null ? (object)DBNull.Value : document.Followers;
                    row[31] = document.Following == null ? (object)DBNull.Value : document.Following;
                    row[32] = document.Type == null ? (object)DBNull.Value : document.Type;
                    row[33] = document.SearchTerm_Id;
                    
                    row[34] = document.ToAuthorUniqueId;
                    row[35] = document.ToAuthorId;
                    row[36] = document.ToAuthorName;
                    row[37] = document.ToAuthorUri;
                    row[38] = document.ToAuthorImageUri;
                    row[39] = document.ToAuthorAge == null ? (object)DBNull.Value : document.ToAuthorAge;
                    row[40] = document.ToAuthorGender == null ? (object)DBNull.Value : document.ToAuthorGender;
                    row[40] = document.ToAuthorLocation == null ? (object)DBNull.Value : document.ToAuthorLocation;
                    row[42] = document.ToAuthorTimeZone == null ? (object)DBNull.Value : document.ToAuthorTimeZone;
                    row[43] = document.ToFollowers == null ? (object)DBNull.Value : document.ToFollowers;
                    row[44] = document.ToFollowing == null ? (object)DBNull.Value : document.ToFollowing;
                    
                    Util.Log.WriteOperationStarted(typeof(IndexDAL), " After Creating Row", " \n");
                    delayedPersistenceTable[document.OrganizationName].Rows.Add(row);
                    Util.Log.WriteOperationStarted(typeof(IndexDAL), " After Adding Row", " \n");
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(IndexDAL), "PersistDocument", ex);
            }

            return documentID;
        }

        public static bool PersistIndex(DocumentInfo document, long documentID, List<string> terms, bool webIndex, Dictionary<string, DataTable> delayedPersistenceTable, string guid)
        {
            string query = CommonDAL.GetOrgStoredProcedureName(document.OrganizationName,
                    CommonDAL.StoredProcedures.AddIndex);

            if (webIndex)
                query = CommonDAL.GetCommonStoredProcedureName(CommonDAL.StoredProcedures.AddIndex);

            DataTable termsDataTable = new DataTable();
            termsDataTable.Columns.Add(new DataColumn("term", typeof(String)));
            termsDataTable.Columns.Add(new DataColumn("position", typeof(int)));

            int i = 1;
            foreach (string term in terms)
            {
                termsDataTable.Rows.Add(term, i++);
            }
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@Document_Id", documentID));
            SqlParameter tvp = new SqlParameter("@Terms", termsDataTable);
            tvp.SqlDbType = SqlDbType.Structured;
            parameters.Add(tvp);
            parameters.Add(new SqlParameter("@Document_Time", document.ContentTime));
            parameters.Add(new SqlParameter("@Document_TermCount", terms.Count));
            parameters.Add(new SqlParameter("@DocumentSource_Id", document.DocumentSourceId));
            parameters.Add(new SqlParameter("@DocumentSourceType_Id", document.DocumentSourceTypeId));

            if (delayedPersistenceTable == null)
            {
                var result = DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.StoredProcedure, guid);
                DBUtil.CloseConnection(guid);
                return result;
            }
            else
            {
                i = 1;
                foreach (string term in terms)
                {
                    var row = delayedPersistenceTable[document.OrganizationName].NewRow();

                    row[0] = documentID;
                    row[1] = term;
                    row[2] = i++;
                    row[3] = document.ContentTime;
                    row[4] = terms.Count;
                    row[5] = document.DocumentSourceId;
                    row[6] = document.DocumentSourceTypeId;
                    row[7] = document.ContentUniqueId;
                    delayedPersistenceTable[document.OrganizationName].Rows.Add(row);
                }

                return true;
            }
        }

        public static void PersistTimes(string stagingType, long stagingId, long sentimentTime, long documentTime, long parseTime, long indexTime, long stuckTime)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@staging_type", stagingType));
            parameters.Add(new SqlParameter("@staging_id", stagingId));
            parameters.Add(new SqlParameter("@senti", sentimentTime));
            parameters.Add(new SqlParameter("@doc", documentTime));
            parameters.Add(new SqlParameter("@index", indexTime));
            parameters.Add(new SqlParameter("@parse", parseTime));
            parameters.Add(new SqlParameter("@stuck", stuckTime));
            var guid = Guid.NewGuid().ToString();
            DAL.DBUtil.ExecuteNonQuery("INSERT INTO test(staging_type,staging_id,senti,doc,[index],parse,stuck,inserttime)"
                + "VALUES (@staging_type,@staging_id,@senti,@doc,@index,@parse,@stuck,GETDATE())", parameters, null, CommandType.Text, guid);
            DBUtil.CloseConnection(guid);
        }

        public static void PersistData(Dictionary<string, DataTable> PersistDocumentQueue, Dictionary<string, DataTable> PersistIndexQueue, List<DocumentSourceTypeIndexingInfo> viewName, Util.DataPersistenceMode persistenceMode)
        {
            Util.Log.WriteOperationStarted(typeof(IndexDAL), "PersistData", viewName[0].ViewName);
            //			using( var scope = new System.Transactions.TransactionScope( ) )


            {
                foreach (var key in PersistDocumentQueue.Keys)
                {

                    Util.Log.WriteOperationStarted(typeof(IndexDAL), "Total Document Count going = ", PersistDocumentQueue[key].Rows.Count + " \n");
                    string query = "use Insight360_" + key + "; exec " + CommonDAL.GetOrgStoredProcedureName(key,
                            CommonDAL.StoredProcedures.AddDocument) + "sMultiple @documents";
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    SqlParameter tvp = new SqlParameter("@documents", PersistDocumentQueue[key]);
                    tvp.SqlDbType = SqlDbType.Structured;
                    tvp.TypeName = "dbo.DocumentsTableType";
                    parameters.Add(tvp);

                    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Insight360ClientDBTemplateConnection"].ConnectionString.Replace("Insight360_", "Insight360_" + key)))
                    {
                        connection.Open();
                        ////		using( var trx = connection.BeginTransaction( ) )
                        //		{
                        using (var command = new SqlCommand(query, connection))
                        {
                            try
                            {
                                command.CommandTimeout = 0;
                                //	command.Transaction = trx;
                                command.Parameters.Add(parameters[0]);
                                Util.Log.WriteOperationStarted(typeof(IndexDAL), "PersistData-AddDocuments_Multiple", viewName[0].ViewName);
                                using (var reader = command.ExecuteReader())
                                {
                                    Util.Log.WriteOperationCompleted(typeof(IndexDAL), "PersistData-AddDocuments_Multiple", viewName[0].ViewName);
                                    var table = new DataTable("results");
                                    table.Load(reader);

                                    Util.Log.WriteOperationStarted(typeof(IndexDAL), " Total Document Count returning = \n ", table.Rows.Count + " \n");


                                    if (PersistIndexQueue.ContainsKey(key))
                                    {

                                        for (var i = 0; i < table.Rows.Count; ++i)
                                        {
                                            var rows = PersistIndexQueue[key].Select("TempIdentifier='" + table.Rows[i][0] + "'");
                                            if (rows != null)
                                            {
                                                for (var j = 0; j < rows.Length; ++j)
                                                {
                                                    rows[j][0] = table.Rows[i][1];
                                                }
                                            }

                                        }

                      

                                    }
                                }



                                query = "use Insight360_" + key + "; exec " + CommonDAL.GetOrgStoredProcedureName(key,
                        CommonDAL.StoredProcedures.AddIndex) + "_Multiple @documentsterms";
                                command.CommandText = query;
                                command.Parameters.Clear();

                                tvp = new SqlParameter("@documentsterms", PersistIndexQueue[key]);
                                tvp.SqlDbType = SqlDbType.Structured;
                                tvp.TypeName = "dbo.DocumentTermsTableType";
                                command.Parameters.Add(tvp);

                                Util.Log.WriteOperationStarted(typeof(IndexDAL), "PersistData-AddIndex_Multiple", viewName[0].ViewName);
                                using (var reader = command.ExecuteReader())
                                {                                    
                                    var table = new DataTable("results");
                                    table.Load(reader);
                                }
                                Util.Log.WriteOperationCompleted(typeof(IndexDAL), "PersistData-AddIndex_Multiple", viewName[0].ViewName);

                                //if( persistenceMode == Insight360.Util.DataPersistenceMode.SingleDataSource )
                                //{
                                //    command.CommandText = "update WebMiner_Common.dbo." + viewName [ 0 ].ViewName + Environment.NewLine
                                //        + "set Staging_ProcessingState = " + ( int ) Util.DataProcessingState.Processed + ", Staging_UpdateTime = getdate( ) " + Environment.NewLine
                                //        + " where staging_id in ( " + string.Join( ", " ,
                                //        ( from record in PersistDocumentQueue.Keys
                                //          from row in PersistDocumentQueue [ record ].AsEnumerable( )
                                //          select row.Field<long>( 2 ).ToString( ) ).ToArray( ) ) + " )";

                                //    var obj = command.ExecuteNonQuery( );
                                //}
                                //else
                                //{
                                //    for( var i = 0 ; i < viewName.Count ; ++i )
                                //    {
                                //        command.CommandText = "update WebMiner_Common.dbo." + viewName [ i ].ViewName + Environment.NewLine
                                //            + "set Staging_ProcessingState = " + ( int ) Util.DataProcessingState.Processed + ", Staging_UpdateTime = getdate( ) " + Environment.NewLine
                                //            + " where staging_id in ( " + string.Join( ", " ,
                                //            ( from record in PersistDocumentQueue.Keys
                                //              from row in PersistDocumentQueue [ record ].AsEnumerable( )
                                //              where row.Field<int>( 0 ) == viewName [ i ].DocumentSourceTypeId
                                //              select row.Field<long>( 2 ).ToString( ) ).ToArray( ) ) + " )";

                                //        var obj = command.ExecuteNonQuery( );
                                //    }
                                //}

                                //       trx.Commit();
                            }
                            catch (Exception ex)
                            {
                                Util.Log.WriteDbException(typeof(IndexDAL), "PersistData", ex, command);
                                //		trx.Rollback( );
                            }
                        }
                        //}
                    }
                }
            }

            var keys = PersistDocumentQueue.Keys.ToArray();
            for (var i = 0; i < keys.Length; ++i)
                PersistDocumentQueue[keys[i]].Clear();

            keys = PersistIndexQueue.Keys.ToArray();
            for (var i = 0; i < keys.Length; ++i)
                PersistIndexQueue[keys[i]].Clear();

            Util.Log.WriteOperationCompleted(typeof(IndexDAL), "PersistData", viewName[0].ViewName);
        }




    }


}
