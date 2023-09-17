using System;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            // uncomment this to debug tests in nresx.commandline
            // this is workaround for "ignoring environment variables" in launchsettings.json by test runner
            //Environment.SetEnvironmentVariable( "DEBUG_COMMAND_LINE", "true" );

            TestBase.CleanOutputDir();
        }
    }
}