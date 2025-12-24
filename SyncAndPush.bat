@echo off

set PUBLIC_REPO=D:\Repos\EMILtools-Public
set PRIVATE_REPO=D:\Repos\EMIL\EMIL

echo --- 1/4 SYNCING FILES ---
powershell -ExecutionPolicy Bypass -File "./ReleaseTools.ps1"

echo --- 2/4 PUSHING PUBLIC REPO ---
cd /d "%PUBLIC_REPO%"
git add --all
git diff-index --quiet HEAD -- || git commit -m "Auto-sync from Private"
git push origin main

echo --- 3/4 PUSHING PRIVATE REPO ---
cd /d "%PRIVATE_REPO%"
git add .
git diff-index --quiet HEAD -- || git commit -m "Sync to public"
git push origin main

echo --- 4/4 SYNC AND PUSH COMPLETE ---
pause
