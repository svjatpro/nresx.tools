$ErrorActionPreference = 'Stop'
$toolsDir = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)"

$packageArgs = @{
  packageName    = 'nresx'
  unzipLocation  = $toolsDir
  url64bit       = 'https://github.com/svjatpro/nresx.tools/releases/download/v1.0.0/nresx-win-x64.zip'
  checksum64     = 'd8144e9789b1d940c19ab0b5623d52e3fedcd3eeee9ca411bdf8cf025bb6a44d'
  checksumType64 = 'sha256'
}

Install-ChocolateyZipPackage @packageArgs
