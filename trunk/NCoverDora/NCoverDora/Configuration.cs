using System.Collections.Generic;

namespace NCoverDora
{
    public class Configuration
    {
        public Configuration()
        {
            ClassExclusions = new List<string>();
            AssemblyExclusions = new List<string>();
            NamespaceExclusions = new List<string>();
        }

        public List<string> NamespaceExclusions { get; set; }

        public List<string> AssemblyExclusions { get; set; }

        public List<string> ClassExclusions { get; set; }

        public bool ConsoleOutput { get; set; }
    }
}