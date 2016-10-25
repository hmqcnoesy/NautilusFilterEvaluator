using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusFilterEvaluator
{
    class FilterArg
    {
        public int FilterArgId { get; set; }
        public int FilterId { get; set; }
        public string Name { get; set; }
        public string ArgType { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{FilterId} {Name} {Value}";
        }
    }
}
