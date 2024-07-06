using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.Data;
using System.Configuration;

namespace Insight360.InfluenceCalculator.Klout
{
    public class Operations
    {

        private static string dKey = "knx275mm8ez5q6gaj9sehthw";

        public static List<string> GetScores(string devKey, string users)
        {
            try
            {
                if (string.IsNullOrEmpty(devKey))
                    devKey = dKey;

                string urlToCall = "http://api.klout.com/1/klout.xml?key=" + devKey + "&users=" + users;

                WebRequest wrKlout;
                wrKlout = WebRequest.Create(urlToCall);

                WebResponse resKlout = wrKlout.GetResponse();
                Stream objStream = resKlout.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string strText = objReader.ReadToEnd();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = strText;
                XmlNode root = xmlDoc.DocumentElement;

                XmlNodeList nodeUsers = root.SelectNodes("user");

                List<string> lstScores = new List<string>();

                foreach (XmlNode node in nodeUsers)
                {
                    string nodeValue = node.SelectSingleNode("twitter_screen_name").InnerText;
                    nodeValue += "," + node.SelectSingleNode("kscore").InnerText;
                    lstScores.Add(nodeValue);
                }

                return lstScores;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public static List<KloutUser> GetUsers(string devKey, string users)
        {
            try
            {
                if (string.IsNullOrEmpty(devKey))
                    devKey = dKey;

                string urlToCall = "http://api.klout.com/1/users/show.xml?key=" + devKey + "&users=" + users;

                WebRequest wrKlout;
                wrKlout = WebRequest.Create(urlToCall);

                WebResponse resKlout = wrKlout.GetResponse();
                Stream objStream = resKlout.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string strText = objReader.ReadToEnd();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = strText;
                XmlNode root = xmlDoc.DocumentElement;

                XmlNodeList nodeUsers = root.SelectNodes("user");

                List<KloutUser> lstUsers = new List<KloutUser>();

                foreach (XmlNode node in nodeUsers)
                {
                    KloutUser kUser = new KloutUser();
                    XmlNode scoreNode = node.SelectSingleNode("score");

                    kUser.twitter_screen_name = node.SelectSingleNode("twitter_screen_name").InnerText;
                    kUser.twitter_id = node.SelectSingleNode("twitter_id").InnerText;
                    kUser.kscore = scoreNode.SelectSingleNode("kscore").InnerText;
                    kUser.slope = scoreNode.SelectSingleNode("slope").InnerText;
                    kUser.description = scoreNode.SelectSingleNode("description").InnerText;
                    kUser.kclass_id = scoreNode.SelectSingleNode("kclass_id").InnerText;
                    kUser.kclass = scoreNode.SelectSingleNode("kclass").InnerText;
                    kUser.kclass_description = scoreNode.SelectSingleNode("kclass_description").InnerText;
                    kUser.network_score = scoreNode.SelectSingleNode("network_score").InnerText;
                    kUser.amplification_score = scoreNode.SelectSingleNode("amplification_score").InnerText;
                    kUser.true_reach = scoreNode.SelectSingleNode("true_reach").InnerText;
                    kUser.delta_1day = scoreNode.SelectSingleNode("delta_1day").InnerText;
                    kUser.delta_5day = scoreNode.SelectSingleNode("delta_5day").InnerText;

                    lstUsers.Add(kUser);
                }

                return lstUsers;
            }
            catch (Exception ex)
            {
                return new List<KloutUser>();
            }

        }

        public static List<string> GetTopics(string devKey, string users)
        {
            try
            {
                if (string.IsNullOrEmpty(devKey))
                    devKey = dKey;

                string urlToCall = "http://api.klout.com/1/users/topics.xml?key=" + devKey + "&users=" + users;

                WebRequest wrKlout;
                wrKlout = WebRequest.Create(urlToCall);

                WebResponse resKlout = wrKlout.GetResponse();
                Stream objStream = resKlout.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string strText = objReader.ReadToEnd();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = strText;
                XmlNode root = xmlDoc.DocumentElement;

                XmlNodeList nodeUsers = root.SelectNodes("user");

                List<string> lstScores = new List<string>();

                foreach (XmlNode node in nodeUsers)
                {
                    string nodeValue = node.SelectSingleNode("twitter_screen_name").InnerText;
                    nodeValue += "," + node.SelectSingleNode("kscore").InnerText;
                    lstScores.Add(nodeValue);
                }

                return lstScores;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public static List<string> GetInfluencedBy(string devKey, string user)
        {
            try
            {
                if (string.IsNullOrEmpty(devKey))
                    devKey = dKey;

                string urlToCall = "http://api.klout.com/1/soi/influenced_by.xml?key=" + devKey + "&users=" + user;

                WebRequest wrKlout;
                wrKlout = WebRequest.Create(urlToCall);

                WebResponse resKlout = wrKlout.GetResponse();
                Stream objStream = resKlout.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string strText = objReader.ReadToEnd();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = strText;
                XmlNode root = xmlDoc.DocumentElement;

                XmlNode nodeInfluencers = root.SelectSingleNode("user").SelectSingleNode("influencers");

                List<string> lstUsers = new List<string>();

                for (int i = 0; i < nodeInfluencers.ChildNodes.Count; ++i)
                {

                    string nodeValue = nodeInfluencers.ChildNodes[i].InnerText;
                    nodeValue += "," + nodeInfluencers.ChildNodes[++i].InnerText;
                    lstUsers.Add(nodeValue);
                }
                return lstUsers;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public static List<string> GetInfluencerOf(string devKey, string users)
        {
            try
            {
                if (string.IsNullOrEmpty(devKey))
                    devKey = dKey;

                string urlToCall = "http://api.klout.com/1/soi/influencer_of.xml?key=" + devKey + "&users=" + users;

                WebRequest wrKlout;
                wrKlout = WebRequest.Create(urlToCall);

                WebResponse resKlout = wrKlout.GetResponse();
                Stream objStream = resKlout.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string strText = objReader.ReadToEnd();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.InnerXml = strText;
                XmlNode root = xmlDoc.DocumentElement;

                XmlNode nodeInfluencers = root.SelectSingleNode("user").SelectSingleNode("influencees");

                List<string> lstUsers = new List<string>();

                for (int i = 0; i < nodeInfluencers.ChildNodes.Count; ++i)
                {

                    string nodeValue = nodeInfluencers.ChildNodes[i].InnerText;
                    nodeValue += "," + nodeInfluencers.ChildNodes[++i].InnerText;
                    lstUsers.Add(nodeValue);
                }
                return lstUsers;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public static void CollectScores()
        {
            try
            {
                //Get Twitter Users
                Log("Klout:: Get Twitter Users");
                DataTable dtUsers = DBOperations.GetTwitterUserNames();
                string users = string.Empty;
                foreach (DataRow dRow in dtUsers.Rows)
                {
                    users += dRow[0].ToString() + ",";
                }
                Log("Klout:: Got Twitter Users: " + users);

                //Now Get the Scores
                List<KloutUser> kUsers = GetUsers("", users);
                Log("Klout:: Got Scores, Count is: " + kUsers.Count.ToString());
                //Now Insert these Scores
                foreach (KloutUser kUser in kUsers)
                {
                    Log("Klout:: Inserting Score for: " + kUser.twitter_screen_name);
                    DBOperations.InsertKloutScores(kUser);
                    Log("Klout:: Score inserted for for: " + kUser.twitter_screen_name);
                    //{
                    //    Log("Klout:: Updating Status for: " + kUser.twitter_screen_name);
                    //    DBOperations.UpdateTwitterKloutStatus(kUser.twitter_screen_name, 1);
                    //}
                    //else
                    //    Log("Klout:: Score didnt insert for: " + kUser.twitter_screen_name);
                }
                //Now Update their Statuses
                foreach (DataRow dRow in dtUsers.Rows)
                {
                    //users += dRow[0].ToString() + ",";
                    Log("Klout:: Updating Status for: " + dRow[0].ToString());
                    DBOperations.UpdateTwitterKloutStatus(dRow[0].ToString(), 1);
                    Log("Klout:: Status updated for: " + dRow[0].ToString());
                }
            }
            catch (Exception ex)
            {
                Log("Klout:: Exception occured: " + ex.Message);

            }

        }

        private static void Log(string message)
        {
            try
            {
                if (string.Equals(ConfigurationManager.AppSettings["EnableLogging"], "1"))
                {
                    //Get the File Name 
                    string fileName = ConfigurationManager.AppSettings["FilePath"] + ConfigurationManager.AppSettings["FileName"] + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + ".txt";

                    if (!File.Exists(fileName))
                        File.Create(fileName);
                    //File.WriteAllText(fileName, Environment.NewLine + message);

                    StreamWriter wr = File.AppendText(fileName);

                    wr.Write(Environment.NewLine + message + "  :: " + DateTime.Now.ToString());

                    wr.Flush();

                    wr.Close();
                }
            }
            catch (Exception ex) { }
        }
    }
}
