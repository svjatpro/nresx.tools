using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NResx.Tools
{
    public enum ResourceFormatType
    {
        NA = 0x00,

        Resx = 0x01,
        Resw = 0x02,
        YamlGeneric = 0x03,
        YamlRailsi18n = 0x04,
        Json = 0x05,

    }

    public class ResourceElement
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }
    }

    internal interface IFileLoader
    {
        bool LoadResourceFile( Stream stream, out List<ResourceElement> elements );
    }

    internal class FileLoaderResx : IFileLoader
    {
        public bool LoadResourceFile( Stream stream, out List<ResourceElement> elements )
        {
            var doc = XDocument.Load( stream );
            var entries = doc.Root?.Elements( "data" );

            elements = entries?
                .Select( e => new ResourceElement
                {
                    Key = e.Attribute( "name" )?.Value,
                    Value = e.Element( "value" )?.Value,
                    Comment = e.Element( "comment" )?.Value
                } )
                .ToList();

            return true;
        }
    }


    public class ResourceFile
    {
        private static readonly Dictionary<string, ResourceFormatType> TypesMap =
            new Dictionary<string, ResourceFormatType>
            {
                { ".resx", ResourceFormatType.Resx },
                { ".resw", ResourceFormatType.Resw },
            };

        private static readonly Dictionary<ResourceFormatType, Type> LoadersMap =
            new Dictionary<ResourceFormatType, Type>
            {
                //{ ResourceFormatType.NA, typeof(FileLoaderUnknown) }, // todo: detect type by loaded content
                {ResourceFormatType.Resx, typeof(FileLoaderResx)},
                {ResourceFormatType.Resw, typeof(FileLoaderResx)}
            };

        #region Private methods
        
        private bool TryDetectType( string path, out ResourceFormatType type )
        {
            var ext = Path.GetExtension( path );
            if ( TypesMap.ContainsKey( ext ) )
            {
                type = TypesMap[ext];
                return true;
            }
            type = ResourceFormatType.NA;
            return false;
        }

        private bool TryDetectType( Stream stream, out ResourceFormatType type )
        {
            //var ext = Path.GetExtension( path );
            //if ( TypesMap.ContainsKey( ext ) )
            //{
            //    type = TypesMap[ext];
            //    return true;
            //}
            type = ResourceFormatType.NA;
            return false;
        }
        
        #endregion

        public ResourceFormatType ResourceFormat { get; }
        public IEnumerable<ResourceElement> Elements { get; }

        public ResourceFile( string path )
        {
            if ( TryDetectType( path, out var type ) )
            {
                ResourceFormat = type;
            }
            else
            {
                // todo: detect type by content
                throw new FileLoadException( "the file is in unknown format" );
            }

            if ( !LoadersMap.ContainsKey( ResourceFormat ) )
                throw new FileLoadException( "the file is in unknown format" );

            using ( var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                var parser = (IFileLoader) Activator.CreateInstance( LoadersMap[ResourceFormat] );
                if ( parser.LoadResourceFile( stream, out var elements ) )
                {
                    Elements = elements;
                }


            }
        }
        public ResourceFile( Stream stream )
        {
            //DetectType( stream );
            //return ParseXmlFile( XDocument.Load( stream ) );
        }

        public ResourceFile( ResourceFormatType resourceFormat )
        {
            ResourceFormat = resourceFormat;
            Elements = new List<ResourceElement>();
        }


    }
}
