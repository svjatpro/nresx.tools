using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Xml.Linq;
using NResx.Tools.Formatters;

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

        public ResourceFile( Stream stream, ResourceFormatType resourceFormat = ResourceFormatType.NA )
        {
            IFileFormatter parser;
            if ( resourceFormat != ResourceFormatType.NA && GetTypeInfo( t => t.type == resourceFormat, out var t1 ) )
            {
                ResourceFormat = resourceFormat;
                parser = t1.formatter();
            }
            else if ( GetTypeInfo( stream, out var type ) )
            {
                ResourceFormat = type.type;
                parser = type.formatter();
            }
            else
            {
                // todo: detect type by content
                throw new FileLoadException( "the file is in unknown format" );
            }

            if ( parser.LoadResourceFile( stream, out var elements ) )
            {
                ElementsList = elements;
            }
        }

        public ResourceFile()
        {
            ResourceFormat = ResourceFormatType.NA;
            ElementsList = new List<ResourceElement>();
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

        #region Save

        public void Save( string path )
        {
            Save( path, ResourceFormat );
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

        public void Save( Stream stream )
        {
            Save( stream, ResourceFormat );
        }

        public void Save( Stream stream, ResourceFormatType type )
        {
            if ( !GetTypeInfo( t => t.type == type, out var tinfo ) )
            {
                throw new InvalidOperationException( "Unknown format" );
            }
            var formatter = tinfo.formatter();

            formatter.SaveResourceFile( stream, ElementsList );
        }

        public Stream SaveToStream()
        {
            var ms = new MemoryStream();
            Save( ms );
            
            return new MemoryStream( ms.GetBuffer() );
        }

        #endregion
        // (path, options)
        // (stream, options)

    }
}
