using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Insight360.DAL
{
    public class DBUtil
    {
		static object lockObj = new object( );

        public const string DynamicParameterName = "@dynamic_param";

        public static string ConnectionString { get; set; }

        static Dictionary<string, SqlConnection> connectionsCache = new Dictionary<string, SqlConnection>();

		public static void WriteCachedConnections( )
		{
			Util.Log.WriteOperationStarted( typeof( DBUtil ) , "WriteCachedConnections" , "Active connections present: " + string.Join( "," , connectionsCache.Keys.ToArray( ) ) );
		}

        private static SqlCommand PrepareCommand(string query, List<SqlParameter> parameters,ArrayList dynamicParameters, CommandType commandType,string guid)
        {
            if (dynamicParameters != null)
            {
                if (parameters == null)
                    parameters = new List<SqlParameter>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dynamicParameters.Count; i++)
                {
                    if (i != 0)
                        sb.Append(",");
                    
                    sb.Append(DynamicParameterName + i);
                    parameters.Add(new SqlParameter(DynamicParameterName + i, dynamicParameters[i]));

                }
                query = query.Replace(DynamicParameterName, sb.ToString());
            }

            
            SqlConnection connection = GetDatabaseConnection(guid);
            OpenConnection(guid);
            SqlCommand command = new SqlCommand(query, connection);
            command.CommandType = commandType;
            command.CommandTimeout = 30 * 60;
            if (parameters != null)
                command.Parameters.AddRange(parameters.ToArray());

            //Util.Log.WriteDbCommand(typeof(DBUtil), "PrepareCommand for guid: " + guid, command);
            return command;
        }

        private static void DisposeCommand(SqlCommand command)
        {
            if (command != null)
                command.Dispose();
        }

        public static DataTable FetchRecords(string query, List<SqlParameter> parameters, ArrayList dynamicParameters, CommandType commandType, string guid)
        {
            SqlCommand command = null;
            DataTable table = new DataTable();
            try
            {
				if( string.IsNullOrEmpty( guid ) )
					guid = Guid.NewGuid( ).ToString( );

				guid = guid.Replace( '-' , '_' );
				
				command = PrepareCommand( query , parameters , dynamicParameters , commandType , guid );
                command.CommandTimeout = 60;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(table);
            }
            catch (Exception ex)
            {
                Exception e = new Exception(query);
                Util.Log.WriteDbException(typeof(DBUtil),"FetchRecords hello" + e, ex, command);
            }
            finally
            {
                DisposeCommand(command);
                CloseConnection(guid);
            }
            return table;
        }

        public static DataTable FetchRecords(string query, List<SqlParameter> parameters, ArrayList dynamicParaeters, CommandType commandType)
        {
            string guid = Guid.NewGuid().ToString();
            DataTable ret = FetchRecords(query, parameters,dynamicParaeters, commandType, guid);
            CloseConnection(guid);
            return ret;
        }


        public static DataSet DSFetchRecords(string query, List<SqlParameter> parameters, ArrayList dynamicParameters, CommandType commandType, string guid)
        {
            SqlCommand command = null;
            DataSet ds = new DataSet();
            try
            {
                if (string.IsNullOrEmpty(guid))
                    guid = Guid.NewGuid().ToString();

                guid = guid.Replace('-', '_');

                command = PrepareCommand(query, parameters, dynamicParameters, commandType, guid);
                command.CommandTimeout = 60;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(ds);
            }
            catch (Exception ex)
            {
                Exception e = new Exception(query);
                Util.Log.WriteDbException(typeof(DBUtil), "FetchRecords hello" + e, ex, command);
            }
            finally
            {
                DisposeCommand(command);
                CloseConnection(guid);
            }
            return ds;
        }

        public static DataSet DSFetchRecords(string query, List<SqlParameter> parameters, ArrayList dynamicParaeters, CommandType commandType)
        {
            string guid = Guid.NewGuid().ToString();
            DataSet ret = DSFetchRecords(query, parameters, dynamicParaeters, commandType, guid);
            CloseConnection(guid);
            return ret;
        }

        public static bool ExecuteNonQuery( string query , List<SqlParameter> parameters , ArrayList dynamicParameters , CommandType commandType , string guid )
		{
			bool result = false;
            SqlCommand command = null;
            try
            {
				if( string.IsNullOrEmpty( guid ) )
					guid = Guid.NewGuid( ).ToString( );

				guid = guid.Replace( '-' , '_' );
				
				command = PrepareCommand( query , parameters , dynamicParameters , commandType , guid );
                command.CommandTimeout = 120;
                command.ExecuteNonQuery();
                result = true;
            }
            catch (Exception ex)
            {
                Util.Log.WriteDbException(typeof(DBUtil), "ExecuteNonQuery", ex, command);
            }
            finally
            {
                DisposeCommand(command);
                CloseConnection(guid);
            }
            return result;
        }

		public static bool ExecuteNonQuery( string query , List<SqlParameter> parameters , ArrayList dynamicParameters , CommandType commandType )
		{
			string guid = Guid.NewGuid().ToString();
			bool ret = ExecuteNonQuery( query , parameters , dynamicParameters , commandType , guid );
            CloseConnection(guid);
            return ret;
        }

        public static object ExecuteScalar(string query, List<SqlParameter> parameters, ArrayList dynamicParameters, CommandType commandType, string guid)
        {
            object returnValue = null;
            SqlCommand command = null;
            try
            {
				if( string.IsNullOrEmpty( guid ) )
					guid = Guid.NewGuid( ).ToString( );

				guid = guid.Replace( '-' , '_' );
				
				command = PrepareCommand( query , parameters , dynamicParameters , commandType , guid );
                returnValue = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Util.Log.WriteDbException(typeof(DBUtil), "ExecuteScalar", ex, command);
            }
            finally
            {
                DisposeCommand(command);
                CloseConnection(guid);
            }
            return returnValue;
        }

        public static object ExecuteScalar(string query, List<SqlParameter> parameters, ArrayList dynamicParameters, CommandType commandType)
        {
            string guid = Guid.NewGuid().ToString();
            object ret = ExecuteScalar(query, parameters, dynamicParameters, commandType, guid);
            CloseConnection(guid);
            return ret;
        }


        public static void CloseConnection(string guid)
        {
            try
            {
                guid = guid.Replace('-', '_');

                SqlConnection connection = GetDatabaseConnection(guid);

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();

					lock( lockObj )
					{
						if( connectionsCache.ContainsKey( guid ) )
							connectionsCache.Remove( guid );
					}
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DBUtil), "CloseConnection", ex);
            }
        }


        static void OpenConnection(string guid)
        {
            try
            {
                SqlConnection connection = GetDatabaseConnection(guid);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DBUtil), "OpenConnection", ex);
            }
        }

        static SqlConnection GetDatabaseConnection(string guid)
        {
            guid = guid.Replace('-', '_');
            SqlConnection connection = null;

			lock( lockObj )
            {
                if (connectionsCache.ContainsKey(guid))
                {
                    connection = connectionsCache[guid];
                }
                else
                {
                    connection = new SqlConnection(ConnectionString);
                    connectionsCache.Add(guid, connection);
                }
            }

            return connection;
        }


        public static SqlTransaction BeginTransaction(string guid, IsolationLevel level)
        {            
            try
            {
                SqlConnection connection = GetDatabaseConnection(guid);

                return connection.BeginTransaction(level);
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DBUtil), "BeginTransaction", ex);
            }
            return null;
        }

        public static SqlTransaction BeginTransaction(string guid)
        {
            try
            {
                SqlConnection connection = GetDatabaseConnection(guid);

                return connection.BeginTransaction();
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(DBUtil), "BeginTransaction", ex);
            }
            return null;
        }
       
    }
}
