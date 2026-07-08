using NUnit.Framework;

namespace nresx.Core.Tests
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