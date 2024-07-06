using System;
using System.Collections.Generic;


/// <summary>
/// Summary description for Paragraph
/// </summary>
/// 

namespace Insight360.BO.Parser
{

    public class Paragraph : IEntity
    {
        private string originalText;
        private string filteredText;

        //Category variables will store the winner or max category
        private string category;
        private double categoryWeight;
        private double confidenceInterval;

        //These variables will be used for Delta Rule only
        private string targetCategory;         
        private double targetWeight;
        private double targetConfidenceInterval;

        //List will store all the weights with positive, negative and neutral on various indices repectively
        //the index number of this and bagOfSentence will correspond with 1 to 3
        private List<double> resultantWeights;
        private List<double> resultantConfidenceIntervals; 

        private List<Sentence> bagOfSentences;

        public Paragraph()
        {
            this.OriginalText                   = "";
            this.FilteredText                   = "";
            this.CategoryWeight                 = 0.1;
            this.ConfidenceInterval             = 0.1;
            this.BagOfSentences                 = new List<Sentence>();
            this.resultantWeights               = new List<double>();
            this.resultantConfidenceIntervals   = new List<double>();
            
            //Finally add the double variables required in both the lists
            this.initList();
        }

        public Paragraph(string text)
        {
            this.OriginalText                   = text;
            this.FilteredText                   = "";
            this.CategoryWeight                 = 0.1;
            this.ConfidenceInterval             = 0.1;
            this.BagOfSentences                 = new List<Sentence>();
            this.resultantWeights               = new List<double>();
            this.resultantConfidenceIntervals   = new List<double>();

            //Finally add the double variables required in both the lists
            this.initList();
        }

        public void initList()
        {
            this.resultantWeights.Add(0.0);
            this.resultantWeights.Add(0.0);
            this.resultantWeights.Add(0.0);

            //We will a value each for positive,negative and neutral
            this.resultantConfidenceIntervals.Add(0.0);
            this.resultantConfidenceIntervals.Add(0.0);
            this.resultantConfidenceIntervals.Add(0.0);
        }

        public string OriginalText
        {
            get { return originalText; }
            set { originalText = value; }
        }

        public string FilteredText
        {
            get { return filteredText; }
            set { filteredText = value; }
        }

        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        public double CategoryWeight
        {
            get { return categoryWeight; }
            set { categoryWeight = value; }
        }

        public double ConfidenceInterval
        {
            get { return confidenceInterval; }
            set { confidenceInterval = value; }
        }

        public string TargetCategory
        {
            get { return targetCategory; }
            set { targetCategory = value; }
        }

        public double TargetWeight
        {
            get { return targetWeight; }
            set { targetWeight = value; }
        }

        public double TargetConfidenceInterval
        {
            get { return targetConfidenceInterval; }
            set { targetConfidenceInterval = value; }
        }

        public List<double> ResultantWeights
        {
            get { return resultantWeights; }
            set { resultantWeights = value; }
        }

        public List<double> ResultantConfidenceIntervals
        {
            get { return resultantConfidenceIntervals; }
            set { resultantConfidenceIntervals = value; }
        }

        public List<Sentence> BagOfSentences
        {
            get { return bagOfSentences; }
            set { bagOfSentences = value; }
        }

        public void ResetWeights()
        {
            this.resultantWeights[0] = 0.0;
            this.resultantWeights[1] = 0.0;
            this.resultantWeights[2] = 0.0;
        }

        public void ResetConfidenceInterval()
        {
            this.resultantConfidenceIntervals[0] = 0.0;
            this.resultantConfidenceIntervals[1] = 0.0;
            this.resultantConfidenceIntervals[2] = 0.0;
        }
    }
}
