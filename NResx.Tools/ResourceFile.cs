using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Aspose.Cells;
using NResx.Tools.Winforms;

namespace NResx.Tools
{
    public enum ResourceFormatType
    {
        NA = 0x00,

        Resx = 0x01,
        Resw = 0x02,

        Yml = 0x03,
        Yaml = 0x04,

        Json = 0x05,
    }

    public class ResourceElement
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }
    }

    internal interface IFileFormatter
    {
        bool LoadResourceFile( Stream stream, out List<ResourceElement> elements );
        void SaveResourceFile( Stream stream, List<ResourceElement> elements );
    }

    internal class FileFormatterResx : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out List<ResourceElement> elements )
        {
            using var reader = new ResXResourceReader( stream );
            reader.UseResXDataNodes = true;
            var result = new List<ResourceElement>();
            foreach ( DictionaryEntry item in reader )
            {
                var node = item.Value as ResXDataNode;
                var nodeInfo = node?.GetDataNodeInfo();
                result.Add( new ResourceElement
                {
                    Key = item.Key.ToString(),
                    Value = nodeInfo?.ValueData ?? item.Value.ToString(),
                    Comment = nodeInfo?.Comment
                } );
            }
            elements = result;
            return true;
        }

        public void SaveResourceFile( Stream stream, List<ResourceElement> elements )
        {
            using var writer = new ResXResourceWriter( stream );
            elements.ForEach( el =>
            {
                writer.AddResource( new ResXDataNode( el.Key, el.Value ){ Comment = el.Comment } );
            } );
            writer.Generate();
            writer.Close();
        }
    }

    internal class FileFormatterYaml : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out List<ResourceElement> elements )
        {
            // todo: replace stub implementation
            var resRegex = new Regex( @"^\s*([a-zA-Z0-9_.]+)\s*:\s*""([^""]*)""\s*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

            elements = new List<ResourceElement>();
            using var reader = new StreamReader( stream );
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine();
                if ( string.IsNullOrWhiteSpace( line ) )
                    continue;
                var match = resRegex.Match( line );
                if ( match.Success && match.Groups.Count > 1 )
                {
                    elements.Add( new ResourceElement
                    {
                        Key = match.Groups[1].Value,
                        Value = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty
                    } );
                }
            }

            return true;
        }

        public void SaveResourceFile( Stream stream, List<ResourceElement> elements )
        {
            using var writer = new StreamWriter( stream );
            foreach ( var element in elements )
            {
                writer.WriteLine( $"{element.Key}: \"{element.Value}\"" );
            }
        }
    }

    public class ResourceFile
    {
        #region Private fields

        private readonly List<ResourceElement> ElementsList;

        private static readonly List<(string extensions, ResourceFormatType type, Type formatter)> TypesMap =
            new List<( string extensions, ResourceFormatType type, Type formatter)>
            {
                ( extensions: ".resx", type: ResourceFormatType.Resx, typeof(FileFormatterResx) ),
                ( extensions: ".resw", type: ResourceFormatType.Resw, typeof(FileFormatterResx) ),
                ( extensions: ".yml", type: ResourceFormatType.Yml, typeof(FileFormatterYaml) ),
                ( extensions: ".yaml", type: ResourceFormatType.Yaml, typeof(FileFormatterYaml) ),
            };

        #endregion

        #region Private methods

        private bool GetTypeInfo( 
            Func<(string extensions, ResourceFormatType type, Type formatter),bool> comparer, 
            out (string extensions, ResourceFormatType type, Func<IFileFormatter> formatter) type )
        {
            var tinfo = TypesMap.SingleOrDefault( comparer );
            var result = tinfo.type != ResourceFormatType.NA;
            
            if(result)
                type = (tinfo.extensions, tinfo.type, () => (IFileFormatter) Activator.CreateInstance( tinfo.formatter ));
            else
                type = default;

            return result;
        }

        private bool GetTypeInfo( string path, out (string extensions, ResourceFormatType type, Func<IFileFormatter> formatter) type )
        {
            var ext = Path.GetExtension( path );
            if ( string.IsNullOrWhiteSpace( ext ) )
            {
                type = default;
                return false;
            }

            var result = GetTypeInfo( t => t.extensions == ext, out var tinfo );
            type = tinfo;

            return result;
        }

        private bool GetTypeInfo( Stream stream, out (string extensions, ResourceFormatType type, Func<IFileFormatter> formatter) type )
        {
            var name = ( stream as FileStream )?.Name;
            var result = GetTypeInfo( name, out var tinfo );
            type = tinfo;

            return result;
        }

        #endregion

        public ResourceFormatType ResourceFormat { get; }

        public IEnumerable<ResourceElement> Elements => ElementsList;

        public ResourceFile( string path )
        {
            if( GetTypeInfo( path, out var type ) )
            {
                ResourceFormat = type.type;
            }
            else
            {
                // todo: detect type by content
                throw new FileLoadException( "the file is in unknown format" );
            }

            using ( var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                var parser = type.formatter();
                if ( parser.LoadResourceFile( stream, out var elements ) )
                {
                    ElementsList = elements;
                }
            }
        }

        public ResourceFile( Stream stream )
        {
            if ( GetTypeInfo( stream, out var type ) )
            {
                ResourceFormat = type.type;
            }
            else
            {
                // todo: detect type by content
                throw new FileLoadException( "the file is in unknown format" );
            }

            var parser = type.formatter();
            if ( parser.LoadResourceFile( stream, out var elements ) )
            {
                ElementsList = elements;
            }
        }

        public ResourceFile( ResourceFormatType resourceFormat )
        {
            ResourceFormat = resourceFormat;
            ElementsList = new List<ResourceElement>();
        }

        public void AddElement( string key, string value, string comment )
        {
            ElementsList.Add( new ResourceElement
            {
                Key = key,
                Value = value,
                Comment = comment
            } );
        }

        public void Save( string path )
        {

        }
        public void Save( string path, ResourceFormatType type )
        {
            if ( !GetTypeInfo( t => t.type == type, out var tinfo ) )
            {
                throw new InvalidOperationException( "Unknown format" );
            }

            var targetPath = Path.ChangeExtension( path, tinfo.extensions );
            var formatter = tinfo.formatter();

            formatter.SaveResourceFile( 
                new FileStream( targetPath, FileMode.CreateNew ),
                ElementsList );
        }
    }
}
