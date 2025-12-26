@echo off

set PUBLIC_REPO=D:\Repos\EMILtools-Public
set PRIVATE_REPO=D:\Repos\EMIL\EMIL

echo --- 1/4 SYNCING FILES ---
powershell -ExecutionPolicy Bypass -File "./ReleaseTools.ps1"

echo --- 2/4 CHECKING REMOTE STATUS ---
cd /d "%PUBLIC_REPO%"

git fetch origin

FOR /F "delims=" %%i IN ('git rev-parse HEAD') DO set LOCAL=%%i
FOR /F "delims=" %%i IN ('git rev-parse origin/main') DO set REMOTE=%%i

IF NOT "%LOCAL%"=="%REMOTE%" (
    echo.
    echo **************************************
    echo  Your local branch is BEHIND remote.
    echo  Pulling now before pushing...
    echo **************************************
    echo.
	echo Pulling latest changes...
	git pull --rebase
)

echo --- 3/4 PUSHING PUBLIC REPO ---
cd /d "%PUBLIC_REPO%"
git add --all
git diff-index --quiet HEAD -- || git commit -m "Auto-sync from Private"
git push origin main

echo --- 4/4 SYNC AND PUSH COMPLETE ---
echo Waiting 3 seconds before closing...
timeout /t 3 /nobreak >nul
exit /b 0
