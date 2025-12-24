@echo off

set PUBLIC_REPO=D:\Repos\EMILtools-Public
set PRIVATE_REPO=D:\Repos\EMIL\EMIL

echo --- 1/3 SYNCING FILES ---
powershell -ExecutionPolicy Bypass -File "./ReleaseTools.ps1"

echo --- 2/3 PUSHING PUBLIC REPO ---
cd /d "%PUBLIC_REPO%"
git add --all
git diff-index --quiet HEAD -- || git commit -m "Auto-sync from Private"
git push origin main

echo --- 3/3 SYNC AND PUSH COMPLETE ---
exit /b 0
