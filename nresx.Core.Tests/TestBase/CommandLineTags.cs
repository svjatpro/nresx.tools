namespace nresx.Core.Tests
{
    public class CommandLineTags
    {
        /// <summary> Path to test files folder </summary>
        public const string FilesDir = "[Files]";

        /// <summary> Path to output files folder </summary>
        public const string OutputDir = "[Output]";

        /// <summary>
        /// Default resource file
        ///  can be used with dynamic resource type/extension, like [SourceFile.yaml]
        /// </summary>
        public const string SourceFile = "[SourceFile]";

        /// <summary>
        /// Generated destination file path
        ///  equal to "[Output]\\[UniqueKey].resx"
        ///  or can be used with dynamic resource type/extension, like [DestFile.yaml]
        /// </summary>
        public const string DestFile = "[DestFile]";

        /// <summary>
        /// Temporary file, copied from default resource file
        ///  will copy default resource file to  "[Output]\\[UniqueKey].resx"
        ///  can be used with dynamic resource type/extension, like [TmpFile.yaml]
        /// </summary>
        public const string TemporaryFile = "[TmpFile]";

        /// <summary>
        /// New generated file name, with unique name, and Output dir as a path
        /// </summary>
        public const string NewFile = "[NewFile]";

        /// <summary> Generated unique key </summary>
        public const string UniqueKey = "[UniqueKey]";
    }
}