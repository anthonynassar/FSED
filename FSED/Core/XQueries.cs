using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public class XQueries
    {
        public static string GetMetaID => "declare variable $meta as xs:string external;for $x in fn:doc('Input data/tbl_METAX.xml')/tbl_META/Meta where $x/Label=$meta return $x/ID/text()";
        public static string ExtractTime => "declare variable $id as xs:integer external;" +
            "<TimeStamps>" +
            "{for $dategroup at $indice in fn:doc('Input Data/TestInputXML.xml')/tbl_IMG_META/IMG_META/META[$id] " +
            "return" +
            "<TS id = '{$indice}' >" +
            "{$dategroup/text()}" +
            "</TS>}" +
            "</TimeStamps>";
        public static string ExtractGeo => "declare variable $Country as xs:integer external; " +
            "declare variable $Region as xs:integer external;" +
            "declare variable $City as xs:integer external;" +
            "declare variable $Street as xs:integer external;" +
            "<GeoLocations>" +
            "{for $geogroup at $indice in fn:doc('Input Data/TestInputXML.xml')/tbl_IMG_META/IMG_META return" +
            "<GL id = '{$indice}' >" +
            "<Country>{$geogroup/META[$Country]/text()}</Country>" +
            "<Region>{$geogroup/META[$Region]/text()}</Region>" +
            "<City>{$geogroup/META[$City]/text()}</City>" +
            "<Street>{$geogroup/META[$Street]/text()}</Street>" +
            "</GL>}" +
            "</GeoLocations>";
        public static string ExtractSocial => "declare variable $id as xs:integer external;" +
            "<Creators>" +
            "{for $ownergroup at $indice in fn:doc('Input Data/TestInputXML.xml')/tbl_IMG_META/IMG_META/META[$id] " +
            "return" +
            "<Creator id = '{$indice}' >" +
            "{$ownergroup/text()}" +
            "</Creator>}" +
            "</Creators>";

        public static string ExtractTimeArray => "for $dategroup at $indice in distinct-values(fn:doc('Extraction/Time/TimeStamps.xml')/TimeStamps/TS) order by $dategroup ascending return $dategroup";
        public static string GetT => "for $dategroup at $indice in fn:doc('Extraction/Time/TimeStamps.xml')/TimeStamps/TS return $dategroup/text()";
        public static string ExtractGeoArray => "for $geogroup at $indice in distinct-values(fn:doc('Extraction/Geo/GeoLocations.xml')/GeoLocations/GL/{0}) return $geogroup";
        public static string ExtractGeoArray2 => "for $geogroup at $indice in fn:doc('Extraction/Geo/GeoLocations.xml')/GeoLocations/GL/{0} return $geogroup/text()";
        public static string ExtractSocialArray => "for $ownergroup at $indice in distinct-values(fn:doc('Extraction/Social/Creators.xml')/Creators/Creator) return $ownergroup";
        public static string GetS => "for $ownergroup at $indice in fn:doc('Extraction/Social/Creators.xml')/Creators/Creator return $ownergroup/text()";
        public static string AddObjects => "for $x in fn:doc('Input data/TestInputXML.xml')/tbl_IMG_META/IMG_META return $x/@id_image/string()";
        public static string CurrentObjects => "for $x in fn:doc('Results/Elementary Event Candidates/EEC1.xml')/Elementary_Events/EE/Photos return $x/string()";

        // TESTING
        public static string Test => "for $i in 1 to 10 return $i* $i";
        public static string GetBook => "for $x in fn:doc('data/books.xml')//ITEM where $x/TITLE='Pride and Prejudice' return $x/PRICE/text()";

    }
}
