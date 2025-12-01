using System.Linq;
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
        saved.Elements[key1]!.Comment.Should().Be( comment1 );
        saved.Elements[key1]!.Comments.Should().HaveCount( 1 );
        saved.Elements[key1]!.Comments[0].Value.Should().Be( comment1 );
        saved.Elements[key1]!.Comments[0].Type.Should().Be( CommentType.Translator );
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
        source.Elements[key1]!.Comments.Add( new Comment( CommentType.Translator, comment2 ) );
        var targetPath = GetOutputPath( TestData.UniqueKey(), format );
        source.Save( targetPath );

        var saved = new ResourceFile( targetPath );
        saved.Elements[key1]!.Comment.Should().Be( comment1 );
        saved.Elements[key1]!.Comments.Should().HaveCount( 2 );
        saved.Elements[key1]!.Comments[0].Value.Should().Be( comment1 );
        saved.Elements[key1]!.Comments[0].Type.Should().Be( CommentType.Translator );
        saved.Elements[key1]!.Comments[1].Value.Should().Be( comment2 );
        saved.Elements[key1]!.Comments[1].Type.Should().Be( CommentType.Translator );
    }

    [Test]
    public void ElementDifferentComment()
    {
        const ResourceFormatType format = ResourceFormatType.Po;
        const string key1 = "key1";
        var comments = new[]
        {
            ( type: CommentType.Translator, value: "comment1" ),
            ( type: CommentType.Extracted, value: "comment2" ),
            ( type: CommentType.Reference, value: "comment3" ),
            ( type: CommentType.Flags, value: "comment4" ),
            ( type: CommentType.PreviousValue, value: "comment5" ),
        };

        var source = new ResourceFile( format );
        source.Elements.Add( key1, "value" );
        foreach ( var (type, value) in comments )
        {
            source.Elements[key1]!.Comments.Add( new Comment( type, value ) );
        }
        var targetPath = GetOutputPath( TestData.UniqueKey(), format );
        source.Save( targetPath );

        // validate raw file
        var savedFile = TestHelper.ReadFile( targetPath );
        savedFile[3].Should().Be( $"#  {comments[0].value}" );
        savedFile[4].Should().Be( $"#. {comments[1].value}" );
        savedFile[5].Should().Be( $"#: {comments[2].value}" );
        savedFile[6].Should().Be( $"#, {comments[3].value}" );
        savedFile[7].Should().Be( $"#| {comments[4].value}" );

        // validate loading
        var saved = new ResourceFile( targetPath );
        var el = saved.Elements[key1];
        el!.Comments.Select( c => ( c.Type, c.Value ) ).Should().BeEquivalentTo( comments );
    }

    [Test]
    public void GlobalCommentsTest()
    {
        const ResourceFormatType format = ResourceFormatType.Po;
        var comments = new[]
        {
            ( type: CommentType.Translator, value: "comment1" ),
            ( type: CommentType.Extracted, value: "comment2" ),
            ( type: CommentType.Reference, value: "comment3" ),
            ( type: CommentType.Flags, value: "comment4" ),
            ( type: CommentType.PreviousValue, value: "comment5" ),
        };

        var source = new ResourceFile(format);
        foreach (var (type, value) in comments)
        {
            source.Comments.Add(new Comment(type, value));
        }
        var targetPath = GetOutputPath(TestData.UniqueKey(), format);
        source.Save(targetPath);

        // validate raw file
        var savedFile = TestHelper.ReadFile(targetPath);
        savedFile[0].Should().Be($"#  {comments[0].value}");
        savedFile[1].Should().Be($"#. {comments[1].value}");
        savedFile[2].Should().Be($"#: {comments[2].value}");
        savedFile[3].Should().Be($"#, {comments[3].value}");
        savedFile[4].Should().Be($"#| {comments[4].value}");

        // validate loading
        var saved = new ResourceFile(targetPath);
        saved.Comments.Select(c => (c.Type, c.Value)).Should().BeEquivalentTo(comments);
    }
}