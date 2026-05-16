using nresx.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace nresx.Tools.Formatters
{
    internal class FileFormatterPo( ResourceFileOption? options = null ) : IFileFormatter
    {
        #region Private fields

        private const string MsgIdTag = "msgid";
        private const string MsgIdPluralTag = "msgid_plural";
        private const string MsgStrTag = "msgstr";
        private const string MsgCtxtTag = "msgctxt";

        private readonly ResourceFileOptionPo Options = (options as ResourceFileOptionPo) ?? new ResourceFileOptionPo();


        private static readonly Dictionary<CommentType, string> CommentPrefix = new()
        {
            { CommentType.Translator, "#  " },
            { CommentType.Extracted, "#. " },
            { CommentType.Reference, "#: " },
            { CommentType.Flags, "#, " },
            { CommentType.PreviousValue, "#| " },
        };

        #endregion

        #region Private methods

        private Dictionary<string, string> ParseHeaders( string lines )
        {
            return lines
                .Split( [Environment.NewLine], StringSplitOptions.RemoveEmptyEntries )
                .Select( h => h.Split([": "], StringSplitOptions.None ) )
                .Where( parts =>
                    parts.Length == 2 &&
                    ( !string.IsNullOrWhiteSpace( parts[1] ) || Options.IgnoreEmptyHeaders == false ) )
                .Select( parts => new { key = parts[0], value = parts[1] } )
                .ToDictionary( h => h.key, h => h.value );
        }

        private enum ElementParseState { None, Comment, MsgId, MsgIdPlural, MsgStr, MsgStrPlural, MsgContext }
        private static ResourceElement ParseElement(List<string> lines)
        {
            var element = new ResourceElement { Type = ResourceElementType.String };
            var state = ElementParseState.None;
            var pluralIndex = 0;
            var propLines = new List<string>();
            var commentTypes = CommentPrefix.ToDictionary(c => c.Value, c => c.Key);

            foreach (var line in lines)
            {
                switch (line)
                {
                    case "#":
                        break;
                    case var _ when line.StartsWith("#"):
                        ParseProperty( ElementParseState.Comment );
                        if ( line.Length <= 3 || !commentTypes.TryGetValue( line.Substring( 0, 3 ), out var commentType ) )
                        {
                            break;
                        }
                        element.Comments.Add(new Comment(commentType, line.Substring(3)));
                        break;
                    case var _ when line.StartsWith($"{MsgCtxtTag} "):
                        ParseProperty( ElementParseState.MsgContext );
                        if (ParseLine(line, $@"{MsgCtxtTag}\s+", out var ctxt))
                        {
                            propLines.Add( ctxt );
                        }
                        break;
                    case var _ when line.StartsWith( $"{MsgIdTag} " ):
                        ParseProperty( ElementParseState.MsgId );
                        if ( ParseLine( line, $@"{MsgIdTag}\s+", out var id ) )
                        {
                            propLines.Add( id );
                        }
                        break;
                    case var _ when line.StartsWith( $"{MsgIdPluralTag} " ):
                        ParseProperty( ElementParseState.MsgIdPlural );
                        if ( ParseLine( line, $@"{MsgIdPluralTag}\s+", out var idPlural ) )
                        {
                            propLines.Add( idPlural );
                        }
                        break;
                    case var _ when line.StartsWith( $"{MsgStrTag} " ):
                        ParseProperty( ElementParseState.MsgStr );
                        if ( ParseLine( line, $@"{MsgStrTag}\s+", out var str ) )
                        {
                            propLines.Add( str );
                        }
                        break;
                    case var _ when line.StartsWith($"{MsgStrTag}[") && ParsePluralIndex( line, out var idx ):
                        ParseProperty( ElementParseState.MsgStrPlural, idx );
                        if (ParseLine(line, $@"{MsgStrTag}\[{idx}\]\s+", out var plural))
                        {
                            propLines.Add( plural );
                        }
                        break;
                    case var _ when line.Trim( ' ' ).StartsWith( "\"" ):
                        if ( state != ElementParseState.None && ParseLine( line, "", out var next ) )
                        {
                            propLines.Add( next );
                        }
                        break;
                }
            }
            ParseProperty(ElementParseState.None);

            return element;

            bool ParsePluralIndex(string line, out int index)
            {
                var r1 = new Regex(@$"^{MsgStrTag}\[(\d+)\]").Match(line);
                if (r1 is { Success: true, Groups.Count: > 1 } && int.TryParse(r1.Groups[1].Value, out var idx))
                {
                    index = idx;
                    return true;
                }
                index = -1;
                return false;
            }
            bool ParseLine( string line, string prefix, out string value )
            {
                var r1 = new Regex( @$"^{prefix}""(.*)""" ).Match( line );
                if (r1 is { Success: true, Groups.Count: > 1 })
                {
                    value = r1.Groups[1].Value;
                    return true;
                }
                value = string.Empty;
                return false;
            }
            void ParseProperty(ElementParseState nextProp, int index = -1)
            {
                if (propLines.Any())
                {
                    var value = string.Join("", propLines).Replace(@"\n", Environment.NewLine);
                    switch (state)
                    {
                        case ElementParseState.MsgContext:
                            element.Context = value;
                            break;
                        case ElementParseState.MsgId:
                            element.Key = value;
                            break;
                        case ElementParseState.MsgIdPlural:
                            element.KeyPlural = value;
                            break;
                        case ElementParseState.MsgStr:
                            element.Value = value;
                            break;
                        case ElementParseState.MsgStrPlural:
                            element.ValuePlurals[pluralIndex] = value;
                            break;
                    }
                    propLines.Clear();
                }

                state = nextProp;
                pluralIndex = index;
            }
        }

        #endregion

        public bool LoadResourceFile(
            Stream stream, 
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            if ( LoadRawElements( stream, out var raw, out headers, out comments ) )
            {
                //var dictionary = new Dictionary<string, ResourceElement>();
                var result = new List<ResourceElement>();
                var elementsList = raw.ToList();
                foreach (var el in elementsList)
                {
                    // todo: validate plural rules (header) with element's plural forms
                    //  add errors

                    if (string.IsNullOrWhiteSpace(el.Key) /*|| dictionary.ContainsKey(el.Key)*/)
                        continue;
                    result.Add( el );
                    //dictionary.Add(el.Key, el);
                }
                //elements = dictionary.Values.ToList();
                elements = result.ToList();
                return true;
            }

            elements = [];
            headers = [];
            comments = [];

            return false;
        }

        public bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            using var reader = new StreamReader( stream );
            headers = [];
            comments = [];

            var result = new List<ResourceElement>();
            var elementLines = new List<string>();
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine();

                if ( string.IsNullOrWhiteSpace( line ) )
                {
                    var el = ParseElement( elementLines );
                    if ( string.IsNullOrWhiteSpace( el.Key ) && !result.Any() )
                    {
                        headers = ParseHeaders( el.Value );
                        comments = el.Comments;
                    }
                    else
                    {
                        result.Add( el );
                    }
                    elementLines.Clear();
                }
                else
                {
                    elementLines.Add( line );
                }
            }
            if( elementLines.Any() )
                result.Add( ParseElement( elementLines ) );

            elements = result;
            
            return true;
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null )
        {
            using var writer = new StreamWriter( stream );

            // write global comments
            if (comments.Any())
            {
                foreach (var comment in comments)
                {
                    if (!CommentPrefix.TryGetValue(comment.Type, out var prefix))
                        continue;
                    writer.WriteLine($"{prefix}{comment.Value}");
                }
            }
            //else
            //{
            //    writer.WriteLine("#");
            //}

            // write headers
            WriteMultilineProperty(string.Empty, MsgIdTag);
            var headersPrepared = string.Join(
                Environment.NewLine,
                [/*string.Empty,*/..headers.Select(h => $"{h.Key}: {h.Value}\\n")]);
            WriteMultilineProperty( headersPrepared, MsgStrTag );
            writer.WriteLine();

            // write elements
            foreach ( var element in elements )
            {
                // write comment
                foreach ( var comment in element.Comments )
                {
                    if( !CommentPrefix.TryGetValue( comment.Type, out var prefix ) ) continue; //
                    writer.WriteLine($"{prefix}{comment.Value}");
                }

                // write context
                if ( element.Context != null )
                {
                    WriteMultilineProperty( element.Context, MsgCtxtTag );
                }

                // write key
                WriteMultilineProperty( element.Key, MsgIdTag );
                
                // write key plural
                if ( !string.IsNullOrWhiteSpace( element.KeyPlural ) )
                {
                    WriteMultilineProperty(element.KeyPlural!, MsgIdPluralTag);
                }

                // write value
                WriteMultilineProperty( element.Value ?? string.Empty, MsgStrTag );

                // write value plural
                foreach ( var plural in element.ValuePlurals )
                {
                    WriteMultilineProperty( plural.Value, $"{MsgStrTag}[{plural.Key}]" );
                }

                // write empty line between elements
                writer.WriteLine();
            }

            return;

            void WriteMultilineProperty( string property, string tag )
            {
                //var propLines = property.SplitLines().Select( l => $"{l}\\n" ).ToList();
                var propLines = property.SplitLines().ToList(); //.Select( l => $"{l}\\n" ).ToList();

                if ( propLines.Count > 1 )
                {
                    for (var i = 0; i < propLines.Count - 1; i++)
                    {
                        if( !string.IsNullOrWhiteSpace( propLines[i] ) )
                            propLines[i] = $"{propLines[i]}\\n";
                    }
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
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;
    }
}
