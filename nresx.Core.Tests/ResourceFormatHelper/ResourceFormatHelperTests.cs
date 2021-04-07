﻿using System.Threading.Tasks;
using NUnit.Framework;

namespace NResx.Tools.Tests.ResourceFormatHelper
{
    [TestFixture]
    public class ResourceFormatHelperTests
    {
        [TestCase( @"fd\df\res.resx", ExpectedResult = ResourceFormatType.Resx )]
        [TestCase( @".reSw", ExpectedResult = ResourceFormatType.Resw )]
        [TestCase( @".yml", ExpectedResult = ResourceFormatType.Yml )]
        [TestCase( @".yaml", ExpectedResult = ResourceFormatType.Yaml )]
        public async Task<ResourceFormatType> DetectFormatByExtension( string path )
        {
            Tools.ResourceFormatHelper.DetectFormatByExtension( path, out var res );
            return res;
        }
    }
}
