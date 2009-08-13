using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
            var programArgs = new Arguments();

            if (!CommandLine.CommandLineArgs(args, programArgs))
                return;

            // load config
            XDocument configDocument = XDocument.Load(programArgs.ConfigFileName, LoadOptions.None);

            var config = new Configuration();

            LoadExclusions(configDocument, config);

            var moduleThresholds = new Dictionary<string, double>();

            IEnumerable<XElement> moduleThresholdNodes = configDocument.XPathSelectElements("//ModuleThreshold");
            foreach (XElement node in moduleThresholdNodes)
            {
                var mt = Serialisation.Deserialise<ModuleThreshold>(node);

                // remove trailing .dll
                string moduleName;

                if (mt.ModuleName.EndsWith(".dll"))
                    moduleName = mt.ModuleName.Substring(0, mt.ModuleName.Length - 4);
                else
                    moduleName = mt.ModuleName;

                moduleThresholds.Add(moduleName, mt.SatisfactoryCoverage);
            }
            XElement failIfBelowMinimumNode = configDocument.XPathSelectElement("//FailIfBelowMinimum");

            bool failIfBelowMinimum = false;

            if (failIfBelowMinimumNode != null)
            {
                failIfBelowMinimum = bool.Parse(failIfBelowMinimumNode.Value);
            }

            // Load NCover data
            Coverage coverage = LoadCoverageData(programArgs);

            bool failed = false;

            var logDocument = new XDocument();
            var logRoot = new XElement("NCoverDora");
            logDocument.Add(logRoot);

            var exclusionCache = new List<string>();

            foreach (Module module in coverage.Modules)
            {
                // check module name against assemblies
                bool excluded = GetExcluded(module.assembly, config.AssemblyExclusions);

                if (excluded)
                {
                    Debug.WriteLine(module.assembly, "Skipping assembly");
                    continue;
                }

                Debug.WriteLine(module.assembly, "Processing");

                // seqpnt visitcount
                int count = 0;
                int visited = 0;

                if (module.Methods != null)
                {
                    foreach (Method method in module.Methods)
                    {
                        if (method.Excluded)
                        {
                            Debug.WriteLine(method.Name, "Method excluded");
                            continue;
                        }

                        string className = method.Class;

                        if (exclusionCache.Contains(className))
                        {
                            Debug.WriteLine(className, "Skipping class (cached)");
                            continue;
                        }

                        excluded = GetExcluded(className, config.ClassExclusions);

                        if (excluded)
                        {
                            exclusionCache.Add(className);

                            Debug.WriteLine(className, "Skipping class");
                            continue;
                        }

                        // namespace applies to stuff before last .
                        string classNamespace = GetClassNamespace(method);

                        excluded = GetExcluded(classNamespace, config.NamespaceExclusions);

                        if (excluded)
                        {
                            exclusionCache.Add(className);

                            Debug.WriteLine(classNamespace, "Skipping namespace");
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
                        failed = LogModuleStatistics(moduleThresholds, logRoot, module, count, visited, failed);
                    }
                }
            }
            logDocument.Save(programArgs.LogFileName);

            // Set non-zero exit code if we failed
            if (failed && failIfBelowMinimum)
            {
                Environment.ExitCode = 1;
            }
            else
            {
                Environment.ExitCode = 0;
            }

            Console.WriteLine("NCoverDora finished with exit code {0}", Environment.ExitCode);

            // everything passed
#if DEBUG
            Console.Read();
#endif
            return;
        }

        private static string GetClassNamespace(Method method)
        {
            string classNamespace = method.Class;
            int lastIndexOfDot = classNamespace.LastIndexOf('.');
            if (lastIndexOfDot >= 0)
            {
                classNamespace = classNamespace.Substring(0, lastIndexOfDot);
            }
            return classNamespace;
        }

        private static bool LogModuleStatistics(IDictionary<string, double> moduleThresholds, XContainer logRoot,
                                                Module module, int count, int visited, bool failed)
        {
            var logModule = new XElement("module");
            string moduleName = module.assembly;

            double percentage = (((double) visited)/count)*100;
            Console.Write("Module {0}, {1} of {2} ({3:#0.#;#;0}%)", moduleName, visited, count,
                          percentage);

            logModule.SetAttributeValue("assembly", moduleName);
            logModule.SetAttributeValue("visited", visited);
            logModule.SetAttributeValue("count", count);
            logModule.SetAttributeValue("coverage", percentage);

            if (moduleThresholds.ContainsKey(moduleName))
            {
                double thresholdPercentage = moduleThresholds[moduleName];
                if (percentage < thresholdPercentage)
                {
                    Console.Write(" Failed");
                    logModule.SetAttributeValue("passed", false);
                    failed = true;
                }
                else
                {
                    Console.Write(" Passed");
                    logModule.SetAttributeValue("passed", true);
                }
                Console.Write(" ({0}% minimum)", thresholdPercentage);
                logModule.SetAttributeValue("threshold", thresholdPercentage);

                Console.WriteLine();
            }
            else
            {
                logModule.SetAttributeValue("passed", true);
                Console.WriteLine();
            }

            logRoot.Add(logModule);
            return failed;
        }

        private static Coverage LoadCoverageData(Arguments programArgs)
        {
            XDocument document = XDocument.Load(programArgs.CoverageFileName);

            return Serialisation.Deserialise<Coverage>(document);
        }

        private static bool GetExcluded(string className, IEnumerable<string> exclusions)
        {
            bool excluded = false;
            foreach (string pattern in exclusions)
            {
                if (Regex.IsMatch(className, pattern))
                {
                    Debug.WriteLine("Matched", pattern);

                    excluded = true;
                    break;
                }
            }
            return excluded;
        }

        private static
            void LoadExclusions
            (XNode config, Configuration configuration)
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
                            configuration.AssemblyExclusions.Add(pattern);
                            break;
                        case ExclusionType.Namespace:
                            configuration.NamespaceExclusions.Add(pattern);
                            break;
                        case ExclusionType.Class:
                            configuration.ClassExclusions.Add(pattern);
                            break;
                    }
                }
            }
        }
    }
}