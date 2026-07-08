using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using nresx.Core.Exceptions;
using nresx.Core.Extensions;
using nresx.Core.Formatters;

namespace nresx.Core;

/// <summary>
/// Classification of a comment line associated with a resource element or
/// a whole resource file. Modeled after PO/gettext comment conventions; other
/// formats default to <see cref="Translator"/>.
/// </summary>
public enum CommentType
{
    /// <summary>Unspecified or unknown comment type.</summary>
    None = 0,
    /// <summary>Translator-facing comment (default for formats that have a single comment kind).</summary>
    Translator = 0x01,
    /// <summary>Comment extracted from source code (PO <c>#.</c>).</summary>
    Extracted = 0x02,
    /// <summary>Source reference, e.g. file:line (PO <c>#:</c>).</summary>
    Reference = 0x03,
    /// <summary>Format/processing flags (PO <c>#,</c>, e.g. <c>fuzzy</c>).</summary>
    Flags = 0x04,
    /// <summary>Previous untranslated value before the most recent edit (PO <c>#|</c>).</summary>
    PreviousValue = 0x05,
}

/// <summary>
/// A single comment line attached to a <see cref="ResourceFile"/> or <see cref="ResourceElement"/>.
/// </summary>
public class Comment
{
    /// <summary>The kind of comment (see <see cref="CommentType"/>).</summary>
    public CommentType Type { get; set; }
    /// <summary>The comment text, without the leading marker.</summary>
    public string? Value { get; set; }

    /// <summary>Creates a new comment of the given type and text.</summary>
    public Comment( CommentType type, string? value )
    {
        Type = type;
        Value = value;
    }
}

/// <summary>
/// In-memory representation of a localized resource file (resx, po, json, yaml, xliff, …).
/// Load via constructors, manipulate <see cref="Elements"/>, then call <see cref="Save(string,bool,ResourceFileOption?)"/>
/// (or <see cref="SaveAsync(string,bool,ResourceFileOption?,System.Threading.CancellationToken)"/>) to persist.
/// </summary>
public class ResourceFile
{
    #region Private fields

    private readonly IFileFormatter SourceFormatter;
    private readonly Dictionary<string, string> _headers = new();
    private readonly List<Comment> _comments = new();
    private readonly List<ResourceElementError> _validationErrors = new();

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

    // Builds the headers snapshot passed to formatters at save time. Injects the current
    // Culture as the Language header without mutating the in-memory _headers state.
    private Dictionary<string, string> PrepareHeaders()
    {
        var snapshot = new Dictionary<string, string>( _headers );
        if ( !Equals( Culture, CultureInfo.InvariantCulture ) )
            snapshot[ResourceFileHeaders.Language] = Culture.Name;
        return snapshot;
    }

    #endregion

    /// <summary>Format of the underlying resource file (or <see cref="ResourceFormatType.NA"/> for unsaved blank instances).</summary>
    public ResourceFormatType FileFormat { get; }

    /// <summary>
    /// Culture this file represents. Derived from an underscore-suffixed file name
    /// (e.g. <c>strings_de.resx</c> → <c>de</c>) or from the <c>Language</c> header when the
    /// file has one; defaults to <see cref="CultureInfo.InvariantCulture"/>. For broader
    /// path-based detection (dot suffix, culture directory) see
    /// <c>PathExtensions.TryToExtractCultureFromPath</c>.
    /// </summary>
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Free-form headers (key→value). Format-specific; e.g. PO files use this for <c>Content-Type</c>, <c>Plural-Forms</c>, etc.
    /// Read-only view; use <see cref="SetHeader"/> / <see cref="RemoveHeader"/> / <see cref="ClearHeaders"/> to mutate.
    /// Setting the <c>Language</c> header also syncs <see cref="Culture"/>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers => _headers;

    /// <summary>True when this instance was constructed in-memory (not loaded from disk).</summary>
    public bool IsNewFile { get; }

    /// <summary>Bare file name of the source file (when loaded from disk); empty otherwise.</summary>
    public string FileName { get; }

    /// <summary>Absolute path of the source file (when loaded from disk); empty otherwise.</summary>
    public string AbsolutePath { get; }

    /// <summary>The resource entries themselves. Use indexers and <c>Add</c>/<c>Remove</c> methods to mutate.</summary>
    public ResourceElements Elements { get; private set; } = new ResourceElements();

    /// <summary>
    /// File-level comments (those not attached to any specific element).
    /// Read-only view; use <see cref="AddComment(Comment)"/> / <see cref="RemoveComment"/> / <see cref="ClearComments"/> to mutate.
    /// </summary>
    public IReadOnlyList<Comment> Comments => _comments;

    /// <summary>Sets or replaces a header value. When <paramref name="name"/> is <c>Language</c>, also updates <see cref="Culture"/>.</summary>
    public void SetHeader( string name, string value )
    {
        if ( name == null ) throw new ArgumentNullException( nameof( name ) );
        _headers[name] = value ?? string.Empty;

        if ( name == ResourceFileHeaders.Language && !string.IsNullOrWhiteSpace( value ) )
        {
            try
            {
                var c = new CultureInfo( value );
                if ( !Equals( c, CultureInfo.InvariantCulture ) )
                    Culture = c;
            }
            catch
            {
                // ignore malformed culture names - header is still stored as authored
            }
        }
    }

    /// <summary>Removes a header. Returns true if it was present.</summary>
    public bool RemoveHeader( string name ) => _headers.Remove( name );

    /// <summary>Removes all headers.</summary>
    public void ClearHeaders() => _headers.Clear();

    /// <summary>Appends a file-level comment.</summary>
    public void AddComment( Comment comment )
    {
        if ( comment == null ) throw new ArgumentNullException( nameof( comment ) );
        _comments.Add( comment );
    }

    /// <summary>Convenience overload: creates and appends a <see cref="Comment"/> of the given type and text.</summary>
    public void AddComment( CommentType type, string? value ) => _comments.Add( new Comment( type, value ) );

    /// <summary>Removes the given file-level comment instance. Returns true if it was present.</summary>
    public bool RemoveComment( Comment comment ) => _comments.Remove( comment );

    /// <summary>Removes all file-level comments.</summary>
    public void ClearComments() => _comments.Clear();

    /// <summary>
    /// Validation findings produced during load (in <see cref="LoadMode.Strict"/> or <see cref="LoadMode.Lenient"/> mode)
    /// or by the most recent call to <see cref="Validate"/>. Always empty in <see cref="LoadMode.Raw"/>.
    /// </summary>
    public IReadOnlyList<ResourceElementError> ValidationErrors => _validationErrors;

    /// <summary>Re-runs validation against the current state and refreshes <see cref="ValidationErrors"/>. Does not throw.</summary>
    public IReadOnlyList<ResourceElementError> Validate()
    {
        _validationErrors.Clear();
        this.Elements.ValidateElements( out var errors );
        _validationErrors.AddRange( errors );
        return _validationErrors;
    }

    // Called from the load ctors after the formatter has populated Elements/Headers/Comments.
    // Strict: throw on any Error-severity finding; Lenient: collect; Raw: skip.
    private void RunPostLoadValidation( LoadMode mode )
    {
        if ( mode == LoadMode.Raw ) return;

        this.Elements.ValidateElements( out var errors );
        _validationErrors.AddRange( errors );

        if ( mode == LoadMode.Strict && _validationErrors.Any( e => e.ErrorType.GetSeverity() == ResourceElementErrorSeverity.Error ) )
        {
            throw new ValidationException( _validationErrors.ToList() );
        }
    }

    #region Static members

    /// <summary>
    /// Loads raw element rows from a file without deduping by key - duplicates are preserved
    /// in document order. Use this when you need to inspect the file as authored, including
    /// duplicate or malformed entries.
    /// </summary>
    /// <exception cref="UnknownResourceFormatException">Thrown when the file extension is not recognized.</exception>
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
    /// <summary>
    /// Stream variant of <see cref="LoadRawElements(string)"/>. Format is taken from
    /// <paramref name="resourceFormat"/> when supplied; otherwise inferred from the
    /// stream's filename if it has one.
    /// </summary>
    /// <exception cref="UnknownResourceFormatException">Thrown when the format cannot be determined.</exception>
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

    /// <summary>
    /// Loads a resource file from disk. The format is inferred from the file extension.
    /// If the file does not exist, the instance is created in <see cref="IsNewFile"/> mode
    /// (no error) so callers can populate elements and call <see cref="Save(string,bool,ResourceFileOption?)"/>.
    /// </summary>
    /// <exception cref="UnknownResourceFormatException">Thrown when the file extension is not recognized.</exception>
    public ResourceFile( string path, ResourceFileOption? options = null )
    {
        if ( FormatRegistry.TryGetByExtension( path, out var descriptor ) )
        {
            FileFormat = descriptor!.Type;
            SourceFormatter = descriptor.CreateFormatter( options );
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
            foreach ( var kv in headers ) _headers[kv.Key] = kv.Value;
            if ( _headers.TryGetValue( ResourceFileHeaders.Language, out var languageHeader ) )
            {
                var c = new CultureInfo( languageHeader );
                if ( !Equals( c, CultureInfo.InvariantCulture ) )
                    Culture = c;
            }

            _comments.AddRange( comments );
        }

        RunPostLoadValidation( options?.LoadMode ?? LoadMode.Strict );
    }

    /// <summary>
    /// Loads a resource file from a stream. Specify <paramref name="resourceFormat"/> when
    /// the stream's source format can't be inferred (e.g. <c>MemoryStream</c>).
    /// </summary>
    /// <exception cref="UnknownResourceFormatException">Thrown when the format cannot be determined.</exception>
    public ResourceFile(
        Stream stream,
        ResourceFormatType resourceFormat = ResourceFormatType.NA,
        ResourceFileOption? options = null )
    {
        if ( resourceFormat != ResourceFormatType.NA && FormatRegistry.TryGetByType( resourceFormat, out var byType ) )
        {
            FileFormat = resourceFormat;
            SourceFormatter = byType!.CreateFormatter( options );
        }
        else if ( FormatRegistry.TryGetByStream( stream, out var byStream ) )
        {
            FileFormat = byStream!.Type;
            SourceFormatter = byStream.CreateFormatter( options );
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

        RunPostLoadValidation( options?.LoadMode ?? LoadMode.Strict );
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

        Culture = GetCultureByName( path );

        var fileInfo = new FileInfo( path );
        FileName = fileInfo.Name;
        AbsolutePath = fileInfo.FullName;

        if ( SourceFormatter.LoadResourceFile( loadedStream, out var elements, out var headers, out var comments ) )
        {
            Elements = new ResourceElements( elements );
            foreach ( var kv in headers ) _headers[kv.Key] = kv.Value;
            if ( _headers.TryGetValue( ResourceFileHeaders.Language, out var languageHeader ) )
            {
                var c = new CultureInfo( languageHeader );
                if ( !Equals( c, CultureInfo.InvariantCulture ) )
                    Culture = c;
            }
            _comments.AddRange( comments );
        }

        RunPostLoadValidation( options?.LoadMode ?? LoadMode.Strict );
    }

    /// <summary>Creates a new, empty resource file with no specific format set.</summary>
    public ResourceFile( ResourceFileOption? options = null )
    {
        IsNewFile = true;
        FileFormat = ResourceFormatType.NA;
        Elements = new ResourceElements();
    }

    /// <summary>Creates a new, empty resource file of the given format.</summary>
    public ResourceFile( ResourceFormatType fileFormat, ResourceFileOption? options = null )
    {
        IsNewFile = true;

        FileFormat = fileFormat;
        if ( FormatRegistry.TryGetByType( fileFormat, out var descriptor ) )
            SourceFormatter = descriptor!.CreateFormatter( options );

        Elements = new ResourceElements();
    }

    #region Save

    /// <summary>
    /// Saves to a file path using the format the file was loaded as
    /// (or constructed with). The extension is forced to match the format.
    /// </summary>
    public void Save( string path, bool createDir = false, ResourceFileOption? options = null )
    {
        Save( path, FileFormat, createDir, options );
    }

    /// <summary>
    /// Saves to <paramref name="path"/> as <paramref name="type"/>.
    /// </summary>
    /// <param name="path">Destination path. The extension is rewritten to match <paramref name="type"/> unless it already does.</param>
    /// <param name="type">Format to write.</param>
    /// <param name="createDir">If true, missing parent directories are created.</param>
    /// <exception cref="UnknownResourceFormatException">Thrown when <paramref name="type"/> is not registered.</exception>
    /// <exception cref="ResourceFormatMismatchException">Thrown when <paramref name="path"/>'s extension belongs to a different known format than <paramref name="type"/>.</exception>
    public void Save(
        string path,
        ResourceFormatType type,
        bool createDir = false,
        ResourceFileOption? options = null )
    {
        if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
        {
            throw new UnknownResourceFormatException();
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
        formatter.SaveResourceFile( stream, Elements, PrepareHeaders(), _comments, options );
    }

    public void Save( Stream stream, ResourceFileOption options = null )
    {
        Save( stream, FileFormat, options );
    }

    public void Save( Stream stream, ResourceFormatType type, ResourceFileOption options = null )
    {
        if ( !FormatRegistry.TryGetByType( type, out var descriptor ) )
        {
            throw new UnknownResourceFormatException();
        }
        var formatter = descriptor!.CreateFormatter( null );
        formatter.SaveResourceFile( stream, Elements, PrepareHeaders(), _comments, options );
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
            throw new UnknownResourceFormatException();

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
            formatter.SaveResourceFile( memory, Elements, PrepareHeaders(), _comments, options );
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
            throw new UnknownResourceFormatException();

        var formatter = descriptor!.CreateFormatter( null );

        cancellationToken.ThrowIfCancellationRequested();

        byte[] bytes;
        using ( var memory = new MemoryStream() )
        {
            formatter.SaveResourceFile( memory, Elements, PrepareHeaders(), _comments, options );
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
    // refuse to silently overwrite - the caller has a typo somewhere.
    // RSX-116 policy: extension + explicit format must agree when both are specified.
    private static void EnsurePathFormatConsistent( string path, ResourceFormatType type )
    {
        if ( !FormatRegistry.TryGetByExtension( path, out var pathDescriptor ) ) return;
        if ( pathDescriptor!.Type == type ) return;

        throw new ResourceFormatMismatchException(
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

