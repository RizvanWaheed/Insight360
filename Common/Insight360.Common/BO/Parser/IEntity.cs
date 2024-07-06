using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for IEntity
/// </summary>
/// 

namespace Insight360.BO.Parser
{

    public interface IEntity
    {
        string OriginalText
        {
            get;
            set;
        }
        string FilteredText
        {
            get;
            set;
        }

        string Category
        {
            get;
            set;
        }
        double CategoryWeight
        {
            get;
            set;
        }
        double ConfidenceInterval
        {
            get;
            set;
        }

        //These variables will be used for Delta Rule only
        string TargetCategory
        {
            get;
            set;
        }

        double TargetWeight
        {
            get;
            set;
        }

        double TargetConfidenceInterval
        {
            get;
            set;
        }

        //List will store all the weights with positive, negative and neutral on various indices repectively
        //the index number of this and bagOfParagraphs will correspond with 1 to 3
        List<double> ResultantWeights
        {
            get;
            set;
        }

        List<double> ResultantConfidenceIntervals
        {
            get;
            set;
        }

        //Finally declare a bag which will contain the main collection i.e paragraphs or sentences
        //List<IEntity> Bag
        //{
        //    get;
        //    set;
        //}
    }
}