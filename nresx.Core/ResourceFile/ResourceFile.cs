using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using nresx.Tools.Exceptions;
using nresx.Tools.Formatters;

namespace nresx.Tools
{
    public class ResourceFile
    {
        #region Private fields

        private static readonly List<(string extensions, ResourceFormatType type, Type formatter)> TypesMap =
            new()
            {
                ( extensions: ".resx", type: ResourceFormatType.Resx, typeof(FileFormatterResx) ),
                ( extensions: ".resw", type: ResourceFormatType.Resw, typeof(FileFormatterResx) ),
                ( extensions: ".yml", type: ResourceFormatType.Yml, typeof(FileFormatterYaml) ),
                ( extensions: ".yaml", type: ResourceFormatType.Yaml, typeof(FileFormatterYaml) ),
                ( extensions: ".txt", type: ResourceFormatType.PlainText, typeof(FileFormatterPlainText) ),
            };

        #endregion

        #region Private methods

        private static bool GetTypeInfo( 
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

        private static bool GetTypeInfo( string path, out (string extensions, ResourceFormatType type, Func<IFileFormatter> formatter) type )
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

        private static bool GetTypeInfo( Stream stream, out (string extensions, ResourceFormatType type, Func<IFileFormatter> formatter) type )
        {
            var name = ( stream as FileStream )?.Name;
            var result = GetTypeInfo( name, out var tinfo );
            type = tinfo;

            return result;
        }

        #endregion

        public ResourceFormatType FileFormat { get; }
        
        public bool IsNewFile { get; }
        public bool HasChanges { get; } = false;
        
        public string FileName { get; }
        public string AbsolutePath { get; }

        public readonly ResourceElements Elements;

        #region Static members

        public static IEnumerable<ResourceElement> LoadRawElements( string path )
        {
            if ( !GetTypeInfo( path, out var type ) )
            {
                // todo: detect type by content
                throw new UnknownResourceFormatException();
            }

            var fileInfo = new FileInfo( path );
            if ( !fileInfo.Exists )
                return new ResourceElement[0];

            using var stream = new FileStream( fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var parser = type.formatter();
            if ( parser.LoadRawElements( stream, out var elements ) )
                return elements;

            return new ResourceElement[0];
        }
        public static IEnumerable<ResourceElement> LoadRawElements( Stream stream, ResourceFormatType resourceFormat = ResourceFormatType.NA )
        {
            IFileFormatter parser;
            if ( resourceFormat != ResourceFormatType.NA && GetTypeInfo( t => t.type == resourceFormat, out var t1 ) )
            {
                parser = t1.formatter();
            }
            else if ( GetTypeInfo( stream, out var type ) )
            {
                parser = type.formatter();
            }
            else
            {
                // todo: detect type by content
                throw new UnknownResourceFormatException();
            }

            if ( parser.LoadRawElements( stream, out var elements ) )
            {
                return elements;
            }

            return new ResourceElement[0];
        }

        #endregion

        public ResourceFile( string path )
        {
            if( GetTypeInfo( path, out var type ) )
            {
                FileFormat = type.type;
            }
            else
            {
                // todo: detect type by content
                throw new UnknownResourceFormatException();
            }

            var fileInfo = new FileInfo( path );
            FileName = fileInfo.Name;
            AbsolutePath = fileInfo.FullName;
            if ( !fileInfo.Exists )
            {
                IsNewFile = true;
                Elements = new ResourceElements();
                return;
            }

            using var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var parser = type.formatter();
            if ( parser.LoadResourceFile( stream, out var elements ) )
            {
                Elements = new ResourceElements(elements);
            }
        }

        public ResourceFile( Stream stream, ResourceFormatType resourceFormat = ResourceFormatType.NA )
        {
            IFileFormatter parser;
            if ( resourceFormat != ResourceFormatType.NA && GetTypeInfo( t => t.type == resourceFormat, out var t1 ) )
            {
                FileFormat = resourceFormat;
                parser = t1.formatter();
            }
            else if ( GetTypeInfo( stream, out var type ) )
            {
                FileFormat = type.type;
                parser = type.formatter();
            }
            else
            {
                // todo: detect type by content
                throw new UnknownResourceFormatException();
            }

            var path = ( stream as FileStream )?.Name;
            if ( !string.IsNullOrWhiteSpace( path ) )
            {
                var fileInfo = new FileInfo( path );
                FileName = fileInfo.Name;
                AbsolutePath = fileInfo.FullName;
            }

            if ( parser.LoadResourceFile( stream, out var elements ) )
            {
                Elements = new ResourceElements( elements );
            }
        }

        public ResourceFile()
        {
            IsNewFile = true;
            FileFormat = ResourceFormatType.NA;
            Elements = new ResourceElements();
        }
        public ResourceFile( ResourceFormatType fileFormat )
        {
            IsNewFile = true;
            FileFormat = fileFormat;
            Elements = new ResourceElements();
        }

        #region Save

        public void Save( string path, bool createDir = false )
        {
            Save( path, FileFormat, createDir );
        }
        public void Save( string path, ResourceFormatType type, bool createDir = false )
        {
            if ( !GetTypeInfo( t => t.type == type, out var tinfo ) )
            {
                throw new InvalidOperationException( "Unknown format" );
            }

            var targetPath = Path.ChangeExtension( path, tinfo.extensions );
            var formatter = tinfo.formatter();

            var fileInfo = new FileInfo( targetPath );
            if( fileInfo.Exists )
                fileInfo.Delete();

            var dir = new DirectoryInfo( Path.GetDirectoryName( targetPath ) ?? string.Empty );
            if ( !dir.Exists && createDir )
            {
                dir.Create();
            }

            formatter.SaveResourceFile( new FileStream( targetPath, FileMode.CreateNew ), Elements );
        }

        public void Save( Stream stream )
        {
            Save( stream, FileFormat );
        }

        public void Save( Stream stream, ResourceFormatType type )
        {
            if ( !GetTypeInfo( t => t.type == type, out var tinfo ) )
            {
                throw new InvalidOperationException( "Unknown format" );
            }
            var formatter = tinfo.formatter();
            formatter.SaveResourceFile( stream, Elements );
        }

        public Stream SaveToStream()
        {
            var ms = new MemoryStream();
            Save( ms );
            
            return new MemoryStream( ms.GetBuffer() );
        }

        #endregion
    }
}
