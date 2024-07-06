using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Insight360.BO;
using Insight360.BO.Parser;

namespace Insight360.BL.Parser
{
    public class DocumentManager
    {
        public static long wordPosition = 0;

        public static ArrayList parseDocument(String fileContent, Hashtable filterList)
        {
            ArrayList wordList = new ArrayList();

            try
            {
                wordPosition = 0;
                wordList = new ArrayList();
                Document document = new Document(fileContent);

                document = parseParagraphs(document);

                for (int i = 0; i < document.BagOfParagraphs.Count; i++)
                {
                    Paragraph paragraph = document.BagOfParagraphs[i];
                    paragraph = parseSentences(paragraph);

                    for (int j = 0; j < paragraph.BagOfSentences.Count; j++)
                    {
                        Sentence sentence = paragraph.BagOfSentences[j];
                        ArrayList words = parseWords(sentence);

                        foreach (Word wd in words)
                        {
                            if (filterList.ContainsKey(wd.Text.ToLower()) == false)
                            {
                                wordList.Add(wd);
                            }
                        }

                    }
                }
                return wordList;
            }
            catch (Exception ex)
            {

            }
            return wordList;
        }

        public static Document parseParagraphs(Document document)
        {
            DocumentParser parser = new DocumentParser(document);
            document = parser.ParseDocument();
            return document;
        }

        public static Paragraph parseSentences(Paragraph paragraph)
        {
            ParagraphParser parser = new ParagraphParser();
            paragraph = parser.ParseParagraph(paragraph);
            return paragraph;
        }

        public static ArrayList parseWords(Sentence sentence)
        {
            SentenceParser parser = new SentenceParser();
            sentence = parser.ParseSentence(sentence);
            ArrayList wordsList = new ArrayList();

            foreach (Word wd in sentence.BagOfWords)
            {
                wordPosition = wd.Index - wd.Text.Length;
                //wd.Index = wordPosition;

                if (wd.Text.Contains("@") == false && wd.Text.Equals("rt") == false)
                {
                    wordsList.Add(wd);
                }
            }

            return wordsList;
        }
    }
}
