using Microsoft.Pex.Framework;
using NCoverDora;
using NUnit.Framework;

namespace NCoverDoraTests
{
    [TestFixture, PexClass(typeof(CommandLine))]
    public partial class CommandLineTests
    {

        [PexMethod]
        public void Check_Arguments([PexAssumeNotNull] string[] args)
        {
            Arguments programArguments = new Arguments();
            CommandLine.CommandLineArgs(args, programArguments);
        }
    }
}