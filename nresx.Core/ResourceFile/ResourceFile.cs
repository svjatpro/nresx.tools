using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using nresx.Tools.Exceptions;
using nresx.Tools.Formatters;

namespace nresx.Tools;

public enum CommentType
{
    None = 0,
    Translator = 0x01,
    Extracted = 0x02,
    Reference = 0x03,
    Flags = 0x04,
    PreviousValue = 0x05,
}

public class Comment
{
    public CommentType Type { get; set; }
    public string? Value { get; set; }

    public Comment( CommentType type, string? value )
    {
        Type = type;
        Value = value;
    }
}

public class ResourceFile
{
    #region Static members

    private static readonly List<(string extensions, ResourceFormatType type, Type formatter)> TypesMap =
    [
        (extensions: ".resx", type: ResourceFormatType.Resx, typeof(FileFormatterResx)),
        (extensions: ".resw", type: ResourceFormatType.Resw, typeof(FileFormatterResx)),
        (extensions: ".yml", type: ResourceFormatType.Yml, typeof(FileFormatterYaml)),
        (extensions: ".yaml", type: ResourceFormatType.Yaml, typeof(FileFormatterYaml)),
        (extensions: ".txt", type: ResourceFormatType.PlainText, typeof(FileFormatterPlainText)),
        (extensions: ".po", type: ResourceFormatType.Po, typeof(FileFormatterPo)),
        (extensions: ".json", type: ResourceFormatType.Json, typeof(FileFormatterJson)),
    ];

    #endregion

    #region Private fields

    private readonly IFileFormatter SourceFormatter;
    private ResourceFileOption ResourceOptions;

    #endregion

    #region Private methods

    private static bool GetTypeInfo( 
        Func<(string extensions, ResourceFormatType type, Type formatter),bool> comparer, 
        out (string extensions, ResourceFormatType type, Func<ResourceFileOption?, IFileFormatter> formatter) type )
    {
        var tInfo = TypesMap.SingleOrDefault( comparer );
        var result = tInfo.type != ResourceFormatType.NA;
        
        if(result)
            type = (tInfo.extensions, tInfo.type, options => 
                tInfo.formatter.GetConstructor( [typeof(ResourceFileOption)] ) != null ?
                (IFileFormatter) Activator.CreateInstance( tInfo.formatter, options ) :
                (IFileFormatter) Activator.CreateInstance( tInfo.formatter ));
        else
            type = default;

        return result;
    }

    private static bool GetTypeInfo( 
        string path, 
        out (string extensions, ResourceFormatType type, Func<ResourceFileOption?, IFileFormatter> formatter) type )
    {
        var ext = Path.GetExtension( path );
        if ( string.IsNullOrWhiteSpace( ext ) )
        {
            type = default;
            return false;
        }

        var result = GetTypeInfo( t => t.extensions == ext, out type );

        return result;
    }

    private static bool GetTypeInfo( 
        Stream stream, 
        out (string extensions, ResourceFormatType type, Func<ResourceFileOption?, IFileFormatter> formatter) type )
    {
        var name = ( stream as FileStream )?.Name;
        var result = GetTypeInfo( name!, out type );

        return result;
    }

    private static CultureInfo GetCultureByName( string path )
    {
        var culture = CultureInfo.InvariantCulture;

        var fileName = Path.GetFileNameWithoutExtension( path );
        if ( string.IsNullOrWhiteSpace( fileName ) )
            return culture;

        var parts = fileName.Split( '_' );
        if ( parts.Length < 2 || string.IsNullOrWhiteSpace( parts.Last() ) )
            return culture;

        try
        {
            culture = new CultureInfo(parts.Last());
        }
        catch
        {
            // ignored
        }

        return culture;
    }

    private Dictionary<string, string> PrepareHeaders()
    {
        if ( !Equals( Culture, CultureInfo.InvariantCulture ) )
            Headers["Language"] = Culture.Name;
        return Headers;
    }

    #endregion

    public ResourceFormatType FileFormat { get; }
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    public Dictionary<string, string> Headers { get; set; } = [];

    public bool IsNewFile { get; }
    public bool HasChanges { get; } = false;
    
    public string FileName { get; }
    public string AbsolutePath { get; }

    public readonly ResourceElements Elements;
    public readonly List<Comment> Comments = [];

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
            return [];

        using var stream = new FileStream( fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
        var parser = type.formatter( null );
        return parser.LoadRawElements( stream, out var elements, out _, out _ ) ? elements : [];
    }
    public static IEnumerable<ResourceElement> LoadRawElements(
        Stream stream, 
        ResourceFormatType resourceFormat = ResourceFormatType.NA )
    {
        IFileFormatter parser;
        if ( resourceFormat != ResourceFormatType.NA && GetTypeInfo( t => t.type == resourceFormat, out var t1 ) )
        {
            parser = t1.formatter( null );
        }
        else if ( GetTypeInfo( stream, out var type ) )
        {
            parser = type.formatter( null );
        }
        else
        {
            // todo: detect type by content
            throw new UnknownResourceFormatException();
        }

        return parser.LoadRawElements( stream, out var elements, out _, out _ ) ? elements : [];
    }

    #endregion

    public ResourceFile( string path, ResourceFileOption? options = null )
    {
        if( GetTypeInfo( path, out var type ) )
        {
            FileFormat = type.type;
            SourceFormatter = type.formatter( options );
            ResourceOptions = options;
        }
        else
        {
            // todo: detect type by content
            throw new UnknownResourceFormatException();
        }

        // set culture by file name
        Culture = GetCultureByName( path );

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
        if ( SourceFormatter.LoadResourceFile( stream, out var elements, out var headers, out var comments ) )
        {
            Elements = new ResourceElements(elements);

            // override culture by header
            Headers = headers;
            if ( Headers.TryGetValue( "Language", out var languageHeader ) ) // make constants
            {
                var c = new CultureInfo( languageHeader );
                if(!Equals( c, CultureInfo.InvariantCulture ) )
                    Culture = c;
            }

            Comments = comments;
        }
    }

    public ResourceFile( 
        Stream stream, 
        ResourceFormatType resourceFormat = ResourceFormatType.NA, 
        ResourceFileOption? options = null )
    {
        if ( resourceFormat != ResourceFormatType.NA && GetTypeInfo( t => t.type == resourceFormat, out var t1 ) )
        {
            FileFormat = resourceFormat;
            SourceFormatter = t1.formatter( options );
            ResourceOptions = options;
        }
        else if ( GetTypeInfo( stream, out var type ) )
        {
            FileFormat = type.type;
            SourceFormatter = type.formatter( options );
            ResourceOptions = options;
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

        if ( SourceFormatter.LoadResourceFile( stream, out var elements, out _, out _ ) )
        {
            Elements = new ResourceElements( elements );
        }
    }

    public ResourceFile( ResourceFileOption? options = null )
    {
        IsNewFile = true;
        FileFormat = ResourceFormatType.NA;
        Elements = new ResourceElements();
        ResourceOptions = options;
    }
    public ResourceFile( ResourceFormatType fileFormat, ResourceFileOption? options = null )
    {
        IsNewFile = true;
        ResourceOptions = options;

        FileFormat = fileFormat;
        if ( GetTypeInfo( t => t.type == fileFormat, out var tInfo ) )
            SourceFormatter = tInfo.formatter( options );

        Elements = new ResourceElements();
    }

    #region Save

    public void Save( string path, bool createDir = false, ResourceFileOption? options = null )
    {
        Save( path, FileFormat, createDir, options );
    }
    public void Save( 
        string path,
        ResourceFormatType type,
        bool createDir = false,
        ResourceFileOption? options = null )
    {
        if ( !GetTypeInfo( t => t.type == type, out var tInfo ) )
        {
            throw new InvalidOperationException( "Unknown format" );
        }

        var targetPath = Path.ChangeExtension( path, tInfo.extensions );
        var formatter = tInfo.formatter( null );

        var fileInfo = new FileInfo( targetPath );
        if( fileInfo.Exists )
            fileInfo.Delete();

        var dirName = Path.GetDirectoryName( targetPath );
        if ( !string.IsNullOrWhiteSpace( dirName ) )
        {
            var dir = new DirectoryInfo( Path.GetDirectoryName( targetPath ) ?? string.Empty );
            if ( !dir.Exists && createDir )
            {
                dir.Create();
            }
        }

        using var stream = new FileStream( targetPath, FileMode.CreateNew );
        formatter.SaveResourceFile( stream, Elements, PrepareHeaders(), Comments, options );
    }

    public void Save( Stream stream, ResourceFileOption options = null )
    {
        Save( stream, FileFormat, options );
    }

    public void Save( Stream stream, ResourceFormatType type, ResourceFileOption options = null )
    {
        if ( !GetTypeInfo( t => t.type == type, out var tInfo ) )
        {
            throw new InvalidOperationException( "Unknown format" );
        }
        var formatter = tInfo.formatter( null );
        formatter.SaveResourceFile( stream, Elements, PrepareHeaders(), Comments, options );
    }

    public Stream SaveToStream()
    {
        using var ms = new MemoryStream();
        Save( ms );
        
        return new MemoryStream( ms.ToArray() );
    }

    #endregion

    #region Format info

    public bool ElementHasKey => SourceFormatter?.ElementHasKey ?? true;

    public bool ElementHasComment => SourceFormatter?.ElementHasComment ?? true;

    #endregion
}

