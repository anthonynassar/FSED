using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public class Edge
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }

        public Edge(string id, string label, string value)
        {
            Id = id;
            Label = label;
            Value = value;
        }
    }
}
