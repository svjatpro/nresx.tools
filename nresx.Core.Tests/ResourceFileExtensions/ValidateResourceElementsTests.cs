using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFileExtensions
{
    [TestFixture]
    public class ValidateResourceElementsTests : TestBase
    {
        [Test]
        public async Task ValidateSucessfull()
        {
            var res = GetExampleResourceFile();

            res.ValidateElements( out var errors ).Should().BeTrue();
            errors.Should().BeEmpty( );
        }

        [Test]
        public async Task ValidateDuplicatedElements()
        {
            var res = GetExampleResourceFile();
            res.Elements.Add( res.Elements[1].Key, UniqueKey() );
            res.Elements.Add( res.Elements[2].Key, UniqueKey() );

            res.ValidateElements( out var errors ).Should().BeFalse();
            errors.Should().BeEquivalentTo(
                new ResourceElementError( ResourceElementErrorType.Duplicate, res.Elements[1].Key ),
                new ResourceElementError( ResourceElementErrorType.Duplicate, res.Elements[2].Key ) );
        }

        [Test]
        public async Task ValidatePossibleDuplicatedElements()
        {
            var res = GetExampleResourceFile();
            var keyDuplicated = $"{res.Elements[1].Key}.Content";
            res.Elements.Add( keyDuplicated, UniqueKey() );

            res.ValidateElements( out var errors ).Should().BeFalse();
            errors.Should().BeEquivalentTo( new ResourceElementError( ResourceElementErrorType.PossibleDuplicate, keyDuplicated ) );
        }

        [Test]
        public async Task ValidateEmptyKey()
        {
            var res = GetExampleResourceFile();
            res.Elements[0].Key = string.Empty;

            res.ValidateElements( out var errors ).Should().BeFalse();
            errors.Should().HaveCount( 1 );
            var error = errors.Single();
            error.ErrorType.Should().Be( ResourceElementErrorType.EmptyKey );
            error.ElementKey.Should().Be( string.Empty );
            error.Message.Should().Contain( res.Elements[0].Value );
        }

        [Test]
        public async Task ValidateEmptyValue()
        {
            var res = GetExampleResourceFile();
            res.Elements[0].Value = string.Empty;
            res.Elements[1].Value = string.Empty;

            res.ValidateElements( out var errors ).Should().BeFalse();
            errors.Should().BeEquivalentTo( 
                new ResourceElementError( ResourceElementErrorType.EmptyValue, res.Elements[0].Key ),
                new ResourceElementError( ResourceElementErrorType.EmptyValue, res.Elements[1].Key ) );
        }
    }
}