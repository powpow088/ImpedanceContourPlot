@echo off
cd /d "%~dp0"
echo 正在啟動生產環境預覽伺服器...
start http://localhost:4173/
call npm run preview
pause
