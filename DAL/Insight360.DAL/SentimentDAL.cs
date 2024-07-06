using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Insight360.DAL
{
    public static class SentimentDAL
    {
        const int PageSize = 20;
        public static List<BO.DocumentInfo> FetchDocuments(string orgName, int topCount)
        {
            List<BO.DocumentInfo> documents = new List<Insight360.BO.DocumentInfo>();

            DataTable table = DBUtil.FetchRecords(
                " SELECT TOP " + topCount + " Id,text from " + CommonDAL.GetOrgTableName(orgName, CommonDAL.Tables.Documents)
                + " WITH(READPAST) WHERE SentimentProcessingState=" + (int)Util.DataProcessingState.UnProcessed
                + " ORDER BY time desc"
                , null
                , null
                , System.Data.CommandType.Text);

            foreach (DataRow r in table.Rows)
            {
                BO.DocumentInfo doc = new Insight360.BO.DocumentInfo();
                doc.StagingId = (long)r["Id"];
                doc.ContentText = (string)r["text"];
                doc.OrganizationName = orgName;
                documents.Add(doc);
            }
            UpdateProcessingState(documents, Insight360.Util.DataProcessingState.Processing);

            return documents;
        }

        public static void PersistSentiment(string orgName, List<BO.DocumentInfo> documents)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                if (documents[i].SentimentScore != null)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@score", documents[i].SentimentScore));
                    parameters.Add(new SqlParameter("@id", documents[i].StagingId));
					var guid = Guid.NewGuid( ).ToString( );
					DBUtil.ExecuteNonQuery( " UPDATE " + CommonDAL.GetOrgTableName( orgName , CommonDAL.Tables.Documents )
						+ " SET SentimentScore = @score"
						+ " ,SentimentProcessingState=" + ( int ) Util.DataProcessingState.Processed
						+ " ,UpdateTime=GETDATE()"
						+ " WHERE Id = @id" , parameters , null , CommandType.Text , guid );
					DBUtil.CloseConnection( guid );
                }
            }
        }

        public static void UpdateProcessingState(List<BO.DocumentInfo> documents, Util.DataProcessingState state)
        {
            if (documents.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" Id in (");

                for (int i = 0; i < documents.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    sb.Append(documents[i].StagingId);
                }
                sb.Append(")");
                CommonDAL.UpdateProcessingState(CommonDAL.GetOrgTableName(documents[0].OrganizationName, CommonDAL.Tables.Documents)
                    , "SentimentProcessingState", "UpdateTime", sb.ToString(), state);
            }
        }

    }
}
