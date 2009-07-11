using System;
using System.Xml.Serialization;

namespace NCoverDora
{
    /// <remarks/>
    [Serializable()]
    [XmlType(AnonymousType = true)]
    public partial class SequencePoint
    {
        /// <remarks/>
        [XmlAttribute("visitcount")]
        public int VisitCount { get; set; }

        /// <remarks/>
        [XmlAttribute("line")]
        public int Line { get; set; }

        /// <remarks/>
        [XmlAttribute("column")]
        public int Column { get; set; }

        /// <remarks/>
        [XmlAttribute("endline")]
        public int EndLine { get; set; }

        /// <remarks/>
        [XmlAttribute("endcolumn")]
        public int EndColumn { get; set; }

        /// <remarks/>
        [XmlAttribute("excluded")]
        public bool Excluded { get; set; }

        /// <remarks/>
        [XmlAttribute("document")]
        public string SourceFile { get; set; }
    }
}