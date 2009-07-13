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
        private static int Main(string[] args)
        {
            var namespaceExclusions = new List<string>();
            var assemblyExclusions = new List<string>();
            var classExclusions = new List<string>();

            string logFileName = "NCoverDora.log";
            string configFileName = null;
            string coverageFileName = null;

            if (args.Length == 0)
            {
                Console.WriteLine("NCoverDora v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
                Console.WriteLine("Usage: ncoverdora -config <configfile> -coverage <coveragefile> [-log <logfilename>]");
                return 0;
            }

            // load arguments
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-log":
                        i++;
                        logFileName = args[i];
                        break;
                    case "-config":
                        i++;
                        configFileName = args[i];
                        break;
                    case "-coverage":
                        i++;
                        coverageFileName = args[i];
                        break;
                }
            }

            if ( string.IsNullOrEmpty( configFileName ) )
            {
                Console.WriteLine("Missing parameter -config <configfilename>");
                return -1;
            }

            if ( string.IsNullOrEmpty( coverageFileName ) )
            {
                Console.WriteLine("Missing parameter -coverage <coveragefilename>");
                return -1;
            }

            // load config
            XDocument config = XDocument.Load(configFileName, LoadOptions.None);


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

            XDocument document = XDocument.Load(coverageFileName);

            Coverage coverage;

            using (XmlReader reader = document.CreateReader())
            {
                var s = new XmlSerializer(typeof (Coverage));

                coverage = (Coverage) s.Deserialize(reader);
            }

            var logDocument = new XDocument();
            var logRoot = new XElement("NCoverDora");
            logDocument.Add(logRoot);

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
                    var logModule = new XElement("module");
                    var moduleName = module.assembly;

                    double percentage = (((double) visited)/count)*100;
                    Console.Write( "Module {0}, {1} of {2} ({3:#0.#;#;0}%)", moduleName, visited, count,
                                  percentage);

                    logModule.SetAttributeValue( "assembly", moduleName );
                    logModule.SetAttributeValue("visited", visited);
                    logModule.SetAttributeValue("count", count);
                    logModule.SetAttributeValue("coverage", percentage);

                    if (moduleThresholds.ContainsKey(moduleName))
                    {
                        var thresholdPercentage = moduleThresholds[moduleName];
                        if ( percentage < thresholdPercentage )
                        {
                            Console.Write(" Failed");
                            logModule.SetAttributeValue("passed", false);
                        }
                        else
                        {
                            Console.Write( " Passed" );
                            logModule.SetAttributeValue( "passed", true );
                        }
                        Console.Write(" ({0}% minimum)", thresholdPercentage);
                        logModule.SetAttributeValue( "threshold", thresholdPercentage );

                        Console.WriteLine();
                    }
                    else
                    {
                        logModule.SetAttributeValue( "passed", true );
                        Console.WriteLine();
                    }

                    logRoot.Add( logModule );

                }
            }

            logDocument.Save(logFileName);

            //Console.Read();
            return 0;
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