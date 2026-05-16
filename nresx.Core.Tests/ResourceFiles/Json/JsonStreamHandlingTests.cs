using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Json
{
    [TestFixture]
    public class JsonStreamHandlingTests : TestBase
    {
        [Test]
        public void Save_DoesNotCloseCallerStream()
        {
            var res = GetExampleResourceFile();

            var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Json );

            // Should still be able to use the stream after Save
            ms.Position = 0;
            var content = new StreamReader( ms ).ReadToEnd();
            content.Should().NotBeEmpty();
        }

        [Test]
        public void Load_DoesNotCloseCallerStream()
        {
            var jsonText = "{ \"a\": \"v1\", \"b\": \"v2\" }";
            var ms = new MemoryStream( Encoding.UTF8.GetBytes( jsonText ) );

            var _ = new ResourceFile( ms, ResourceFormatType.Json );

            // Stream should still be readable / positioned after load
            ms.CanRead.Should().BeTrue();
        }

        [Test]
        public void RoundTrip_ViaStream_NoStreamReopen()
        {
            var source = GetExampleResourceFile();

            using var ms = new MemoryStream();
            source.Save( ms, ResourceFormatType.Json );
            ms.Position = 0;

            var loaded = new ResourceFile( ms, ResourceFormatType.Json );

            loaded.Elements.Select( e => e.Key ).Should()
                .BeEquivalentTo( source.Elements.Select( e => e.Key ) );
        }
    }
}
