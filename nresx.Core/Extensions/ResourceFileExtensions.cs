using System;

namespace nresx.Tools.Extensions
{
    public static class ResourceFileExtensions
    {
        public static void ConvertElements( this ResourceFile resourceFile, Action<ResourceElement> convertAction )
        {
            resourceFile.ConvertElements( null, convertAction );
        }
        public static void ConvertElements( 
            this ResourceFile resourceFile, 
            Func<ResourceElement, bool> predicate,
            Action<ResourceElement> convertAction )
        {
            foreach ( var element in resourceFile.Elements )
            {
                if ( predicate != null && !predicate( element ) )
                    continue;
                convertAction( element );
            }
        }
        
        public static void AddPrefix( this ResourceFile resourceFile, string prefix )
        {
            resourceFile.ConvertElements( 
                el => el.Type == ResourceElementType.String && !el.Value.StartsWith( prefix ),
                el => el.Value = $"{prefix}{el.Value}" );
        }

        public static void RemovePrefix( this ResourceFile resourceFile, string prefix )
        {
            resourceFile.ConvertElements(
                el => el.Type == ResourceElementType.String && el.Value.StartsWith( prefix ),
                el => el.Value = el.Value.Substring( prefix.Length ) );
        }

        public static void AddPostfix( this ResourceFile resourceFile, string postfix )
        {
            resourceFile.ConvertElements(
                el => el.Type == ResourceElementType.String && !el.Value.EndsWith( postfix ),
                el => el.Value = $"{el.Value}{postfix}" );
        }
        
        public static void RemovePostfix( this ResourceFile resourceFile, string postfix )
        {
            resourceFile.ConvertElements(
                el => el.Type == ResourceElementType.String && el.Value.EndsWith( postfix ),
                el => el.Value = el.Value.Substring( 0, el.Value.Length - postfix.Length ) );
        }
    }
}
