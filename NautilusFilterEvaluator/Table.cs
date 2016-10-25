using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusFilterEvaluator
{
    class Table
    {
        public int TableId { get; set; }
        public string DatabaseName { get; set; }
        public int? ExtraTableId { get; set; }
        public string UniqueKey { get; set; }

        public override string ToString()
        {
            return DatabaseName;
        }
    }
}
