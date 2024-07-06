using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Insight360.Blogger
{
    public class DBOperations
    {
        public static SqlCommand GetCommand()
        {

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["YouTubeDS"].ConnectionString);
            con.Open();
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            return com;
        }

        public static string GetBlogID(string postID)
        {
            SqlCommand com = GetCommand();

            com.CommandText = "GetBlogID";

            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.Add(new SqlParameter("@PostID", postID));

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString();
            return string.Empty;
        }

    }
}
