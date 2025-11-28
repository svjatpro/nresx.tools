using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Po;

[TestFixture]
public class PoResourceFileCommentsTests : TestBase
{
    [Test]
    public void SingleElementTranslatorComment()
    {
        const ResourceFormatType format = ResourceFormatType.Po;
        const string key1 = "key1";
        const string value1 = "value1";
        const string comment1 = "comment1";

        var source = new ResourceFile( format );
        source.Elements.Add( key1, value1, comment1 );
        var targetPath = GetOutputPath( TestData.UniqueKey(), format );
        source.Save( targetPath );
        
        var saved = new ResourceFile( targetPath );
        saved.Elements[key1].Comment.Should().Be( comment1 );
        saved.Elements[key1].Comments.Should().HaveCount( 1 );
        saved.Elements[key1].Comments[0].Value.Should().Be( comment1 );
        saved.Elements[key1].Comments[0].Type.Should().Be( CommentType.Translator );
    }

    [Test]
    public void MultipleElementTranslatorComment()
    {
        const ResourceFormatType format = ResourceFormatType.Po;
        const string key1 = "key1";
        const string value1 = "value1";
        const string comment1 = "comment1";
        const string comment2 = "comment2";

        var source = new ResourceFile( format );
        source.Elements.Add( key1, value1, comment1 );
        source.Elements[key1].Comments.Add( new Comment( CommentType.Translator, comment2 ) );
        var targetPath = GetOutputPath( TestData.UniqueKey(), format );
        source.Save( targetPath );

        var saved = new ResourceFile( targetPath );
        saved.Elements[key1].Comment.Should().Be( comment1 );
        saved.Elements[key1].Comments.Should().HaveCount( 2 );
        saved.Elements[key1].Comments[0].Value.Should().Be( comment1 );
        saved.Elements[key1].Comments[0].Type.Should().Be( CommentType.Translator );
        saved.Elements[key1].Comments[1].Value.Should().Be( comment2 );
        saved.Elements[key1].Comments[1].Type.Should().Be( CommentType.Translator );
    }
}