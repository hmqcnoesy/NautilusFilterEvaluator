using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusFilterEvaluator
{
    class Filter
    {
        public int FilterId { get; set; }
        public string Name { get; set; }
        public int TableId { get; set; }
        public string WhereStatement { get; set; }
        public string Sql { get; set; }
        public string Status { get; set; }
        public long MillisecondsToComplete { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
