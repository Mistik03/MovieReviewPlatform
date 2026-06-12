# Builds the final submission archive in the structure required by the course guidelines:
#   slides/  video/  paper/  code/
# Run from anywhere:  powershell -File submission/make_submission_zip.ps1

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$stage = Join-Path $env:TEMP "marquee_submission"
$zipName = "ArbXhelili_final_project.zip"
$zipPath = Join-Path (Split-Path $repo -Parent) $zipName

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Path $stage\slides, $stage\video, $stage\paper, $stage\code | Out-Null

Copy-Item "$repo\submission\slides\*" $stage\slides -Recurse
Copy-Item "$repo\submission\paper\*" $stage\paper -Recurse
Copy-Item "$repo\submission\video\*" $stage\video -Recurse

# code/ = a clean export of the repository (tracked files only — no node_modules, bin, obj, secrets)
git -C $repo archive --format=zip --output "$env:TEMP\marquee_code.zip" HEAD
Expand-Archive "$env:TEMP\marquee_code.zip" -DestinationPath $stage\code
Remove-Item "$env:TEMP\marquee_code.zip"

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$stage\*" -DestinationPath $zipPath
Remove-Item $stage -Recurse -Force

Write-Host "Submission archive created: $zipPath"
Write-Host "Reminder: record the demo video into submission/video/ before the final run."
