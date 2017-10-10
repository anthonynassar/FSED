using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public class ElementaryEvent
    {
        public string Id { get; set; }
        public List<MetadataAttributes> MetadataAttributes { get; set; }
        public List<string> Images { get; set; }

        public ElementaryEvent() { }

        public ElementaryEvent(string id, List<MetadataAttributes> metadataAttributes, List<string> images)
        {
            Id = id;
            MetadataAttributes = metadataAttributes;
            Images = images;
        }
    }
}
