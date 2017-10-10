using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public class MetadataAttributes
    {
        public string Label { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public Interval Interval { get; set; }

        public MetadataAttributes(string label, string type, string value, Interval interval)
        {
            Label = label;
            Type = type;
            Value = value;
            Interval = interval;
        }
    }

    public class MetadataAttribute<TValue>
    {
        public string Label { get; set; }
        public string Type { get; set; }
        public TValue Value { get; set; }

        public MetadataAttribute() { }

        public MetadataAttribute(string label, string type, TValue value)
        {
            Label = label;
            Type = type;
            Value = value;
        }
    }
}
