using System;
using System.Reflection;

namespace NCoverDora
{
    public class CommandLine
    {
        public static bool CommandLineArgs(string[] args, Arguments programArguments)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("NCoverDora v{0}", Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine();
                Console.WriteLine(
                    "Usage: ncoverdora -config <configfile> -coverage <coveragefile> [-log <logfilename>]");
                Environment.ExitCode = 2;
                return false;
            }

            // load arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-log":
                        i++;
                        programArguments.LogFileName = args[i];
                        break;
                    case "-config":
                        i++;
                        programArguments.ConfigFileName = args[i];
                        break;
                    case "-coverage":
                        i++;
                        programArguments.CoverageFileName = args[i];
                        break;
                }
            }

            if (String.IsNullOrEmpty(programArguments.ConfigFileName))
            {
                Console.WriteLine("Missing parameter -config <configfilename>");
                Environment.ExitCode = 2;
                return false;
            }

            if (String.IsNullOrEmpty(programArguments.CoverageFileName))
            {
                Console.WriteLine("Missing parameter -coverage <coveragefilename>");
                Environment.Exit(2);
            }
            return true;
        }
    }
}