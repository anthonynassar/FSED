using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public class Interval
    {
        public DateTime BorneSup { get; set; }
        public DateTime BorneInf { get; set; }

        public Interval() { }
        public Interval(DateTime inf, DateTime sup)
        {
            BorneInf = inf;
            BorneSup = sup;
        }
    }
}
