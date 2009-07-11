using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace NCoverDora
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            var namespaceExclusions = new List<string>();
            var assemblyExclusions = new List<string>();
            var classExclusions = new List<string>();

            // load config
            XDocument config = XDocument.Load("NCoverExplorer2.config", LoadOptions.None);

            LoadExclusions(config, assemblyExclusions, namespaceExclusions, classExclusions);

            var moduleThresholds = new Dictionary<string, double>();

            IEnumerable<XElement> moduleThresholdNodes = config.XPathSelectElements("//ModuleThreshold");
            foreach (XElement node in moduleThresholdNodes)
            {
                using (XmlReader reader = node.CreateReader())
                {
                    var s = new XmlSerializer(typeof (ModuleThreshold));
                    var mt = (ModuleThreshold) s.Deserialize(reader);

                    // remove trailing .dll
                    string moduleName;

                    if (mt.ModuleName.EndsWith(".dll"))
                        moduleName = mt.ModuleName.Substring(0, mt.ModuleName.Length - 4);
                    else
                        moduleName = mt.ModuleName;

                    moduleThresholds.Add(moduleName, mt.SatisfactoryCoverage);
                }
            }

            XDocument document = XDocument.Load("coverage2.xml");

            Coverage coverage;

            using (XmlReader reader = document.CreateReader())
            {
                var s = new XmlSerializer(typeof (Coverage));

                coverage = (Coverage) s.Deserialize(reader);
            }


            foreach (Module module in coverage.Modules)
            {
                bool excluded = false;

                // check module name against assemblies
                foreach (string pattern in assemblyExclusions)
                {
                    if (Regex.IsMatch(module.assembly, pattern))
                    {
                        excluded = true;
                        break;
                    }
                }

                if (excluded)
                {
                    Debug.WriteLine(module.assembly, "Skipping");
                    continue;
                }

                Debug.WriteLine(module.assembly, "Processing");

                // seqpnt visitcount
                int count = 0;
                int visited = 0;

                if (module.Methods != null)
                    foreach (Method method in module.Methods)
                    {
                        if (method.Excluded)
                        {
                            Debug.WriteLine(method.Name, "Excluded");
                            continue;
                        }

                        foreach (string pattern in classExclusions)
                        {
                            if (Regex.IsMatch(method.Class, pattern))
                            {
                                excluded = true;
                                break;
                            }
                        }

                        if (excluded)
                        {
                            Debug.WriteLine(method.Class, "Skipping");
                            continue;
                        }


                        if (method.SequencePoints != null)
                            foreach (SequencePoint sequencePoint in method.SequencePoints)
                            {
                                if (sequencePoint.Excluded)
                                    continue;

                                count++;

                                if (sequencePoint.VisitCount > 0)
                                    visited++;
                            }
                    }

                if (count > 0)
                {
                    double percentage = (((double) visited)/count)*100;
                    Console.Write("Module {0}, {1} of {2} ({3:#0.#;#;0}%)", module.assembly, visited, count,
                                      percentage);

                    var moduleName = module.assembly;
                    if (moduleThresholds.ContainsKey(moduleName))
                    {
                        var thresholdPercentage = moduleThresholds[moduleName];
                        if (percentage < thresholdPercentage)
                            Console.Write(" Failed");
                        else
                        {
                            Console.Write(" Passed");
                            
                        }
                        Console.Write(" ({0}% minimum)", thresholdPercentage);
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                }
            }
            Console.Read();
        }

        private static void LoadExclusions(XDocument config, List<string> assemblyExclusions,
                                           List<string> namespaceExclusions, List<string> classExclusions)
        {
            IEnumerable<XElement> coverageExclusionNodes = config.XPathSelectElements("//CoverageExclusion");
            foreach (XElement node in coverageExclusionNodes)
            {
                using (XmlReader reader = node.CreateReader())
                {
                    var s = new XmlSerializer(typeof (CoverageExclusion));
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
                            break;
                        case ExclusionType.Namespace:
                            namespaceExclusions.Add(pattern);
                            break;
                        case ExclusionType.Class:
                            classExclusions.Add(pattern);
                            break;
                    }
                }
            }
        }
    }
}