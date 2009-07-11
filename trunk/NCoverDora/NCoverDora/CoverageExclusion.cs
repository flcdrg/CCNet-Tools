using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NCoverDora
{
    [XmlRoot()]
    public class CoverageExclusion
    {
        [XmlElement(ElementName="ExclusionType")]
        public ExclusionType ExclusionType { get; set; }

        [XmlElement()]
        public string Pattern { get; set; }

        [XmlElement()]
        public bool IsRegex { get; set; }

        [XmlElement()]
        public bool Enabled { get; set; }

    }

    public enum ExclusionType
    {
        Assembly,

        Namespace,

        Class
    }
}
