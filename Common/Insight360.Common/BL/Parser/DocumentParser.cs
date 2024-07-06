using System;
using System.Collections.Generic;
using Insight360.BO;
using Insight360.BO.Parser;

/// <summary>
/// Summary description for DocumentParser
/// </summary>
/// 

namespace Insight360.BL.Parser
{
    public class DocumentParser
    {

        private Document document;

        public DocumentParser()
        {
            this.document = new Document();
        }

        public DocumentParser(Document document)
        {
            this.document = document;
        }
        
        //The document is the major entity here so we start by finding the paragraphs
        public Document ParseDocument()
        {
            //Declare local scope variables to hold start index and list for storing paragraphs
            int index                   = 0;
            int startIndex              = 0;
            List<Paragraph> paragraphs  = new List<Paragraph>();

            if (!string.IsNullOrEmpty(this.document.OriginalText))
            {
                string[] splitted = this.document.OriginalText.Split(new char[] { '\n' });
                for (int i = 0; i < splitted.Length; i++)
                {
                    splitted[i] = splitted[i].Trim();
                    if (!string.IsNullOrEmpty(splitted[i]))
                    {
                        Paragraph paragraph = new Paragraph(splitted[i]);
                        paragraphs.Add(paragraph);
                    }
                }
                this.document.BagOfParagraphs = paragraphs;
            }
            else
            {
                this.document.BagOfParagraphs = null;
            }

            return document;

            //if (!document.OriginalText.Equals("") && document.OriginalText != null)
            //{
            //    while ((index = this.document.OriginalText.IndexOf("\n", startIndex)) > 0)
            //    {
            //        index = index - startIndex;
            //        string temp         = this.document.OriginalText.Substring(startIndex, index);
            //        if (!String.IsNullOrEmpty(temp.Trim()))
            //        {
            //            if (temp.Length > 1)
            //            {
            //                Paragraph paragraph = new Paragraph(temp);
            //                paragraphs.Add(paragraph);
            //            }
            //        }
            //        startIndex = startIndex + index + 1;
            //    }

            //    //This check will ensure that we get in the last paragraph as it might be missing a carriage statement
            //    if ((startIndex - 1) < this.document.OriginalText.Length && paragraphs.Count != 0)
            //    {
            //        string temp         = this.document.OriginalText.Substring(startIndex, this.document.OriginalText.Length - startIndex);

            //        if (!String.IsNullOrEmpty(temp.Trim()))
            //        {
            //            if (temp.Length > 1)
            //            {
            //                Paragraph paragraph = new Paragraph(temp);
            //                paragraphs.Add(paragraph);
            //            }
            //        }
            //    }

            //    //Check if it was able to find a paragraph else just assign the text as a paragraph
            //    if (paragraphs.Count == 0)
            //    {
            //        if (!String.IsNullOrEmpty(this.document.OriginalText.Trim()))
            //        {
            //            if (this.document.OriginalText.Length > 1)
            //            {
            //                Paragraph paragraph = new Paragraph(this.document.OriginalText);
            //                paragraphs.Add(paragraph);
            //            }
            //        }
            //    }
                
            //    this.document.BagOfParagraphs = paragraphs;
            //}
            //else
            //{
            //    document.BagOfParagraphs = null;
            //}

            //return document;
        }

        //PROPERTIES DEFINITION
        
        public Document Document
        {
            get { return document; }
            set { document = value; }
        }
    }
}