using FluentAssertions;
using nresx.Tools;
using nresx.Tools.ResourceFile;
using NUnit.Framework;
using System.IO;

namespace nresx.Core.Tests.ResourceFiles.Po;

[TestFixture]
public class PoResourceFileTests : TestBase
{
    [TestCase("fr-FR")]
    public void SaveCultureMetadata(string cultureCode)
    {
        var format = ResourceFormatType.Po;
        var source = new ResourceFile(GetTestPath(TestData.ExampleResourceFile, format));
        var targetPath = GetOutputPath(TestData.UniqueKey(), format);

        source.Culture = new System.Globalization.CultureInfo(cultureCode);
        source.Save(targetPath);

        var saved = new ResourceFile(targetPath);
        saved.Culture.Name.Should().Be(cultureCode);
    }
    
    [Test]
    public void LoadHeaders()
    {
        var resPath = GetTestPath( @"po\Headers.po" );
        var res = new ResourceFile( resPath, new ResourceFileOption { IgnoreEmptyHeaders = false } );

        res.Headers["Content-Transfer-Encoding"].Should().Be( "8bit" );
        res.Headers["Content-Type"].Should().Be( "text/plain; charset=utf-8" );
        res.Headers["Language"].Should().Be( "en_US" );
        res.Headers["Language-Team"].Should().Be( "" );
        res.Headers["Last-Translator"].Should().Be( "" );
        res.Headers["Plural-Forms"].Should().Be( "nplurals=2; plural=(n != 1);" );
        res.Headers["PO-Revision-Date"].Should().Be( "" );
        res.Headers["POT-Creation-Date"].Should().Be( "2021-09-25 08:30+0200" );
        res.Headers["Project-Id-Version"].Should().Be( "" );
        res.Headers["Report-Msgid-Bugs-To"].Should().Be( "" );
        res.Headers["Header1"].Should().Be( "the header" );
    }

    [Test]
    public void SaveHeaders()
    {
        var resPath = GetTestPath( @"po\Headers.po" );
        var options = new ResourceFileOption { IgnoreEmptyHeaders = false };
        var res = new ResourceFile( resPath, options );

        const string header2 = "Header2";
        const string value2 = "the second header";
        res.Headers.Add( header2, value2 );

        var targetPath = GetOutputPath( TestData.UniqueKey(), ResourceFormatType.Po );
        res.Save( targetPath );

        var savedContent = new ResourceFile( targetPath, options );
        savedContent.Headers["Header1"].Should().Be( "the header" );
        savedContent.Headers["Content-Transfer-Encoding"].Should().Be( "8bit" );
        savedContent.Headers["Content-Type"].Should().Be( "text/plain; charset=utf-8" );
        savedContent.Headers["Language"].Should().Be( "en_US" );
        savedContent.Headers["Language-Team"].Should().Be( "" );
        savedContent.Headers["Last-Translator"].Should().Be( "" );
        savedContent.Headers["Plural-Forms"].Should().Be( "nplurals=2; plural=(n != 1);" );
        savedContent.Headers["PO-Revision-Date"].Should().Be( "" );
        savedContent.Headers["POT-Creation-Date"].Should().Be( "2021-09-25 08:30+0200" );
        savedContent.Headers["Project-Id-Version"].Should().Be( "" );
        savedContent.Headers["Report-Msgid-Bugs-To"].Should().Be( "" );
    }
}