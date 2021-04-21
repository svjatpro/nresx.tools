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
            TestBase.CleanOutputDir();
        }
    }
}