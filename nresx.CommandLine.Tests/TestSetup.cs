//using System;

using System.IO;
using System.Text.Json;
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
            // this is a workaround for "ignoring environment variables" in launchsettings.json bug by test runner
            // Environment.SetEnvironmentVariable( "DEBUG_COMMAND_LINE", "true" );

            TestBase.CleanOutputDir();
        }

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            var json = JsonSerializer.Serialize(
                CommandsStatistics.Commands,
                new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(TestData.OutputFolder, "commands_stats.json");
            File.WriteAllText(path, json);
        }
    }
}