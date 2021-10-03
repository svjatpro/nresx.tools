using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace nresx.Tools.Formatters
{
    internal class FileFormatterPo : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            if ( LoadRawElements( stream, out var raw ) )
            {
                var dictionary = new Dictionary<string, ResourceElement>();
                foreach ( var el in raw )
                {
                    if ( !dictionary.ContainsKey( el.Key ) )
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
        private void ParseElementProperty( ResourceElement element, List<string> propLines, ElementParseState property )
        {
            switch ( property )
            {
                case ElementParseState.Msgstr:
                    element.Value = string.Join( Environment.NewLine, propLines );
                    break;
            }
        }
        private ResourceElement ParseElement( List<string> lines )
        {
            var element = new ResourceElement{ Type = ResourceElementType.String };
            var state = ElementParseState.None;
            var propLines = new List<string>();

            void parseProperty( ElementParseState nextProp )
            {
                if ( propLines.Any() )
                {
                    switch ( state )
                    {
                        case ElementParseState.Msgstr:
                            element.Value = string.Join( Environment.NewLine, propLines );
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

                        var comment = l.Length > 2 ? l.Substring( 2 ) : l;
                        element.Comment = comment;
                        break;
                    case var l when l.StartsWith( "msgid " ):
                        parseProperty( ElementParseState.Msgid );

                        var r1 = new Regex( @"msgid\s+""(.*)""" ).Match( l );
                        if ( r1.Success && r1.Groups.Count > 1 )
                        {
                            var key = r1.Groups[1].Value;
                            element.Key = key;
                        }
                        break;
                    case var l when l.StartsWith( "msgstr " ):
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

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements, bool validate = true )
        {
            using var writer = new StreamWriter( stream );
            var serializer = new SerializerBuilder()
                .Build();

            var body = elements.ToDictionary( el => el.Key, el => el.Value );
            serializer.Serialize( writer, body );
        }
    }
}
