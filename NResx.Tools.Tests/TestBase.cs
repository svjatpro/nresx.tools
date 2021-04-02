﻿using System;
using System.Linq;
using FluentAssertions;

namespace NResx.Tools.Tests
{
    public class TestBase
    {
        protected void ValidateElements( Tools.ResourceFile resource )
        {
            var elements = resource.Elements.ToList();

            elements
                .Select( e => (key: e.Key, val: e.Value) )
                .Should().BeEquivalentTo(
                    (key: "Entry1.Text", val: "Value1"),
                    (key: "Entry2", val: "Value2"),
                    (key: "Entry3", val: "Value3") );
        }

        protected string UniqueKey( int length = 8 )
        {
            var key = Convert.ToBase64String( Guid.NewGuid().ToByteArray() )
                .Replace( "+", "" )
                .Replace( "/", "" )
                .Replace( "=", "" );
            return key.Substring( 0, Math.Min( length, key.Length ) );
        }

        protected Tools.ResourceFile GetExampleResourceFile()
        {
            var example = new Tools.ResourceFile( @"Files\Resources.resx" );
            return example;
        }
    }
}