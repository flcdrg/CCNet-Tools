using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NCoverDora
{
    [XmlRoot()]
    public class ModuleThreshold
    {

        [XmlAttribute("ModuleName")]
        public string ModuleName { get; set; }

        [XmlAttribute("SatisfactoryCoverage")]
        public double SatisfactoryCoverage { get; set; }

    }
}
