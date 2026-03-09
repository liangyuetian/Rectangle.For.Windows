#Requires -RunAsAdministrator

Write-Host "正在停止资源管理器..." -ForegroundColor Yellow
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue

Write-Host "切换到图标缓存目录..." -ForegroundColor Yellow
Set-Location "$env:LOCALAPPDATA\Microsoft\Windows\Explorer" -ErrorAction Stop

Write-Host "去除属性并删除缓存文件..." -ForegroundColor Yellow
Get-Item -Path iconcache* -Force -ErrorAction SilentlyContinue | ForEach-Object { $_.Attributes = 'Normal' }
Remove-Item -Path iconcache* -Force -ErrorAction SilentlyContinue

Write-Host "重启资源管理器..." -ForegroundColor Yellow
Start-Process explorer

Write-Host "`n清理完成！如果仍有文件没删掉，请重启电脑后再运行一次此脚本。" -ForegroundColor Green
Write-Host "图标会在几秒到几分钟内自动重建。" -ForegroundColor Green
Pause