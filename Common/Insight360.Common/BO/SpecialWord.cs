using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SpecialWord
/// </summary>
/// 

namespace Insight360.BO
{

    public class SpecialWord
    {

        private long id;
        private string text;
        private string partOfSpeech;
        private char active;

        public SpecialWord()
        {
            this.id = 0;
            this.text = "";
            this.partOfSpeech = "";
            this.active = 'N';
        }

        public SpecialWord(long id, string text, string partOfSpeech, char active)
        {
            this.id = id;
            this.text = text;
            this.partOfSpeech = partOfSpeech;
            this.active = active;
        }

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public string PartOfSpeech
        {
            get { return partOfSpeech; }
            set { partOfSpeech = value; }
        }

        public char Active
        {
            get { return active; }
            set { active = value; }
        }
    }
}
