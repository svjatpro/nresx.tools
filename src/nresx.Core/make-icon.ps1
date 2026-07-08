Add-Type -AssemblyName System.Drawing

$size = 256
$bg = [System.Drawing.Color]::FromArgb(31, 41, 55)      # slate-800
$white = [System.Drawing.Color]::FromArgb(248, 250, 252) # slate-50
$orange = [System.Drawing.Color]::FromArgb(251, 146, 60) # orange-400
$fontSize = 150
$yOffset = -4

$enye = [char]0x00F1
$parts = @(
    @{ Text = "{";   Color = $white  }
    @{ Text = $enye; Color = $orange }
    @{ Text = "}";   Color = $white  }
)

$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

$bgBrush = New-Object System.Drawing.SolidBrush $bg
$g.FillRectangle($bgBrush, 0, 0, $size, $size)

$font = New-Object System.Drawing.Font "Segoe UI", $fontSize, ([System.Drawing.FontStyle]::Bold)
$fmt = [System.Drawing.StringFormat]::GenericTypographic

$widths = @()
$heights = @()
$totalWidth = 0.0
foreach ($p in $parts) {
    $sz = $g.MeasureString($p.Text, $font, ([System.Drawing.PointF]::new(0, 0)), $fmt)
    $widths += $sz.Width
    $heights += $sz.Height
    $totalWidth += $sz.Width
}

$cursorX = ($size - $totalWidth) / 2.0
$yCenter = ($size / 2.0) + $yOffset

for ($i = 0; $i -lt $parts.Count; $i++) {
    $brush = New-Object System.Drawing.SolidBrush $parts[$i].Color
    $y = $yCenter - $heights[$i] / 2.0
    $point = New-Object System.Drawing.PointF $cursorX, $y
    $g.DrawString($parts[$i].Text, $font, $brush, $point, $fmt)
    $cursorX += $widths[$i]
}

$bmp.Save("$PSScriptRoot\icon.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "Wrote $PSScriptRoot\icon.png"
