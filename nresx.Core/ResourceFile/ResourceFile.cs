using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    #region Private fields

    private readonly IFileFormatter SourceFormatter;
    private ResourceFileOption ResourceOptions;

    #endregion

    #region Private methods

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
            Headers[ResourceFileHeaders.Language] = Culture.Name;
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

    public ResourceElements Elements { get; private set; } = new ResourceElements();
    public List<Comment> Comments { get; private set; } = [];

    #region Static members

    public static IEnumerable<ResourceElement> LoadRawElements( string path )
    {
        if ( !FormatRegistry.TryGetByExtension( path, out var descriptor ) )
        {
            // todo: detect type by content
            throw new UnknownResourceFormatException();
        }

        var fileInfo = new FileInfo( path );
        if ( !fileInfo.Exists )
            return [];

        using var stream = new FileStream( fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
        var parser = descriptor!.CreateFormatter( null );
        return parser.LoadRawElements( stream, out var elements, out _, out _ ) ? elements : [];
    }
    public static IEnumerable<ResourceElement> LoadRawElements(
        Stream stream,
        ResourceFormatType resourceFormat = ResourceFormatType.NA )
    {
        IFileFormatter parser;
        if ( resourceFormat != ResourceFormatType.NA && FormatRegistry.TryGetByType( resourceFormat, out var byType ) )
        {
            parser = byType!.CreateFormatter( null );
        }
        else if ( FormatRegistry.TryGetByStream( stream, out var byStream ) )
        {
            parser = byStream!.CreateFormatter( null );
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
        if ( FormatRegistry.TryGetByExtension( path, out var descriptor ) )
        {
            FileFormat = descriptor!.Type;
            SourceFormatter = descriptor.CreateFormatter( options );
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
            if ( Headers.TryGetValue( ResourceFileHeaders.Language, out var languageHeader ) )
            {
                var c = new CultureInfo( languageHeader );
                if ( !Equals( c, CultureInfo.InvariantCulture ) )
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
        if ( resourceFormat != ResourceFormatType.NA && FormatRegistry.TryGetByType( resourceFormat, out var byType ) )
        {
            FileFormat = resourceFormat;
            SourceFormatter = byType!.CreateFormatter( options );
            ResourceOptions = options;
        }
        else if ( FormatRegistry.TryGetByStream( stream, out var byStream ) )
        {
            FileFormat = byStream!.Type;
            SourceFormatter = byStream.CreateFormatter( options );
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

    // Private constructor used by LoadAsync(string path):
    // path provides metadata (FileFormat, FileName, AbsolutePath, Culture),
    // loadedStream provides content (already read into memory async).
    private ResourceFile( string path, Stream loadedStream, ResourceFileOption? options )
    {
        if ( !FormatRegistry.TryGetByExtension( path, out var descriptor ) )
            throw new UnknownResourceFormatException();

        FileFormat = descriptor!.Type;
        SourceFormatter = descriptor.CreateFormatter( options );
        ResourceOptions = options;

        Culture = GetCultureByName( path );

        var fileInfo = new FileInfo( path );
        FileName = fileInfo.Name;
        AbsolutePath = fileInfo.FullName;

        if ( SourceFormatter.LoadResourceFile( loadedStream, out var elements, out var headers, out var comments ) )
        {
            Elements = new ResourceElements( elements );
            Headers = headers;
            if ( Headers.TryGetValue( ResourceFileHeaders.Language, out var languageHeader ) )
            {
                var c = new CultureInfo( languageHeader );
                if ( !Equals( c, CultureInfo.InvariantCulture ) )
                    Culture = c;
            }
            Comments = comments;
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
        if ( FormatRegistry.TryGetByType( fileFormat, out var descriptor ) )
            SourceFormatter = descriptor!.CreateFormatter( options );

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
        if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
        {
            throw new InvalidOperationException( "Unknown format" );
        }

        EnsurePathFormatConsistent( path, type );

        var targetPath = Path.ChangeExtension( path, descriptor!.Extension );
        var formatter = descriptor.CreateFormatter( null );

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
        if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
        {
            throw new InvalidOperationException( "Unknown format" );
        }
        var formatter = descriptor!.CreateFormatter( null );
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

    #region Async load

    public static async Task<ResourceFile> LoadAsync(
        string path,
        ResourceFileOption? options = null,
        CancellationToken cancellationToken = default )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileInfo = new FileInfo( path );
        if ( !fileInfo.Exists )
            return new ResourceFile( path, options );

        var buffer = await ReadAllBytesAsync( path, cancellationToken ).ConfigureAwait( false );

        cancellationToken.ThrowIfCancellationRequested();

        using var ms = new MemoryStream( buffer );
        return new ResourceFile( path, ms, options );
    }

    public static async Task<ResourceFile> LoadAsync(
        Stream stream,
        ResourceFormatType resourceFormat = ResourceFormatType.NA,
        ResourceFileOption? options = null,
        CancellationToken cancellationToken = default )
    {
        if ( stream == null ) throw new ArgumentNullException( nameof( stream ) );

        cancellationToken.ThrowIfCancellationRequested();

        var sourcePath = ( stream as FileStream )?.Name;

        using var ms = new MemoryStream();
        await stream.CopyToAsync( ms, 81920, cancellationToken ).ConfigureAwait( false );
        ms.Position = 0;

        cancellationToken.ThrowIfCancellationRequested();

        if ( !string.IsNullOrWhiteSpace( sourcePath ) )
            return new ResourceFile( sourcePath!, ms, options );

        return new ResourceFile( ms, resourceFormat, options );
    }

    public static async Task<IEnumerable<ResourceElement>> LoadRawElementsAsync(
        string path,
        CancellationToken cancellationToken = default )
    {
        if ( !FormatRegistry.TryGetByExtension( path, out var descriptor ) )
            throw new UnknownResourceFormatException();

        var fileInfo = new FileInfo( path );
        if ( !fileInfo.Exists ) return [];

        var buffer = await ReadAllBytesAsync( path, cancellationToken ).ConfigureAwait( false );

        cancellationToken.ThrowIfCancellationRequested();

        using var ms = new MemoryStream( buffer );
        var parser = descriptor!.CreateFormatter( null );
        return parser.LoadRawElements( ms, out var elements, out _, out _ ) ? elements : [];
    }

    public static async Task<IEnumerable<ResourceElement>> LoadRawElementsAsync(
        Stream stream,
        ResourceFormatType resourceFormat = ResourceFormatType.NA,
        CancellationToken cancellationToken = default )
    {
        if ( stream == null ) throw new ArgumentNullException( nameof( stream ) );

        IFileFormatter parser;
        if ( resourceFormat != ResourceFormatType.NA && FormatRegistry.TryGetByType( resourceFormat, out var byType ) )
            parser = byType!.CreateFormatter( null );
        else if ( FormatRegistry.TryGetByStream( stream, out var byStream ) )
            parser = byStream!.CreateFormatter( null );
        else
            throw new UnknownResourceFormatException();

        cancellationToken.ThrowIfCancellationRequested();

        using var ms = new MemoryStream();
        await stream.CopyToAsync( ms, 81920, cancellationToken ).ConfigureAwait( false );
        ms.Position = 0;

        cancellationToken.ThrowIfCancellationRequested();

        return parser.LoadRawElements( ms, out var elements, out _, out _ ) ? elements : [];
    }

    #endregion

    #region Async save

    public Task SaveAsync(
        string path,
        bool createDir = false,
        ResourceFileOption? options = null,
        CancellationToken cancellationToken = default )
    {
        return SaveAsync( path, FileFormat, createDir, options, cancellationToken );
    }

    public async Task SaveAsync(
        string path,
        ResourceFormatType type,
        bool createDir = false,
        ResourceFileOption? options = null,
        CancellationToken cancellationToken = default )
    {
        if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
            throw new InvalidOperationException( "Unknown format" );

        EnsurePathFormatConsistent( path, type );

        var targetPath = Path.ChangeExtension( path, descriptor!.Extension );
        var formatter = descriptor.CreateFormatter( null );

        var fileInfo = new FileInfo( targetPath );
        if ( fileInfo.Exists )
            fileInfo.Delete();

        var dirName = Path.GetDirectoryName( targetPath );
        if ( !string.IsNullOrWhiteSpace( dirName ) )
        {
            var dir = new DirectoryInfo( dirName! );
            if ( !dir.Exists && createDir )
                dir.Create();
        }

        cancellationToken.ThrowIfCancellationRequested();

        byte[] bytes;
        using ( var memory = new MemoryStream() )
        {
            formatter.SaveResourceFile( memory, Elements, PrepareHeaders(), Comments, options );
            bytes = memory.ToArray();
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var fs = new FileStream(
            targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true );
        await fs.WriteAsync( bytes, 0, bytes.Length, cancellationToken ).ConfigureAwait( false );
    }

    public Task SaveAsync(
        Stream stream,
        ResourceFileOption? options = null,
        CancellationToken cancellationToken = default )
    {
        return SaveAsync( stream, FileFormat, options, cancellationToken );
    }

    public async Task SaveAsync(
        Stream stream,
        ResourceFormatType type,
        ResourceFileOption? options = null,
        CancellationToken cancellationToken = default )
    {
        if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
            throw new InvalidOperationException( "Unknown format" );

        var formatter = descriptor!.CreateFormatter( null );

        cancellationToken.ThrowIfCancellationRequested();

        byte[] bytes;
        using ( var memory = new MemoryStream() )
        {
            formatter.SaveResourceFile( memory, Elements, PrepareHeaders(), Comments, options );
            bytes = memory.ToArray();
        }

        cancellationToken.ThrowIfCancellationRequested();

        await stream.WriteAsync( bytes, 0, bytes.Length, cancellationToken ).ConfigureAwait( false );
    }

    public async Task<Stream> SaveToStreamAsync( CancellationToken cancellationToken = default )
    {
        var ms = new MemoryStream();
        await SaveAsync( ms, cancellationToken: cancellationToken ).ConfigureAwait( false );
        ms.Position = 0;
        return ms;
    }

    #endregion

    // If `path` has an extension we recognize and it conflicts with `type`,
    // refuse to silently overwrite — the caller has a typo somewhere.
    // RSX-116 policy: extension + explicit format must agree when both are specified.
    private static void EnsurePathFormatConsistent( string path, ResourceFormatType type )
    {
        if ( !FormatRegistry.TryGetByExtension( path, out var pathDescriptor ) ) return;
        if ( pathDescriptor!.Type == type ) return;

        throw new InvalidOperationException(
            $"Format mismatch: path '{path}' has extension '{pathDescriptor.Extension}' (format: {pathDescriptor.Type}), but the requested format is {type}. " +
            $"Either change the path extension or omit the explicit format." );
    }

    #region Private async helpers

    // Reads the entire file into a byte buffer using async I/O. Honors cancellation.
    private static async Task<byte[]> ReadAllBytesAsync( string path, CancellationToken cancellationToken )
    {
        using var fs = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            bufferSize: 4096, useAsync: true );

        var buffer = new byte[fs.Length];
        var totalRead = 0;
        while ( totalRead < buffer.Length )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var n = await fs.ReadAsync( buffer, totalRead, buffer.Length - totalRead, cancellationToken ).ConfigureAwait( false );
            if ( n == 0 ) break;
            totalRead += n;
        }
        return buffer;
    }

    #endregion
}

