using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Core;
using nresx.Core.Exceptions;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    // RSX-116: when both the path extension and an explicit format type are
    // provided to Save, they must agree. If they conflict, throw — don't
    // silently rewrite the extension (the old behavior).
    [TestFixture]
    public class SavePathFormatPriorityTests : TestBase
    {
        [Test]
        public void Save_ExtensionMatchesFormat_Succeeds()
        {
            var res = GetExampleResourceFile();
            var path = GetOutputPath( UniqueKey(), ResourceFormatType.Yaml );

            res.Save( path, ResourceFormatType.Yaml );

            File.Exists( path ).Should().BeTrue();
        }

        [Test]
        public void Save_ExtensionConflictsWithFormat_Throws()
        {
            var res = GetExampleResourceFile();
            var path = GetOutputPath( UniqueKey(), ResourceFormatType.Yaml );

            Action act = () => res.Save( path, ResourceFormatType.Json );

            act.Should().Throw<ResourceFormatMismatchException>()
                .WithMessage( "*Format mismatch*" );
        }

        [Test]
        public void Save_UnknownExtensionWithFormat_UsesFormatExtension()
        {
            var res = GetExampleResourceFile();
            var basePath = GetOutputPath( UniqueKey() );
            var pathWithUnknownExt = Path.ChangeExtension( basePath, ".bak" );

            res.Save( pathWithUnknownExt, ResourceFormatType.Yaml );

            var expectedPath = Path.ChangeExtension( pathWithUnknownExt, ".yaml" );
            File.Exists( expectedPath ).Should().BeTrue( "format wins when extension is unknown" );
        }

        [Test]
        public async Task SaveAsync_ExtensionConflictsWithFormat_Throws()
        {
            var res = GetExampleResourceFile();
            var path = GetOutputPath( UniqueKey(), ResourceFormatType.Yaml );

            Func<Task> act = async () => await res.SaveAsync( path, ResourceFormatType.Json );

            await act.Should().ThrowAsync<ResourceFormatMismatchException>()
                .WithMessage( "*Format mismatch*" );
        }
    }
}
