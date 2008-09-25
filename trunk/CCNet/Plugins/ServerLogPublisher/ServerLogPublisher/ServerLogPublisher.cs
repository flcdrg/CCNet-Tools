
using System;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using log4net.Repository.Hierarchy;
using log4net.Appender;
using log4net;
using System.IO;
using ThoughtWorks.CruiseControl.Core.Tasks;
using System.Globalization;

namespace Gardiner.CruiseControl.Tasks
{
    /// <summary>
    /// Read events from log4net's <see cref="MemoryAppender"/> and write those related to this
    /// project out to an xml file that can be merged into the build log.
    /// </summary>
    [ReflectorType("serverLogPublisher")]
    public class ServerLogPublisher : ITask
    {
        public const string DefaultLogFileName = "server-log.xml";

        [ReflectorProperty("LogFileName", Required = false)]
        public string LogFileName { get; set; }

        public ServerLogPublisher()
        {
            LogFileName = DefaultLogFileName;
        }

        #region ITask Members

        public void Run(IIntegrationResult result)
        {
            Hierarchy hierarchy;
            MemoryAppender memoryAppender;

            // Get the default hierarchy for log4net
            hierarchy = (Hierarchy)LogManager.GetRepository();

            // Get the appender named "MemoryAppender" from the <root> logger
            memoryAppender = (MemoryAppender)hierarchy.Root.GetAppender("MemoryAppender");

            if (memoryAppender != null)
            {
                Console.WriteLine("Getting events from MemoryAppender");

                var events = memoryAppender.GetEvents();

                // we've seen all of these events now.
                memoryAppender.Clear();

                string serverLogOutputFile = ServerLogOutputFile(result);

                using (var writer = new System.Xml.XmlTextWriter(serverLogOutputFile, System.Text.Encoding.UTF8))
                {
                    writer.Formatting = System.Xml.Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("serverLog");

                    foreach (var ev in events)
                    {
                        if (ev.ThreadName == result.ProjectName)
                        {
                            writer.WriteStartElement("item");

                            // 2008-09-25 21:29:07
                            writer.WriteAttributeString("date", ev.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("level", ev.Level.DisplayName);
                            writer.WriteString(ev.RenderedMessage.Trim());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                if (File.Exists(serverLogOutputFile))
                {
                    result.AddTaskResult(new FileTaskResult(serverLogOutputFile));
                }
            }

        }

        #endregion

        private string ServerLogOutputFile(IIntegrationResult result)
        {
            return Path.Combine(result.ArtifactDirectory, LogFileName);
        }
    }
}
