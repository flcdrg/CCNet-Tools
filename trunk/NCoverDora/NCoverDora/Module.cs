using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NCoverDora
{
    /// <remarks/>
    [Serializable()]
    [XmlType(AnonymousType = true)]
    public partial class Module
    {
        /// <remarks/>
        [XmlElement("method", Form = XmlSchemaForm.Unqualified)]
        public Method[] Methods { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string moduleId { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string name { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string assembly { get; set; }

        /// <remarks/>
        [XmlAttribute]
        public string assemblyIdentity { get; set; }
    }
}