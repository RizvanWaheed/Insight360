using System;
using System.Collections.Generic;
using Insight360.BO;
using Insight360.BO.Parser;
//using TwitterService.Util;

/// <summary>
/// Summary description for SentenceParser
/// </summary>
/// 

namespace Insight360.BL.Parser
{
    public class SentenceParser
    {
        private Sentence sentence;

        public SentenceParser()
        {

        }

        public Sentence ParseSentence(Sentence sentence)
        {
            this.Sentence = sentence;
            //Declare local scope variables to hold start index and list for storing words
            //int index = 0;
            int startIndex = 0;
            List<Word> words = new List<Word>();
            //bool delimeterFlag = true;

            if (!string.IsNullOrEmpty(this.Sentence.OriginalText))
            {
                string[] splitted = this.sentence.OriginalText.Split(new char[] { ' ', ',', ';' });
                for (int i = 0; i < splitted.Length; i++)
                {
                    if (!string.IsNullOrEmpty(splitted[i]) && !this.numberCheck(splitted[i]))
                    {
                        Word word = new Word(this.cleanWord(splitted[i].ToLower()));
                        word.Index = this.sentence.OriginalText.IndexOf(splitted[i], startIndex);
                        startIndex = (int)word.Index + 1;
                        words.Add(word);                        
                    }
                }
                this.Sentence.BagOfWords = words;
            }
            else
            {
                this.Sentence.BagOfWords = null;
            }

            return sentence;
            

            //if (!this.Sentence.OriginalText.Equals("") && this.Sentence.OriginalText != null)
            //{
            //    while (delimeterFlag == true)
            //    {
            //        int spaceIndex = this.Sentence.OriginalText.IndexOf(" ", startIndex);
            //        int commaIndex = this.Sentence.OriginalText.IndexOf(",", startIndex);
            //        int colonIndex = this.Sentence.OriginalText.IndexOf(";", startIndex);

            //        if (spaceIndex != -1 || commaIndex != -1 || colonIndex != -1)
            //        {

            //            index = General.minValue(spaceIndex, commaIndex, colonIndex) - startIndex;

            //            string temp = this.Sentence.OriginalText.Substring(startIndex, index);
            //            temp = temp.Trim();
            //            //APPLY TEXT FILTERS HERE
            //            if (!String.IsNullOrEmpty(temp))
            //            {
            //                Word word   = new Word(this.cleanWord(temp.ToLower()));
            //                word.Index  = startIndex + index;

            //                if (this.numberCheck(word.Text) == false)
            //                {
            //                    words.Add(word);
            //                }
            //            }
            //            startIndex = startIndex + index + 1;
            //        }
            //        else
            //        {
            //            delimeterFlag = false;
            //        }

            //        //() != -1
            //    }

            //    //This check will ensure that we get in a word where last space is missing a period
            //    if ((startIndex - 1) < this.Sentence.OriginalText.Length && words.Count != 0)
            //    {
            //        string temp = this.Sentence.OriginalText.Substring(startIndex, this.Sentence.OriginalText.Length - startIndex);
            //        temp = temp.Trim();
            //        Word word   = new Word(this.cleanWord(temp.ToLower()));
            //        word.Index  = startIndex;

            //        if (this.numberCheck(word.Text) == false)
            //        {
            //            words.Add(word);
            //        }
            //    }

            //    //Check if it was able to find a word else just assign the sentence as a word
            //    if (words.Count == 0)
            //    {
            //        this.sentence.OriginalText = this.sentence.OriginalText.Trim();
            //        Word word   = new Word(this.cleanWord(this.Sentence.OriginalText.ToLower()));
            //        word.Index  = 0;

            //        if (this.numberCheck(word.Text) == false)
            //        {
            //            words.Add(word);
            //        }
            //    }

            //    this.Sentence.BagOfWords = words;
            //}
            //else
            //{
            //    this.Sentence.BagOfWords = null;
            //}

            //return sentence;
        }

        private string cleanWord(string wordText)
        {
            int index = 0;

            while ((index = wordText.IndexOf("\\")) != -1)
            {
                if (index < wordText.Length - 1)
                {
                    wordText.Substring(index, 1);
                }
                else
                {
                    if (wordText.IndexOf('s') == index + 1 || wordText.IndexOf('r') == index + 1 || wordText.IndexOf('n') == index + 1)
                    {
                        wordText.Substring(index, 2);
                    }
                }
            }

            return wordText;
        }

        private bool numberCheck(string text)
        {
            long number = 0;
            DateTime time = DateTime.Now;


            if (long.TryParse(text, out number) == true)
            {
                return true;
            }

            if (DateTime.TryParse(text, out time) == true)
            {
                return true;
            }

            if (text.Length <= 2)
            {
                return true;
            }

            return false;
            
        }

        public Sentence Sentence
        {
            get { return sentence; }
            set { sentence = value; }
        }
    }
}