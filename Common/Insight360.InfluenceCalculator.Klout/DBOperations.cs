using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Insight360.InfluenceCalculator.Klout
{
    public class DBOperations
    {
        public static SqlCommand GetCommand()
        {

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["KloutCS"].ConnectionString);
            con.Open();
            SqlCommand com = new SqlCommand();
            com.Connection = con;
            return com;
        }

        public static DataTable GetTwitterUserNames()
        {
            SqlCommand com = GetCommand();

            com.CommandText = "GetTwitterUserNamesForKloutScore";

            com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter da = new SqlDataAdapter(com);

            DataTable dt = new DataTable();
            da.Fill(dt);

            com.Connection.Close();

            return dt;
        }

        public static bool UpdateTwitterKloutStatus(string screenName, Int16 status)
        {
            SqlCommand com = GetCommand();

            com.Parameters.Add("@twitter_screen_name", screenName);
            com.Parameters.Add("@status", status);


            com.CommandText = "UpdateTwitterKloutStatus";
            com.CommandType = CommandType.StoredProcedure;

            com.ExecuteNonQuery();

            com.Connection.Close();

            return true;


        }

        public static bool InsertKloutScores(KloutUser kUser)
        {
            SqlCommand com = GetCommand();

            com.Parameters.Add("@twitter_id", kUser.twitter_id);
            com.Parameters.Add("@twitter_screen_name", kUser.twitter_screen_name);
            com.Parameters.Add("@kscore", kUser.kscore);
            com.Parameters.Add("@slope", kUser.slope);
            com.Parameters.Add("@description", kUser.description);
            com.Parameters.Add("@kscore_description", kUser.kscore_description);
            com.Parameters.Add("@network_score", kUser.network_score);
            com.Parameters.Add("@amplification_score", kUser.amplification_score);
            com.Parameters.Add("@true_reach", kUser.true_reach);
            com.Parameters.Add("@kclass", kUser.kclass);
            com.Parameters.Add("@kclass_id", kUser.kclass_id);
            com.Parameters.Add("@kclass_description", kUser.kclass_description);
            com.Parameters.Add("@delta_1day", kUser.delta_1day);
            com.Parameters.Add("@delta_5day", kUser.delta_5day);


            com.CommandText = "InsertKloutScores";
            com.CommandType = CommandType.StoredProcedure;

            com.ExecuteNonQuery();

            com.Connection.Close();

            return true;


        }
    }
}
