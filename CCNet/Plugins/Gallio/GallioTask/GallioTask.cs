using System;
using System.Diagnostics;
using System.IO;
using Exortech.NetReflector;
using Gallio.Collections;
using Gallio.Model.Filters;
using Gallio.Reflection;
using Gallio.Runner;
using Gallio.Runtime;
using Gallio.Runtime.Logging;
using Gallio.Runtime.ProgressMonitoring;
using ThoughtWorks.CruiseControl.Core;

namespace Gardiner.CruiseControl.Tasks
{
    [ReflectorType("gallio")]
    public class GallioTask
    {
        private IIntegrationResult _result;

/*
        /r:NCover /rnf:$(report) /rt:Xml-Inline /v:Quiet /rd:$(projectArtifacts) /abd:$(projectBase)$(folder) $(projectBase)$(folder)\$(assembly)
*/

        [ReflectorArray("assemblies")] public string[] Assemblies = new string[0];

        [ReflectorProperty("doNotRun", Required = false)] public bool DoNotRun;
        [ReflectorProperty("filter", Required = false)] public string Filter = string.Empty;

        [ReflectorArray("hintDirectories", Required = false)] public string[] HintDirectories = new string[0];
        [ReflectorProperty("ignoreAnnotations", Required = false)] public bool IgnoreAnnotations;

        [ReflectorProperty("noEchoResults", Required = false)] public bool NoEchoResults;

        [ReflectorArray("pluginDirectories", Required = false)] public string[] PluginDirectories = new string[0];

        [ReflectorProperty("reportDirectory", Required = false)] public string ReportDirectory = string.Empty;

        [ReflectorProperty("reportNameFormat", Required = false)] public string ReportNameFormat = "test-report-{0}-{1}";

        [ReflectorArray("reportTypes", Required = false)] public string[] ReportTypes = new[] {"Xml-Inline"};

        [ReflectorArray("runnerExtensions", Required = false)] public string[] RunnerExtensions = new string[0];

        [ReflectorProperty("runnerType", Required = false)] public string RunnerType = "Local";
        [ReflectorProperty("showReports", Required = false)] public bool ShowReports;

        [ReflectorProperty("applicationBaseDirectory", Required = false)]
        public string ApplicationBaseDirectory { get; set; }

        [ReflectorProperty("workingDirectory", Required = false)]
        public string WorkingDirectory { get; set; }

        public void Run(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask("Gallio starting..");
            result.AddTaskResult("yay");

            _result = result;

            var logger = new EventLogger();
            logger.LogMessage += logger_LogMessage;

            RunTests(logger);
        }

        private void logger_LogMessage(object sender, LogMessageEventArgs e)
        {
            _result.BuildProgressInformation.SignalStartRunTask(e.Message);
            _result.AddTaskResult(e.Message);
            Debug.WriteLine(e.Message);
        }

        private int RunTests(ILogger logger)
        {
            var launcher = new TestLauncher
                               {
                                   Logger = logger,
                                   ProgressMonitorProvider = new LogProgressMonitorProvider(logger),
                                   RuntimeSetup = new RuntimeSetup(),
                                   ReportDirectory = ReportDirectory,
                                   ReportNameFormat = ReportNameFormat,
                                   TestRunnerFactoryName = RunnerType,
                                   DoNotRun = DoNotRun,
                                   IgnoreAnnotations = IgnoreAnnotations,
                                   EchoResults = (!NoEchoResults),
                                   ShowReports = ShowReports
                               };

            launcher.RuntimeSetup.PluginDirectories.AddRange(PluginDirectories);

            // Set the installation path explicitly to ensure that we do not encounter problems
            // when the test assembly contains a local copy of the primary runtime assemblies
            // which will confuse the runtime into searching in the wrong place for plugins.
            launcher.RuntimeSetup.RuntimePath =
                Path.GetDirectoryName(AssemblyUtils.GetFriendlyAssemblyLocation(typeof (GallioTask).Assembly));

            launcher.TestPackageConfig.HostSetup.ShadowCopy = false; // Arguments.ShadowCopy;
            launcher.TestPackageConfig.HostSetup.ApplicationBaseDirectory = ApplicationBaseDirectory;
            launcher.TestPackageConfig.HostSetup.WorkingDirectory = WorkingDirectory;

            launcher.TestPackageConfig.AssemblyFiles.AddRange(Assemblies);
            launcher.TestPackageConfig.HintDirectories.AddRange(HintDirectories);

            GenericUtils.AddAll(ReportTypes, launcher.ReportFormats);

            GenericUtils.AddAll(RunnerExtensions, launcher.TestRunnerExtensionSpecifications);

            if (!String.IsNullOrEmpty(Filter))
                launcher.TestExecutionOptions.Filter = FilterUtils.ParseTestFilter(Filter);

            TestLauncherResult result = launcher.Run();
            // DisplayResultSummary( result );

            return result.ResultCode;
        }
    }

/*    internal class CruiseProgressProvider : IProgressMonitorProvider
    {
        #region IProgressMonitorProvider Members

        public void Run(TaskWithProgress task)
        {
        }

        #endregion
    }*/
}