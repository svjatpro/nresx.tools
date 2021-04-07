// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace nresx.Winforms
{
    static internal class ClientUtils
    {
        private static string NamespaceSystem => "System.Resources.";
        private static string NamespaceModified => "nresx.Winforms.";

        // ExecutionEngineException is obsolete and shouldn't be used (to catch, throw or reference) anymore.
        // Pragma added to prevent converting the "type is obsolete" warning into build error.
        // File owner should fix this.
        public static bool IsCriticalException(Exception ex)
        {
            return ex is NullReferenceException
                    || ex is StackOverflowException
                    || ex is OutOfMemoryException
                    || ex is System.Threading.ThreadAbortException
                    || ex is ExecutionEngineException
                    || ex is IndexOutOfRangeException
                    || ex is AccessViolationException;
        }

        public static string ToModifiedNamespace( this string fullName )
        {
            return fullName.Replace( NamespaceSystem, NamespaceModified );
        }

        public static string ToSystemNamespace( this string fullName )
        {
            return fullName.Replace( NamespaceModified, NamespaceSystem );
        }
    }
}
