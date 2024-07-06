using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.BO
{
    public class Location
    {
        public Location() { }
        public int Id;
        public int SearchTerm_Id;
        public string name;
        public decimal latitude;
        public decimal longitude;
        public string TwitterGranuality;
        public string TwitterPlaceId;
    }
}
