using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NCoverDora
{
    internal class Serialisation
    {
        public static T Deserialise<T>(XNode element)
        {
            using (XmlReader reader = element.CreateReader())
            {
                var s = new XmlSerializer(typeof (T));
                return (T) s.Deserialize(reader);
            }
        }
    }
}