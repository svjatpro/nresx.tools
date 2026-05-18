using System.Collections.Generic;
using System.Linq;
using nresx.Core.Extensions;

namespace nresx.Core.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="ResourceFile"/> constructors in <see cref="LoadMode.Strict"/> when post-load validation finds
    /// one or more Error-severity issues (duplicate keys, empty keys, etc.). The full list of findings — including
    /// warnings that did not by themselves trigger the throw — is exposed via <see cref="Errors"/>.
    /// </summary>
    public class ValidationException : NresxException
    {
        /// <summary>The full set of validation findings produced for the file (both errors and warnings).</summary>
        public IReadOnlyList<ResourceElementError> Errors { get; }

        /// <summary>Creates a new <see cref="ValidationException"/> carrying the given findings.</summary>
        public ValidationException( IReadOnlyList<ResourceElementError> errors )
            : base( BuildMessage( errors ) )
        {
            Errors = errors;
        }

        private static string BuildMessage( IReadOnlyList<ResourceElementError> errors )
        {
            var errorCount = errors.Count( e => e.ErrorType.GetSeverity() == ResourceElementErrorSeverity.Error );
            return $"Resource file failed strict-mode validation: {errorCount} error(s), {errors.Count - errorCount} warning(s).";
        }
    }
}
