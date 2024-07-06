using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Insight360.DAL
{
    public static class EmailAlertsDAL
    {
        public static DataSet GetPostsForEmailAlerts()
        {
            string query = "EmailAlerts_GetPostsForEmail";
            return DBUtil.DSFetchRecords(query, null, null, CommandType.StoredProcedure);
        }

        public static void UpdatePostsSentForEmailAlerts(DataTable dtPosts, string type, int interval = 0)
        {
            string query = "EmailAlerts_UpdatePostsSentForEmail";
            DataTable DocumentIdsDataTable = new DataTable();
            DocumentIdsDataTable.Columns.Add(new DataColumn("DocumentId", typeof(int)));

            foreach (DataRow post in dtPosts.Rows)
            {
                var DocumentIdsRow = DocumentIdsDataTable.NewRow();
                DocumentIdsRow["DocumentId"] = Convert.ToInt32(post["Id"]);
                DocumentIdsDataTable.Rows.Add(DocumentIdsRow);
            }
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter docIds = new SqlParameter("@EmailAlerts", DocumentIdsDataTable);
            docIds.SqlDbType = SqlDbType.Structured;
            parameters.Add(docIds);
            parameters.Add(new SqlParameter("@interval", interval));
            parameters.Add(new SqlParameter("@type", type));

            var guid = Guid.NewGuid().ToString();
            DBUtil.ExecuteNonQuery(query, parameters, null, CommandType.StoredProcedure, guid);
        }

    }
}
