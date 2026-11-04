param([string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.8f1\Editor\Unity.exe',
    [ValidateSet('M1','M2')][string]$Milestone = 'M1')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
# Reuse only the generated validation cache, never the open working project.
$cacheRoot = Join-Path $projectRoot '.m0-validation'
$stage = Get-ChildItem -LiteralPath $cacheRoot -Directory | Where-Object {
    $_.Name -match '^\d{8}-\d{6}$' -and (Test-Path (Join-Path $_.FullName 'ProjectSettings/ProjectVersion.txt'))
} | Sort-Object Name -Descending | Select-Object -First 1
if (-not $stage) { throw 'Run the M0 isolated build first to create a validation cache.' }
$stagePath = $stage.FullName
if (Test-Path (Join-Path $stagePath 'Temp/UnityLockfile')) { throw 'Validation Editor may still be running. Close it before rebuilding.' }
foreach ($folder in @('Assets','Packages','ProjectSettings')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $folder) -Destination $stagePath -Recurse -Force
}
$buildPath = Join-Path $projectRoot "Build/Windows-$Milestone/QuaThangNayDa-$Milestone.exe"
$logDirectory = Join-Path $projectRoot "Logs/$Milestone"
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$logPath = Join-Path $logDirectory ('build-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')
Write-Host "$Milestone build log: $logPath"
$buildClass = if ($Milestone -eq 'M2') { 'MorningTools' } else { 'TravelTools' }
$arguments = @('-batchmode','-nographics','-quit','-projectPath', ('"' + $stagePath + '"'),
    '-buildTarget','Win64','-executeMethod',"QuaThangNayDa.Editor.$buildClass.BuildBatch",
    "-$($Milestone.ToLower())-build-output", ('"' + $buildPath + '"'), '-logFile', ('"' + $logPath + '"'))
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Unity build failed: $($process.ExitCode). See $logPath" }
if (-not (Test-Path $buildPath)) { throw 'M1 executable missing.' }
# Copy generated scene/materials only when absent from the real project.
$generated = @('Scenes/TravelTest.unity','Art/Materials/M1Road.mat','Art/Materials/M1Buildings.mat','Art/Materials/M1Scooter.mat')
if ($Milestone -eq 'M2') { $generated += @('Scenes/MorningTest.unity','Data/MorningConfig.asset') }
foreach ($relative in $generated) {
    foreach ($suffix in @('.meta','')) {
        $target = Join-Path $projectRoot "Assets/_Project/$relative$suffix"
        if (-not (Test-Path -LiteralPath $target)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            Copy-Item -LiteralPath (Join-Path $stagePath "Assets/_Project/$relative$suffix") -Destination $target
        }
    }
}
foreach ($meta in Get-ChildItem -LiteralPath "$stagePath/Assets" -Filter '*.meta' -Recurse) {
    $target = Join-Path $projectRoot $meta.FullName.Substring($stagePath.Length + 1)
    if ((Test-Path -LiteralPath $target.Substring(0,$target.Length-5)) -and -not (Test-Path -LiteralPath $target)) {
        Copy-Item -LiteralPath $meta.FullName -Destination $target
    }
}
Write-Host "$Milestone ready: $buildPath"
