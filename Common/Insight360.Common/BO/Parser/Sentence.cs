using System;
using System.Collections.Generic;
/// <summary>
/// Summary description for Sentence
/// </summary>
/// 

namespace Insight360.BO.Parser
{

    public class Sentence : IEntity
    {
        private string originalText;
        private string filteredText;        

        //Category variables will store the winner or max category
        private string category;        
        private double categoryWeight;
        private double confidenceInterval;

        //These variables will be used for Delta Rule only        
        private double targetWeight;
        private double targetConfidenceInterval;

        //We will use this list to store all the weights with positive, negative and neutral on various indices repectively
        private string targetCategory;         
        private List<double> resultantWeights;        
        private List<double> resultantConfidenceIntervals;        

        //These bags will contain pos as described by their names
        private List<Word> bagOfNegations;
        private List<Word> bagOfNouns;
        private List<Word> bagOfProperNouns;
        private List<Word> bagOfArticles;
        private List<Word> bagOfPrepositions;
        private List<Word> bagOfConjunctions;
        private List<Word> bagOfWords;       

        public Sentence()
        {
            this.originalText                   = "";
            this.filteredText                   = "";
            this.categoryWeight                 = 0.1;
            this.confidenceInterval             = 0.1;
            this.resultantWeights               = new List<double>();
            this.resultantConfidenceIntervals   = new List<double>();
            this.bagOfNegations                 = new List<Word>();
            this.bagOfNouns                     = new List<Word>();
            this.bagOfProperNouns               = new List<Word>();
            this.bagOfArticles                  = new List<Word>();
            this.bagOfPrepositions              = new List<Word>();
            this.bagOfConjunctions              = new List<Word>();
            this.bagOfWords                     = new List<Word>();
            this.resultantWeights               = new List<double>();
            this.resultantConfidenceIntervals   = new List<double>();
            //Finally add the double variables required in both the lists
            this.initList();
        }

        public Sentence(string text)
        {
            this.originalText                   = text;
            this.filteredText                   = "";
            this.categoryWeight                 = 0.1;
            this.confidenceInterval             = 0.1;
            this.resultantWeights               = new List<double>();
            this.resultantConfidenceIntervals   = new List<double>();
            this.bagOfNegations                 = new List<Word>();
            this.bagOfNouns                     = new List<Word>();
            this.bagOfProperNouns               = new List<Word>();
            this.bagOfArticles                  = new List<Word>();
            this.bagOfPrepositions              = new List<Word>();
            this.bagOfConjunctions              = new List<Word>();
            this.bagOfWords                     = new List<Word>();
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

        public List<Word> BagOfNegations
        {
            get { return bagOfNegations; }
            set { bagOfNegations = value; }
        }

        public List<Word> BagOfNouns
        {
            get { return bagOfNouns; }
            set { bagOfNouns = value; }
        }

        public List<Word> BagOfProperNouns
        {
            get { return bagOfProperNouns; }
            set { bagOfProperNouns = value; }
        }

        public List<Word> BagOfArticles
        {
            get { return bagOfArticles; }
            set { bagOfArticles = value; }
        }

        public List<Word> BagOfPrepositions
        {
            get { return bagOfPrepositions; }
            set { bagOfPrepositions = value; }
        }

        public List<Word> BagOfConjunctions
        {
            get { return bagOfConjunctions; }
            set { bagOfConjunctions = value; }
        }

        public List<Word> BagOfWords
        {
            get { return bagOfWords; }
            set { bagOfWords = value; }
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