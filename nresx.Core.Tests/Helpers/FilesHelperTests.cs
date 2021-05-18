using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.Core.Tests.Helpers
{
    [TestFixture]
    public class FilesHelperTests : TestBase
    {
        #region Private members

        private List<string> PrepareFiles( out string fileKey )
        {
            fileKey = UniqueKey();
            var dirKey = UniqueKey();
            new DirectoryInfo( Path.Combine( TestData.OutputFolder, dirKey ) ).Create();
            var filePath1 = GetOutputPath( $"{fileKey}_11.resx" );
            var filePath2 = GetOutputPath( $"{fileKey}_2.resx" );
            var filePath3 = GetOutputPath( $"{dirKey}\\{fileKey}_33.resx" );

            CopyTemporaryFile( destPath: filePath1 );
            CopyTemporaryFile( destPath: filePath2 );
            CopyTemporaryFile( destPath: filePath3 );

            return new List<string> { filePath1, filePath2, filePath3 };
        }

        private void ValidateSearchResult( string fileKey, string[] successFiles, string[] failedFiles, bool recursive = false )
        {
            var actualProcessed = new List<string>();
            var actualFailed = new List<string>();

            FilesHelper.SearchResourceFiles(
                $"{TestData.OutputFolder}\\{fileKey}*.resx",
                (file, res ) => actualProcessed.Add( res.Name ),
                (f, ex) => actualFailed.Add( f.Name ),
                recursive: recursive);

            successFiles.Select( Path.GetFileName )
                .Should()
                .BeEquivalentTo( actualProcessed.Select( Path.GetFileName ) );
            failedFiles.Select( Path.GetFileName )
                .Should()
                .BeEquivalentTo( actualFailed.Select( Path.GetFileName ) );
        }

        #endregion

        [Test]
        public async Task SearchResourceFiles()
        {
            var files = PrepareFiles( out var fileKey );

            ValidateSearchResult( fileKey, new []{files[0], files[1]}, new string[]{});
        }

        [Test]
        public async Task SearchResourceFilesRecursive()
        {
            var files = PrepareFiles( out var fileKey );

            ValidateSearchResult( fileKey, new[] { files[0], files[1], files[2] }, new string[] { }, recursive: true );
        }

        [Test]
        public async Task SearchResourceFilesWithFailedOne()
        {
            var files = PrepareFiles( out var fileKey );
            var wrongFile = GetOutputPath( $"{fileKey}_0", ResourceFormatType.Resx );
            new FileInfo( GetTestPath( TestData.WrongFormatResourceFile ) ).CopyTo( wrongFile );

            ValidateSearchResult( fileKey, new[] { files[0], files[1] }, new[] { wrongFile } );
        }
    }
}
