using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NCoverDora
{
    /// <remarks/>
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class Method
    {
        /// <remarks/>
        [XmlElement("seqpnt", Form = XmlSchemaForm.Unqualified)]
        public SequencePoint[] SequencePoints { get; set; }

        /// <remarks/>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <remarks/>
        [XmlAttribute("excluded")]
        public bool Excluded { get; set; }

        /// <remarks/>
        [XmlAttribute("instrumented")]
        public bool Instrumented { get; set; }

        /// <remarks/>
        [XmlAttribute("class")]
        public string Class { get; set; }
    }
}