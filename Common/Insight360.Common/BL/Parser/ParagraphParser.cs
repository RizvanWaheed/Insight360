using System;
using System.Collections.Generic;
using Insight360.BO;
using Insight360.BO.Parser;
//using TwitterService.Util;

/// <summary>
/// Summary description for ParagraphParser
/// </summary>
/// 

namespace Insight360.BL.Parser
{
    public class ParagraphParser
    {
        private Paragraph paragraph;

        public ParagraphParser()
        {
            this.paragraph = new Paragraph();
        }

        public ParagraphParser(Paragraph paragraph)
        {
            this.paragraph = paragraph;
        }

        public Paragraph ParseParagraph(Paragraph paragraph)
        {
            this.paragraph              = paragraph;
            //Declare local scope variables to hold start index and list for storing sentences
            int index                   = 0;
            int startIndex              = 0;
            List<Sentence> sentences    = new List<Sentence>();
            bool delimeterFlag          = true;

            if (!string.IsNullOrEmpty(this.paragraph.OriginalText))
            {
                string[] splitted = this.paragraph.OriginalText.Split(new char[] { '.', '?', '!' });
                for (int i = 0; i < splitted.Length; i++)
                {
                    splitted[i] = splitted[i].Trim();
                    if (!string.IsNullOrEmpty(splitted[i]) )
                    {
                        Sentence sentence = new Sentence(splitted[i]);
                        sentences.Add(sentence);
                    }
                }
                this.paragraph.BagOfSentences = sentences;
            }
            else
            {
                this.paragraph.BagOfSentences = null;
            }

            return paragraph;

            //if (!this.paragraph.OriginalText.Equals("") && this.paragraph.OriginalText != null)
            //{
            //    while (delimeterFlag == true)
            //    {
            //        int spaceIndex = this.paragraph.OriginalText.IndexOf(".", startIndex);
            //        int commaIndex = this.paragraph.OriginalText.IndexOf("?", startIndex);
            //        int colonIndex = this.paragraph.OriginalText.IndexOf("!", startIndex);

            //        if (spaceIndex != -1 || commaIndex != -1 || colonIndex != -1)
            //        {

            //            index = General.minValue(spaceIndex, commaIndex, colonIndex) - startIndex;

            //            string temp = this.paragraph.OriginalText.Substring(startIndex, index);

            //            //APPLY TEXT FILTERS HERE
            //            if (!String.IsNullOrEmpty(temp.Trim()))
            //            {
            //                if (temp.Trim().Length > 1)
            //                {
            //                    Sentence sentence = new Sentence(temp.Trim());
            //                    sentences.Add(sentence);
            //                }
            //            }
            //            startIndex = startIndex + index + 1;
            //        }
            //        else
            //        {
            //            delimeterFlag = false;
            //        }                    
            //    }

            //    //This check will ensure that we get in a sentence where last line is missing a period
            //    if ((startIndex - 1) < this.paragraph.OriginalText.Length && sentences.Count != 0)
            //    {
            //        string temp = this.paragraph.OriginalText.Substring(startIndex, this.paragraph.OriginalText.Length - startIndex);
            //        if (!temp.Equals("") && !temp.Equals("\r") && !temp.Equals("\n"))
            //        {
            //            if (temp.Trim().Length > 1)
            //            {
            //                Sentence sentence = new Sentence(temp.Trim());
            //                sentences.Add(sentence);
            //            }
            //        }
            //    }

            //    //Check if it was able to find sentence else just assign the text as a sentence
            //    if (sentences.Count == 0)
            //    {
            //        if (!String.IsNullOrEmpty(this.paragraph.OriginalText.Trim()))
            //        {
            //            if (this.paragraph.OriginalText.Trim().Length > 1)
            //            {
            //                Sentence sentence = new Sentence(this.paragraph.OriginalText.Trim());
            //                sentences.Add(sentence);
            //            }
            //        }
            //    }

            //    this.paragraph.BagOfSentences = sentences;
            //}
            //else
            //{
            //    paragraph.BagOfSentences = null;
            //}

            //return paragraph;
        }

        //PROPERTIES DEFINITION

        public Paragraph Paragraph
        {
            get { return paragraph; }
            set { paragraph = value; }
        }
    }
}