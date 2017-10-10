using Saxon.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using colibri.lib;
using System.Xml.Linq;
using java.util;
using System.Globalization;

namespace FSED
{
    public class ExtractionStorageModule
    {
        public string DateTimeFormat { get; set; }
        public string BaseUri { get; set; }
        public List<ElementaryEvent> ElementaryEvents { get; set; }

        public ExtractionStorageModule(string baseUri)
        {
            BaseUri = baseUri;
            ElementaryEvents = new List<ElementaryEvent>();
    }

        public int GetMetaID(String meta)
        {
            Processor processor = new Processor();
            XQueryCompiler compiler = processor.NewXQueryCompiler();
            //compiler.DeclareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
            compiler.BaseUri = BaseUri;
            XQueryExecutable exp = compiler.Compile(XQueries.GetMetaID);
            XQueryEvaluator eval = exp.Load();
            eval.SetExternalVariable(new QName("meta"), new XdmAtomicValue(meta));
            //XdmAtomicValue result = (XdmAtomicValue)eval.EvaluateSingle();
            XdmItem value = eval.EvaluateSingle();
            IEnumerator e = value.GetEnumerator();
            while (e.MoveNext())
            {
                //XdmItem item = (XdmItem)e.Current;
                //Console.WriteLine(item.ToString());
                return Convert.ToInt32(e.Current.ToString());
            }
            return -1;
        }

        public void ExecuteQuery(string query, string destination, string dimension, string granularity)
        {
            Processor processor = new Processor();
            XQueryCompiler compiler = processor.NewXQueryCompiler();
            compiler.BaseUri = BaseUri;
            XQueryExecutable exp = compiler.Compile(query);
            XQueryEvaluator eval = exp.Load();

            if (dimension.Equals("Geo", StringComparison.InvariantCultureIgnoreCase))
            {
                int id = GetMetaID("Country");
                int id1 = GetMetaID("Region");
                int id2 = GetMetaID("City");
                int id3 = GetMetaID("Street");
                eval.SetExternalVariable(new QName("Country"), new XdmAtomicValue(id));
                eval.SetExternalVariable(new QName("Region"), new XdmAtomicValue(id1));
                eval.SetExternalVariable(new QName("City"), new XdmAtomicValue(id2));
                eval.SetExternalVariable(new QName("Street"), new XdmAtomicValue(id3));
                //expr.bindInt(new QName("Country"), id, null);
                //expr.bindInt(new QName("Region"), id1, null);
                //expr.bindInt(new QName("City"), id2, null);
                //expr.bindInt(new QName("Street"), id3, null);
            }
            else if (dimension.Equals("Time", StringComparison.InvariantCultureIgnoreCase))
            {
                int id = GetMetaID("Date/Time Original");
                eval.SetExternalVariable(new QName("id"), new XdmAtomicValue(id));
            }
            else if (dimension.Equals("Social", StringComparison.InvariantCultureIgnoreCase))
            {
                int id = GetMetaID("Artist");
                eval.SetExternalVariable(new QName("id"), new XdmAtomicValue(id));
            }

            Serializer qout = new Serializer();
            qout.SetOutputProperty(Serializer.METHOD, "xml");
            qout.SetOutputProperty(Serializer.INDENT, "yes");
            FileStream outStream = new FileStream(BaseUri + destination, FileMode.Create, FileAccess.Write);
            qout.SetOutputStream(outStream);
            Console.WriteLine("Output written to " + destination);
            eval.Run(qout);
            outStream.Dispose();
            outStream.Close();
            qout.Close();
        }

        public void GetElementaryEventCandidates(Relation relation, string[] dimensions)
        {
            int nbAttr = dimensions.Length;
            Lattice lattice = new HybridLattice(relation);
            Iterator it = lattice.conceptIterator(Traversal.TOP_ATTRSIZE);

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            XmlElement root = (XmlElement)doc.AppendChild(doc.CreateElement("Elementary_Events"));

            while (it.hasNext())
            {
                Concept concept = it.next() as Concept;
                ElementaryEvent elementaryEvent = new ElementaryEvent();
                if (concept.getAttributes().size() == nbAttr)
                {
                    ComparableSet cs = concept.getObjects();
                    Iterator objects = cs.iterator();
                    ComparableSet cs1 = concept.getAttributes();
                    Iterator attributes = cs1.iterator();
                    String[] compare = new String[nbAttr];
                    int j = 0;
                    while (attributes.hasNext())
                    {
                        string temp = attributes.next().ToString();
                        compare[j] = temp.Split('_')[0];
                        //int prefix = temp.indexOf("|");
                        //compare[j] = temp.substring(0, prefix);
                        j++;
                    }
                    Array.Sort(compare);
                    bool areEqual = compare.SequenceEqual(dimensions);
                    //bool areEqual = this.bruteforce2(compare, dimensions);
                    if (areEqual)
                    {
                        XmlElement ee = doc.CreateElement("EE");
                        string attrId = Guid.NewGuid().ToString();
                        elementaryEvent.Id = attrId;
                        ee.SetAttribute("Id", attrId);
                        List<string> imagesId = new List<string>();

                        while (objects.hasNext())
                        {
                            //Element obj = new Element("Photos");
                            XmlElement obj = doc.CreateElement("Photos");
                            string objectId = objects.next().ToString();
                            XmlText objValue = doc.CreateTextNode(objectId);
                            imagesId.Add(objectId);
                            obj.AppendChild(objValue);
                            ee.AppendChild(obj);
                        }
                        elementaryEvent.Images = imagesId;
                        List<MetadataAttributes> attributesList = new List<MetadataAttributes>();

                        attributes = cs1.iterator();
                        while (attributes.hasNext())
                        {
                            XmlElement obj = doc.CreateElement("Attributes");
                            string attrValue = attributes.next().ToString();
                            XmlText objValue = doc.CreateTextNode(attrValue);
                            MetadataAttributes currentAttr = ConvertStringMetadataAttribute(attrValue);
                            attributesList.Add(currentAttr);
                            obj.AppendChild(objValue);
                            ee.AppendChild(obj);
                        }
                        root.AppendChild(ee);
                        elementaryEvent.MetadataAttributes = attributesList;
                        ElementaryEvents.Add(elementaryEvent);
                    }
                    
                }
            }
            doc.AppendChild(root);
            doc.InsertBefore(xmldecl, root);

            doc.Save(BaseUri + @"Results\Elementary Event Candidates\EEC1.xml");
            //Console.WriteLine(doc.OuterXml);
            StringBuilder sb = new StringBuilder();
            TextWriter tr = new StringWriter(sb);
            XmlTextWriter wr = new XmlTextWriter(tr);
            wr.Formatting = Formatting.Indented;
            doc.Save(wr);
            wr.Close();
        }

        private MetadataAttributes ConvertStringMetadataAttribute(string attrValue) 
        {
            string[] input = attrValue.Split('_');
            Interval interval = new Interval();
            if(input[0] == "Time")
            {
                //[10/9/2017 12:41:01 PM;10/9/2017 1:08:54 PM[
                // [17-08-15 09:16:00 AM;18-08-15 09:16:00 AM[
                string[] boundaries = input[1].Split(';');
                string lowBound = boundaries[0].Substring(1);
                string dateTimeFormat = Helpers.DetectDateTimeFormat(lowBound);
                //dateTimeFormat = "dd-MM-yy HH:mm:ss tt";
                interval.BorneInf = DateTime.ParseExact(lowBound, dateTimeFormat, null);
                string upperBound = boundaries[1].Substring(0, boundaries[0].Length - 1);
                //dateTimeFormat = Helpers.DetectDateTimeFormat(upperBound);
                interval.BorneSup = DateTime.ParseExact(upperBound, dateTimeFormat, null);

                return new MetadataAttributes(input[0], "Interval", "", interval);
            }
            else
            {
                return new MetadataAttributes(input[0], "Nominal", input[1], null);
            }
        }

        public void Combine(string firstFile, string secondFile, string destination)
        {
            XDocument xml1 = XDocument.Load(BaseUri + firstFile);
            XDocument xml2 = XDocument.Load(BaseUri + secondFile);

            //Combine and remove duplicates
            //var combinedUnique = xml1.Descendants("AllNodes").Union(xml2.Descendants("AllNodes"));

            //Combine and keep duplicates
            var combinedWithDups = xml1.Descendants("EE").Concat(xml2.Descendants("EE"));

            XDocument doc = new XDocument( new XElement("Elementary_Events", combinedWithDups));
            doc.Save(BaseUri + destination);
        }

        public void GetRemainingCandidates(Relation relation, string v, string[] dimensions, string[] dim, List<string> diff)
        {
            int nbAttr = dimensions.Length;
            Lattice lattice = new HybridLattice(relation);
            Iterator it = lattice.conceptIterator(Traversal.TOP_ATTRSIZE);

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            XmlElement root = (XmlElement)doc.AppendChild(doc.CreateElement("Elementary_Events"));
            while (it.hasNext())
            {
                Concept c = it.next() as Concept;
                ElementaryEvent elementaryEvent = new ElementaryEvent();
                if (c.getAttributes().size() > nbAttr && c.getObjects().isEmpty() == false)
                {
                    ComparableSet cs = c.getObjects();
                    Iterator objects = cs.iterator();
                    ComparableSet cs1 = c.getAttributes();
                    Iterator attributes = cs1.iterator();
                    string[] compare = new string[dim.Length];
                    int j = 0;
                    while (attributes.hasNext())
                    {
                        string temp = attributes.next().ToString();
                        compare[j] = temp.Split('_')[0];
                        j++;
                    }

                    bool areEqual = true;
                    foreach (var item in dimensions)
                    {
                        areEqual &= compare.Contains(item);
                    }
                    
                    List<string> photos = new List<string>();
                    string photo = "";
                    if (areEqual)
                    {
                        XmlElement ee = doc.CreateElement("EE");
                        var attrId = Guid.NewGuid().ToString();
                        ee.SetAttribute("Id", attrId);
                        elementaryEvent.Id = attrId;
                        List<string> imagesId = new List<string>();

                        while (objects.hasNext())
                        {
                            photo = objects.next().ToString();
                            if (diff.Contains(photo))
                            {
                                photos.Add(photo);
                            }
                        }
                        if (photos.Count > 0)
                        {
                            foreach (var item in photos)
                            {
                                XmlElement obj = doc.CreateElement("Photos");
                                XmlText objValue = doc.CreateTextNode(item);
                                imagesId.Add(item);
                                obj.AppendChild(objValue);
                                ee.AppendChild(obj);
                            }
                            attributes = cs1.iterator();
                            elementaryEvent.Images = imagesId;
                            List<MetadataAttributes> attributesList = new List<MetadataAttributes>();

                            while (attributes.hasNext())
                            {
                                XmlElement obj = doc.CreateElement("Attributes");
                                string attrValue = attributes.next().ToString();
                                XmlText objValue = doc.CreateTextNode(attrValue);
                                MetadataAttributes currentAttr = ConvertStringMetadataAttribute(attrValue);
                                attributesList.Add(currentAttr);
                                obj.AppendChild(objValue);
                                ee.AppendChild(obj);
                            }
                            root.AppendChild(ee);
                            elementaryEvent.MetadataAttributes = attributesList;
                            ElementaryEvents.Add(elementaryEvent);
                        }
                    }
                }
            }
            doc.AppendChild(root);
            doc.InsertBefore(xmldecl, root);

            doc.Save(BaseUri + @"Results\Elementary Event Candidates\EEC2.xml");
            //Console.WriteLine(doc.OuterXml);
            StringBuilder sb = new StringBuilder();
            TextWriter tr = new StringWriter(sb);
            XmlTextWriter wr = new XmlTextWriter(tr);
            wr.Formatting = Formatting.Indented;
            doc.Save(wr);
            wr.Close();
        }

        public List<string> GetObjects(string query)
        {
            List<string> objects = new List<string>();

            Processor processor = new Processor();
            XQueryCompiler compiler = processor.NewXQueryCompiler();
            compiler.BaseUri = BaseUri;
            XQueryExecutable exp = compiler.Compile(query);
            XQueryEvaluator eval = exp.Load();
            XdmValue value = eval.Evaluate();
            IEnumerator e = value.GetEnumerator();

            while (e.MoveNext())
            {
                objects.Add(e.Current.ToString());
            }
            return objects;
        }

        public void ExtractAll(string[] dimensions, string geoGranularity)
        {
            foreach (var dim in dimensions)
            {
                if (dim.Equals("Time", StringComparison.InvariantCultureIgnoreCase))
                {
                    ExecuteQuery(XQueries.ExtractTime, @"Extraction\Time\TimeStamps.xml", "Time", "");
                }
                else if (dim.Equals("Geo", StringComparison.InvariantCultureIgnoreCase))
                {
                    ExecuteQuery(XQueries.ExtractGeo, @"Extraction\Geo\GeoLocations.xml", "Geo", "");
                }
                else if (dim.Equals("Social", StringComparison.InvariantCultureIgnoreCase))
                {
                    ExecuteQuery(XQueries.ExtractSocial, @"Extraction\Social\Creators.xml", "Social", "");
                }
            }
        }

        public List<MetadataAttributes> GetTime(string query)
        {
            List<MetadataAttributes> ts = new List<MetadataAttributes>();

            Processor processor = new Processor();
            XQueryCompiler compiler = processor.NewXQueryCompiler();
            compiler.BaseUri = BaseUri;
            XQueryExecutable exp = compiler.Compile(query);
            XQueryEvaluator eval = exp.Load();
            //eval.Evalua
            XdmValue value = eval.Evaluate();
            IEnumerator e = value.GetEnumerator();

            while (e.MoveNext())
            {
                Interval interval = new Interval();
                MetadataAttributes time = new MetadataAttributes("Time", "Interval", e.Current.ToString(), interval);
                ts.Add(time);
            }
            return ts;
        }

        public List<MetadataAttributes> GetSocial(string query)
        {
            List<MetadataAttributes> ts = new List<MetadataAttributes>();

            Processor processor = new Processor();
            XQueryCompiler compiler = processor.NewXQueryCompiler();
            compiler.BaseUri = BaseUri;
            XQueryExecutable exp = compiler.Compile(query);
            XQueryEvaluator eval = exp.Load();
            XdmValue value = eval.Evaluate();
            IEnumerator e = value.GetEnumerator();

            while (e.MoveNext())
            {
                Interval interval = new Interval();
                MetadataAttributes social = new MetadataAttributes("Social", "Nominal", e.Current.ToString(), interval);
                ts.Add(social);
            }

            return ts;
        }

        public List<MetadataAttributes> GetGeo(string granularity, bool distinct = true)
        {
            string query = "";

            if (granularity.Equals("Country", StringComparison.InvariantCultureIgnoreCase))
                query = String.Format(distinct ? XQueries.ExtractGeoArray : XQueries.ExtractGeoArray2, "Country");
            else if (granularity.Equals("Region", StringComparison.InvariantCultureIgnoreCase))
                query = String.Format(distinct ? XQueries.ExtractGeoArray : XQueries.ExtractGeoArray2, "Region");
            else if (granularity.Equals("City", StringComparison.InvariantCultureIgnoreCase))
                query = String.Format(distinct ? XQueries.ExtractGeoArray : XQueries.ExtractGeoArray2, "City");
            else
                query = String.Format(distinct ? XQueries.ExtractGeoArray : XQueries.ExtractGeoArray2, "Street"); ;

            List<MetadataAttributes> ts = new List<MetadataAttributes>();

            Processor processor = new Processor();
            XQueryCompiler compiler = processor.NewXQueryCompiler();
            compiler.BaseUri = BaseUri;
            XQueryExecutable exp = compiler.Compile(query);
            XQueryEvaluator eval = exp.Load();
            XdmValue value = eval.Evaluate();
            IEnumerator e = value.GetEnumerator();

            while (e.MoveNext())
            {
                Interval interval = new Interval();
                var current = e.Current.ToString();
                if (!current.Equals("")) // a revoir avec elio
                {
                    MetadataAttributes geo = new MetadataAttributes("Geo", "Nominal", current, interval);
                    ts.Add(geo);
                }
            }
            //ts.Reverse();

            return ts;
        }

        public List<Interval> ConstructInterval(List<MetadataAttributes> input, string timeGranularity)
        {
            List<Interval> output = new List<Interval>();
            string tmp = input.ElementAt(0).Value;
            if (tmp.Equals("0000-00-00"))
                tmp = input.ElementAt(1).Value;

            string tmp1 = input.Last().Value;
            //DateTime tmin = DateTime.ParseExact(tmp, "yyyy-MM-dd HH:mm:ss", null);
            //DateTime tmax = DateTime.ParseExact(tmp1, "yyyy-MM-dd HH:mm:ss", null);
            DateTime tmin = DateTime.ParseExact(tmp, DateTimeFormat, null);
            DateTime tmax = DateTime.ParseExact(tmp1, DateTimeFormat, null);

            int sizefor = 0;
            TimeSpan duration = tmax - tmin;
            switch (timeGranularity)
            {
                case "year":
                    sizefor = (int)duration.TotalDays / 365;
                    break;
                case "month":
                    sizefor = (int)duration.TotalDays / 30;
                    break;
                case "week":
                    sizefor = (int)duration.TotalDays / 7;
                    break;
                case "day":
                    sizefor = (int)duration.TotalDays;
                    break;
                case "hour":
                    sizefor = (int)duration.TotalHours;
                    break;
                case "minute":
                    sizefor = (int)duration.TotalMinutes;
                    break;
                case "second":
                    sizefor = (int)duration.TotalSeconds;
                    break;
            }
            for (int i = 0; i <= sizefor; i++)
            {
                Interval tempInterval = new Interval();
                tempInterval.BorneInf = AddGranularity(timeGranularity, tmin, i);
                tempInterval.BorneSup = AddGranularity(timeGranularity, tmin, i + 1);

                if (tempInterval.BorneSup.CompareTo(tmax) > 0)
                {
                    tempInterval.BorneSup = tmax;
                    output.Add(tempInterval);
                    break;
                }
                else
                {
                    output.Add(tempInterval);
                }
            }
            return output;
        }

        private DateTime AddGranularity(string timeGranularity, DateTime input, int value)
        {
            DateTime output = new DateTime();

            switch (timeGranularity)
            {
                case "year":
                    output = input.AddYears(value);
                    break;
                case "month":
                    output = input.AddMonths(value);
                    break;
                case "week":
                    output = input.AddDays(value * 7);
                    break;
                case "day":
                    output = input.AddDays(value);
                    break;
                case "hour":
                    output = input.AddHours(value);
                    break;
                case "minute":
                    output = input.AddMinutes(value);
                    break;
                case "second":
                    output = input.AddSeconds(value);
                    break;
            }
            return output;
        }

        public string GetCorrespondingInterval(string time, List<Interval> intervals)
        {
            //DateTime ts = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", null);
            DateTime ts = DateTime.ParseExact(time, DateTimeFormat, null);
            foreach (var interval in intervals)
            {
                if (ts.CompareTo(interval.BorneInf) >= 0 && ts.CompareTo(interval.BorneSup) < 0)
                {
                    return "[" + interval.BorneInf.ToString("u") + ";" + interval.BorneSup.ToString("u") + "[";
                }
            }

            Interval last = intervals.Last();
            if (last.BorneSup.CompareTo(ts) == 0)
                return "[" + last.BorneInf.ToString("u") + ";" + last.BorneSup.ToString("u") + "[";
            else
                return "ERROR";

        }

        public void WriteXml(List<Edge> relations)
        {
            XmlDocument doc = new XmlDocument();
            // Create an XML declaration. 
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";

            // create root node
            XmlElement root = (XmlElement)doc.AppendChild(doc.CreateElement("Relations"));
            double cnt = 0;
            int totalRelations = relations.Count;
            foreach (var item in relations)
            {
                if (cnt % 10000 == 0)
                    Console.WriteLine("Progress: " + ((cnt / totalRelations) * 100) + "%");
                Console.WriteLine("Progress: 100%");

                XmlElement relation = doc.CreateElement("Relation");
                root.AppendChild(relation);

                // 1
                XmlElement imageId = doc.CreateElement("Image_ID");
                XmlText id = doc.CreateTextNode(item.Id);
                imageId.AppendChild(id);
                relation.AppendChild(imageId);
                // 2
                XmlElement metaAttr = doc.CreateElement("Metadata_attribute");
                metaAttr.SetAttribute("Label", item.Label);
                XmlText metaValue = doc.CreateTextNode(item.Value);
                metaAttr.AppendChild(metaValue);
                relation.AppendChild(metaAttr);

                root.AppendChild(relation);
                cnt++;
            }
            doc.AppendChild(root);
            doc.InsertBefore(xmldecl, root);

            // Media for XML data output
            doc.Save(BaseUri + @"Results\CrossTable\JTest.xml");
            StringBuilder sb = new StringBuilder();
            TextWriter tr = new StringWriter(sb);
            XmlTextWriter wr = new XmlTextWriter(tr);
            wr.Formatting = Formatting.Indented;
            doc.Save(wr);
            wr.Close();
            //return sb.ToString();
        }

    }
}