Add-Type -AssemblyName System.Drawing

$pngPath = "src/Rectangle.Windows/Assets/AppIcon.png"
$icoPath = "src/Rectangle.Windows/Assets/AppIcon.ico"

$bmp = [System.Drawing.Bitmap]::FromFile($pngPath)
$icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())
$stream = [System.IO.File]::Create($icoPath)
$icon.Save($stream)
$stream.Close()
$bmp.Dispose()

Write-Host "Icon created successfully at $icoPath"
