using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace NCoverDora
{
    class Program
    {
        public XDocument Serialize<T>(T source)
        {
            XDocument target = new XDocument();
            XmlSerializer s = new XmlSerializer(typeof(T));
            System.Xml.XmlWriter writer = target.CreateWriter();
            s.Serialize(writer, source);
            writer.Close();
            return target;
        }
        public XDocument Deserialize<T>(T source)
        {
            XDocument target = new XDocument();
            XmlSerializer s = new XmlSerializer(typeof(T));
            System.Xml.XmlWriter writer = target.CreateWriter();
            s.Serialize(writer, source);
            writer.Close();
            return target;
        }

        static void Main(string[] args)
        {

            var namespaceExclusions = new List<string>();
            var assemblyExclusions = new List<string>();
            var classExclusions = new List<string>();

            // load config
            var config = XDocument.Load("NCoverExplorer.config", LoadOptions.None);


            var coverageExclusionNodes = config.XPathSelectElements("//CoverageExclusion");
            foreach (var node in coverageExclusionNodes)
            {
                using (var reader = node.CreateReader())
                {
                    XmlSerializer s = new XmlSerializer(typeof(CoverageExclusion));
                    var c = (CoverageExclusion) s.Deserialize(reader);
                    
                    string pattern;

                    if (c.IsRegex)
                        pattern = c.Pattern;
                    else
                    {
                        pattern = "^" + Regex.Escape(c.Pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                    }

                    switch (c.ExclusionType)
                    {
                        case ExclusionType.Assembly:
                            assemblyExclusions.Add(pattern);
                        case ExclusionType.Namespace:
                            namespaceExclusions.Add(pattern);
                        case ExclusionType.Class:
                            classExclusions.Add(pattern);
                    }
                }
            }

            var document = new System.Xml.XPath.XPathDocument("coverage.xml");


            var navigator = document.CreateNavigator();
            var nodes = navigator.Select("//module");

            while (nodes.MoveNext())
            {
                var module = nodes.Current;

                Debug.WriteLine(nodes.Current.Name);

                var assemblyName = module.GetAttribute("assembly", "");
                bool excluded = false;

                // check module name against assemblies
                foreach (var pattern in assemblyExclusions)
                {
                    if (Regex.IsMatch(assemblyName, pattern))
                    {
                        excluded = true;
                        break;
                    }
                }

                if (excluded)
                    continue;

                // seqpnt visitcount
                int count = 0;
                int visited = 0;

                var kids = module.Select("./method/seqpnt");

                while (kids.MoveNext())
                {
                    count++;

                    var visitcount = kids.Current.GetAttribute("visitcount", "");

                    if (visitcount == "1")
                        visited++;
                }

                Console.WriteLine("Module {0}, {1} of {2} ({3:F})", module.GetAttribute("assembly", ""), visited, count, visited / count);
            }

            Console.Read();

        }

    }
}
