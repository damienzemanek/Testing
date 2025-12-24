@echo off
echo --- SYNCING TO PUBLIC ---
powershell -ExecutionPolicy Bypass -File "./ReleaseTools.ps1"

echo --- PUSHING PUBLIC REPO ---
cd /d "D:\Repos\EMILtools-Public\EMILtools"
git add .
git commit -m "Auto-sync from Private"
git push origin main

echo --- PUSHING PRIVATE REPO ---
cd /d "D:\Repos\EMIL\EMIL"
git add .
git commit -m "Sync to public"
git push origin main

pause
