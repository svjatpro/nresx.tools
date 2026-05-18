using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Po;

[TestFixture]
public class PoResourceFileContextTests : TestBase
{
    [Test]
    public void LoadSaveKeyPluralTest()
    {
        var res = new ResourceFile( ResourceFormatType.Po );

        res.Elements.Add( "key1", "value1", context: "context1" );
        res.Elements.Add( "key2", "value2" );
        res.Elements.Add( "key1", "value11", context: "context2" );

        var targetPath = GetOutputPath( TestData.UniqueKey(), ResourceFormatType.Po );
        res.Save( targetPath );

        var saved = new ResourceFile( targetPath );
        saved.Elements.Should().HaveCount( 3 );
        saved.Elements["key1", "context1"]?.Value.Should().Be( "value1" );
        saved.Elements["key1", "context2"]?.Value.Should().Be( "value11" );
    }
}