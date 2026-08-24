@echo off
cd /d "%~dp0"
echo 正在打包成靜態網頁 (dist)...
call npm run build
echo 打包完成！產物在 dist 資料夾內。
pause
