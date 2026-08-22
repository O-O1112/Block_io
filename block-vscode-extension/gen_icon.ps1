$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$bmp = New-Object System.Drawing.Bitmap(128, 128)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(42, 42, 42))

$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 6)
$pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

# B letter left bar
$g.DrawRectangle($pen, 25, 20, 19, 76)

# B letter upper bump
$points1 = @(
    [System.Drawing.Point]::new(50, 20),
    [System.Drawing.Point]::new(89, 20),
    [System.Drawing.Point]::new(96, 42),
    [System.Drawing.Point]::new(89, 57),
    [System.Drawing.Point]::new(50, 57)
)
$g.DrawPolygon($pen, $points1)

# B letter lower bump
$points2 = @(
    [System.Drawing.Point]::new(50, 70),
    [System.Drawing.Point]::new(96, 70),
    [System.Drawing.Point]::new(102, 86),
    [System.Drawing.Point]::new(96, 102),
    [System.Drawing.Point]::new(50, 102)
)
$g.DrawPolygon($pen, $points2)

$bmp.Save("$PSScriptRoot\icon.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()
Write-Host "icon.png created successfully"
