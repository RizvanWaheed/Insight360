using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Globalization;
using Insight360.Common;

namespace Insight360.DataProcessor
{
    public static class Sentiment
    {
        public static int? GetSentimentScore(string text)
        {
            int? score = null;
            try
            {
                //if (Properties.Settings.Default.EnableSentiment)
                //{
                //    SentimentsService.ClassificationService client = new SentimentsService.ClassificationService();
                //    string sentiment = client.ClassifySentence(text);

                //    const string sentiment_token = "#44#Imran-Rims-Mohsin#44#";
                //    string[] splitted = sentiment.Split(
                //            new string[] { ",", sentiment_token },
                //            StringSplitOptions.None);

                //    if (splitted[0] == "NEGATIVE")
                //        score = -1 * int.Parse(splitted[1]);
                //    else if (splitted[0] == "POSITIVE")
                //        score = int.Parse(splitted[1]);
                //    else
                //        score = 0;
                //}
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(Sentiment), "GetSentimentScore", ex);
            }

            return score;
        }

        public static void SentimentThread()
        {
            Util.Log.WriteOperationStarted(typeof(Sentiment), "SentimentThread", "Thread Enter");
            try
            {
                bool exit = false;
                exit = DataProcessor.StopEvent.WaitOne(1);
                while (!exit)
                {
                    var orgs = DAL.ConfigDAL.FetchOrganizations();

                    for (int i = 0; i < orgs.Count; i++)
                    {
                        var documents = DAL.SentimentDAL.FetchDocuments(orgs[i], AppSettings.TopCount_Sentiment);
                        Util.Log.WriteOperationCompleted(typeof(Sentiment), "SentimentThread", documents.Count + " documents fetched for " + orgs[i]);
                        if (documents.Count > 0)
                        {
                            for (int j = 0; j < documents.Count; j++)
                            {
                                documents[j].SentimentScore = GetSentimentScore(documents[j].ContentText);
                            }
                            DAL.SentimentDAL.PersistSentiment(orgs[i], documents);


                            Util.Log.WriteOperationCompleted(typeof(Sentiment), "SentimentThread", documents.Count + " documents analyzed for " + orgs[i]);
                        }
                        exit = DataProcessor.StopEvent.WaitOne(1);
                        if (exit)
                            break;
                    }
                    exit = DataProcessor.StopEvent.WaitOne((int)AppSettings.DataProcessor_WaitInterval_NothingFetched.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Util.Log.WriteException(typeof(Sentiment), "SentimentThread", ex);
            }
            finally
            {
                Util.Log.WriteOperationCompleted(typeof(Sentiment), "SentimentThread", "Thread Exit");
            }
        }
    }
}
