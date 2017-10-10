using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSED;
using colibri.lib;
using System.Data.SqlClient;

namespace FSEDTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string geoGranularity = "City";
            string timeGranularity = "day";
            bool writeXML = false;

            // Enable this on cloud
            //ExtractionStorageModule esm = new ExtractionStorageModule("D:/home/site/wwwroot/MetadataExtractor/");
            // Enable this locally
            ExtractionStorageModule esm = new ExtractionStorageModule("C:/Users/Nassar/Source/Repos/F-SED/F-SED/bin/Debug/");
            string[] dim = { "Geo", "Social", "Time" }; // all dimensions
            string[] dimensions = { "Geo", "Time" }; // required dimensions

            Console.WriteLine("Extracting all metadata...");
            esm.ExtractAll(dim, geoGranularity);

            List<MetadataAttributes> timeDistinctMetadata = new List<MetadataAttributes>();
            List<MetadataAttributes> timeMetadata = new List<MetadataAttributes>();
            List<MetadataAttributes> geoDistinctMetadata = new List<MetadataAttributes>();
            List<MetadataAttributes> geoMetadata = new List<MetadataAttributes>();
            List<MetadataAttributes> socialDistinctMetadata = new List<MetadataAttributes>();
            List<MetadataAttributes> socialMetadata = new List<MetadataAttributes>();

            Console.WriteLine("Extracting distinct and duplicate time metadata...");
            timeDistinctMetadata = esm.GetTime(XQueries.ExtractTimeArray);
            timeMetadata = esm.GetTime(XQueries.GetT);

            Console.WriteLine("Extracting distinct and duplicate geo metadata...");
            geoDistinctMetadata = esm.GetGeo(geoGranularity);
            geoMetadata = esm.GetGeo(geoGranularity, false);

            Console.WriteLine("Extracting distinct and duplicate social metadata...");
            socialDistinctMetadata = esm.GetSocial(XQueries.ExtractSocialArray);
            socialMetadata = esm.GetSocial(XQueries.GetS);

            Console.WriteLine("Creating time intervals");
            List<Interval> intervals = new List<Interval>();
            //Detect time format
            Console.WriteLine(timeDistinctMetadata[0].Value);
            esm.DateTimeFormat = Helpers.DetectDateTimeFormat(timeDistinctMetadata[0].Value);
            intervals = esm.ConstructInterval(timeDistinctMetadata, timeGranularity);

            Console.WriteLine("Retrieving a list of objects...");
            List<string> allObjects = esm.GetObjects(XQueries.AddObjects);
            List<FSED.Edge> relations = new List<FSED.Edge>();

            Console.WriteLine("Extracting time/object relations...");
            for (int i = 0; i < allObjects.Count; i++)
            {
                string interval = esm.GetCorrespondingInterval(timeMetadata.ElementAt(i).Value, intervals);
                FSED.Edge newRelation = new FSED.Edge(allObjects.ElementAt(i), "Time", interval);
                relations.Add(newRelation);
            }

            Console.WriteLine("Extracting geo/object relations...");
            for (int i = 0; i < allObjects.Count; i++)
            {
                FSED.Edge newRelation = new FSED.Edge(allObjects.ElementAt(i), "Geo", geoMetadata.ElementAt(i).Value);
                relations.Add(newRelation);
            }

            Console.WriteLine("Extracting social/object relations...");
            for (int i = 0; i < allObjects.Count; i++)
            {
                FSED.Edge newRelation = new FSED.Edge(allObjects.ElementAt(i), "Social", socialMetadata.ElementAt(i).Value);
                relations.Add(newRelation);
            }

            Console.WriteLine("Resetting some lists...");
            timeDistinctMetadata = new List<MetadataAttributes>();
            timeMetadata = new List<MetadataAttributes>();
            geoDistinctMetadata = new List<MetadataAttributes>();
            geoMetadata = new List<MetadataAttributes>();
            socialDistinctMetadata = new List<MetadataAttributes>();
            socialMetadata = new List<MetadataAttributes>();
            intervals = new List<Interval>();


            Console.WriteLine("Number of detected relations: " + relations.Count);
            if (writeXML)
            {
                Console.WriteLine("Writing relations in XML...");
                esm.WriteXml(relations);
            }

            Console.WriteLine("Building HashRelation object");
            Relation relation = new HashRelation();
            foreach (var item in relations)
            {
                relation.add(item.Id, item.Label + "_" + item.Value);
            }

            Console.WriteLine("Retrieving elementary events...");
            esm.GetElementaryEventCandidates(relation, dimensions);

            List<string> currentphotos = esm.GetObjects(XQueries.CurrentObjects);
            HashSet<string> consideredObjects = new HashSet<string>(currentphotos);
            HashSet<string> remainingObjects = new HashSet<string>();
            foreach (var item in allObjects)
            {
                if (!consideredObjects.Contains(item))
                {
                    remainingObjects.Add(item);
                }
            }

            List<string> diff = remainingObjects.ToList<string>();

            Console.WriteLine("Retrieving undetected elementary events...");
            esm.GetRemainingCandidates(relation, @"Results\Elementary Event Candidates\EEC2.xml", dimensions, dim, diff);
            esm.Combine(@"Results\Elementary Event Candidates\EEC1.xml", @"Results\Elementary Event Candidates\EEC2.xml", @"Results\Elementary Events\EE.xml");

            List<ElementaryEvent> elementaryEvents = new List<ElementaryEvent>();
            elementaryEvents = esm.ElementaryEvents;
            Console.WriteLine("Number of elementary events: " + elementaryEvents.Count);

            // connection sql
            string dbConncetionString = Constants.DbConncetionString;
            // this Id will be detected directly from the uploaded blob (input xml file)
            string sharingSpaceId = "04e3dc35-a9e9-4504-a874-df90d783f038";

            // constraints of current event
            // retrieve them by executing SQL queries
            Dictionary<string, object> eventDimensionValues = new Dictionary<string, object>();
            foreach (var dimension in dimensions)
            {
                switch (dimension)
                {
                    case "Time":
                        Interval currentSSInterval = new Interval();
                        string lowBound = "01/01/1980 12:00:00", upperBound = "01/01/2035 12:00:00";
                        lowBound = Services.GetConstraints(dbConncetionString, sharingSpaceId, "begin");
                        upperBound = Services.GetConstraints(dbConncetionString, sharingSpaceId, "end");
                        string dateTimeFormat = Helpers.DetectDateTimeFormat(lowBound);
                        currentSSInterval.BorneInf = DateTime.ParseExact(lowBound, dateTimeFormat, null);
                        currentSSInterval.BorneSup = DateTime.ParseExact(upperBound, dateTimeFormat, null);
                        eventDimensionValues.Add("Time", currentSSInterval);
                        break;
                    case "Social":
                        string currentSSOwner = Services.GetConstraints(dbConncetionString, sharingSpaceId, "owner");
                        eventDimensionValues.Add("Social", currentSSOwner);
                        break;
                    case "Geo":
                        string currentSSAddress = Services.GetConstraints(dbConncetionString, sharingSpaceId, "fulladdress");
                        string currentSSGeoValue = Helpers.GetSpecificValue(currentSSAddress, geoGranularity);
                        eventDimensionValues.Add("Geo", currentSSGeoValue);
                        break;
                    default:
                        break;
                }
            }
            
            // Verify events
            bool timeCheck = false, geoCheck = false, socialCheck = false;
            bool timeExist = dimensions.Contains("Time"), geoExist = dimensions.Contains("Geo"), socialExist = dimensions.Contains("Social");
            List<string> irrelevantObjects = new List<string>(allObjects);

            foreach (var item in elementaryEvents)
            {
                foreach (var attr in item.MetadataAttributes)
                {
                    if (attr.Label.Equals("Time") && timeExist)
                        timeCheck = Helpers.CompareTimeIntervals(eventDimensionValues["Time"] as Interval, attr.Interval);
                    else if (attr.Label.Equals("Geo") && geoExist)
                        geoCheck = attr.Value.Equals(eventDimensionValues["Geo"]);
                    else if (attr.Label.Equals("Social") && socialExist)
                        socialCheck = attr.Value.Equals(eventDimensionValues["Social"]);
                }

                if ((timeCheck || !timeExist) && (geoCheck || !geoExist) && (socialCheck || !socialExist))
                {
                    Console.WriteLine("Event captured " + item.Id);
                    foreach (var img in item.Images)
                    {
                        irrelevantObjects.RemoveAll(im => im.Equals(img));
                    }
                    break;
                }
            }

            // unlink irrelevant photos from current sharing space
            foreach (var item in irrelevantObjects)
            {
                Console.WriteLine("Unlinking image: " + item);
                //Services.RemoveObjectFromSharingSpace(connectionString, item);
            }

            // update events to verified
            Console.WriteLine("Updating event to verified");
            Services.UpdateSharingSpace(dbConncetionString, sharingSpaceId);
            
            // END
            Console.WriteLine("Done");
            while (true) ;

        }
    }
}
