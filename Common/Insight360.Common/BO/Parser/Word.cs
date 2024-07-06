using System;

/// <summary>
/// Summary description for Word
/// </summary>
/// 

namespace Insight360.BO.Parser
{
    public class Word
    {
        private long id;        
        private long version;        
        private string text;
        private double positive;
        private double negative;
        private double neutral;
        private string partOfSpeech;
        private string language;
        private long positiveCount;        
        private long negativeCount;        
        private long neutralCount;
        private long categorizationCount;
        private string meaning;
        private string usage;
        private long index;        
        private DateTime creationDate;        
        private DateTime lastUpdated;
        private long featureId;
        private bool newWordFlag;

        public Word()
        {
            this.id                     = 0;
            this.text                   = "";
            this.version                = 1;
            this.positive               = 0.0;
            this.negative               = 0.0;
            this.neutral                = 0.0;
            this.partOfSpeech           = "none";
            this.language               = "";
            this.positiveCount          = 0;
            this.negativeCount          = 0;
            this.neutralCount           = 0;
            this.categorizationCount    = 0;
            this.meaning                = "";
            this.usage                  = "";
            this.featureId              = 0;
            this.newWordFlag            = true;
        }

        public Word(string text)
        {
            this.id                     = 0;
            this.text                   = text;
            this.version                = 1;
            this.positive               = 0.0;
            this.negative               = 0.0;
            this.neutral                = 0.0;
            this.partOfSpeech           = "none";
            this.language               = "";
            this.positiveCount          = 0;
            this.negativeCount          = 0;
            this.neutralCount           = 0;
            this.categorizationCount    = 0;
            this.meaning                = "";
            this.usage                  = "";
            this.featureId              = 0;
            this.newWordFlag            = true;
        }

        public Word(long id, long version, string text, double positive, double negative, double neutral, string partOfSpeech, string language, DateTime creationDate, DateTime lastUpdated)
        {
            this.id             = id;
            this.version        = version;
            this.text           = text;
            this.positive       = positive;
            this.negative       = negative;
            this.neutral        = neutral;
            this.partOfSpeech   = partOfSpeech;
            this.language       = language;
            this.creationDate   = creationDate;
            this.lastUpdated    = lastUpdated;
            this.featureId      = 0;
            this.newWordFlag    = true;
        }

        public Word copyAllValues(Word source, Word destination)
        {
            destination.Id                  = source.Id;
            destination.Text                = source.Text;
            destination.Positive            = source.Positive;
            destination.Negative            = source.Negative;
            destination.Neutral             = source.Neutral;
            destination.PartOfSpeech        = source.PartOfSpeech;
            destination.Language            = source.Language;
            destination.PositiveCount       = source.PositiveCount;
            destination.NegativeCount       = source.NegativeCount;
            destination.NeutralCount        = source.NeutralCount;
            destination.CategorizationCount = source.CategorizationCount;
            destination.CreationDate        = source.CreationDate;
            destination.LastUpdated         = source.LastUpdated;
            destination.Version             = source.Version;
            destination.meaning             = source.meaning;
            destination.usage               = source.usage;
            destination.NewWordFlag         = source.newWordFlag;
            destination.featureId           = source.featureId;

            return destination;
        }

        public double GetWeightByCategoryName(string category)
        {
            if (category.Equals(Categories.POSITIVE))
            {
                return positive;
            }

            else if (category.Equals(Categories.NEGATIVE))
            {
                return negative;
            }

            else if (category.Equals(Categories.NEUTRAL))
            {
                return neutral;
            }

            return 0.0;
        }

        public void SetWeightByCategoryName(string category, double weight)
        {
            if (category.Equals(Categories.POSITIVE))
            {
                positive = weight; 
            }

            else if (category.Equals(Categories.NEGATIVE))
            {
                negative = weight;
            }

            else if (category.Equals(Categories.NEUTRAL))
            {
                neutral = weight;
            }
        }

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        public long Version
        {
            get { return version; }
            set { version = value; }
        }
        
        public string Text
        {
            get { return text; }
            set { text = value.ToLower(); }
        }

        public double Positive
        {
            get { return positive; }
            set { positive = value; }
        }

        public double Negative
        {
            get { return negative; }
            set { negative = value; }
        }

        public double Neutral
        {
            get { return neutral; }
            set { neutral = value; }
        }

        public string PartOfSpeech
        {
            get { return partOfSpeech; }
            set { partOfSpeech = value; }
        }

        public string Language
        {
            get { return language; }
            set { language = value; }
        }

        public long PositiveCount
        {
            get { return positiveCount; }
            set { positiveCount = value; }
        }

        public long NegativeCount
        {
            get { return negativeCount; }
            set { negativeCount = value; }
        }

        public long NeutralCount
        {
            get { return neutralCount; }
            set { neutralCount = value; }
        }

        public long CategorizationCount
        {
            get { return categorizationCount; }
            set { categorizationCount = value; }
        }

        public string Meaning
        {
            get { return meaning; }
            set { meaning = value; }
        }

        public string Usage
        {
            get { return usage; }
            set { usage = value; }
        }

        public long Index
        {
            get { return index; }
            set { index = value; }
        }

        public DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }

        public DateTime LastUpdated
        {
            get { return lastUpdated; }
            set { lastUpdated = value; }
        }

        public long FeatureId
        {
            get { return featureId; }
            set { featureId = value; }
        }

        public bool NewWordFlag
        {
            get { return newWordFlag; }
            set { newWordFlag = value; }
        }
    }
}