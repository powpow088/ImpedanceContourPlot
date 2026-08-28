Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   開始打包 阻抗等高線分析 工具..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 關閉可能仍在背景運行的舊程序
Stop-Process -Name "阻抗等高線分析" -ErrorAction SilentlyContinue

Write-Host "`n[1/2] 正在編譯前端網頁資源 (Vite)..." -ForegroundColor Yellow
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "[錯誤] 前端編譯失敗！" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/2] 正在產生專屬圖示免安裝啟動器 (.exe)..." -ForegroundColor Yellow
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (Test-Path $csc) {
    & $csc /nologo /target:winexe /win32icon:app.ico /out:dist\阻抗等高線分析.exe launcher\Launcher.cs
    Copy-Item -Path "app.ico" -Destination "dist\app.ico" -Force
    Write-Host "[成功] 已產出: dist\阻抗等高線分析.exe" -ForegroundColor Green
} else {
    Write-Host "[提示] 未偵測到 C# 編譯器" -ForegroundColor DarkYellow
}

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "打包完成！" -ForegroundColor Green
Write-Host "發布時只需將【dist】資料夾壓縮提供給對方，" -ForegroundColor White
Write-Host "對方雙擊【阻抗等高線分析.exe】即可直接開啟！" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green