using nresx.Core.Extensions;

namespace nresx.Core;

/// <summary>
/// A single validation finding scoped to the file it was raised against. Wraps the element-level
/// <see cref="ResourceElementError"/> (which carries the error type, element key, and message) with
/// the path of the file, so cross-file validation results can be reported as data.
/// </summary>
public class ResourceValidationIssue
{
    /// <summary>Absolute path of the file the finding belongs to.</summary>
    public string FilePath { get; }

    /// <summary>The underlying element-level validation error (type, element key, message).</summary>
    public ResourceElementError Error { get; }

    /// <summary>Creates a new file-scoped validation issue.</summary>
    public ResourceValidationIssue( string filePath, ResourceElementError error )
    {
        FilePath = filePath;
        Error = error;
    }
}
