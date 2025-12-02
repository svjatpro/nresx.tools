using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Po;

[TestFixture]
public class PoResourceFilePluralTests : TestBase
{
    [Test]
    public void LoadSaveKeyPluralTest()
    {
        var res = new ResourceFile( ResourceFormatType.Po );

        res.Elements.Add( "key1", "value1", keyPlural: "keys1" );
        res.Elements.Add( "key2", "value2" );

        var targetPath = GetOutputPath( TestData.UniqueKey(), ResourceFormatType.Po );
        res.Save( targetPath );

        var saved = new ResourceFile( targetPath );
        saved.Elements.Should().HaveCount( 2 );
        saved.Elements["key1"]?.KeyPlural.Should().Be( "keys1" );
        saved.Elements["key2"]?.KeyPlural.Should().BeNull();
    }

    [Test]
    public void PluralValueTest()
    {
        var res = new ResourceFile( ResourceFormatType.Po );
        res.Elements.Add( "key1", "value1", plurals: [( 0, "v0" ), ( 1, "v1" )] );
        res.Elements.Add( "key2", value: null, plurals: [( 0, "v0" ), ( 1, "v1" )] );

        var targetPath = GetOutputPath( TestData.UniqueKey(), ResourceFormatType.Po );
        res.Save( targetPath );

        var saved = new ResourceFile( targetPath );
        saved.Elements["key1"]?.GetValue().Should().Be( "value1" );
        saved.Elements["key1"]?.GetValue( 0 ).Should().Be( "v0" );
        saved.Elements["key1"]?.GetValue( 1 ).Should().Be( "v1" );
        saved.Elements["key1"]?.GetValue( 2 ).Should().Be( "value1" );
        saved.Elements["key2"]?.GetValue().Should().Be( string.Empty );
        saved.Elements["key2"]?.GetValue( 0 ).Should().Be( "v0" );
        saved.Elements["key2"]?.GetValue( 1 ).Should().Be( "v1" );
        saved.Elements["key2"]?.GetValue( 2 ).Should().Be( string.Empty );
    }
}