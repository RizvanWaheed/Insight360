using System;

/// <summary>
/// Summary description for Categories
/// </summary>
/// 

namespace Insight360.Util
{

    public class Categories
    {

        //LEARNING RATE

        public const double LEARNINGRATE = 1;
        public const double ERRORINTERVAL = 0.01;

        //Attribute name constants for the Learning rate table
        public const string POSITIVELEARNING = "PositiveLearning";
        public const string NEGATIVELEARNING = "NegativeLearning";
        public const string NEUTRALLEARNING = "NeutralLearning";
        public const string POSITIVEVOCABULARY = "PositiveVocabulary";
        public const string NEGATIVEVOCABULARY = "NegativeVocabulary";
        public const string NEUTRALVOCABULARY = "NeutralVocabulary";
        public const string VOCABULARY = "Vocabulary";
        public static string[] LEARNINGATTRIBUTES = new string[] { POSITIVELEARNING, NEGATIVELEARNING, NEUTRALLEARNING, POSITIVEVOCABULARY, NEGATIVEVOCABULARY, NEUTRALVOCABULARY, VOCABULARY };

        //Category Constants

        public const string POSITIVE = "POSITIVE";
        public const string NEGATIVE = "NEGATIVE";
        public const string NEUTRAL = "NEUTRAL";
        public const string UNDECIDED = "UNDECIDED";

        //Parts of Speech Constants

        public const string NOUN = "NOUN";
        public const string PROPERNOUN = "PROPERNOUN";
        public const string PREPOSITION = "PREPOSITION";
        public const string ARTICLE = "ARTICLE";
        public const string NEGATION = "NEGATION";
        public const string ADJECTIVE = "ADJECTIVE";
        public const string ADVERB = "ADVERB";
        public const string VERB = "VERB";
        public const string CONJUNCTION = "CONJUNCTION";
        public const string HELPINGVERB = "HELPING VERB";
        public const string PRONOUN = "PRONOUN";
        public const string EXCLUSION = "EXCLUSION";

        //CONSTANT FOR UN-SUPERVISED LEARNING
        public const long CATEGORIZATIONCOUNT = 2;

        //TABLE CONSTANTS
        public const string WORDTABLE = "Word";

        //OBJECTS, FEATURES, ATTRIBUTES AND FEATURE INDICATORS
        public const string SPECIALFEATURE = "SPECIALFEATURE";
        public const string FEATURE = "FEATURE";
        public const string HUMAN = "HUMAN";
        public const string PRODUCT = "PRODUCT";
        public const string PROCESS = "PROCESS";
        public const string CONCEPT = "CONCEPT";
        public const string FEATUREATTRIBUTE = "ATTRIBUTE";
        public const string FEATUREINDICATORS = "FEATUREINDICATORS";
        public const string VAS = "VAS";
        public const string FEATURELINK = "LINK";
        public const string FEATURESYNONYM = "SYNONYM";
        public const string FEATUREINFO = "INFO";
    }
}
