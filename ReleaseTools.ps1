# Define paths relative to this script's location
$sourcePath = "D:/Repos/EMIL/EMIL/Assets/EMILtools-Private/"
$publicPath = "D:/Repos/EMILtools-Public/EMILtools/"

# List of specific files to sync
$files = @(
    "Utilities/SignalUtility.cs",
    "Extensions/DelegateEX.cs",
    "Extensions/NumEX.cs",
    "Extensions/EnumerateEX.cs"
)

# List of folders to sync recursively
$folders = @(
    "Timers"
)

# 1. Clean the target public folder first
if (Test-Path $publicPath) { 
    Write-Host "Cleaning target folder..." -ForegroundColor Cyan
    Remove-Item $publicPath -Recurse -Force 
}
New-Item -ItemType Directory -Path $publicPath -Force

# 2. Copy individual files logic
Write-Host "Syncing files..." -ForegroundColor Cyan
foreach ($f in $files) {
    $srcFile = Join-Path $sourcePath $f
    $destFile = Join-Path $publicPath $f
    $destDir = Split-Path $destFile
    
    if (!(Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force }
    
    if (Test-Path $srcFile) {
        Copy-Item $srcFile $destFile -Force
        # Sync meta file
        if (Test-Path ($srcFile + ".meta")) {
            Copy-Item ($srcFile + ".meta") ($destFile + ".meta") -Force
        }
        Write-Host "Synced: $f"
    } else {
        Write-Host "Warning: Source file not found - $f" -ForegroundColor Yellow
    }
}

# 3. Sync folders using robust copy
Write-Host "Syncing folders..." -ForegroundColor Cyan
foreach ($folder in $folders) {
    $srcDir = Join-Path $sourcePath $folder
    $destDir = Join-Path $publicPath $folder
    
    if (Test-Path $srcDir) {
        # Using Copy-Item with Recurse and Container logic
        Copy-Item -Path $srcDir -Destination $publicPath -Recurse -Force
        Write-Host "Synced folder: $folder"
    }
}

Write-Host "`nSync Complete! Private -> Public" -ForegroundColor Green
