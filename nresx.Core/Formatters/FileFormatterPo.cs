using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using nresx.Tools.Extensions;

namespace nresx.Tools.Formatters
{
    internal class FileFormatterPo : IFileFormatter
    {
        #region Private fields

        private const string MsgIdTag = @"msgid";
        private const string MsgStrTag = @"msgstr";


        #endregion

        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            if ( LoadRawElements( stream, out var raw ) )
            {
                var dictionary = new Dictionary<string, ResourceElement>();
                foreach ( var el in raw )
                {
                    if ( !string.IsNullOrWhiteSpace( el.Key ) && !dictionary.ContainsKey( el.Key ) )
                        dictionary.Add( el.Key, el );
                }
                elements = dictionary.Values.ToList();
                return true;
            }

            elements = null;
            return false;
        }

        public bool LoadRawElements( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            using var reader = new StreamReader( stream );

            var headers = false;
            var result = new List<ResourceElement>();
            var elementLines = new List<string>();
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine();

                if ( headers )
                {
                    // parse headers
                }
                else
                {
                    if ( string.IsNullOrWhiteSpace( line ) )
                    {
                        result.Add( ParseElement( elementLines) );
                        elementLines.Clear();
                    }
                    else
                    {
                        elementLines.Add( line );
                    }
                }
            }
            if( elementLines.Any() )
                result.Add( ParseElement( elementLines ) );

            elements = result;
            return true;
        }

        private enum ElementParseState { None, Comment, Msgid, Msgstr }
        private ResourceElement ParseElement( List<string> lines )
        {
            var element = new ResourceElement{ Type = ResourceElementType.String };
            var state = ElementParseState.None;
            var propLines = new List<string>();

            void parseProperty( ElementParseState nextProp )
            {
                if ( propLines.Any() )
                {
                    var value = string.Join( "", propLines ).Replace( @"\n", Environment.NewLine );
                    switch ( state )
                    {
                        case ElementParseState.Msgid:
                            element.Key = value;
                            break;
                        case ElementParseState.Msgstr:
                            element.Value = value;
                            break;
                    }
                    propLines.Clear();
                }

                state = nextProp;
            }

            foreach ( var line in lines )
            {
                switch (line)
                {
                    case var l when l == "#":
                        break;
                    case var l when l.StartsWith( "# " ):
                        parseProperty( ElementParseState.Comment );

                        var comment = l.Length > 2 ? l.Substring( 2 ) : string.Empty;
                        element.Comment = comment;
                        break;
                    case var l when l.StartsWith( $"{MsgIdTag} " ):
                        parseProperty( ElementParseState.Msgid );

                        var r1 = new Regex( @"msgid\s+""(.*)""" ).Match( l );
                        if ( r1.Success && r1.Groups.Count > 1 )
                        {
                            var value = r1.Groups[1].Value;
                            propLines.Add( value );
                        }
                        break;
                    case var l when l.StartsWith( $"{MsgStrTag} " ):
                        parseProperty( ElementParseState.Msgstr );

                        var r2 = new Regex( @"msgstr\s+""(.*)""" ).Match( l );
                        if ( r2.Success && r2.Groups.Count > 1 )
                        {
                            var value = r2.Groups[1].Value;
                            propLines.Add( value );
                        }
                        break;
                    case var l when l.Trim( ' ' ).StartsWith( "\"" ):
                        if ( state != ElementParseState.None )
                        {
                            var r3 = new Regex( @"""(.*)""" ).Match( l );
                            if ( r3.Success && r3.Groups.Count > 1 )
                            {
                                propLines.Add( r3.Groups[1].Value );
                            }
                        }
                        break;
                }
            }
            parseProperty( ElementParseState.None );

            return element;
        }

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements, ResourceFileOption options = null )
        {
            using var writer = new StreamWriter( stream );

            void WriteMultilineProperty( string property, string tag )
            {
                //var propLines = property.SplitLines().Select( l => $"{l}\\n" ).ToList();
                var propLines = property.SplitLines().ToList(); //.Select( l => $"{l}\\n" ).ToList();

                if ( propLines.Count > 1 )
                {
                    for ( var i = 0; i < propLines.Count - 1; i++ )
                        propLines[i] = $"{propLines[i]}\\n";
                    propLines.Insert( 0, string.Empty );
                }

                if ( propLines.Any() )
                {
                    writer.WriteLine( $"{tag} \"{propLines[0]}\"" );
                }

                if ( propLines.Count > 1 )
                {
                    foreach ( var valueLine in propLines.Skip( 1 ) )
                    {
                        writer.WriteLine( $"\"{valueLine}\"" );
                    }
                }
            }

            // write headers

            // write elements
            foreach ( var element in elements )
            {
                // write comment
                writer.WriteLine( $"# {element.Comment ?? string.Empty}" );

                // write key
                WriteMultilineProperty( element.Key, MsgIdTag );

                // write value
                WriteMultilineProperty( element.Value, MsgStrTag );

                // write empty line between elements
                writer.WriteLine();
            }
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;
    }
}
