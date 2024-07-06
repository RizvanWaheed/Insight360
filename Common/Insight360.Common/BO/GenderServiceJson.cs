using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight360.BO.Gender
{
    public class Result
    {
        public string gender { get; set; }
        public bool status { get; set; }
    }

    public class GenderJson
    {
        public Result result { get; set; }
    }
}
